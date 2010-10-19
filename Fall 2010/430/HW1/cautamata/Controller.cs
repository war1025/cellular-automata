
using System.Collections.Generic;
using CAutamata;
using System.Threading;

namespace CAServer {

	public class WrapperController : IController {

		private static readonly IController controller = new Controller();

		public bool init(string code, uint defaultState) {
			return controller.init(code, defaultState);
		}

		public bool reinit(uint defaultState) {
			return controller.reinit(defaultState);
		}

		public bool start() {
			return controller.start();
		}

		public bool stop() {
			return controller.stop();
		}

		public bool step() {
			return controller.step();
		}

		public Dictionary<Point, uint> pullChanges() {
			return controller.pullChanges();
		}

		public bool pushChanges(Dictionary<Point, uint> changes) {
			return controller.pushChanges(changes);
		}

		public bool shutdown() {
			return controller.shutdown();
		}


	}

	public class Controller : IController {

		private ICASettings caSettings;
		private CABoard board;
		private Dictionary<Point, uint> accumulated;
		private uint[][] lastState;

		private Queue<StateEvent> queue;
		private State state;

		private object accumulatorLock;
		private object queueLock;

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

			new Thread(caRunner).Start();
		}

		public bool init(string code, uint defaultState) {
			if(state == State.UnInited) {
				caSettings = CACompiler.compile(code);
				if(caSettings != null) {
					return reinit(defaultState);
				} else {
					return false;
				}
			} else {
				return false;
			}
		}

		public bool reinit(uint defaultState) {
			if(state == State.UnInited) {
				if(caSettings != null) {
					for(int i = 0; i < 500; i++) {
						for(int j = 0; j < 500; j++) {
							lastState[i][j] = defaultState;
						}
					}
					board = new CABoard(500, defaultState);
					board.setCASettings(caSettings);
					state = State.Stopped;
					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}
		}

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

		public Dictionary<Point, uint> pullChanges() {
			Dictionary<Point, uint> ret = null;
			lock(accumulatorLock) {
				ret = accumulated;
				accumulated = new Dictionary<Point, uint>();
			}
			Dictionary<Point, uint> ret2 = new Dictionary<Point, uint>();
			foreach( KeyValuePair<Point, uint> kv in ret) {
				Point p = kv.Key;
				if(!(lastState[p.x][p.y] == kv.Value)) {
					ret2[p] = kv.Value;
					lastState[p.x][p.y] = kv.Value;
				}
			}
			return ret2;
		}

		public bool pushChanges(Dictionary<Point, uint> changes) {
			if(state == State.Stopped) {
				board.userChanged(changes);
				return true;
			} else {
				return false;
			}
		}

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

		private enum State {
			UnInited, Running, Stopped, Step
		}

		private class StateEvent {

			private State s;
			private object o;
			private bool valid;

			public StateEvent(State s) {
				this.s = s;
				this.o = new object();
			}

			public State S {
				get {
					return s;
				}
			}

			public void Wait() {
				lock(o) {
					if(!valid) {
						Monitor.Wait(o);
					}
				}
			}

			public void Validate() {
				lock(o) {
					valid = true;
					Monitor.PulseAll(o);
				}
			}
		}

		private void caRunner() {
			State curState = State.UnInited;
			StateEvent curEvent = null;
			while(true) {
				if(curEvent != null) {
					curEvent.Validate();
					curEvent = null;
				}
				lock(queueLock) {
					while((curState != State.Running) && (queue.Count == 0)) {
						Monitor.Wait(queueLock);
					}
					if(queue.Count > 0) {
						curEvent = queue.Dequeue();
						curState = curEvent.S;
					}
				}

				IDictionary<Point, uint> change = board.step();

				lock(accumulatorLock) {
					foreach(KeyValuePair<Point, uint> kv in change) {
						accumulated[kv.Key] = kv.Value;
					}
				}
			}
		}

	}


}
