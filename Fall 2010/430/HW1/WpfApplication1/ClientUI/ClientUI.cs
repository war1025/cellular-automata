
using System.Collections.Generic;
using System.Threading;

using CAutamata;


namespace CAClient {

	/**
	 * Types of errors the CA may encounter
	 **/
	public enum CAErrorType { StateLoad, CALoad, StateSave, Play, Stop, Step, Clear, Update }
	/**
	 * The states the CA can be in
	 **/
	public enum State { UnInited, Running, Stopped }

	/**
	 * Indicates that updates have been made to the CA board state
	 *
	 * @param updates The updates to the board
	 * @param colors Suggested colors for each state
	 **/
	public delegate void CAStateUpdate(Dictionary<Point, uint> updates, System.Drawing.Color[] colors);

	/**
	 * Indicates that the CA board state has been cleared
	 *
	 * @param state The state that the board was cleared to
	 * @param colors Suggested colors for each state
	 **/
	public delegate void CAStateClear(uint state, System.Drawing.Color[] colors);

	/**
	 * Indicates that the CA simulation has transitioned to a new state
	 *
	 * @param newState The state the simulation has transitioned to
	 **/
	public delegate void CAStateTransition(State newState);

	/**
	 * Indicates that an error has occured within the CA
	 *
	 * @param type The type of the error
	 * @param message A message describing the error
	 **/
	public delegate void CAError(CAErrorType type, string message);

	/**
	 * Indicates that one of the colors for a CA state has changed.
	 * This requires a complete repaint of the board
	 *
	 * @param board The complete board state
	 * @param colors Suggested colors for each state
	 **/
	public delegate void CAColorChange(uint[][] board, System.Drawing.Color[] colors);

	/**
	 * An internal event for adding calls to the proper thread
	 **/
	internal delegate void CAStateEvent();

	/**
	 * ClientUI is a further abstraction on top of IController. It implements an event loop to synchronize calls to the
	 * Server. It also tracks the state of the CA and emits events when changes occur to the CA.
	 **/
	public class ClientUI {

		// The controller backing the ClientUI
		private ClientController controller;
		// The colors for each state
		private System.Drawing.Color[] colors;
		// The address that the server is at
		private string address;

		// The current state of the CA on the client side
		private uint[][] board;
		// How many states there should be in the CA.
		private uint numStates;

		// Changes made on the Client side that need to be pushed to the server before
		// stepping or running the simulation
		private Dictionary<Point, uint> pendingChanges;

		// A lock for the queue
		private object queueLock;
		// The message queue for the event loop
		private Queue<CAStateEvent> queue;
		// The CA's current state
		private State curState;

		// Whether the ClientUI is running
		private bool running;

		// Events corresponding to the delegates described above.
		public event CAStateUpdate caUpdated;
		public event CAStateClear caCleared;
		public event CAStateTransition caTransition;
		public event CAError caError;
		public event CAColorChange caColorChange;

		/**
		 * Create a ClientUI with a server at the given address
		 *
		 * @param address The address the server is publishing from
		 **/
		public ClientUI(string address) {

			this.controller = new ClientController(address);
			this.address = address;

			this.queueLock = new object();
			this.queue = new Queue<CAStateEvent>();

			this.pendingChanges = new Dictionary<Point, uint>();
			this.curState = State.UnInited;
			this.running = true;

			this.board = new uint[500][];
			for(int i = 0; i < 500; i++) {
				board[i] = new uint[500];
			}

			// Start the queueThread event loop
			var thread = new Thread(queueThread);
			thread.Start();

		}

		/**
		 * A request to start the CA.
		 * Any pending changes will be pushed to server.
		 * The controller will then issue a start command.
		 * Upon success, the CA state will transition and changes will be pulled.
		 **/
		public void start() {

			enqueue(() => {
				if(curState == State.Stopped) {
					if(pendingChanges.Count > 0) {
						controller.pushChanges(toArr(pendingChanges));
						pendingChanges.Clear();
					}
					if(controller.start()) {
						stateTransition(State.Running);
						pullChanges();
					} else {
						sendError(CAErrorType.Play, "Could not start simulation");
					}
				} else {
					sendError(CAErrorType.Play, "Simulation must be stopped before it can be started");
				}
			});
		}

