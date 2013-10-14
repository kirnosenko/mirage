using System;

namespace Mirage.Compiler
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Title = "Mirage CIL compiler 1.0.0 alpha";

			/*
			string srcFilename = args[0];
			string assemblyFilename = "";
			string src = "++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.>.";
			*/
			CilCompiler c = new CilCompiler();
			//c.CheckSyntax(src);
			c.Compile("hello");
		}
	}
}
