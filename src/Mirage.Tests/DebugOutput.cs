using System;
using System.Collections.Generic;

namespace Mirage
{
	public class DebugOutput
	{
		private List<byte> buffer = new List<byte>(64 * 1024);

		public void Output(byte[] data)
		{
			foreach (var b in data)
			{
				buffer.Add(b);
			}
		}
		public byte[] GetAndClear
		{
			get
			{
				byte[] array = buffer.ToArray();
				buffer.Clear();
				return array;
			}
		}
		public static implicit operator Action<byte[]>(DebugOutput output)
		{
			return output.Output;
		}
	}
}
