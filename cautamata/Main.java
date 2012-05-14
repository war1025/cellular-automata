package cautamata;

import java.awt.Point;

import java.util.Map;
import java.util.HashMap;
import java.util.Scanner;

public class Main {

	public static void main(String[] args) {
		int size = 500;
		CABoard board = new CABoard(size, 0);
		board.setCASettings(new Life());

		int[][] bd = new int[size][size];
		Map<Point, Integer> glider = new HashMap<Point, Integer>();

		glider.put(new Point(1,0), 1);
		glider.put(new Point(0,2), 1);
		glider.put(new Point(1,2), 1);
		glider.put(new Point(3,1), 1);
		glider.put(new Point(4,2), 1);
		glider.put(new Point(5,2), 1);
		glider.put(new Point(6,2),1);

		for(Point p : glider.keySet()) {
			bd[p.x][p.y] = glider.get(p);
		}

		board.userChanged(glider);

		//printBoard(bd);
		int numSteps = 0;
		/*Scanner stdin = new Scanner(System.in);
		while(true) {
			String next = stdin.nextLine();
			if(next.equals("q")) {
				break;
			}
			int steps = 1;*/

		for(int i = 0; i < 10000; i++) {
			Map<Point, Integer> changes = null;

			numSteps++;
			changes = board.step();

			for(Point p : changes.keySet()) {
				bd[p.x][p.y] = changes.get(p);
			}

			//System.out.println("Step : " + numSteps);
			//System.out.println("NumChanges : " + changes.size());
			//printBoard(bd);
		}
		System.out.println("Step : " + numSteps);

	}

	private static void printBoard(int[][] board) {
		for(int[] i : board) {
			for(int j : i) {
				System.out.print(j);
			}
			System.out.println();
		}
	}
}
