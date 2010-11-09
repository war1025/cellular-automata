
using CAutamata;

using System;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace CAServer {

	public class CACompiler {

		public static ICASettings compile(string code, out string errors) {
			var csCompiler = new CSharpCodeProvider();
			var s = new string[] {code};
			var compilerParams = new CompilerParameters(new string[] {"ICASettings.dll"});
			compilerParams.GenerateExecutable = false;
			compilerParams.GenerateInMemory = true;
			var results = csCompiler.CompileAssemblyFromSource(compilerParams, s);
			if (results.Errors.HasErrors) {
				var sb = new System.Text.StringBuilder();
				foreach (CompilerError error in results.Errors) {
					sb.AppendLine(error.ErrorText);
				}
				errors = sb.ToString();
				return null;
			} else {
				errors = "";
			}
			var assembly = results.CompiledAssembly;
			foreach( Type t in assembly.GetTypes()) {
				if(typeof(ICASettings).IsAssignableFrom(t)) {
					return t.GetConstructor(new Type[] {}).Invoke(new object[] {}) as ICASettings;
				}
			}
			return null;
		}

	}

}
