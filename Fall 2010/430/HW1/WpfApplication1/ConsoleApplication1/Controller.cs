
using System.Collections.Generic;
using CAutamata;
using System.Threading;
using System.ServiceModel;

namespace CAServer {

	/**
	 * The Controller class that we publish over WCF
	 **/
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
	public class Controller : IController {

		// The ICASettings instance that is driving the board.
		private ICASettings caSettings;
		// The current state board
		private CABoard board;
		// The map of changes since the last pull
		private Dictionary<Point, uint> accumulated;
		// The last state the client was in
		private uint[][] lastState;

		// Event queue so things stay synchronized
		private Queue<StateEvent> queue;
		private State state;

		// Locks
		private object accumulatorLock;
		private object queueLock;

		/**
		 * Creates a new Controller instance
		 *
		 * This creates the runner thread and and initializes the state variables we know about right now
		 **/
		public Controller() {
			this.accumulated = new Dictionary<Point, uint>();
			this.state = State.UnInited;
			this.queue = new Queue<StateEvent>();

			this.accumulatorLock = new object();
			this.queueLock = new object();

			this.lastState = new uint[500][];
			for(int i = 0; i < 500; i++) {
				lastState[i] = new uint[500];
			}

			var runner = new Thread(caRunner);
			runner.IsBackground = true;
			runner.Start();
		}

		/**
		 * Attempts to initialize the controller.
		 *
		 * @param code The code for the CASettings
		 * @param defaultState The default state for the board
		 * @param errors Any errors that happen during compile of the CASettings
		 *
		 * @return Whether the Controller was successfully inited
		 **/
		public bool init(string code, uint defaultState, out string errors) {
			if(state == State.UnInited) {
				caSettings = CACompiler.compile(code, out errors);
				if(caSettings != null) {
					return reinit(defaultState);
				} else {
					return false;
				}
			} else {
				errors = "";
				return false;
			}
		}

