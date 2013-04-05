using System;
using System.IO;
using System.Text;

namespace Mirage
{
	class Program
	{
		static void Main(string[] args)
		{
			Machine machine = new Machine(64 * 1024);
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
