

using System.Collections.Generic;
using System.Threading;

namespace CAutamata {

	public class CABoard {

		private uint[][] board;
		private uint numStates;

		private ICollection<Point> changed;

		private ICASettings caSettings;

		private Point[] invNeighborhood;

		private StepPart[] stepThreads;

		public CABoard(uint size, uint defaultState) {

			board = new uint[size][];
			for(int i = 0; i < size; i++) {
				board[i] = new uint[size];
				for(int j = 0; j < size; j++) {
					board[i][j] = defaultState;
				}
			}

			stepThreads = new StepPart[System.Environment.ProcessorCount];

			for(int i = 0; i < stepThreads.Length; i++) {
				stepThreads[i] = new StepPart(this, board);
				var thread = new Thread(stepThreads[i].stepThread);
				thread.IsBackground = true;
				thread.Start();
			}

		}

		public void closeBoard() {
			foreach( StepPart sp in stepThreads) {
				sp.setRange(null, -1, -1);
			}
		}

		public void setCASettings(ICASettings caSettings) {
			this.caSettings = caSettings;
			Point[] neighb = caSettings.Neighborhood;
			invNeighborhood = new Point[neighb.Length];
			for(int i = 0; i < invNeighborhood.Length; i++) {
				Point p = new Point(neighb[i]);
				p.x *= -1;
				p.y *= -1;
				invNeighborhood[i] = p;
			}
		}

		public void userChanged(IDictionary<Point, uint> changes) {
			if(changed != null) {
				changed = new HashSet<Point>(changed);
				foreach(Point p in changes.Keys) {
					changed.Add(p);
				}
			}
			foreach (Point p in changes.Keys) {
				board[p.x][p.y] = changes[p];
			}
		}

		public IDictionary<Point, uint> step() {
			IDictionary<Point, uint> changes = new Dictionary<Point, uint>();
			if(changed == null) {
				for(int i = 0; i < board.Length; i++) {
					for(int j = 0; j < board[i].Length; j++) {
						uint val = nextState(i,j);
						if(val != board[i][j]) {
							changes[new Point(i,j)] = val;
						}
					}
				}
			} else {
				HashSet<Point> points = nextRound(changed);
				if(points.Count < 50) {
					foreach(Point p in points) {
						uint val = nextState(p.x,p.y);
						if(val != board[p.x][p.y]) {
							changes[p] = val;
						}
					}
				} else {
					Point[] pts = new Point[points.Count];
					points.CopyTo(pts);
					int step = pts.Length / stepThreads.Length;
					int start = 0;
					for(int i = 0; i < stepThreads.Length; i++) {
						stepThreads[i].setRange(pts, start, ((i + 1) < stepThreads.Length) ? start + step : pts.Length);
						start += step;
					}
					foreach(StepPart p in stepThreads) {
						foreach(KeyValuePair<Point, uint> kv in p.results()) {
							changes[kv.Key] = kv.Value;
						}
					}
				}
			}
			changed = changes.Keys;
			foreach (Point p in changed) {
				board[p.x][p.y] = changes[p];
			}
			return changes;
		}

		private HashSet<Point> nextRound(ICollection<Point> changed) {
			HashSet<Point> nextRound = new HashSet<Point>();
			foreach(Point p in changed) {
				foreach(Point i in invNeighborhood) {
					nextRound.Add(new Point(sanitized(p.x + i.x), sanitized(p.y + i.y)));
				}
			}
			return nextRound;
		}

		private uint nextState(int x, int y) {
			Point[] neighborhood = caSettings.Neighborhood;
			uint[] nVals = new uint[neighborhood.Length];
			for(int i = 0; i < neighborhood.Length; i++) {
				Point p = new Point(neighborhood[i]);
				p.x = sanitized(p.x + x);
				p.y = sanitized(p.y + y);
				nVals[i] = board[p.x][p.y];
			}
			return caSettings.nextState(nVals);
		}

		private int sanitized(int val) {
			if(val < 0) {
				val %= board.GetLength(0);
				if(val < 0) {
					val += board.GetLength(0);
				}
			} else if(val >= board.GetLength(0)) {
				val %= board.GetLength(0);
			}
			return val;
		}

		private class StepPart {
			private object objLock;
			private object rangeLock;
			private bool valid;
			private bool rangeSet;
			private int start;
			private int end;

			private uint[][] board;
			private CABoard parent;
			private Point[] pointArr;
			private Dictionary<Point, uint> parts;

			public StepPart(CABoard parent, uint[][] board) {
				this.objLock = new object();
				this.rangeLock = new object();
				this.valid = false;
				this.rangeSet = false;
				this.start = 0;
				this.end = 0;

				this.board = board;
				this.parent = parent;
				this.parts = new Dictionary<Point, uint>();
			}

			public void waitAndInvalidate() {
				lock(objLock) {
					while(!valid) {
						Monitor.Wait(objLock);
					}
					valid = false;
				}
			}

			public void validate() {
				lock(objLock) {
					valid = true;
					Monitor.PulseAll(objLock);
				}
			}

			public void setRange(Point[] pointArr, int start, int end) {
				lock(rangeLock) {
					this.pointArr = pointArr;
					this.start = start;
					this.end = end;
					rangeSet = true;
					Monitor.PulseAll(rangeLock);
				}
			}

			public Dictionary<Point, uint> results() {
				waitAndInvalidate();
				return parts;
			}

			public void stepThread() {
				while(true) {
					lock(rangeLock) {
						while(!rangeSet) {
							Monitor.Wait(rangeLock);
						}
						rangeSet = false;
					}
					if(start < 0) {
						break;
					}
					parts.Clear();
					for(int i = start; i < end; i++) {
						Point p = pointArr[i];
						uint val = parent.nextState(p.x,p.y);
						if(val != board[p.x][p.y]) {
							parts[p] = val;
						}
					}
					validate();
				}
			}
		}
	}

}
