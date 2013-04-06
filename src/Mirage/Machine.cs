using System;

namespace Mirage
{
	/// <summary>
	/// Machine implemenatation made with simplicity in mind not performance.
	/// </summary>
	public class Machine
	{
		protected byte[] memory;
		protected int pointerLo, pointerHi;

		public Machine(int memorySize)
		{
			memory = new byte[memorySize];
			Reset();
		}
		public Machine(byte[] memory)
		{
			this.memory = memory;
			Reset();
		}
		public void Reset()
		{
			pointerLo = 0;
			pointerHi = 0;
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
						IncHiPointer();
						break;
					case '[':
						DecHiPointer();
						break;
					case '#':
						ReflectHiPointer();
						break;
					case '$':
						LoadHiPointer();
						break;
					case '=':
						DragLoPointer();
						break;
					case '%':
						XchPointers();
						break;
					case ')':
						Inc();
						break;
					case '(':
						Dec();
						break;
					case '_':
						Clear();
						break;
					case '~':
						Not();
						break;
					case '>':
						Shr();
						break;
					case '<':
						Shl();
						break;
					case '&':
						And();
						break;
					case '|':
						Or();
						break;
					case '^':
						Xor();
						break;
					case '+':
						Add();
						break;
					case '-':
						Sub();
						break;
					case '!':
						Output();
						break;
					case '?':
						Input();
						break;
					case '{':
						if (JmpForward())
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
						if (JmpBack())
						{
							int closed = 1;
							while (closed > 0)
							{
								opcode = src[--pc-1];
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
						}
						break;
					default:
						break;
				}
			}
		}
		public IInputOutputChannel OutputChannel
		{
			get; set;
		}
		public IInputOutputChannel InputChannel
		{
			get; set;
		}

		protected void IncHiPointer()
		{
			pointerHi++;
		}
		protected void DecHiPointer()
		{
			pointerHi--;
		}
		protected void ReflectHiPointer()
		{
			pointerHi = pointerLo - (pointerHi - pointerLo);
		}
		protected void LoadHiPointer()
		{
			if (pointerHi == pointerLo)
			{
				return;
			}
			byte[] word = GetWord();
			pointerHi = 0;
			
			int counter = word.Length-1;
			while (counter >= 0)
			{
				pointerHi = pointerHi << 8;
				pointerHi |= word[counter];
				counter--;
			}
		}
		protected void DragLoPointer()
		{
			pointerLo = pointerHi;
		}
		protected void XchPointers()
		{
			int temp = pointerLo;
			pointerLo = pointerHi;
			pointerHi = temp;
		}

		protected void Inc()
		{
			byte[] word = GetWord();

			int counter = 0;
			int carry = 1;
			while (carry > 0)
			{
				int sum = word[counter] + carry;
				carry = sum >= 256 ? 1 : 0;
				word[counter] = (byte)(sum & 0xFF);
				counter++;
			}

			SetWord(word);
		}
		protected void Dec()
		{
			byte[] word = GetWord();

			int counter = 0;
			int carry = 1;
			while (carry > 0)
			{
				int sum = word[counter] - carry;
				carry = sum < 0 ? 1 : 0;
				word[counter] = sum < 0 ? (byte)0xFF : (byte)(sum & 0xFF);
				counter++;
			}

			SetWord(word);
		}

		protected void Clear()
		{
			byte[] word = GetWord();

			int counter = 0;
			while (counter < word.Length)
			{
				word[counter] = 0;
				counter++;
			}

			SetWord(word);
		}
		protected void Not()
		{
			byte[] word = GetWord();

			int counter = 0;
			while (counter < word.Length)
			{
				word[counter] = (byte)~word[counter];
				counter++;
			}

			SetWord(word);
		}
		protected void Shr()
		{
			byte[] word = GetWord();

			int counter = word.Length-1;
			byte carry = 0;
			while (counter >= 0)
			{
				byte b = word[counter];
				word[counter] = (byte)(((b >> 1) & 0xFF) | carry);
				carry = (b & 0x01) != 0 ? (byte)128 : (byte)0;
				counter--;
			}

			SetWord(word);
		}
		protected void Shl()
		{
			byte[] word = GetWord();

			int counter = 0;
			byte carry = 0;
			while (counter < word.Length)
			{
				byte b = word[counter];
				word[counter] = (byte)(((b << 1) & 0xFF) | carry);
				carry = (b & 0x80) != 0 ? (byte)1 : (byte)0;
				counter++;
			}

			SetWord(word);
		}

