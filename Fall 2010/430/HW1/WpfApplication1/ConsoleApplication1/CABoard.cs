

using System.Collections.Generic;
using System.Threading;

namespace CAutamata {

	/**
	 * A board for a CA to be simulated on.
	 * An arbitrary instance of ICASettings can be used.
	 * The board can be of arbitrary size.
	 * Multiple processors will be taken advantage of to step the CA.
	 **/
	public class CABoard {

		/**
		 * The board that the state is stored in
		 **/
		private uint[][] board;

		/**
		 * A collection of points that changed last step.
		 * This is important because only points whose neighborhood changed last step can
		 * change state the next step.
		 **/
		private ICollection<Point> changed;

		/**
		 * The CASettings we are using to run this simulation
		 **/
		private ICASettings caSettings;

		/**
		 * The inverse neighborhood. As stated above, only points whose neighborhood changed in the last step
		 * can change this step. The inverse neighborhood lets us quickly calculate which points had their neighborhood affected.
		 **/
		private Point[] invNeighborhood;

		/**
		 * Threads for stepping the CA. There is one thread  for each processor available.
		 **/
		private StepPart[] stepThreads;

		/**
		 * Creates a CABoard of the given size in the given state
		 *
		 * @param size The size of board to use.
		 * @param defaultState The state to initialize the board to
		 **/
		public CABoard(uint size, uint defaultState) {

			// Create the board, clear to default state
			board = new uint[size][];
			for(int i = 0; i < size; i++) {
				board[i] = new uint[size];
				for(int j = 0; j < size; j++) {
					board[i][j] = defaultState;
				}
			}

			// Create step threads corresponding to the number of processors available
			stepThreads = new StepPart[System.Environment.ProcessorCount];

			for(int i = 0; i < stepThreads.Length; i++) {
				stepThreads[i] = new StepPart(this, board);
				var thread = new Thread(stepThreads[i].stepThread);
				thread.IsBackground = true;
				thread.Start();
			}

		}

		/**
		 * Kills the step threads
		 **/
		public void closeBoard() {
			foreach( StepPart sp in stepThreads) {
				sp.setRange(null, -1, -1);
			}
		}

		/**
		 * Set the CASettings to use.
		 * Calculate the inverse neighborhood
		 *
		 * @param caSettings The CASettings used to drive the simulation
		 **/
		public void setCASettings(ICASettings caSettings) {
			this.caSettings = caSettings;
			Point[] neighb = caSettings.Neighborhood;
			invNeighborhood = new Point[neighb.Length];
			for(int i = 0; i < invNeighborhood.Length; i++) {
				Point p = new Point(neighb[i]);
				p.x *= -1;
				p.y *= -1;
				invNeighborhood[i] = p;
			}
		}

		/**
		 * Indiate the points that the user has changed since the last step
		 *
		 * Add these points to the changed collection, update the board state appropriately
		 *
		 * @param changes Changes the user has made to the board state
		 **/
		public void userChanged(IDictionary<Point, uint> changes) {
			if(changed != null) {
				changed = new HashSet<Point>(changed);
				foreach(Point p in changes.Keys) {
					changed.Add(p);
				}
			}
			foreach (Point p in changes.Keys) {
				board[p.x][p.y] = changes[p];
			}
		}

		/**
		 * Step the simulation one step. Return any state changes.
		 *
		 * This involves calculating which points to check and running nextState on each point.
		 * If the state differs from the current state, mark it down.
		 **/
		public IDictionary<Point, uint> step() {
			IDictionary<Point, uint> changes = new Dictionary<Point, uint>();
			// Initially the entire board needs to be checked.
			if(changed == null) {
				for(int i = 0; i < board.Length; i++) {
					for(int j = 0; j < board[i].Length; j++) {
						uint val = nextState(i,j);
						if(val != board[i][j]) {
							changes[new Point(i,j)] = val;
						}
					}
				}
			// Otherwise check only the points that could have changed
			} else {
				// Calculate the points that need to be checked
				HashSet<Point> points = nextRound(changed);
				// If there are less than  50, just run serially.
				if(points.Count < 50) {
					foreach(Point p in points) {
						uint val = nextState(p.x,p.y);
						if(val != board[p.x][p.y]) {
							changes[p] = val;
						}
					}
				// Otherwise use the step threads
				} else {
					// First copy the points into  an array. This allows for easy partitioning of the work
					Point[] pts = new Point[points.Count];
					points.CopyTo(pts);
					// Calculate about how many points each thread should check
					int step = pts.Length / stepThreads.Length;
					int start = 0;
					// Set the range of each thread.
					// Once the range is set, that thread will start processing its points asynchronously
					// The final step thread needs to check to the end of the array instead of just <step> places
					for(int i = 0; i < stepThreads.Length; i++) {
						stepThreads[i].setRange(pts, start, ((i + 1) < stepThreads.Length) ? start + step : pts.Length);
						start += step;
					}
					// Join the values each thread comes up with back into one changes map.
					// The results method blocks until the calculations for that thread are available.
					foreach(StepPart p in stepThreads) {
						foreach(KeyValuePair<Point, uint> kv in p.results()) {
							changes[kv.Key] = kv.Value;
						}
					}
				}
			}
			// Update the set of points that changed and update the  board state.
			changed = changes.Keys;
			foreach (Point p in changed) {
				board[p.x][p.y] = changes[p];
			}
			return changes;
		}

