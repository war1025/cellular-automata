
using System;
using System.Collections.Generic;

namespace CAClient {

	public class Test {

		public static void Main(string[] args) {

			var client = new ClientUI("http://localhost:8080");

			client.caUpdated += (p,c) => {
					foreach( var kv in p) {
						Console.WriteLine(kv.Key + ": " + kv.Value);
					}
					Console.WriteLine(p.Count);
				};

			client.caError += (e, m) => {Console.WriteLine(m);};

			client.loadCA(args[0]);
			client.loadState(args[1]);

			client.start();

			for(int i = 0; i < 10; i++) {
				Console.ReadLine();
				client.pullChanges();
			}

			client.stop();

			client.saveState(args[2]);

			client.shutdown();

		}
	}
}
