using System;
using System.Collections.Generic;

namespace Mirage.Cmd
{
	public class AsciiConsoleInput : IInputOutputChannel
	{
		private Queue<byte> buffer;
		
		public AsciiConsoleInput()
		{
			buffer = new Queue<byte>(255);
		}
		public void InputOutput(byte[] data)
		{
			if (buffer.Count < data.Length)
			{
				string text = Console.ReadLine();
				if ((text != null) && (text.Length > 0))
				{
					foreach (var c in text)
					{
						buffer.Enqueue(Convert.ToByte(c));
					}
				}
				buffer.Enqueue(0);
			}
			
			for (int i = 0; i < data.Length; i++)
			{
				data[i]	= buffer.Dequeue();
			}
		}
	}
}
