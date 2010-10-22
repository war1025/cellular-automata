


namespace CAClient {

	public delegate void CAStateUpdate(Dictionary<Point, uint> updates, System.Drawing.Color[] colors);

	public class ClientUI {

		private ClientController controller;
		private System.Drawing.Color[] colors;

		private uint[][] board;
		private uint numStates;

		private Dictionary<Point, uint> pendingChanges;

		private event CAStateUpdate caUpdated;

		public ClientUI(string address) {

			this.controller = new ClientController(address);

		}

		public void connectStateUpdate(CAStateUpdate d) {
			caUpdated += d;
		}

		public void disconnectStateUpdate(CAStateUpdate d) {
			caUpdated -= d;
		}

		public bool start() {
			if(pendingChanges.Count > 0) {
				controller.pushChanges(pendingChanges);
				pendingChanges.Clear();
			}
			return controller.start();
		}

		public bool stop() {
			var ret = controller.stop();
			var changes = controller.pullChanges();
			updateUI(changes);
		}

		public bool step() {
			if(pendingChanges.Count > 0) {
				controller.pushChanges(pendingChanges);
				pendingChanges.Clear();
			}
			var ret = controller.step();
			var changes = controller.pullChanges();
			updateUI(changes)
		}

		public bool setState(Point p, uint state) {
			pendingChanges[p] = state;
			Dictionary<Point, uint> dict = new Dictionary<Point, uint>();
			dict[p] = state;
			updateUI(dict);
		}

		public bool toggleState(Point p) {
			uint newState = (board[p.x][p.y] + 1) % numStates;

