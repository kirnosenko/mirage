using System;
using System.IO;
using System.Text;

namespace Mirage
{
	class Program
	{
		static void Main(string[] args)
		{
			Interpreter interpreter = new Interpreter();
			string src;

			while ((src = GetCmd()) != "")
			{
				interpreter.Interpret(src);
			}
		}
		static string GetCmd()
		{
			Console.Write("> ");
			return Console.ReadLine();
		}
	}
}
