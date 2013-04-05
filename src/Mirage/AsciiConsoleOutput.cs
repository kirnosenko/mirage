using System;

namespace Mirage
{
	public class AsciiConsoleOutput : IByteOutput
	{
		public void Output(byte value)
		{
			Console.Write(Convert.ToChar(value));
		}
	}
}
