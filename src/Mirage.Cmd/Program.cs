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
			Console.Title = "Mirage interactive interpreter 0.9.0 alpha";

			Interpreter interpreter = new Interpreter(64 * 1024);
			Stopwatch time = null;
			string src;
			
			if (args.Length > 0)
			{
				if (File.Exists(args[0]))
				{
					using (TextReader file = new StreamReader(args[0]))
					{
						src = file.ReadToEnd();
						time = Stopwatch.StartNew();
						interpreter.Run(src);
						time.Stop();
					}
				}
			}
			
			while ((src = GetCmd(time)) != null)
			{
				time = Stopwatch.StartNew();
				interpreter.Run(src);
				time.Stop();
			}
		}
		static string GetCmd(Stopwatch time)
		{
			if (time != null)
			{
				long ms = time.ElapsedMilliseconds;
				long s = ms / 1000;
				ms -= s * 1000;
				long m = s / 60;
				s -= m * 60;
				long h = m / 60;
				m -= h * 60;

				Console.WriteLine();
				Console.Write("[");
				if (h > 0) Console.Write("{0}h ", h);
				if (m > 0) Console.Write("{0}m ", m);
				if (s > 0) Console.Write("{0}s ", s);
				Console.Write("{0}ms]", ms);
			}
			Console.Write(" > ");
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
