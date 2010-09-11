package cautamata;

import java.awt.Point;

import java.util.Map;
import java.util.HashMap;
import java.util.Scanner;

public class Main {

	public static void main(String[] args) {
		CABoard board = new CABoard(20, 2, 0);
		board.setCASettings(new Life());

		int[][] bd = new int[20][20];
		Map<Point, Integer> glider = new HashMap<Point, Integer>();
		glider.put(new Point(1,0),1);
		glider.put(new Point(2,1),1);
		glider.put(new Point(2,2),1);
		glider.put(new Point(1,2),1);
		glider.put(new Point(0,2),1);

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
