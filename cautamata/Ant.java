package cautamata;

import java.awt.Point;

public class Ant implements CASettings {

	public int numStates() {
		return 10;
	}

	public Point[] getNeighborhood() {
		return new Point[] {
			new Point(0,0), new Point(0,1), new Point(0,-1),
			new Point(1,0), new Point(-1,0)};
	}

	public int nextState(int[] neighborhood) {
		int val = neighborhood[0];
		if(1 <= val && val <= 4) {
			return 5;
		} else if(6 <= val && val <= 9) {
			return 0;
		}
		val = neighborhood[1];
		if(val == 2 || val == 8) {
			return neighborhood[0] + 1;
		}
		val = neighborhood[2];
		if(val == 3 || val == 7) {
			return neighborhood[0] + 4;
		}
		val = neighborhood[3];
		if(val == 1 || val == 9) {
			return neighborhood[0] + 3;
		}
		val = neighborhood[4];
		if(val == 4 || val == 6) {
			return neighborhood[0] + 2;
		}
		return neighborhood[0];
	}

}
