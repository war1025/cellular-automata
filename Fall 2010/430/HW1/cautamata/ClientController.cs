
using System.Collections.Generic;
using CAutamata;
using CAServer;

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CAClient {

	public class ControllerClient : ClientBase<IController>, IController {

		public ControllerClient(Binding b, EndpointAddress e) : base(b,e) {

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

		public IDictionary<Point, uint> pullChanges() {
			return Channel.pullChanges();
		}

		public bool pushChanges(IDictionary<Point, uint> changes) {
			return Channel.pushChanges(changes);
		}

		public bool shutdown() {
			return Channel.shutdown();
		}

	}
}
