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
			
			if (args.Length > 0)
			{
				if (File.Exists(args[0]))
				{
					using (TextReader file = new StreamReader(args[0]))
					{
						src = file.ReadToEnd();
						interpreter.Interpret(src);
					}
				}
			}
			
			while ((src = GetCmd()) != null)
			{
				interpreter.Interpret(src);
			}
		}
		static string GetCmd()
		{
			Console.WriteLine();
			Console.Write("> ");
			string cmd = Console.ReadLine();
			if (cmd == null)
			{
				return null;
			}
			if (cmd.ToLower() == "exit")
			{
				return null;
			}

			return cmd;
		}
	}
}
