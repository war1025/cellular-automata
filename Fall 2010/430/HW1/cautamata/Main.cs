
using System;
using System.Collections.Generic;

namespace CAutamata {

	public class Simulation {

		public static void Main(string[] args) {
			CABoard board = new CABoard(20, 2, 0);
			board.setCASettings(new Life());

			uint[,] bd = new uint[20,20];
			IDictionary<Point, uint> glider = new Dictionary<Point, uint>();
			glider[new Point(1,0)] = 1;
			glider[new Point(2,1)] = 1;
			glider[new Point(2,2)] = 1;
			glider[new Point(1,2)] = 1;
			glider[new Point(0,2)] = 1;

			foreach(Point p in glider.Keys) {
				bd[p.x, p.y] = 1;
			}

			board.userChanged(glider);

			printBoard(bd);
			int numSteps = 0;
			while(true) {
				string next = Console.ReadLine();
				if(next == "q") {
					break;
				} else if(next == "g") {
					foreach (Point p in glider.Keys) {
						bd[p.x, p.y] = 1;
					}
					board.userChanged(glider);
					printBoard(bd);
					continue;
				}
				numSteps++;
				IDictionary<Point, uint> changes = board.step();

				foreach (Point p in changes.Keys) {
					bd[p.x, p.y] = changes[p];
				}

				Console.WriteLine("Step : " + numSteps);
				Console.WriteLine("NumChanges : " + changes.Count);
				printBoard(bd);
			}
		}

		private static void printBoard(uint[,] board) {
			System.Text.StringBuilder str = new System.Text.StringBuilder();
			int i = 0;
			int j = 0;
			int length = board.GetLength(0);
			foreach( uint v in board) {
				str.Append(v);
				j++;
				if(j == length) {
					str.AppendLine();
					i++;
					j = 0;
				}
			}
			Console.Write(str);
		}
	}
}
