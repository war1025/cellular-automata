
using CAutamata;

public class WireWorld : ICASettings {

	public uint NumStates {
		get {
			return 4;
		}
	}

	public Point[] Neighborhood {
		get {
			return new Point[] {
				new Point(0,0), new Point(0,1), new Point(0,-1),
				new Point(1,0), new Point(1,1), new Point(1,-1),
				new Point(-1,0), new Point(-1,1), new Point(-1,-1)};
		}
	}

	public uint nextState(uint[] neighborhood) {
		uint val = neighborhood[0];
		if(val == 0) {
			return 0;
		} else if(val == 1) {
			return 2;
		} else if(val == 2) {
			return 3;
		}
		uint sum = 0;
		for(int i = 1; i < neighborhood.Length; i++) {
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
