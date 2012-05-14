

using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System;

namespace CAClient {

	/**
	 * All the components needed to represent a CA to be loaded.
	 **/
	public struct CAComponents {

		public string code;
		public Color[] colors;
		public uint defaultState;
		public uint numStates;

		/**
		 * Creates a CAComponents
		 *
		 * @param code The complete implementation of ICASettings
		 * @param colors Any colors that were specified in the settings file
		 * @param defaultState The default state specified in the settings
		 * @param numStates The number of states specified in the settings
		 **/
		public CAComponents(string code, Color[] colors, uint defaultState, uint numStates) {
			this.code = code;
			this.colors = colors;
			this.defaultState = defaultState;
			this.numStates = numStates;
		}

	}

	/**
	 * Represents a complete state of the board
	 **/
	public struct CAState {

		public uint defaultState;
		public Dictionary<CAutamata.Point, uint> states;

		/**
		 * Creates a CAState
		 *
		 * @param defaultState Usually the most common state on the board
		 * @param states States that differ from the defaul state.
		 **/
		public CAState(uint defaultState, Dictionary<CAutamata.Point, uint> states) {
			this.defaultState = defaultState;
			this.states = states;
		}

	}

	/**
	 * A parser for CA related things. Namely state and settings.
	 **/
	public class CAParser {

		public CAParser() {

		}

		/**
		 * Parse CA settings from a file.
		 *
		 * The format for a CA settings file has the following components:
		 * NumStates : <uint num of states>
		 * DefaultState : <uint default state>
		 * Name : <Valid C# class name>
		 * Neighborhood : {[(x,y)][ ; (x,y)]*}
		 * 		That is, (x,y) pairs enclosed in curly braces and separated by semicolons
		 * Colors : {[#rrggbb][ , #rrggbb]*}
		 * 		That is, HTML style colors separated by commas enclosed in curly braces
		 * Delta : {<next state method body, with the uint[] param named nb>}
		 * 		That is, a method body including the curly braces but not the method signature
		 *
		 * If Name, Neighborhood, or Delta cannot be parsed, they remain set to null.
		 * If NumStates is less than two, or DefaultState is greater equal to NumStates,
		 * or if any of the above is null, a blank CASettings is returned, causing compilation to fail gracefully.
		 *
		 * @param filename The file to read the CASettings from
		 *
		 * @return A CAComponents containing the settings info
		 **/
		public static CAComponents parseCASettings(string filename) {
			using(var settings = File.OpenText(filename)) {
				uint numStates = 0;
				uint defaultState = 0;
				CAutamata.Point[] neighborhood = null;
				Color[] colors = new Color[0];
				string delta = null;
				string name = null;

				string curLine = null;

				while((curLine = settings.ReadLine()) != null) {
					if(curLine.StartsWith("NumStates")) {
						numStates = UInt32.Parse(curLine.Split(new char[] {':'}, 2)[1]);
					} else if(curLine.StartsWith("DefaultState")) {
						defaultState = UInt32.Parse(curLine.Split(new char[] {':'},2)[1]);
					} else if(curLine.StartsWith("Name")) {
						name = curLine.Split(new char[] {':'},2)[1];
					} else if(curLine.StartsWith("Neighborhood")) {
						neighborhood = parseNeighborhood(curLine.Split(new char[] {':'},2)[1], settings);
					} else if(curLine.StartsWith("Colors")) {
						colors = parseColors(curLine.Split(new char[] {':'},2)[1], settings);
					} else if(curLine.StartsWith("Delta")) {
						delta = parseDelta(curLine.Split(new char[] {':'},2)[1], settings);
					}
				}
				if (name == null || neighborhood == null || delta == null || numStates < 2 || defaultState >= numStates) {
					return new CAComponents();
				}
				string code = writeCode(name, numStates, neighborhood, delta);

				return new CAComponents(code, colors, defaultState, numStates);
			}
		}

