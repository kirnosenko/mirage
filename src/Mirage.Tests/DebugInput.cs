using System;
using System.Collections.Generic;

namespace Mirage
{
	public class DebugInput
	{
		private Queue<byte> buffer = new Queue<byte>(1024);

		public void Add(byte[] data)
		{
			for (int i = 0; i < data.Length; i++)
			{
				buffer.Enqueue(data[i]);
			}
		}
		public static implicit operator Action<byte[]>(DebugInput input)
		{
			return input.Input;
		}

		protected void Input(byte[] data)
		{
			int i = 0;

			while (i < data.Length && buffer.Count > 0)
			{
				data[i] = buffer.Dequeue();
				i++;
			}
		}
	}
}
