
using System.Collections.Generic;
using CAutamata;
using CAServer;

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CAClient {

	/**
	 * The Client side implementation of IController.
	 *
	 * See IController for detailed method descriptions.
	 **/
	public class ClientController : ClientBase<IController>, IController {

		/**
		 * Creates a ClientController connecting to the given address
		 *
		 * @param address The address the Server is publishing the service to.
		 **/
		public ClientController(string address) : base(getBinding(), new EndpointAddress(address)) {

		}

		/**
		 * We use a NetTcpBinding with several of the parameters increased to allow for large data transfer
		 **/
		private static NetTcpBinding getBinding() {
			var b = new NetTcpBinding();
			b.MaxReceivedMessageSize = 10000000;
			b.ReaderQuotas.MaxArrayLength = 250000;
			return b;
		}

		public bool init(string code, uint defaultState, out string errors) {
			return Channel.init(code, defaultState, out errors);
		}

		public bool reinit(uint defaultState) {
			return Channel.reinit(defaultState);
		}

		public bool start() {
			return Channel.start();
		}

		public bool stop() {
			return Channel.stop();
		}

		public bool step() {
			return Channel.step();
		}

		public int[] pullChanges() {
			return Channel.pullChanges();
		}

		public bool pushChanges(int[] changes) {
			return Channel.pushChanges(changes);
		}

		public bool shutdown() {
			return Channel.shutdown();
		}

	}
}
