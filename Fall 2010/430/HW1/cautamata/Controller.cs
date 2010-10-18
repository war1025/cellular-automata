
using System.Collections.Generic;
using CAutamata;
using System.Threading;

namespace CAServer {

	public class Controller : IController {

		private ICASettings caSettings;
		private CABoard board;
		private IDictionary<Point, uint> accumulated;

		private Queue<StateEvent> queue;
		private State state;

		private object accumulatorLock;
		private object queueLock;

		public Controller() {
			this.accumulated = new Dictionary<Point, uint>();
			this.state = UnInited;
			this.queue = new Queue<StateEvent>();

			this.accumulatorLock = new object();
			this.queueLock = new object();

			new Thread(caRunner).Start();
		}

		public bool init(string code, uint defaultState) {
			if(state == UnInited) {
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
			if(state == UnInited) {
				if(caSettings != null) {
					board = new CABoard(500, defaultState);
					board.setCASettings(caSettings);
					state = Stopped;
					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}
		}

		public bool start() {
			if(state == Stopped) {
				state = Running;
				var s = new StateEvent(state);
				lock(queueLock) {
					queue.Enqueue(s);
					Monitor.PulseAll(queueLock);
				}
				s.Wait();
				return true;
			} else if(state == Running){
				return true;
			} else {
				return false;
			}
		}

		public bool stop() {
			if(state == Running) {
				state = Stopped;
				var s = new StateEvent(state);
				lock(queueLock) {
					queue.Enqueue(s);
					Monitor.PulseAll(queueLock);
				}
				s.Wait();
				return true;
			} else if(state == Stopped) {
				return true;
			} else {
				return false;
			}
		}

		public bool step() {
			if(state == Stopped) {
				var s = new StateEvent(Step);
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

		public IDictionary<Point, uint> pullChanges() {
			lock(accumulatorLock) {
				IDictionary<Point, uint> ret = accumulated;
				accumulated = new Dictionary<Point, uint>();
				return ret;
			}
		}

		public bool pushChanges(IDictionary<Point, uint> changes) {
			if(state == Stopped) {
				board.userChanged(changes);
				return true;
			} else {
				return false;
			}
		}

		public bool shutdown() {

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
			State curState = UnInited;
			StateEvent curEvent = null;
			while(true) {
				if(curEvent != null) {
					curEvent.Validate();
					curEvent = null;
				}
				lock(queueLock) {
					while((curState != Running) && (queue.Count == 0)) {
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