		/**
		 * A request to stop the CA.
		 * The controller issues a stop command, then pulls changes.
		 * The state is transitioned to stop and the UI is updated.
		 **/
		public void stop() {

			enqueue(() => {
				if(curState == State.Running) {
					controller.stop();
					var changes = toDict(controller.pullChanges());
					stateTransition(State.Stopped);
					updateUI(changes);
				} else {
					sendError(CAErrorType.Stop, "Simulation must be running before it can be stopped");
				}
			});
		}

		/**
		 * A request to step the CA one step.
		 * First, push any pending changes.
		 * Then, step, pull changes, update UI
		 **/
		public void step() {

			enqueue(() => {
				if(curState == State.Stopped) {
					if(pendingChanges.Count > 0) {
						controller.pushChanges(toArr(pendingChanges));
						pendingChanges.Clear();
					}
					controller.step();
					var changes = toDict(controller.pullChanges());
					updateUI(changes);
				} else {
					sendError(CAErrorType.Step, "Simulation must be stopped before it can be stepped");
				}
			});
		}

		/**
		 * A request to set the state of a point
		 * Mark the change as pending and update the UI
		 *
		 * @param p The point the change
		 * @param state The state to transition to
		 **/
		public void setState(Point p, uint state) {

			enqueue(() => {
				if(curState == State.Stopped) {
					pendingChanges[p] = state;
					var dict = new Dictionary<Point, uint>();
					dict[p] = state;
					updateUI(dict);
				} else {
					sendError(CAErrorType.Update, "Simulation must be stopped before points can be updated");
				}
			});
		}

		/**
		 * A request to toggle the state of a point.
		 * This involves advancing the state by one value.
		 * Mark the change as pending and update the UI
		 *
		 * @param p The point to change
		 **/
		public void toggleState(Point p) {

			enqueue(() => {
				if(curState == State.Stopped) {
					uint newState = (board[p.x][p.y] + 1) % numStates;
					pendingChanges[p] = newState;
					var dict = new Dictionary<Point, uint>();
					dict[p] = newState;
					updateUI(dict);
				} else {
					sendError(CAErrorType.Update, "Simulation must be stopped before points can be updated");
				}
			});
		}

		/**
		 * A request to load a CA from file
		 * First, take measures to shutdown any previously running CA.
		 * Then parse the file and have the controller send an init request.
		 * If the init succeeds, setup the colors for the CA.
		 * This involves using any values the file provided and filling in the rest at random.
		 * Transition to a Stopped state and clear the UI to the default state.
		 *
		 * Otherwise, send an error and indicate any compile errors the init request returned.
		 * And transition to an Uninited state
		 *
		 * @param filename The file the CA settings are described in
		 **/
		public void loadCA(string filename) {

			enqueue(() => {
				if((curState == State.Stopped)) {
					if(!controller.shutdown()) {
						sendError(CAErrorType.CALoad, "Could not shut down previous CA");
						return;
					}
				}
				if((curState == State.Stopped) || (curState == State.UnInited)) {
					var comps = CAParser.parseCASettings(filename);
					string errors;
					if(controller.init(comps.code, comps.defaultState, out errors)) {
						this.numStates = comps.numStates;
						this.colors = new System.Drawing.Color[numStates];
						int i = 0;
						for(i = 0; i < comps.colors.Length; i++) {
							colors[i] = comps.colors[i];
						}
						assignColors(i);
						stateTransition(State.Stopped);
						clearUI(comps.defaultState);
					} else {
						stateTransition(State.UnInited);
						sendError(CAErrorType.CALoad, "Could not load the CA from the file\n" + errors);
					}
				} else {
					sendError(CAErrorType.CALoad, "Simulation must be stopped before a new CA can be loaded");
				}
			});
		}

