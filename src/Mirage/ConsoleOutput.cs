/*
 * Mirage programming language
 * 
 * Copyright (C) 2013  Semyon Kirnosenko
 */

using System;
using System.Text;

namespace Mirage
{
	public class ConsoleOutput
	{
		public static implicit operator Action<byte[]>(ConsoleOutput output)
		{
			return output.Output;
		}

		protected void Output(byte[] data)
		{
			Console.Write(Encoding.UTF8.GetString(data));
		}
	}
}
