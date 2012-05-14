
using CAutamata;

using System.ServiceModel;

namespace CAServer {

	/**
	 * This is the interface that is published over WCF.
	 * It has all of the functionality needed to simulate and display a CA
	 **/
	[ServiceContract]
	public interface IController {

		/**
		 * Initializes a CA. This includes compiling the CA code and setting the board to a default state.
		 *
		 * @param code The CA code to compile
		 * @param defaultState The state to initially put the board in
		 * @param errors Any errors that happen when compiling the code
		 *
		 * @return Whether the CA was successfully inited
		 **/
		[OperationContract]
		bool init(string code, uint defaultState, out string errors);

		/**
		 * Keeps the same CA, but clears the board to defaultState
		 *
		 * @param defaultState The state to clear the board to.
		 *
		 * @return Whether or not the board was successfully cleared
		 **/
		[OperationContract]
		bool reinit(uint defaultState);

		/**
		 * Start the CA stepping as quickly as possible
		 *
		 * @return Whether the operation succeeded
		 **/
		[OperationContract]
		bool start();

		/**
		 * Stop a CA in the running state.
		 *
		 * @return Whether the CA is now stopped
		 **/
		[OperationContract]
		bool stop();

		/**
		 * Step a stopped CA through one step of the simulation
		 *
		 * @return Whether the CA successfully stepped
		 **/
		[OperationContract]
		bool step();

		/**
		 * All changes needed to update the Client's view of the CA to the most recent state.
		 * Note that points are encoded to allow for a more efficient transfer.
		 * If point (x,y) is in state z, it will be encoded such that x is the 9 leftmost bits.
		 * y is the next 9 bits, and z is the remaining 14 bits.
		 * This means there is an implicit upper bound of 2^14 states for the CA, which should be more than enough in practice.
		 * By encoding in this way, if 10 points have changed since the last pull of changes, only an array of 10 ints needs to
		 * be sent. These ints each fully qualify a point and its state within the board.
		 *
		 * @return Any differences between the Client's last known state and the current board state
		 **/
		[OperationContract]
		int[] pullChanges();

		/**
		 * Changes that the Client has made and wishes to inform the Server about.
		 * Points are encoded in the same way as in the pullChanges() method.
		 * Changes may only be pushed when the simulation is stopped.
		 *
		 * @param changes The Client's changes
		 *
		 * @return Whether the changes were pushed successfully.
		 **/
		[OperationContract]
		bool pushChanges(int[] changes);

		/**
		 * Shuts down the current simulation. A simulation must be shut down before it can be inited or reinited.
		 *
		 * @return Whether the CA is shut down now.
		 **/
		[OperationContract]
		bool shutdown();

	}

}
