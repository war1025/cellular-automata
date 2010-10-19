

namespace CAutamata {

	public interface ICASettings {

		uint NumStates {
			get;
		}

		Point[] Neighborhood {
			get;
		}

		uint nextState(uint[] neighborhood);
	}

	[System.Serializable]
	public class Point {
		public int x;
		public int y;

		public Point(int x, int y) {
			this.x = x;
			this.y = y;
		}

		public Point(Point p) {
			this.x = p.x;
			this.y = p.y;
		}

		public override int GetHashCode() {
			return (x * 31) + y;
		}

		public override bool Equals(object o) {
			Point p = o as Point;
			if(p != null) {
				return (p.x == this.x) && (p.y == this.y);
			}
			return false;
		}
	}
}
