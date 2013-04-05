﻿using System;
using System.Text;

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
						PointerInc();
						break;
					case '[':
						PointerDec();
						break;
					case '%':
						XchPointers();
						break;
					case '$':
						LoadPointer();
						break;
					case '=':
						DragPointer();
						break;
					case '+':
						Inc();
						break;
					case '-':
						Dec();
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
					case '!':
						Console.Write(ASCIIEncoding.ASCII.GetString(Output()));
						break;
					case '?':
						Input(UTF8Encoding.Unicode.GetBytes(Console.ReadLine()));
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

		public void PointerInc()
		{
			pointerHi++;
		}
		public void PointerDec()
		{
			pointerHi--;
		}
		public void XchPointers()
		{
			int temp = pointerLo;
			pointerLo = pointerHi;
			pointerHi = temp;
		}
		public void LoadPointer()
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
		public void DragPointer()
		{
			pointerLo = pointerHi;
		}

		public void Inc()
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
		public void Dec()
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

		public void Not()
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
		public void Shr()
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
		public void Shl()
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

		public byte[] Output()
		{
			return GetWord();
		}
		public void Input(byte[] input)
		{
			SetWord(input);
		}
		
		public bool JmpForward()
		{
			return ((pointerHi == pointerLo) || WordIsZero());
		}
		public bool JmpBack()
		{
			return ((pointerHi != pointerLo) && !WordIsZero());
		}

		private byte[] GetWord()
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
		private void SetWord(byte[] word)
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
		private bool WordIsZero()
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
		private byte[] GetArgument()
		{
			return null;
		}
	}
}
