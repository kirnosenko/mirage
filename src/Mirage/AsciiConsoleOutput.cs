using System;

namespace Mirage
{
	public class AsciiConsoleOutput
	{
		public void Output(byte[] data)
		{
			foreach (var b in data)
			{
				Console.Write(Convert.ToChar(b));
			}
		}
	}
}
