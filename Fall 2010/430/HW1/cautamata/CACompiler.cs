
using CAutamata;

using System;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace CAServer {

	public class CACompiler {

		public static ICASettings compile(string name, string code) {
			var csCompiler = new CSharpCodeProvider();
			var s = new string[] {code};
			var compilerParams = new CompilerParameters();
			compilerParams.GenerateExecutable = false;
			compilerParams.GenerateInMemory = true;
			var results = csCompiler.CompileAssemblyFromSource(compilerParams, s);
			if(results.Errors.HasErrors) {
				return null;
			}
			var assembly = results.CompiledAssembly;
			return assembly.CreateInstance(name) as ICASettings;
		}

	}

}
