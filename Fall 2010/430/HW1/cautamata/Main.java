package cautamata;

import java.awt.Point;

import java.util.Map;
import java.util.HashMap;
import java.util.Scanner;

public class Main {

	public static void main(String[] args) {
		CABoard board = new CABoard(40, 10, 0);
		board.setCASettings(new Ant());

		int[][] bd = new int[40][40];
		Map<Point, Integer> glider = new HashMap<Point, Integer>();
		glider.put(new Point(1,0),1);

		for(Point p : glider.keySet()) {
			bd[p.x][p.y] = 1;
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
			try {
				steps = Integer.parseInt(next);
			} catch(Exception e) {
				e.printStackTrace();
			}

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
