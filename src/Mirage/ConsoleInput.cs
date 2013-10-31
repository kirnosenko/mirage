/*
 * Mirage programming language
 * 
 * Copyright (C) 2013  Semyon Kirnosenko
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Mirage
{
	public class ConsoleInput
	{
		private Queue<byte> buffer = new Queue<byte>(1000);
		
		public static implicit operator Action<byte[]>(ConsoleInput input)
		{
			return input.Input;
		}

		protected void Input(byte[] data)
		{
			if (buffer.Count == 0)
			{
				string text = Console.ReadLine();
				if ((text != null) && (text.Length > 0))
				{
					byte[] bytes = Encoding.UTF8.GetBytes(text);

					foreach (var b in bytes)
					{
						buffer.Enqueue(b);
					}
				}
				buffer.Enqueue(0);
			}

			for (int i = 0; i < data.Length; i++)
			{
				data[i]	= buffer.Count > 0 ? buffer.Dequeue() : (byte)0;
			}
		}
	}
}