		/**
		 * A request to load a CA using the given parameters
		 * First, take measures to shutdown any previously running CA.
		 * Then parse the parameters and have the controller send an init request.
		 * If the init succeeds, setup the colors for the CA.
		 * This involves filling the values at random.
		 * Transition to a Stopped state and clear the UI to the default state.
		 *
		 * Otherwise, send an error and indicate any compile errors the init request returned.
		 * And transition to an Uninited state
		 *
		 * @param name The name of the CA
		 * @param numStates How many states the CA has
		 * @param defaultState The CA's default state
		 * @param neighborhood The set of points in the neighborhood
		 * @param delta The body of the nextState() function
		 **/
		public void loadCA(string name, uint numStates, uint defaultState, string neighborhood, string delta) {

			enqueue(() => {
				if((curState == State.Stopped)) {
					if(!controller.shutdown()) {
						sendError(CAErrorType.CALoad, "Could not shut down previous CA");
						return;
					}
				}
				if((curState == State.Stopped) || (curState == State.UnInited)) {
					var comps = CAParser.parseCASettings(name, numStates, defaultState, neighborhood, delta);
					string errors;
					if(controller.init(comps.code, comps.defaultState, out errors)) {
						this.numStates = comps.numStates;
						this.colors = new System.Drawing.Color[numStates];
						assignColors(0);
						stateTransition(State.Stopped);
						clearUI(comps.defaultState);
					} else {
						stateTransition(State.UnInited);
						sendError(CAErrorType.CALoad, "Could not load the described CA\n" + errors);
					}
				} else {
					sendError(CAErrorType.CALoad, "Simulation must be stopped before a new CA can be loaded");
				}
			});
		}

		/**
		 * A request to load a state from file
		 * First, parse the state file
		 * Then, shutdown the previous CA and reinit it using the state info.
		 * Clear and Update the UI
		 *
		 * @param filename The file the state info is stored in
		 **/
		public void loadState(string filename) {

			enqueue(() => {
				if(curState == State.Stopped) {
					var state = CAParser.parseCAState(filename);
					controller.shutdown();
					if(controller.reinit(state.defaultState)) {
						controller.pushChanges(toArr(state.states));
						clearUI(state.defaultState);
						updateUI(state.states);
					} else {
						sendError(CAErrorType.StateLoad, "Could not load the CA state from file");
					}
				} else {
					sendError(CAErrorType.StateLoad, "Simulation must be stopped before a state can be loaded");
				}
			});
		}

		/**
		 * A request to clear the CA to a state
		 * If the state is valid within the CA, issue a shutdown and reinit to the controller.
		 * Upon success, clear the UI.
		 *
		 * @param val The state to clear to
		 **/
		public void clearState(uint val) {

			enqueue(() => {
				if (curState == State.Stopped) {
					if (val < numStates) {
						controller.shutdown();
						if (controller.reinit(val)) {
							clearUI(val);
						} else {
							sendError(CAErrorType.StateLoad, "Could not clear the grid to state: " + val);
						}
					} else {
						sendError(CAErrorType.StateLoad, "State value out of range.");
					}
				} else {
					sendError(CAErrorType.StateLoad, "Simulation must be stopped before the grid can be cleared");
				}
			});
		}

		/**
		 * A request to save the state of the CA
		 * Encode the state and write it to file.
		 *
		 * @param filename The file to store the state in
		 **/
		public void saveState(string filename) {

			enqueue(() => {
				if(curState == State.Stopped) {
					CAParser.saveCAState(filename, board);
				} else {
					sendError(CAErrorType.StateSave, "Simulation must be stopped before a state can be saved");
				}
			});
		}

		/**
		 * A request to set the color for a given state
		 * Update the colors array and issue a color updated event
		 *
		 * @param state The state to change the color of
		 * @param color The new color for state
		 **/
		public void setColor(uint state, System.Drawing.Color color) {

			enqueue(() => {
				if(curState == State.Stopped) {
					colors[state] = color;
					colorsUpdated();
				} else {
					sendError(CAErrorType.Update, "Simulation must be stopped to updated colors");
				}
			});
		}

		/**
		 * A request to pull any new changes from the Server
		 * Pull changes and update the UI
		 **/
		public void pullChanges() {

			enqueue(() => {
				if(curState == State.Running) {
					var changes = toDict(controller.pullChanges());
					updateUI(changes);
				}
			});
		}

		/**
		 * A request to shutdown the current CA
		 * Stop and Shutdown the CA. Transition to Uninited. Kill the event loop.
		 **/
		public void shutdown() {

			enqueue(() => {
				if(curState == State.Running) {
					controller.stop();
				}
				if(curState != State.UnInited) {
					controller.shutdown();
				}
				stateTransition(State.UnInited);
				this.running = false;
			});
		}

		/**
		 * Adds a request onto the event queue and notifies the queue thread of the addition
		 *
		 * @param stateEvent The event to execute on the queue thread
		 **/
		private void enqueue(CAStateEvent stateEvent) {
			lock(queueLock) {
				queue.Enqueue(stateEvent);
				Monitor.PulseAll(queueLock);
			}
		}

