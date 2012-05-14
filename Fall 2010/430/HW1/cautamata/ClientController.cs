
using System.Collections.Generic;
using CAutamata;
using CAServer;

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CAClient {

	public class ClientController : ClientBase<IController>, IController {

		public ClientController(string address) : base(new NetTcpBinding(), new EndpointAddress(address)) {

		}

		public bool init(string code, uint defaultState) {
			return Channel.init(code, defaultState);
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

		public Dictionary<Point, uint> pullChanges() {
			return Channel.pullChanges();
		}

		public bool pushChanges(Dictionary<Point, uint> changes) {
			return Channel.pushChanges(changes);
		}

		public bool shutdown() {
			return Channel.shutdown();
		}

	}
}
