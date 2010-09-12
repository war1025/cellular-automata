package cautamata;

import java.awt.Point;

import java.util.Map;
import java.util.HashMap;
import java.util.Scanner;

public class Main {

	public static void main(String[] args) {
		CABoard board = new CABoard(20, 0);
		board.setCASettings(new WireWorld());

		int[][] bd = new int[20][20];
		Map<Point, Integer> glider = new HashMap<Point, Integer>();
		for(int i = 0; i < 20; i++) {
			glider.put(new Point(i,4), 3);
		}
		glider.put(new Point(15,3), 3);
		glider.put(new Point(16,3), 3);
		glider.put(new Point(15,5), 3);
		glider.put(new Point(16,5), 3);
		glider.put(new Point(15,4), 0);
		glider.put(new Point(0,4), 1);
		glider.put(new Point(1,4), 2);

		for(Point p : glider.keySet()) {
			bd[p.x][p.y] = glider.get(p);
		}

		board.userChanged(glider);

		printBoard(bd);
		int numSteps = 0;
		Scanner stdin = new Scanner(System.in);
		while(true) {
			String next = stdin.nextLine();
			if(next.equals("q")) {
				break;
			}
			int steps = 1;


			Map<Point, Integer> changes = null;

			for(int i = 0; i < steps; i++) {
				numSteps++;
				changes = board.step();

				for(Point p : changes.keySet()) {
					bd[p.x][p.y] = changes.get(p);
				}
			}

			System.out.println("Step : " + numSteps);
			System.out.println("NumChanges : " + changes.size());
			printBoard(bd);
		}
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
