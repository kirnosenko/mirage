using System;

namespace Mirage.Cmd
{
	public class Interpreter
	{
		private Machine machine;
		private IInputOutputChannel input;
		private IInputOutputChannel output;
		
		public Interpreter(int memorySize)
			: this(memorySize, null, null)
		{
		}
		public Interpreter(int memorySize, IInputOutputChannel input, IInputOutputChannel output)
		{
			machine = new Machine(memorySize);
			this.input = input != null ? input : new AsciiConsoleInput();
			this.output = output != null ? output : new AsciiConsoleOutput();
		}
		public void Run(string src)
		{
			int pc = 0;
			while (pc < src.Length)
			{
				char opcode = src[pc++];

				switch (opcode)
				{
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
					case ')':
						machine.Inc();
						break;
					case '(':
						machine.Dec();
						break;
					case '_':
						machine.Clear();
						break;
					case '~':
						machine.Not();
						break;
					case '>':
						machine.Shr();
						break;
					case '<':
						machine.Shl();
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
					case '+':
						machine.Add();
						break;
					case '-':
						machine.Sub();
						break;
					case '!':
						machine.Output(output);
						break;
					case '?':
						machine.Input(input);
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