		/**
		 * Fills the color array with random colors starting from start.
		 *
		 * @param start The first state without a color assigned to it.
		 **/
		private void assignColors(int start) {
			var rand = new System.Random();
			for(int i = start; i < colors.Length; i++) {
				colors[i] = System.Drawing.Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
			}
		}

		/**
		 * Decodes values sent over the IController into a Dictionary of point to state
		 *
		 * @param arr The encoded point state values
		 *
		 * @return The values in a decoded form.
		 **/
		private Dictionary<Point, uint> toDict(int[] arr) {
			var ret = new Dictionary<Point, uint>();
			foreach (int i in arr) {
				// First 9 bits
				int x = (i >> 23) & (0x1ff);
				// Second 9 bits
				int y = (i >> 14) & (0x1ff);
				// Last 14 bits
				uint val = (uint)i & (0x3fff);
				ret[new Point(x, y)] = val;
			}
			return ret;
		}

		/**
		 * Encodes a Dictionary of point to uint for transmission over the IController
		 *
		 * @param ret The points and their values
		 *
		 * @return The point value pairs in an encoded form
		 **/
		private int[] toArr(Dictionary<Point, uint> ret) {
			var ret2 = new List<int>();
			foreach (KeyValuePair<Point, uint> kv in ret) {
				Point p = kv.Key;
				// Last 14 bits
				int val = (int)(kv.Value & (0x3fff));
				// First 9 bits
				val |= p.x << 23;
				// Middle 9 bits
				val |= p.y << 14;
				ret2.Add(val);
			}
			return ret2.ToArray();
		}

		/**
		 * Updates the internal board state, checks for illegal state transitions, and issues update events
		 *
		 * @param changes The changes to the UI
		 **/
		private void updateUI(Dictionary<Point, uint> changes) {
			foreach(var kv in changes) {
				Point p = kv.Key;
				board[p.x][p.y] = kv.Value;
				if (kv.Value >= numStates) {
					sendError(CAErrorType.Update, "The CA transitioned to an illegal state and has been shut down.");
					if (curState == State.Running) {
						controller.stop();
					}
					controller.shutdown();
					stateTransition(State.UnInited);
					return;
				}
			}
			if(caUpdated != null) {
				caUpdated(changes, colors);
			}
		}

		/**
		 * Sends an error to any registered listeners
		 *
		 * @param type The type of error
		 * @param message The error message
		 **/
		private void sendError(CAErrorType type, string message) {
			if(caError != null) {
				caError(type, message);
			}
		}

		/**
		 * Check that the clearing state is legal, updated the internal board state, issue an clear event
		 *
		 * @param defaultState The state to clear to
		 **/
		private void clearUI(uint defaultState) {
			if (defaultState >= numStates) {
				sendError(CAErrorType.Update, "The CA transitioned to an illegal state and has been shut down.");
				if (curState == State.Running) {
					controller.stop();
				}
				controller.shutdown();
				stateTransition(State.UnInited);
				return;
			}
			foreach(uint[] b in board) {
				for(int i = 0; i < b.Length; i++) {
					b[i] = defaultState;
				}
			}
			if(caCleared != null) {
				caCleared(defaultState, colors);
			}
		}

		/**
		 * Copy the internal board state and issue a color change event to any listeners
		 **/
		private void colorsUpdated() {
			if(caColorChange != null) {
				uint[][] board2 = new uint[500][];
				for(int i = 0; i < board2.Length; i++) {
					board2[i] = new uint[500];
				}
				for(int i = 0; i < board.Length; i++) {
					for(int j = 0; j < board.Length; j++) {
						board2[i][j] = board[i][j];
					}
				}
				if(caColorChange != null) {
					caColorChange(board2, colors);
				}
			}
		}

		/**
		 * Change the internal state and issue a state transition event to any listeners
		 **/
		private void stateTransition(State state) {
			this.curState = state;
			if (caTransition != null) {
				caTransition(state);
			}
		}

		/**
		 * The queue thread which ensures a synchronous ordering of events in the CA simulation.
		 * This has two purposes. One, it provides a definite ordering of events, eliminating the possibility of race conditions.
		 * Second, it offloads large computations from the rendering thread of the GUI. This leaves the gui responsive even during
		 * heavy network / computation times.
		 **/
		private void queueThread() {

			while(running) {
				CAStateEvent ev = null;
				lock(queueLock) {
					while(queue.Count == 0) {
						Monitor.Wait(queueLock);
					}
					ev = queue.Dequeue();
				}
				ev();
			}
		}
	}
}


