package cautamata;


public class Quadtree {

	private Quadrant root;

	public Quadtree(int size, int defaultVal) {
		root = new LeafQuadrant(0, size, 0, size, defaultVal);
	}

	protected interface Quadrant {

		public boolean isLeaf();

		public boolean inRange(Point p);

		public void setValue(Point p, int value);

		public int getValue(Point p);

	}

	protected class LeafQuadrant implements Quadrant {

		private Point xRange;
		private Point yRange;
		private int value;

		public LeafQuadrant(int xStart, int xEnd, int yStart, int yEnd, int val) {
			this.xRange = new Point(xStart, xEnd);
			this.yRange = new Point(yStart, yEnd);
			this.value = val;
		}

		public boolean isLeaf() {
			return true;
		}

		public boolean inRange(Point p) {
			return (xRange.x <= p.x) && (p.x < xRange.y)
					&& (yRange.x <= p.y) && (p.y < yRange.y);
		}

		public void setValue(Point p, int value) {
			if(!inRange(p)) {
				return;
			}


}
