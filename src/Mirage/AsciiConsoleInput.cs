using System;
using System.Collections.Generic;

namespace Mirage
{
	public class AsciiConsoleInput : IByteInput
	{
		private Queue<byte> buffer;
		
		public AsciiConsoleInput()
		{
			buffer = new Queue<byte>(255);
		}
		public byte Input()
		{
			if (buffer.Count == 0)
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
			
			return buffer.Dequeue();
		}
	}
}
