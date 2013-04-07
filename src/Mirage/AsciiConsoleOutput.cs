using System;

namespace Mirage
{
	public class AsciiConsoleOutput : IInputOutputChannel
	{
		public void InputOutput(byte[] data)
		{
			foreach (var b in data)
			{
				Console.Write(Convert.ToChar(b));
			}
		}
	}
}
