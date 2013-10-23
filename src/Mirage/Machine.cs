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

		public Machine()
			: this(64 * 1024)
		{
		}
		public Machine(int memorySize)
		{
			memory = new byte[memorySize];
			for (int i = 0; i < memorySize; i++)
			{
				memory[i] = 0;
			}
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

		public void IncPointers()
		{
			PointerLo++;
			PointerHi++;
		}
		public void DecPointers()
		{
			PointerLo--;
			PointerHi--;
		}
		public void IncHiPointer()
		{
			PointerHi++;
		}
		public void DecHiPointer()
		{
			PointerHi--;
		}
		public void ReflectHiPointer()
		{
			PointerHi = PointerLo - (PointerHi - PointerLo);
		}
		public void LoadHiPointer()
		{
			if (PointerHi == PointerLo)
			{
				pointerHi = 0;
				return;
			}

			byte[] word = GetWord();
			int newHiPointer = 0;
			
			int counter = word.Length-1;
			while (counter >= 0)
			{
				newHiPointer = newHiPointer << 8;
				newHiPointer |= word[counter];
				counter--;
			}

			PointerHi = newHiPointer;
		}
		public void DragLoPointer()
		{
			pointerLo = pointerHi;
		}
		public void XchPointers()
		{
			int temp = pointerLo;
			pointerLo = pointerHi;
			pointerHi = temp;
		}

		public void Clear()
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
		public void Add()
		{
			if (!ArgumentIsExists())
			{
				return;
			}

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
		public void Dec()
		{
			byte[] word = GetWord();

			int counter = 0;
			int carry = 1;
			while (carry > 0 && counter < word.Length)
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
		public void And()
		{
			Logic((op1, op2) => (byte)(op1 & op2));
		}
		public void Or()
		{
			Logic((op1, op2) => (byte)(op1 | op2));
		}
		public void Xor()
		{
			Logic((op1, op2) => (byte)(op1 ^ op2));
		}
		public void Logic(Func<byte,byte,byte> func)
		{
			if (!ArgumentIsExists())
			{
				return;
			}

			byte[] word = GetWord();
			byte[] argument = GetArgument();

			int counter = 0;
			while (counter < word.Length)
			{
				word[counter] = func(word[counter], argument[counter]);
				counter++;
			}

			SetWord(word);
		}
		public void Sal()
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
		public void Sar()
		{
			byte[] word = GetWord();

			int counter = word.Length - 1;
			byte carry = (byte)(word[counter] & 0x80);
			while (counter >= 0)
			{
				byte b = word[counter];
				word[counter] = (byte)(((b >> 1) & 0xFF) | carry);
				carry = (b & 0x01) != 0 ? (byte)128 : (byte)0;
				counter--;
			}

			SetWord(word);
		}

		public void LoadData(string str)
		{
			byte[] data = null;

			if (str == String.Empty)
			{
				data = new byte[] {};
			}
			else
			{
				if (str.StartsWith("0x"))
				{
					string hexStr = str.Substring(2, str.Length - 2).ToUpper();

					data = new byte[(hexStr.Length + 1) / 2];
					for (int i = 0; i < data.Length; i++)
					{
						int hexStrPos = hexStr.Length - 1 - i * 2;
						data[i] = (byte)HexToInt(hexStr[hexStrPos]);
						if (hexStrPos > 0)
						{
							data[i] += (byte)(HexToInt(hexStr[hexStrPos - 1]) * 16);
						}
					}
				}
				else
				{
					data = Encoding.UTF8.GetBytes(str);
				}
			}

			LoadData(data);
		}
		public void Input(Action<byte[]> input)
		{
			if (input != null)
			{
				byte[] word = new byte[Math.Abs(PointerHi - PointerLo)];
				input(word);
				SetWord(word);
			}
		}
		public void Output(Action<byte[]> output)
		{
			if (output != null)
			{
				byte[] word = GetWord();
				output(word);
			}
		}

		public bool Jz()
		{
			return ((PointerHi == PointerLo) || WordIsZero());
		}
		public bool Jnz()
		{
			return !Jz();
		}

		protected int PointerLo
		{
			get
			{
				return pointerLo;
			}
			set
			{
				pointerLo = value;
				if (pointerLo < 0)
				{
					pointerLo = 0;
				}
				else if (pointerLo > memory.Length)
				{
					pointerLo = memory.Length;
				}
			}
		}
		protected int PointerHi
		{
			get
			{
				return pointerHi;
			}
			set
			{
				pointerHi = value;
				if (pointerHi < 0)
				{
					pointerHi = 0;
				}
				else if (pointerHi > memory.Length)
				{
					pointerHi = memory.Length;
				}
			}
		}
		protected int WordSize
		{
			get
			{
				return Math.Abs(PointerHi - PointerLo);
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
		protected bool ArgumentIsExists()
		{
			int argPointerLo = pointerHi - 2*(pointerHi-pointerLo);

			return ((argPointerLo >= 0) && (argPointerLo <= memory.Length));
		}

		protected void LoadData(byte[] word)
		{
			int sizeDelta = word.Length - Math.Abs(PointerHi - PointerLo);

			PointerHi += PointerHi >= PointerLo ? sizeDelta : -sizeDelta;

			SetWord(word);
		}
		protected int HexToInt(char letter)
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
			return 0;
		}

		protected byte[] GetWord()
		{
			if (PointerHi == PointerLo)
			{
				return new byte[] {};
			}
			int pointerDelta = PointerHi > PointerLo ? 1 : -1;
			int startPointer = PointerLo;
			int endPoiner = PointerHi;
			if (pointerDelta < 0)
			{
				startPointer -= 1;
				endPoiner -= 1;
			}

			byte[] word = new byte[Math.Abs(PointerHi - PointerLo)];
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
			if ((PointerHi == PointerLo) || (word.Length == 0))
			{
				return;
			}
			int pointerDelta = PointerHi > PointerLo ? 1 : -1;
			int startPointer = PointerLo;
			int endPoiner = PointerHi;
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
		protected byte[] GetArgument()
		{
			if (PointerHi == PointerLo)
			{
				return new byte[] {};
			}
			int pointerDelta = PointerHi > PointerLo ? 1 : -1;
			int startPointer = PointerLo;
			int endPoiner = PointerHi;
			if (pointerDelta < 0)
			{
				startPointer -= 1;
				endPoiner -= 1;
				startPointer += Math.Abs(PointerHi - PointerLo);
				endPoiner += Math.Abs(PointerHi - PointerLo);
			}
			else
			{
				startPointer -= Math.Abs(PointerHi - PointerLo);
				endPoiner -= Math.Abs(PointerHi - PointerLo);
			}

			byte[] argument = new byte[Math.Abs(PointerHi - PointerLo)];
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
