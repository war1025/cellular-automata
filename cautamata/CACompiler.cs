
using CAutamata;

using System;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace CAServer {

	public class CACompiler {

		public static ICASettings compile(string code) {
			var csCompiler = new CSharpCodeProvider();
			var s = new string[] {code};
			var compilerParams = new CompilerParameters(new string[] {"ICASettings.dll"});
			compilerParams.GenerateExecutable = false;
			compilerParams.GenerateInMemory = true;
			var results = csCompiler.CompileAssemblyFromSource(compilerParams, s);
			if(results.Errors.HasErrors) {
				return null;
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
