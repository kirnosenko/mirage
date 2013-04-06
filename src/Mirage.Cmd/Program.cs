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

			Machine machine = new Machine(64 * 1024);
			machine.InputChannel = new AsciiConsoleInput();
			machine.OutputChannel = new AsciiConsoleOutput();
			string src;
			
			if (args.Length > 0)
			{
				if (File.Exists(args[0]))
				{
					using (TextReader file = new StreamReader(args[0]))
					{
						src = file.ReadToEnd();
						machine.Run(src);

						//Stopwatch time = Stopwatch.StartNew();
						//machine.Run(src);
						//Console.WriteLine("Done at {0} milliseconds.", time.ElapsedMilliseconds);
					}
				}
			}
			
			while ((src = GetCmd()) != null)
			{
				machine.Run(src);
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
