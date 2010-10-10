

using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System;

namespace CAClient {

	public struct CAComponents {

		public string code;
		public Color[] colors;
		public uint defaultState;

		public CAComponents(string code, Color[] colors, uint defaultState) {
			this.code = code;
			this.colors = colors;
			this.defaultState = defaultState;
		}

	}

	public struct CAState {

		public uint defaultState;
		public IDictionary<Point, uint> states;

		public CAState(uint defaultState, IDictionary<Point, uint> states) {
			this.defaultState = defaultState;
			this.states = states;
		}

	}

	public class CAParser {

		public CAParser() {

		}

		public CAComponents parseCASettings(string filename) {
			var settings = File.OpenText(filename);
			uint numStates = 0;
			uint defaultState = 0;
			Point[] neighborhood = null;
			Color[] colors = null;
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

			string code = writeCode(name, numStates, neighborhood, delta);

			return new CAComponents(code, colors, defaultState);
		}

		public CAState parseCAState(string filename) {
			var settings = File.OpenText(filename);

			uint defaultState = 0;
			var points = new Dictionary<Point, uint>();

			string curLine = null;

			while((curLine = settings.ReadLine()) != null) {
				if(curLine.StartsWith("DefaultState")) {
					defaultState = UInt32.Parse(curLine.Split(new char[] {':'})[1]);
				} else if(curLine.StartsWith("(")) {
					string[] parts = curLine.Split(new char[] {':'});
					string[] point = parts[0].Split(new char[] {'(',',',')'});
					points[new Point(Int32.Parse(point[1]),Int32.Parse(point[2]))] = UInt32.Parse(parts[1]);
				}
			}

			return new CAState(defaultState, points);
		}

		private string parseBrackets(string first, StreamReader reader) {
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
			} while((bracketCount > 0) && ((line = reader.ReadLine()) != null));

			return sb.ToString();
		}

		private Point[] parseNeighborhood(string first, StreamReader reader) {
			string neighborhood = parseBrackets(first, reader);
			string[] points = neighborhood.Split(new char[] {';'});
			Point[] ret = new Point[points.Length];

			for(int i = 0; i < points.Length; i++) {
				string p = points[i];
				string[] parts = p.Split(new char[] {'(',',',')'});
				ret[i] = new Point(Int32.Parse(parts[1]), Int32.Parse(parts[2]));
			}

			return ret;
		}

		private Color[] parseColors(string first, StreamReader reader) {
			string color = parseBrackets(first, reader);

			string[] colors = color.Split(new char[] {'{',',','}'});
			Color[] ret = new Color[colors.Length - 2];

			for(int i = 1; i < colors.Length - 1; i++) {
				ret[i-1] = ColorTranslator.FromHtml(colors[i]);
			}

			return ret;
		}

		private string parseDelta(string first, StreamReader reader) {
			return parseBrackets(first, reader);
		}

		private string writeCode(string name, uint numStates, Point[] neighborhood, string delta) {
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
				sb.Append("new Point( " + neighborhood[i].X + "," + neighborhood[i].Y + ") ");
			}
			sb.AppendLine("};}}");

			sb.AppendLine("public uint nextState(uint[] nb)");
			sb.AppendLine(delta);

			sb.AppendLine("}");

			return sb.ToString();
		}

	}

}
