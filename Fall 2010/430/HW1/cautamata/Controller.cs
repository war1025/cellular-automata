
using System.Collections.Generic;
using CAutamata;
using System.Threading;

namespace CAServer {

	public class Controller : IController {

		private ICASettings caSettings;
		private CABoard board;
		private IDictionary<Point, uint> accumulated;

		private Queue<
		private State state;

		private object accumulatorLock;
		private object queueLock;

		public Controller() {
			this.accumulated = new Dictionary<Point, uint>();
			this.state = UnInited;
		}

		public bool init(string code, uint defaultState) {
			caSettings = CACompiler.compile(code);
			if(caSettings != null) {
				return reinit(defaultState);
			} else {
				return false;
			}
		}

		public bool reinit(uint defaultState) {
			if(caSettings != null) {
				board = new CABoard(500, defaultState);
				board.setCASettings(caSettings);
				state = Stopped;
				return true;
			} else {
				return false;
			}
		}

		public bool start() {
			state = Running;
		}

		public bool stop() {
			state = Stopped;
		}

		public bool step() {

		}

		public IDictionary<Point, uint> pullChanges() {
			lock(accumulatorLock) {
				IDictionary<Point, uint> ret = accumulated;
				accumulated = new Dictionary<Point, uint>();
				return ret;
			}
		}

		public bool pushChanges(IDictionary<Point, uint> changes) {

		}

		public bool shutdown() {

		}

		private enum State {
			UnInited, Running, Stopped
		}

		private void caRunner() {
			while(true) {
				lock(queueLock) {
					if(queue.Count > 0) {

					}
				}
			}
		}

	}


}
