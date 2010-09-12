
using CAutamata;

public class Ant : ICASettings {

	public uint NumStates {
		get {
			return 10;
		}
	}

	public Point[] Neighborhood {
		get {
			return new Point[] {
				new Point(0,0), new Point(0,1), new Point(0,-1),
				new Point(1,0), new Point(-1,0)};
		}
	}

	public uint nextState(uint[] neighborhood) {
		uint val = neighborhood[0];
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
