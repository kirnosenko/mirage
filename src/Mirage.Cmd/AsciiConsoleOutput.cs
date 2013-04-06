using System;

namespace Mirage.Cmd
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
