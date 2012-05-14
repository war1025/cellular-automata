package cautamata;

import java.awt.Point;

public class Quadtree {

	private Quadrant root;

	public Quadtree(int size, int defaultVal) {
		root = new LeafQuadrant(new Point(0,size), new Point(0,size), defaultVal);
	}

	public int getValue(Point p) {
		return root.getValue(p);
	}

	public void setValue(Point p, int value) {
		root = root.setValue(p, value);
	}

	public String toString() {
		return root.toString();
	}

	protected interface Quadrant {

		public boolean isLeaf();

		public boolean inRange(Point p);

		public boolean isTrivial();

		public Quadrant setValue(Point p, int value);

		public int getValue(Point p);

	}

	protected class LeafQuadrant implements Quadrant {

		private Point xRange;
		private Point yRange;
		private int value;

		public LeafQuadrant(Point xRange, Point yRange, int val) {
			this.xRange = new Point(xRange);
			this.yRange = new Point(yRange);
			this.value = val;
		}

		public boolean isLeaf() {
			return true;
		}

		public boolean isTrivial() {
			return (xRange.x == xRange.y) || (yRange.x == yRange.y);
		}

		private boolean isUnit() {
			return ((xRange.y - xRange.x) == 1) && ((yRange.y - yRange.x) == 1);
		}

		public boolean inRange(Point p) {
			return (xRange.x <= p.x) && (p.x < xRange.y)
					&& (yRange.x <= p.y) && (p.y < yRange.y);
		}

		public Quadrant setValue(Point p, int value) {
			if(!inRange(p)) {
				throw new IllegalArgumentException("Point: (" + p.x + ", " + p.y + ") is not in range");
			}
			Quadrant toRet = this;
			if(isUnit()) {
				this.value = value;
			} else {
				toRet = new BranchQuadrant(xRange, yRange, this.value);
				toRet = toRet.setValue(p, value);
			}
			return toRet;
		}

		public int getValue(Point p) {
			return value;
		}

		public String toString() {
			return String.format("Leaf Quadrant: (%d, %d) ; (%d, %d) : %d",xRange.x, xRange.y, yRange.x, yRange.y, value);
		}
	}

	protected class BranchQuadrant implements Quadrant {

		private Quadrant[] quads;

		private Point xRange;
		private Point yRange;

		private int xAvg;
		private int yAvg;

		public BranchQuadrant(Point xRange, Point yRange, int value) {
			this.xRange = new Point(xRange);
			this.yRange = new Point(yRange);

			this.xAvg = (xRange.x + xRange.y) / 2;
			this.yAvg = (yRange.x + yRange.y) / 2;

			this.quads = new Quadrant[4];

			quads[0] = new LeafQuadrant(new Point(xRange.x, xAvg), new Point(yAvg, yRange.y), value);
			quads[1] = new LeafQuadrant(new Point(xAvg, xRange.y), new Point(yAvg, yRange.y), value);
			quads[2] = new LeafQuadrant(new Point(xRange.x, xAvg), new Point(yRange.x, yAvg), value);
			quads[3] = new LeafQuadrant(new Point(xAvg, xRange.y), new Point(yRange.x, yAvg), value);
		}

		public boolean isLeaf() {
			return false;
		}

		public boolean isTrivial() {
			return false;
		}

		public boolean inRange(Point p) {
			return (xRange.x <= p.x) && (p.x < xRange.y)
					&& (yRange.x <= p.y) && (p.y < yRange.y);
		}

		public Quadrant setValue(Point p, int value) {
			if(!inRange(p)) {
				throw new IllegalArgumentException("Point: (" + p.x + ", " + p.y + ") is not in range");
			}
			Quadrant toRet = this;
			for(int i = 0; i < quads.length; i++) {
				if(quads[i].inRange(p)) {
					quads[i] = quads[i].setValue(p, value);
					break;
				}
			}
			boolean collapse = true;
			for(int i = 0; i < quads.length; i++) {
				collapse &= quads[i].isLeaf();
			}
			if(collapse) {
				for(int i = 0; i < quads.length; i++) {
					if(!quads[i].isTrivial()) {
						for(int j = i+1; j < quads.length; j++) {
							if(!quads[j].isTrivial() && (value != ((LeafQuadrant) quads[j]).value)) {
								collapse = false;
								break;
							}
						}
						if(!collapse) {
							break;
						}
					}
				}
			}
			if(collapse) {
				toRet = new LeafQuadrant(xRange, yRange, value);
			}
			return toRet;
		}

		public int getValue(Point p) {
			int val = 0;
			for(Quadrant q : quads) {
				if(q.inRange(p)) {
					val = q.getValue(p);
					break;
				}
			}
			return val;
		}

		public String toString() {
			StringBuilder out = new StringBuilder();
			out.append(String.format("Branch: (%d, %d) ; (%d, %d)\n", xRange.x, xRange.y, yRange.x, yRange.y));
			for(Quadrant q : quads) {
				out.append(q.toString().replaceAll("\n","\n\t"));
				out.append("\n");
			}
			return out.toString();
		}
	}


}
