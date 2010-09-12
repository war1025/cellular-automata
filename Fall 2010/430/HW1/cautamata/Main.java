package cautamata;

import java.awt.Point;

import java.util.Map;
import java.util.HashMap;
import java.util.Scanner;

public class Main {

	public static void main(String[] args) {
		CABoard board = new CABoard(45, 2, 0);
		board.setCASettings(new Life());

		int[][] bd = new int[45][45];
		Map<Point, Integer> glider = new HashMap<Point, Integer>();
		glider.put(new Point(1,0),1);
		glider.put(new Point(0,2),1);
		glider.put(new Point(1,2),1);
		glider.put(new Point(3,1),1);
		glider.put(new Point(4,2),1);
		glider.put(new Point(5,2),1);
		glider.put(new Point(6,2),1);

		for(Point p : glider.keySet()) {
			bd[p.x][p.y] = 1;
		}

		board.userChanged(glider);

		//printBoard(bd);
		int numSteps = 0;
		for(int i = 0; i < 172; i++) {
			Map<Point, Integer> changes = board.step();

			for(Point p : changes.keySet()) {
				bd[p.x][p.y] = changes.get(p);
			}
		}
		System.out.println("Step: " + 172);
		printBoard(bd);
		/*Scanner stdin = new Scanner(System.in);
		while(true) {
			String next = stdin.nextLine();
			if(next.equals("q")) {
				break;
			} else if(next.equals("g")) {
				for(Point p : glider.keySet()) {
					bd[p.x][p.y] = 1;
				}
				board.userChanged(glider);
				printBoard(bd);
				continue;
			}
			numSteps++;
			Map<Point, Integer> changes = board.step();

			for(Point p : changes.keySet()) {
				bd[p.x][p.y] = changes.get(p);
			}

			System.out.println("Step : " + numSteps);
			System.out.println("NumChanges : " + changes.size());
			printBoard(bd);
		}*/
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
