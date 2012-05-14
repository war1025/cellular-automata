
using CAutamata;

using System;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace CAServer {

	/**
	 * Compiler for CASettings
	 **/
	public class CACompiler {

		/**
		 * Compiles the given code, noting any errors if present, else returning the compiled ICASettings instance.
		 *
		 * @param code The code to compile
		 * @param errors Store any compiler errors here.
		 *
		 * @return The compiled CASettings on success.
		 **/
		public static ICASettings compile(string code, out string errors) {
			var csCompiler = new CSharpCodeProvider();
			var s = new string[] {code};
			// We must include the ICASettings dll so that the compiler knows about the ICASettins interface
			var compilerParams = new CompilerParameters(new string[] {"ICASettings.dll"});
			compilerParams.GenerateExecutable = false;
			compilerParams.GenerateInMemory = true;
			var results = csCompiler.CompileAssemblyFromSource(compilerParams, s);
			// Take note of any compiler errors
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
			// Look for a class in the compiled assembly that implements ICASettings.
			// Return that class.
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
