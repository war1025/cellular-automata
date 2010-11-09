
using CAutamata;

using System.ServiceModel;

namespace CAServer {

	[ServiceContract]
	public interface IController {

		[OperationContract]
		bool init(string code, uint defaultState, out string errors);

		[OperationContract]
		bool reinit(uint defaultState);

		[OperationContract]
		bool start();

		[OperationContract]
		bool stop();

		[OperationContract]
		bool step();

		[OperationContract]
		int[] pullChanges();

		[OperationContract]
		bool pushChanges(int[] changes);

		[OperationContract]
		bool shutdown();

	}

}
