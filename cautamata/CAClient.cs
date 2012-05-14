
using System;
using System.Collections.Generic;

namespace CAClient {

	public class Test {

		public static void Main(string[] args) {

			var client = new ClientUI("http://localhost:8080");

			client.caUpdated += (p,c) => {
					var sb = new System.Text.StringBuilder();
					foreach( var kv in p) {
						sb.AppendLine(kv.Key + ": " + kv.Value);
					}
					sb.Append(p.Count);
					Console.WriteLine(sb);
					client.pullChanges();
				};

			client.caError += (e, m) => {Console.WriteLine(m);};

			client.loadCA(args[0]);
			client.loadState(args[1]);

			client.start();

			Console.ReadLine();

			client.stop();

			client.saveState(args[2]);

			client.shutdown();

		}
	}
}
