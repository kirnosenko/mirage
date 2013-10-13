using System;
using System.Text;

namespace Mirage.Cmd
{
	public class Interpreter
	{
		private Machine machine;
		private Action<byte[]> input;
		private Action<byte[]> output;

		public Interpreter(int memorySize, Action<byte[]> input, Action<byte[]> output)
		{
			this.machine = new Machine(memorySize);
			this.input = input;
			this.output = output;
		}
		public void Run(string src)
		{
			int pc = 0;
			while (pc < src.Length)
			{
				char opcode = src[pc++];

				switch (opcode)
				{
					case '>':
						machine.IncPointers();
						break;
					case '<':
						machine.DecPointers();
						break;
					case ']':
						machine.IncHiPointer();
						break;
					case '[':
						machine.DecHiPointer();
						break;
					case '#':
						machine.ReflectHiPointer();
						break;
					case '$':
						machine.LoadHiPointer();
						break;
					case '=':
						machine.DragLoPointer();
						break;
					case '%':
						machine.XchPointers();
						break;
					case '_':
						machine.Clear();
						break;
					case '+':
						machine.Add();
						break;
					case '-':
						machine.Dec();
						break;
					case '~':
						machine.Not();
						break;
					case '&':
						machine.And();
						break;
					case '|':
						machine.Or();
						break;
					case '^':
						machine.Xor();
						break;
					case '"':
						int start = pc;
						int end = start;
						while (src[pc++] != '"')
						{
							end = pc;
						}
						machine.LoadData(ParseString(src.Substring(start, end - start)));
						break;
					case '?':
						machine.Input(input);
						break;
					case '!':
						machine.Output(output);
						break;
					case '{':
						if (machine.Jmp())
						{
							int opened = 1;
							while (opened > 0)
							{
								opcode = src[pc++];
								switch (opcode)
								{
									case '}':
										opened--;
										break;
									case '{':
										opened++;
										break;
									default:
										break;
								}
							}
						}
						break;
					case '}':
						pc--;
						int closed = 1;
						while (closed > 0)
						{
							opcode = src[--pc];
							switch (opcode)
							{
								case '{':
									closed--;
									break;
								case '}':
									closed++;
									break;
								default:
									break;
							}
						}
						break;
					default:
						break;
				}
			}
		}
		
		private byte[] ParseString(string str)
		{
			if (str.Length == 0)
			{
				return new byte[] {};
			}

			byte[] data = null;

			if (str.StartsWith("0x"))
			{
				string hexStr = str.Substring(2, str.Length - 2);

				data = new byte[(hexStr.Length + 1) / 2];
				for (int i = 0; i < data.Length; i++)
				{
					int hexStrPos = hexStr.Length - 1 - i * 2;
					data[i] = (byte)HexToInt(hexStr[hexStrPos]);
					if (hexStrPos > 0)
					{
						data[i]+= (byte)(HexToInt(hexStr[hexStrPos-1]) * 16);
					}
				}
			}

			if (data == null)
			{
				data = Encoding.UTF8.GetBytes(str);
			}

			return data;
		}
		private int HexToInt(char letter)
		{
			int code = (int)letter;
			if ((code > 47) && (code < 58)) // 0..9
			{
				return code - 48;
			}
			if ((code > 64) && (code < 71)) // A..F
			{
				return code - 55;
			}
			if ((code > 96) && (code < 103)) // a..f
			{
				return code - 87;
			}
			return 0;
		}
	}
}
