

namespace CAClient {

	public struct CAComponents {

		public string code;
		public Color[] colors;
		public uint defaultState;

	}

	public class CAParser {

		public CAParser() {

		}

		public CAComponents parseCASettings(string filename) {
			var settings = File.OpenText(filename);
			uint numStates = 0;
			uint defaultState = 0;
			string neighborhood = null;
			string delta = null;
			string colors = null;

			string curLine = null;

			while((curLine = settings.ReadLine()) != null) {
				if(curLine.StartsWith("NumStates")) {
					numStates = UInt32.Parse(curLine.Split(new char[] {':'}, 2)[1]);
				} else if(curLine.StartsWith("DefaultState")) {
					defaultState = UInt32.Parse(curLine.Split(new char[] {':'},2)[1];
				} else if(curLine.StartsWith("Neighborhood")) {
					neighborhood = curLine.Split(new char[] {':'},2)[1];
				} else if(curLine.StartsWith("Colors")) {
					colors = parseColors(curLine, settings);
				} else if(curLine.StartsWith("Delta")) {
					delta = parseDelta(curLine, delta);
				}
			}
		}

	}

}
