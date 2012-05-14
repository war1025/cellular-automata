package cautamata;

import java.awt.Point;

import java.util.Set;
import java.util.HashSet;
import java.util.Map;
import java.util.HashMap;


public class CABoard {

	private int[][] board;
	private int numStates;

	private Set<Point> changed;

	private CASettings caSettings;

	private Point[] invNeighborhood;

	public CABoard(int size, int defaultState) {

		board = new int[size][size];

		for(int[] i : board) {
			for(int j = 0; j < i.length; j++) {
				i[j] = defaultState;
			}
		}
	}

	public void setCASettings(CASettings caSettings) {
		this.caSettings = caSettings;
		invNeighborhood = caSettings.getNeighborhood().clone();
		for(int i = 0; i < invNeighborhood.length; i++) {
			Point p = new Point(invNeighborhood[i]);
			p.x *= -1;
			p.y *= -1;
			invNeighborhood[i] = p;
		}
	}

	public void userChanged(Map<Point, Integer> changes) {
		if(changed != null) {
			changed = new HashSet<Point>(changed);
			changed.addAll(changes.keySet());
		}
		for(Point p : changes.keySet()) {
			board[p.x][p.y] = changes.get(p);
		}
	}

	public Map<Point, Integer> step() {
		Map<Point, Integer> changes = new HashMap<Point, Integer>();
		if(changed == null) {
			for(int i = 0; i < board.length; i++) {
				for(int j = 0; j < board[i].length; j++) {
					int val = nextState(i,j);
					if(val != board[i][j]) {
						changes.put(new Point(i,j), val);
					}
				}
			}
		} else {
			Set<Point> points = nextRound(changed);
			for(Point p : points) {
				int val = nextState(p.x,p.y);
				if(val != board[p.x][p.y]) {
					changes.put(p, val);
				}
			}
		}
		changed = changes.keySet();
		for(Point p : changed) {
			board[p.x][p.y] = changes.get(p);
		}
		return changes;
	}

	private Set<Point> nextRound(Set<Point> changed) {
		Set<Point> nextRound = new HashSet<Point>();
		for(Point p : changed) {
			for(Point i : invNeighborhood) {
				nextRound.add(new Point(sanitized(p.x + i.x), sanitized(p.y + i.y)));
			}
		}
		return nextRound;
	}

	private int nextState(int x, int y) {
		Point[] neighborhood = caSettings.getNeighborhood();
		int[] nVals = new int[neighborhood.length];
		for(int i = 0; i < neighborhood.length; i++) {
			Point p = new Point(neighborhood[i]);
			p.x = sanitized(p.x + x);
			p.y = sanitized(p.y + y);
			nVals[i] = board[p.x][p.y];
		}
		return caSettings.nextState(nVals);
	}

	private int sanitized(int val) {
		if(val < 0) {
			val %= board.length;
			if(val < 0) {
				val += board.length;
			}
		} else if(val >= board.length) {
			val %= board.length;
		}
		return val;
	}

}
