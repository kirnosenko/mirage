using System;

namespace Mirage
{
	public class AsciiConsoleOutput
	{
		public static implicit operator Action<byte[]>(AsciiConsoleOutput output)
		{
			return output.Output;
		}

		protected void Output(byte[] data)
		{
			foreach (var b in data)
			{
				Console.Write(Convert.ToChar(b));
			}
		}
	}
}
