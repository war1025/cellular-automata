

namespace CAutamata {

	/**
	 * The interface that controls the behavior of a CA.
	 **/
	public interface ICASettings {

		/**
		 * The number of states in the CA
		 * States are defined as being [0,NumStates)
		 **/
		uint NumStates {
			get;
		}

		/**
		 * The neighborhood which determines the state of the CA
		 * This is given with respect to the point in question being based at (0,0)
		 * (0,0) must be included explicitly for the point's current state to factor into its next state.
		 **/
		Point[] Neighborhood {
			get;
		}

		/**
		 * Given the values of all points in the neighborhood of a point this function returns the next state for that
		 * point.
		 *
		 * @param neighborhood An array with the value of each point in the neighborhood in the same respective order as Neighborhood
		 *
		 * @return The next state. This should be in the range [0,numStates)
		 **/
		uint nextState(uint[] neighborhood);
	}

	/**
	 * A Point class.
	 **/
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

		public override string ToString() {
			return "Point (" + x + ", " + y + ")";
		}
	}
}