		/**
		 * Given a set of points that changed last round, calculate which points should be checked this round.
		 *
		 * This is done by running each point in changed through the inverse neighborhood. This returns all points for which
		 * a point in changed was in the neighborhood.
		 *
		 * @param changed Points that changed last round
		 *
		 * @return Points that might change this round
		 **/
		private HashSet<Point> nextRound(ICollection<Point> changed) {
			HashSet<Point> nextRound = new HashSet<Point>();
			foreach(Point p in changed) {
				foreach(Point i in invNeighborhood) {
					nextRound.Add(new Point(sanitized(p.x + i.x), sanitized(p.y + i.y)));
				}
			}
			return nextRound;
		}

		/**
		 * Calculate the next state of the point at (x,y)
		 *
		 * This involves constructing the neighborhood taking into account wraparound
		 * Then calling the nextState function
		 *
		 * @param x The x coordinate of the point
		 * @param y The y coordinate of the point
		 *
		 * @return The next state of that point
		 **/
		private uint nextState(int x, int y) {
			Point[] neighborhood = caSettings.Neighborhood;
			uint[] nVals = new uint[neighborhood.Length];
			for(int i = 0; i < neighborhood.Length; i++) {
				Point p = new Point(neighborhood[i]);
				p.x = sanitized(p.x + x);
				p.y = sanitized(p.y + y);
				nVals[i] = board[p.x][p.y];
			}
			return caSettings.nextState(nVals);
		}

		/**
		 * Wraps values outside the bounds of the board into values that are inside the board.
		 *
		 * @param val The coordinate to sanitize
		 *
		 * @return  The coordinate when wrapped around the board until it lands within the board
		 **/
		private int sanitized(int val) {
			if(val < 0) {
				val %= board.GetLength(0);
				if(val < 0) {
					val += board.GetLength(0);
				}
			} else if(val >= board.GetLength(0)) {
				val %= board.GetLength(0);
			}
			return val;
		}

		/**
		 * A thread object responsible for stepping part of the simulation
		 **/
		private class StepPart {
			/**
			 * The lock guarding the results of the step calculations
			 **/
			private object objLock;
			/**
			 * The lock for blocking until range is set
			 **/
			private object rangeLock;

			/**
			 * Are the calculations valid
			 **/
			private bool valid;

			/**
			 * Has the range been set
			 **/
			private bool rangeSet;

			/**
			 * The index to start calculating at within the array inclusive
			 **/
			private int start;

			/**
			 * The index to stop calculating at within the array exclusive
			 **/
			private int end;

			/**
			 * The state board
			 **/
			private uint[][] board;

			/**
			 * The parent class
			 **/
			private CABoard parent;

			/**
			 * The array of points to work on
			 **/
			private Point[] pointArr;

			/**
			 * The changes within the section this stepthread is processing
			 **/
			private Dictionary<Point, uint> parts;

			/**
			 * Creates a StepPart for  the given board
			 *
			 * @param parent The parent of this StepPart
			 * @param board The state board
			 **/
			public StepPart(CABoard parent, uint[][] board) {
				this.objLock = new object();
				this.rangeLock = new object();
				this.valid = false;
				this.rangeSet = false;
				this.start = 0;
				this.end = 0;

				this.board = board;
				this.parent = parent;
				this.parts = new Dictionary<Point, uint>();
			}

			/**
			 * Wait for results to become valid, then immediately invalidate the results so no one else can access them
			 **/
			public void waitAndInvalidate() {
				lock(objLock) {
					while(!valid) {
						Monitor.Wait(objLock);
					}
					valid = false;
				}
			}

			/**
			 * Validate the results
			 **/
			public void validate() {
				lock(objLock) {
					valid = true;
					Monitor.PulseAll(objLock);
				}
			}

			/**
			 * Set the range of points this thread should process
			 *
			 * This causes processing to begin.
			 *
			 * @param pointArr The array of points that need checking
			 * @param start The index to start processing at inclusive
			 * @param end The index to stop processing at exclusive
			 **/
			public void setRange(Point[] pointArr, int start, int end) {
				lock(rangeLock) {
					this.pointArr = pointArr;
					this.start = start;
					this.end = end;
					rangeSet = true;
					Monitor.PulseAll(rangeLock);
				}
			}

			/**
			 * The results of the latest round of processing
			 *
			 * This call blocks until processing is completed
			 *
			 * @return The changes to  the board state found by this thread
			 **/
			public Dictionary<Point, uint> results() {
				waitAndInvalidate();
				return parts;
			}

			/**
			 * The actual step thread.
			 *
			 * Block until a range is given
			 * If the start index is less than 0, this indicates that the thread should terminate
			 * Else, calculate all changes within the range
			 * Validate the changes.
			 * Block for a new range to check
			 **/
			public void stepThread() {
				while(true) {
					lock(rangeLock) {
						while(!rangeSet) {
							Monitor.Wait(rangeLock);
						}
						rangeSet = false;
					}
					if(start < 0) {
						break;
					}
					parts.Clear();
					for(int i = start; i < end; i++) {
						Point p = pointArr[i];
						uint val = parent.nextState(p.x,p.y);
						if(val != board[p.x][p.y]) {
							parts[p] = val;
						}
					}
					validate();
				}
			}
		}
	}

}
