using System;
using System.Collections.Generic;

namespace Mirage
{
	public class AsciiConsoleInput
	{
		private Queue<byte> buffer = new Queue<byte>(255);
		
		public static implicit operator Action<byte[]>(AsciiConsoleInput input)
		{
			return input.Input;
		}

		protected void Input(byte[] data)
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
