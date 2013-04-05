using System;
using System.Text;

namespace Mirage
{
	public class Interpreter
	{
		private Machine machine;

		public Interpreter()
		{
			machine = new Machine(64 * 1024);
		}
		public void Interpret(string src)
		{
			foreach (var c in src)
			{
				switch(c)
				{
					case ']':
						machine.PointerInc();
						break;
					case '[':
						machine.PointerDec();
						break;
					case '%':
						machine.XchPointers();
						break;
					case '+':
						machine.Inc();
						break;
					case '-':
						machine.Dec();
						break;
					case '~':
						machine.Not();
						break;
					case '>':
						machine.Shr();
						break;
					case '<':
						machine.Shl();
						break;
					case '!':
						Console.WriteLine(ASCIIEncoding.ASCII.GetString(machine.Output()));
						break;
					case '?':
						machine.Input(UTF8Encoding.Unicode.GetBytes(Console.ReadLine()));
						break;
					default:
						break;
				}
			}
		}
	}
}
