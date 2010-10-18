
using System;
using System.Collections.Generic;

namespace CAutamata {

	public class Simulation {

		public static void Main(string[] args) {
			ICASettings settings = CAServer.CACompiler.compile(args[0]);
			uint size = 500;
			CABoard board = new CABoard(size, 0);
			board.setCASettings(settings);

			uint[][] bd = new uint[size][];
			for(int i = 0; i < bd.Length; i++) {
				bd[i] = new uint[size];
			}
			IDictionary<Point, uint> glider = new Dictionary<Point, uint>();
			glider[new Point(1,0)] = 1;
			glider[new Point(0,2)] = 1;
			glider[new Point(1,2)] = 1;
			glider[new Point(3,1)] = 1;
			glider[new Point(4,2)] = 1;
			glider[new Point(5,2)] = 1;
			glider[new Point(6,2)] = 1;

			foreach(Point p in glider.Keys) {
				bd[p.x][p.y] = glider[p];
			}

			board.userChanged(glider);

			//printBoard(bd);
			int numSteps = 0;
			int maxChanges = 0;
			/*while(true) {
				string next = Console.ReadLine();
				if(next == "q") {
					break;
				} else if(next == "g") {
					foreach(Point p in glider.Keys) {
						bd[p.x][p.y] = glider[p];
					}
					board.userChanged(glider);
					continue;
				}*/
			for(int i = 0; i < 10000; i++) {
				numSteps++;
				IDictionary<Point, uint> changes = board.step();
				if(changes.Count > maxChanges) {
					maxChanges = changes.Count;
				}

				foreach (Point p in changes.Keys) {
					bd[p.x][p.y] = changes[p];
				}

				//Console.WriteLine("Step : " + numSteps);
				//Console.WriteLine("NumChanges : " + changes.Count);
				//printBoard(bd);
			}
			Console.WriteLine("Max changes in a round: " + maxChanges);
		}

		private static void printBoard(uint[][] board) {
			System.Text.StringBuilder str = new System.Text.StringBuilder();
			foreach (uint[] i in board) {
				foreach (uint j in i) {
					str.Append(j);
				}
				str.AppendLine();
			}
			Console.Write(str);
		}
	}
}
