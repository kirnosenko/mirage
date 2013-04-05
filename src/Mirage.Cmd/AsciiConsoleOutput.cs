using System;

namespace Mirage.Cmd
{
	public class AsciiConsoleOutput : IByteOutput
	{
		public void Output(byte value)
		{
			Console.Write(Convert.ToChar(value));
		}
	}
}
