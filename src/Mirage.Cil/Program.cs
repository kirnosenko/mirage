using System;
using System.IO;

namespace Mirage.Cil
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Title = "Mirage CIL compiler 0.9.0 alpha";

			string srcFilename = "";
			string assemblyFilename = "";
			string src = "++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.>.";

			Compiler c = new Compiler();
			c.CheckSyntax(src);
			c.Compile("test", src);
		}
	}
}
