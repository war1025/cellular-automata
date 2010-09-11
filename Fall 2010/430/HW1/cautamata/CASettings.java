package cautamata;

import java.awt.Point;

public interface CASettings {

	public int numStates();

	public Point[] getNeighborhood();

	public int nextState(int[] neighborhood);

}
