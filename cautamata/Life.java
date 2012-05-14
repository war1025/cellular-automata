package cautamata;

import java.awt.Point;

public class Life implements CASettings {

	public int numStates() {
		return 2;
	}

	public Point[] getNeighborhood() {
		return new Point[] {
			new Point(0,0), new Point(0,1), new Point(0,-1),
			new Point(1,0), new Point(1,1), new Point(1,-1),
			new Point(-1,0), new Point(-1,1), new Point(-1,-1)};
	}

	public int nextState(int[] neighborhood) {
		int sum = 0;
		for(int i = 1; i < neighborhood.length; i++) {
			sum += neighborhood[i];
		}
		switch(sum) {
			case 0 : case 1 : return 0;
			case 2 : return neighborhood[0];
			case 3 : return 1;
			default : return 0;
		}
	}

}