		/**
		 * Parse CA settings from parameters.
		 *
		 * The format for CA settings parameters are as follows:
		 * NumStates : <uint num of states>
		 * DefaultState : <uint default state>
		 * Name : <Valid C# class name>
		 * Neighborhood : {[(x,y)][ ; (x,y)]*}
		 * 		That is, (x,y) pairs enclosed in curly braces and separated by semicolons
		 * Colors : {[#rrggbb][ , #rrggbb]*}
		 * 		That is, HTML style colors separated by commas enclosed in curly braces
		 * Delta : {<next state method body, with the uint[] param named nb>}
		 * 		That is, a method body including the curly braces for the following method
		 * 		uint nextState(uint[] nb)
		 *
		 * When parsing from parameters, no colors are given, and the neighborhood is the only value that
		 * needs to be parsed further.
		 *
		 * If Name, Neighborhood, or Delta cannot be parsed, they remain set to null.
		 * If NumStates is less than two, or DefaultState is greater equal to NumStates,
		 * or if any of the above is null, a blank CASettings is returned, causing compilation to fail gracefully.
		 *
		 * @param name The name of the CA
		 * @param numStates The number of states
		 * @param defaultState The default state
		 * @param neighborhood The neighborhood in the format described above
		 * @param delta The delta function as described above
		 *
		 * @return A CAComponents containing the settings info
		 **/
		public static CAComponents parseCASettings(string name, uint numStates, uint defaultState, string neighborhood, string delta) {
			CAutamata.Point[] nb = parseNeighborhood(neighborhood, null);
			if (name == null || name.Length == 0 || nb == null || numStates < 2 || defaultState >= numStates) {
				return new CAComponents();
			}
			string code = writeCode(name, numStates, nb, delta);

			return new CAComponents(code, new Color[0], defaultState, numStates);
		}

		/**
		 * Parse a CAState from file.
		 *
		 * The format of a state file is as follows:
		 * DefaultState : <uint default state>
		 * [(x,y) : <state>]*
		 *
		 * @param filename The state file to read
		 *
		 * @return A CAState of the state represented in the file
		 **/
		public static CAState parseCAState(string filename) {
			using(var settings = File.OpenText(filename)) {

				uint defaultState = 0;
				var points = new Dictionary<CAutamata.Point, uint>();

				string curLine = null;

				while((curLine = settings.ReadLine()) != null) {
					if(curLine.StartsWith("DefaultState")) {
						defaultState = UInt32.Parse(curLine.Split(new char[] {':'})[1]);
					} else if(curLine.StartsWith("(")) {
						string[] parts = curLine.Split(new char[] {':'});
						if (parts.Length != 2) {
							return new CAState();
						}
						string[] point = parts[0].Split(new char[] {'(',',',')'});
						if (point.Length != 4) {
							return new CAState();
						}
						int x;
						int y;
						uint z;
						if (!int.TryParse(point[1], out x) || !int.TryParse(point[2], out y) || !uint.TryParse(parts[1], out z)) {
							return new CAState();
						}
						points[new CAutamata.Point(x,y)] = z;
					}
				}

				return new CAState(defaultState, points);
			}
		}

		/**
		 * Save a CA state
		 *
		 * First, read through the board, determine the actual default state
		 * Write this to file
		 * Then for every point that differs from the default, write it to file in the form
		 * (x,y) : <state>
		 *
		 * @param filename The file to save to
		 * @param board The CA board state
		 **/
		public static void saveCAState(string filename, uint[][] board) {
			var count = new Dictionary<uint, uint>();
			foreach (uint[] a in board) {
				foreach (uint b in a) {
					if(count.ContainsKey(b)) {
						count[b]++;
					} else {
						count[b] = 0;
					}
				}
			}
			uint max = 0;
			uint defaultState = 0;
			foreach (var kv in count) {
				if(kv.Value > max) {
					max = kv.Value;
					defaultState = kv.Key;
				}
			}

			using(var writer = new StreamWriter(filename)) {

				writer.WriteLine("DefaultState : " + defaultState);

				for(int i = 0; i < board.Length; i++) {
					for(int j = 0; j < board[i].Length; j++) {
						if(board[i][j] != defaultState) {
							writer.WriteLine("(" + i + "," + j + ") : " + board[i][j]);
						}
					}
				}
			}

		}

