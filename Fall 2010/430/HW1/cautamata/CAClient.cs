
using System;
using System.Collections.Generic;
using CAutamata;

namespace CAClient {

	public class Test {

		public static void Main(string[] args) {

			var controller = new ClientController("http://localhost:8080");

			bool ret = false;

			CAParser parser = new CAParser();

			CAComponents comps = parser.parseCASettings(args[0]);
			CAState state = parser.parseCAState(args[1]);

			Console.WriteLine(comps.code);

			ret = controller.init(comps.code, state.defaultState);

			var board = new uint[500][];
			for(int i = 0; i < board.Length; i++) {
				board[i] = new uint[500];
			}

			Console.WriteLine("Initing the controller: " + ret);

			ret = controller.pushChanges(state.states);

			Console.WriteLine("Pushing the Glider: " + ret);

			ret = controller.start();

			Console.WriteLine("Starting the simulation: " + ret);

			for(int i = 0; i < 10; i++) {
				Console.ReadLine();
				IDictionary<Point, uint> dict = controller.pullChanges();

				Console.WriteLine("Changes:");
				foreach(KeyValuePair<Point, uint> kv in dict) {
					Point p = kv.Key;
					board[p.x][p.y] = kv.Value;
					Console.WriteLine(kv.Key + ": " + kv.Value);
				}
				Console.WriteLine(dict.Count);
			}

			ret = controller.stop();

			Console.WriteLine("Stopping Simulation: " + ret);

			IDictionary<Point, uint> dict2 = controller.pullChanges();

			Console.WriteLine("Changes:");
			foreach(KeyValuePair<Point, uint> kv in dict2) {
				Console.WriteLine(kv.Key + ": " + kv.Value);
			}
			Console.WriteLine(dict2.Count);
			controller.shutdown();

			parser.saveCAState(args[2], board);

		}
	}
}
