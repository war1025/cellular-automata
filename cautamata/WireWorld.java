package cautamata;

import java.awt.Point;

public class WireWorld implements CASettings {

	public int numStates() {
		// 0 - Empty
		// 1 - Head
		// 2 - Tail
		// 3 - Conductor
		return 4;
	}

	public Point[] getNeighborhood() {
		return new Point[] {
			new Point(0,0), new Point(0,1), new Point(0,-1),
			new Point(1,0), new Point(1,1), new Point(1,-1),
			new Point(-1,0), new Point(-1,1), new Point(-1,-1)};
	}

	public int nextState(int[] neighborhood) {
		int val = neighborhood[0];
		if(val == 0) {
			return 0;
		} else if(val == 1) {
			return 2;
		} else if(val == 2) {
			return 3;
		}
		int sum = 0;
		for(int i = 1; i < neighborhood.length; i++) {
			if(neighborhood[i] == 1) {
				sum ++;
			}
		}
		if(sum == 1 || sum == 2) {
			return 1;
		}
		return 3;
	}

}
