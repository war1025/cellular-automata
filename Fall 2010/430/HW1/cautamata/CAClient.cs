
using System;
using System.Collections.Generic;
using CAutamata;

namespace CAClient {

	public class Test {

		public static void Main(string[] args) {

			var controller = new ClientController("http://localhost:8080");

			bool ret = false;

			ret = controller.init(args[0], 0);

			Console.WriteLine("Initing the controller: " + ret);

			var glider = new Dictionary<Point, uint>();
			glider[new Point(1,0)] = 1;
			glider[new Point(0,2)] = 1;
			glider[new Point(1,2)] = 1;
			glider[new Point(3,1)] = 1;
			glider[new Point(4,2)] = 1;
			glider[new Point(5,2)] = 1;
			glider[new Point(6,2)] = 1;

			ret = controller.pushChanges(glider);

			Console.WriteLine("Pushing the Glider: " + ret);

			ret = controller.start();

			Console.WriteLine("Stepping the simulation: " + ret);

			Console.ReadLine();

			ret = controller.stop();

			Console.WriteLine("Stopping Simulation: " + ret);

			IDictionary<Point, uint> dict = controller.pullChanges();

			Console.WriteLine("Changes:");
			foreach(KeyValuePair<Point, uint> kv in dict) {
				Console.WriteLine(kv.Key + ": " + kv.Value);
			}
			Console.WriteLine(dict.Count);

		}
	}
}
