

using System.Collections.Generic;

namespace CAutamata {

	public class CABoard {

		private uint[][] board;
		private uint numStates;

		private ICollection<Point> changed;

		private ICASettings caSettings;

		private Point[] invNeighborhood;

		public CABoard(uint size, uint defaultState) {

			board = new uint[size][];
			for(int i = 0; i < size; i++) {
				board[i] = new uint[size];
				for(int j = 0; j < size; j++) {
					board[i][j] = defaultState;
				}
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
			IDictionary<Point, uint> changes = new Dictionary<Point, uint>();;
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
				foreach(Point p in points) {
					uint val = nextState(p.x,p.y);
					if(val != board[p.x][p.y]) {
						changes[p] = val;
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
	}
}
