

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CAServer {

	public class Test {

		public static void Main() {

			var binding = new NetTcpBinding ();
			binding.MaxReceivedMessageSize = 10000000;
			binding.ReaderQuotas.MaxArrayLength = 250000;
			var address = new Uri ("net.tcp://localhost:8080");
			var host = new ServiceHost (typeof(Controller));
			host.AddServiceEndpoint (typeof (IController), binding, address);
			host.Open ();
			Console.WriteLine ("Type [CR] to stop...");
			Console.ReadLine ();
			host.Close ();
		}

	}
}
