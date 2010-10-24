
using System.Collections.Generic;
using CAutamata;


namespace CAClient {

	public enum CAErrorType { StateLoad, CALoad, StateSave, Play, Stop, Step, Clear, Update }
	private enum State { UnInited, Running, Stopped }

	public delegate void CAStateUpdate(Dictionary<Point, uint> updates, System.Drawing.Color[] colors);
	public delegate void CAStateClear(uint state, System.Drawing.Color[] colors);
	public delegate void CAError(CAErrorType type, string message);
	public delegate void CAColorChange(uint[][] board, System.Drawing.Color[] colors);
	internal delegate void CAStateEvent();

	public class ClientUI {

		private ClientController controller;
		private System.Drawing.Color[] colors;

		private uint[][] board;
		private uint numStates;

		private Dictionary<Point, uint> pendingChanges;

		private object queueLock;
		private Queue<CAStateEvent> queue;
		private State curState;

		public event CAStateUpdate caUpdated;
		public event CAStateClear caCleared;
		public event CAError caError;
		public event CAColorChange caColorChange;

		public ClientUI(string address) {

			this.controller = new ClientController(address);

			this.queueLock = new object();
			this.queue = new Queue<CAStateEvent>();

			this.pendingChanges = new Dictionary<Point, uint>();
			this.curState = State.UnInited;

			this.board = new uint[500][];
			for(int i = 0; i < 500; i++) {
				board[i] = new uint[500];
			}

		}

		public void start() {

			enqueue(() => {
				if(curState == State.Stopped) {
					if(pendingChanges.Count > 0) {
						controller.pushChanges(pendingChanges);
						pendingChanges.Clear();
					}
					if(controller.start()) {
						curState = State.Running;
						pullChanges();
					} else {
						sendError(CAErrorType.Play, "Could not start simulation");
					}
				} else {
					sendError(CAErrorType.Play, "Simulation must be stopped before it can be started");
				}
			});
		}

		public void stop() {

			enqueue(() => {
				if(curState == State.Running) {
					var ret = controller.stop();
					var changes = controller.pullChanges();
					curState = State.Stopped;
					updateUI(changes);
				} else {
					sendError(CAErrorType.Stop, "Simulation must be running before it can be stopped");
				}
			});
		}

		public void step() {

			enqueue(() => {
				if(curState == State.Stopped) {
					if(pendingChanges.Count > 0) {
						controller.pushChanges(pendingChanges);
						pendingChanges.Clear();
					}
					var ret = controller.step();
					var changes = controller.pullChanges();
					updateUI(changes);
				} else {
					sendError(CAErrorType.Step, "Simulation must be stopped before it can be stepped");
				}
			});
		}

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

		public void toggleState(Point p) {

			enqueue(() => {
				if(curState == State.Stopped) {
					uint newState = (board[p.x][p.y] + 1) % numStates;
					pendingChanges[p] = newState;
					var dict = new Dictionary<Point, uint>();
					dict[p] = state;
					updateUI(dict);
				} else {
					sendError(CAErrorType.Update, "Simulation must be stopped before points can be updated");
				}
			});
		}

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

					if(controller.init(comps.code, comps.defaultState)) {
						this.numStates = comps.numStates;
						this.colors = new System.Drawing.Color[numStates];
						int i = 0;
						for(i = 0; i < comps.colors.Length; i++) {
							colors[i] = comps.colors[i];
						}
						assignColors(i);
						curState = State.Stopped;
						clearUI(state.defaultState);
					} else {
						sendError(CAErrorType.CALoad, "Could not load the CA from the file");
					}
				} else {
					sendError(CAErrorType.CALoad, "Simulation must be stopped before a new CA can be loaded");
				}
			});
		}

		public void loadState(string filename) {

			enqueue(() => {
				if(curState == State.Stopped) {
					var state = CAParser.parseCAState(filename);
					if(controller.reinit(state.defaultState)) {
						controller.pushChanges(state.states);
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

		public void saveState(string filename) {

			enqueue(() => {
				if(curState == State.Stopped) {
					CAParser.saveCAState(filename, board);
				} else {
					sendError(CAErrorType.StateSave, "Simulation must be stopped before a state can be saved");
				}
			});
		}

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

		public void pullChanges() {

			enqueue(() => {
				if(curState == State.Running) {
					var changes = controller.pullChanges();
					updateUI(changes);
				}
			});
		}
	}
}


