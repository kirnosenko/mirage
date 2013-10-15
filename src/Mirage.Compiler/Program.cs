using System;
using System.IO;

namespace Mirage.Compiler
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Title = "Mirage CIL compiler 1.0.0 alpha";
			Console.WriteLine(Console.Title);

			if (args.Length > 0)
			{
				string fileName = args[0];

				if (File.Exists(fileName))
				{
					CilCompiler c = new CilCompiler();
					c.Compile(fileName);
				}
			}
		}
	}
}
