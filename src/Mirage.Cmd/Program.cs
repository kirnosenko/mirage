using System;
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
			machine.ByteInput = new AsciiConsoleInput();
			machine.ByteOutput = new AsciiConsoleOutput();
			string src;
			
			if (args.Length > 0)
			{
				if (File.Exists(args[0]))
				{
					using (TextReader file = new StreamReader(args[0]))
					{
						src = file.ReadToEnd();
						machine.Run(src);
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
