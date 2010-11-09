
using System.Collections.Generic;
using CAutamata;
using System.Threading;
using System.ServiceModel;

namespace CAServer {

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
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

			var runner = new Thread(caRunner);
			runner.IsBackground = true;
			runner.Start();
		}

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
