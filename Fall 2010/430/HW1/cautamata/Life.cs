
using CAutamata;

public class Life : ICASettings {

	public uint NumStates {
		get {
			return 2;
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
		uint sum = 0;
		for(int i = 1; i < neighborhood.Length; i++) {
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
