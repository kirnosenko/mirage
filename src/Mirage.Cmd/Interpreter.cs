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
					case '(':
						int dataStart = pc;
						int dataEnd = dataStart;
						while (src[pc++] != ')')
						{
							dataEnd = pc;
						}
						machine.LoadData(src.Substring(dataStart, dataEnd - dataStart));
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
	}
}
