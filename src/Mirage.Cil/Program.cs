using System;
using System.IO;

namespace Mirage.Cil
{
	class Program
	{
		static void Main(string[] args)
		{
			string srcFilename = "";
			string assemblyFilename = "";
			string src = "++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.>.";

			Compiler c = new Compiler();
			c.CheckSyntax(src);
			c.Compile("test.exe", src);
		}
	}
}
