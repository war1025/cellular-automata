
using System.Collections.Generic;
using CAutamata;

using System.ServiceModel;

namespace CAServer {

	[ServiceContract( SessionMode = SessionMode.Required )]
	public interface IController {

		[OperationContract]
		bool init(string code, uint defaultState);

		[OperationContract]
		bool reinit(uint defaultState);

		[OperationContract]
		bool start();

		[OperationContract]
		bool stop();

		[OperationContract]
		bool step();

		[OperationContract]
		Dictionary<Point, uint> pullChanges();

		[OperationContract]
		bool pushChanges(Dictionary<Point, uint> changes);

		[OperationContract]
		bool shutdown();

	}

}