		protected void And()
		{
			byte[] word = GetWord();
			byte[] argument = GetArgument();

			int counter = 0;
			while (counter < word.Length)
			{
				word[counter] = (byte)(word[counter] & argument[counter]);
				counter++;
			}

			SetWord(word);
		}
		protected void Or()
		{
			byte[] word = GetWord();
			byte[] argument = GetArgument();

			int counter = 0;
			while (counter < word.Length)
			{
				word[counter] = (byte)(word[counter] | argument[counter]);
				counter++;
			}

			SetWord(word);
		}
		protected void Xor()
		{
			byte[] word = GetWord();
			byte[] argument = GetArgument();

			int counter = 0;
			while (counter < word.Length)
			{
				word[counter] = (byte)(word[counter] ^ argument[counter]);
				counter++;
			}

			SetWord(word);
		}
		protected void Add()
		{
			byte[] word = GetWord();
			byte[] argument = GetArgument();

			int counter = 0;
			int carry = 0;
			while (counter < word.Length)
			{
				int sum = word[counter] + argument[counter] + carry;
				word[counter] = (byte)(sum & 0xFF);
				carry = sum >> 8;
				counter++;
			}

			SetWord(word);
		}
		protected void Sub()
		{
			byte[] word = GetWord();
			byte[] argument = GetArgument();

			int counter = 0;
			int carry = 0;
			while (counter < word.Length)
			{
				int sum = word[counter] - argument[counter] - carry;
				if (sum >= 0)
				{
					carry = 0;
				}
				else
				{
					sum += 256;
					carry++;
				}
				word[counter] = (byte)(sum & 0xFF);
				counter++;
			}

			SetWord(word);
		}

		protected void Output()
		{
			if (OutputChannel != null)
			{
				byte[] word = GetWord();
				OutputChannel.InputOutput(word);
			}
		}
		protected void Input()
		{
			if (InputChannel != null)
			{
				byte[] word = new byte[Math.Abs(pointerHi - pointerLo)];
				InputChannel.InputOutput(word);
				SetWord(word);
			}
		}

		protected bool JmpForward()
		{
			return ((pointerHi == pointerLo) || WordIsZero());
		}
		protected bool JmpBack()
		{
			return ((pointerHi != pointerLo) && !WordIsZero());
		}

		protected byte[] GetWord()
		{
			if (pointerHi == pointerLo)
			{
				return new byte[] {};
			}
			int pointerDelta = pointerHi > pointerLo ? 1 : -1;
			int startPointer = pointerLo;
			int endPoiner = pointerHi;
			if (pointerDelta < 0)
			{
				startPointer -= 1;
				endPoiner -= 1;
			}

			byte[] word = new byte[Math.Abs(pointerHi - pointerLo)];
			int pointer = startPointer;
			int counter = 0;
			do
			{
				word[counter] = memory[pointer];
				pointer += pointerDelta;
				counter++;
			} while (pointer != endPoiner);

			return word;
		}
		protected void SetWord(byte[] word)
		{
			if ((pointerHi == pointerLo) || (word.Length == 0))
			{
				return;
			}
			int pointerDelta = pointerHi > pointerLo ? 1 : -1;
			int startPointer = pointerLo;
			int endPoiner = pointerHi;
			if (pointerDelta < 0)
			{
				startPointer -= 1;
				endPoiner -= 1;
			}

			int pointer = startPointer;
			int counter = 0;
			while ((pointer != endPoiner) && (counter < word.Length))
			{
				memory[pointer] = word[counter];
				pointer += pointerDelta;
				counter++;
			} 
		}
		protected bool WordIsZero()
		{
			byte[] word = GetWord();

			foreach (var b in word)
			{
				if (b != 0)
				{
					return false;
				}
			}

			return true;
		}
		protected byte[] GetArgument()
		{
			if (pointerHi == pointerLo)
			{
				return new byte[] {};
			}
			int pointerDelta = pointerHi > pointerLo ? 1 : -1;
			int startPointer = pointerLo;
			int endPoiner = pointerHi;
			if (pointerDelta < 0)
			{
				startPointer -= 1;
				endPoiner -= 1;
				startPointer += Math.Abs(pointerHi - pointerLo);
				endPoiner += Math.Abs(pointerHi - pointerLo);
			}
			else
			{
				startPointer -= Math.Abs(pointerHi - pointerLo);
				endPoiner -= Math.Abs(pointerHi - pointerLo);
			}

			byte[] argument = new byte[Math.Abs(pointerHi - pointerLo)];
			int pointer = startPointer;
			int counter = 0;
			do
			{
				argument[counter] = memory[pointer];
				pointer += pointerDelta;
				counter++;
			} while (pointer != endPoiner);

			return argument;
		}
	}
}