		/**
		 * Parses a string from a reader which contains matching outer curly braces.
		 *
		 * This is done by counting opening and closing braces.
		 * It could probably done more robustly using a Stack
		 *
		 * @param first The first line to parse
		 * @param reader A reader from which to get the subsequent lines
		 *
		 * @return A string from an opening brace to its matching closing brace
		 **/
		private static string parseBrackets(string first, StreamReader reader) {
			if(!first.Contains("{")) {
				return null;
			}
			StringBuilder sb = new StringBuilder();
			int bracketCount = 0;
			string line = first;
			do {
				foreach(char c in line) {
					switch(c) {
						case '{' : bracketCount++; break;
						case '}' : bracketCount--; break;
					}
				}
				sb.AppendLine(line);
			} while((bracketCount > 0) && (reader != null) && ((line = reader.ReadLine()) != null));

			return sb.ToString();
		}

		/**
		 * Parse a neighborhood string into an array of points
		 *
		 * First parse the brackets.
		 * Then split by semicolon
		 * Now split each of these by ( ) and ,
		 * Each point should have 4 parts. If not the string is incorrectly formatted. Return null.
		 *
		 * @param first The first line
		 * @param reader A Reader to pull subsequent lines from
		 *
		 * @return The neighborhood as a Point[]
		 **/
		private static CAutamata.Point[] parseNeighborhood(string first, StreamReader reader) {
			string neighborhood = parseBrackets(first, reader);
			string[] points = neighborhood.Split(new char[] {';'});
			CAutamata.Point[] ret = new CAutamata.Point[points.Length];

			for(int i = 0; i < points.Length; i++) {
				string p = points[i];
				string[] parts = p.Split(new char[] {'(',',',')'});
				if (parts.Length != 4) {
					return null;
				}
				int x;
				int y;
				if (!int.TryParse(parts[1], out x) || !int.TryParse(parts[2], out y)) {
					return null;
				}
				ret[i] = new CAutamata.Point(x, y);
			}

			return ret;
		}

		/**
		 * Parse colors from string to a Color[]
		 *
		 * First parse brackets.
		 * Then split by { } and ,
		 * Each of the inner parts should  be colors.
		 *
		 * @param first The first line to parse
		 * @param reader Reader to pull subsequent lines from
		 *
		 * @return The Color[] represented by the string
		 **/
		private static Color[] parseColors(string first, StreamReader reader) {
			string color = parseBrackets(first, reader);

			string[] colors = color.Split(new char[] {'{',',','}'});
			Color[] ret = new Color[colors.Length - 2];

			for(int i = 1; i < colors.Length - 1; i++) {
				ret[i-1] = ColorTranslator.FromHtml(colors[i]);
			}

			return ret;
		}

		/**
		 * Parse a delta function.
		 *
		 * This just involves parsing the brackets out
		 *
		 * @param first The first line to parse
		 * @param reader Reader to pull subsequent lines from
		 *
		 * @return A delta function
		 **/
		private static string parseDelta(string first, StreamReader reader) {
			return parseBrackets(first, reader);
		}

		/**
		 * Write an implementation of ICASettings using the given info
		 *
		 * @param name The name of the CA
		 * @param numStates How many states the CA has
		 * @param neighborhood The neighborhood for the CA
		 * @param delta The nextState function
		 *
		 * @return A string containing an implementation of ICASettings
		 **/
		private static string writeCode(string name, uint numStates, CAutamata.Point[] neighborhood, string delta) {
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("using CAutamata;");
			sb.AppendLine("public class " + name + " : ICASettings {");

			sb.AppendLine("public uint NumStates {");
			sb.AppendLine("get {");
			sb.AppendLine("return " + numStates + ";");
			sb.AppendLine("}}");

			sb.AppendLine("public Point[] Neighborhood {");
			sb.AppendLine("get {");
			sb.AppendLine("return new Point[] {");
			for(int i = 0; i < neighborhood.Length; i++) {
				if(i > 0) {
					sb.Append(",");
				}
				sb.Append("new Point( " + neighborhood[i].x + "," + neighborhood[i].y + ") ");
			}
			sb.AppendLine("};}}");

			sb.AppendLine("public uint nextState(uint[] nb)");
			sb.AppendLine(delta);

			sb.AppendLine("}");

			return sb.ToString();
		}

	}

}