		/**
		 * Use the current CASettings, but clear the board to the given state.
		 *
		 * @param defaultState The state to clear to
		 *
		 * @return Whether we successfully reinited
		 **/
		public bool reinit(uint defaultState) {
			if(state == State.UnInited) {
				if(caSettings != null) {
					for(int i = 0; i < 500; i++) {
						for(int j = 0; j < 500; j++) {
							lastState[i][j] = defaultState;
						}
					}
					if(board != null) {
						board.closeBoard();
					}
					board = new CABoard(500, defaultState);
					board.setCASettings(caSettings);
					accumulated.Clear();
					state = State.Stopped;
					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}
		}

		/**
		 * Start the simulation running. We do this by pushing a run event into the queue.
		 * The runner thread will see this and begin stepping as quickly as possible until the state is changed.
		 *
		 * @return Whether we started the simulation
		 **/
		public bool start() {
			if(state == State.Stopped) {
				state = State.Running;
				var s = new StateEvent(state);
				lock(queueLock) {
					queue.Enqueue(s);
					Monitor.PulseAll(queueLock);
				}
				s.Wait();
				return true;
			} else if(state == State.Running){
				return true;
			} else {
				return false;
			}
		}

		/**
		 * Step the simulation. This is done by pushing a stop event to the queue. The runner thread sees this and halts
		 * execution.
		 *
		 * @return Whether we stopped the simulation
		 **/
		public bool stop() {
			if(state == State.Running) {
				state = State.Stopped;
				var s = new StateEvent(state);
				lock(queueLock) {
					queue.Enqueue(s);
					Monitor.PulseAll(queueLock);
				}
				s.Wait();
				return true;
			} else if(state == State.Stopped) {
				return true;
			} else {
				return false;
			}
		}

		/**
		 * Steps the simulation one step. This is done by passing a step event to the queue. The runner thread sees this
		 * event and steps once.
		 *
		 * @return Whether we stepped
		 **/
		public bool step() {
			if(state == State.Stopped) {
				var s = new StateEvent(State.Step);
				lock(queueLock) {
					queue.Enqueue(s);
					Monitor.PulseAll(queueLock);
				}
				s.Wait();
				return true;
			} else {
				return false;
			}
		}

		/**
		 * Send the changes to the client.
		 * We cache the last state sent to the client so that we can send only states that differ
		 * between the last state and the current state. This is important because in the process of running the simulation,
		 * many points may change state only to return to the state they were in on the last pull.
		 * If 600 points have changed state, but only 10 are different now from the last pull, we need only send 10 changes,
		 * not 600.
		 *
		 * For the encoding scheme, see the IController interface
		 *
		 * @return Encoded changes since the last pull
		 **/
		public int[] pullChanges() {
			Dictionary<Point, uint> ret = null;
			lock(accumulatorLock) {
				ret = accumulated;
				accumulated = new Dictionary<Point, uint>();
			}
			var ret2 = new List<int>();
			foreach( KeyValuePair<Point, uint> kv in ret) {
				Point p = kv.Key;
				if(!(lastState[p.x][p.y] == kv.Value)) {
					int val = (int) (kv.Value & (0x3fff));
					val |= p.x << 23;
					val |= p.y << 14;
					ret2.Add(val);
					lastState[p.x][p.y] = kv.Value;
				}
			}
			return ret2.ToArray();
		}

		/**
		 * Update the board state with the given changes.
		 *
		 * For the encoding scheme, see the IController interface
		 *
		 * @param changes Encoded changes that the user has made
		 *
		 * @return Whether the changes were accepted
		 **/
		public bool pushChanges(int[] changes) {
			if(state == State.Stopped) {
				var ret = new Dictionary<Point, uint>();
				foreach( int i in changes) {
					int x = (i >> 23) & (0x1ff);
					int y = (i >> 14) & (0x1ff);
					uint val = (uint)i & (0x3fff);
					ret[new Point(x, y)] = val;
					lastState[x][y] = val;
				}
				board.userChanged(ret);
				return true;
			} else {
				return false;
			}
		}

		/**
		 * Transition the CA into an uninited state. The CA must be inited or reinited before it can be stepped or run again.
		 *
		 * @return Whether the CA was successfully shut down.
		 **/
		public bool shutdown() {
			if(state == State.Stopped) {
				state = State.UnInited;
				var s = new StateEvent(state);
				lock(queueLock) {
					queue.Enqueue(s);
					Monitor.PulseAll(queueLock);
				}
				s.Wait();
				return true;
			} else {
				return false;
			}
		}

		/**
		 * Possible states
		 **/
		private enum State {
			UnInited, Running, Stopped, Step
		}

		/**
		 * A state event. These have a built in Wait and Validate system to ensure that a thread does not
		 * advance until it knows the other thread has read the event and validated it.
		 **/
		private class StateEvent {

			// The state for the event
			private State s;
			// The lock object
			private object o;
			// Validity of the event
			private bool valid;

			/**
			 * Create an event with the given state
			 *
			 * @param s The state for the event
			 **/
			public StateEvent(State s) {
				this.s = s;
				this.o = new object();
			}

			/**
			 * The state for this event
			 **/
			public State S {
				get {
					return s;
				}
			}

			/**
			 * Block until the even is validated
			 **/
			public void Wait() {
				lock(o) {
					if(!valid) {
						Monitor.Wait(o);
					}
				}
			}

			/**
			 * Validate the event
			 **/
			public void Validate() {
				lock(o) {
					valid = true;
					Monitor.PulseAll(o);
				}
			}
		}

		/**
		 * The runner thread. Changes to the board state happen on this thread.
		 **/
		private void caRunner() {
			// The current state of the runner
			State curState = State.UnInited;
			// The event we are processing
			StateEvent curEvent = null;
			while(true) {
				// Validate the event if we can, then discard it.
				if(curEvent != null) {
					curEvent.Validate();
					curEvent = null;
				}
				// Check if there are events to process.
				// If we are running, don't block. Otherwise, do block.
				lock(queueLock) {
					while((curState != State.Running) && (queue.Count == 0)) {
						Monitor.Wait(queueLock);
					}
					if(queue.Count > 0) {
						curEvent = queue.Dequeue();
						curState = curEvent.S;
					}
				}

				// Step the board
				IDictionary<Point, uint> change = board.step();

				// Push changes into the accumulator
				lock(accumulatorLock) {
					foreach(KeyValuePair<Point, uint> kv in change) {
						accumulated[kv.Key] = kv.Value;
					}
				}
			}
		}

	}


}
