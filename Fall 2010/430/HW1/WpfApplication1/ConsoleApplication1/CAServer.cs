

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CAServer {

	/**
	 * Server class for publishing the service over wcf
	 **/
	public class Test {

		/**
		 * Publish the service using a nettcp binding over localhost.
		 * Adjust parameters to allow for large data transfers.
		 **/
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
