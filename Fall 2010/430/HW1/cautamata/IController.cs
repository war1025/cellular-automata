
using System.Collections.Generic;
using CAutamata;

namespace CAServer {

	public interface IController {

		bool init(string code, uint defaultState);

		bool reinit(uint defaultState);

		bool start();

		bool stop();

		bool step();

		IDictionary<Point, uint> pullChanges();

		bool pushChanges(IDictionary<Point, uint> changes);

		bool shutdown();

	}

}
