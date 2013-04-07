using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Mirage.Cmd
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Title = "Mirage 0.0.1 alpha";

			Interpreter interpreter = new Interpreter(64 * 1024);
			string src;
			
			if (args.Length > 0)
			{
				if (File.Exists(args[0]))
				{
					using (TextReader file = new StreamReader(args[0]))
					{
						src = file.ReadToEnd();
						interpreter.Run(src);

						//Stopwatch time = Stopwatch.StartNew();
						//machine.Run(src);
						//Console.WriteLine("Done at {0} milliseconds.", time.ElapsedMilliseconds);
					}
				}
			}
			
			while ((src = GetCmd()) != null)
			{
				interpreter.Run(src);
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
