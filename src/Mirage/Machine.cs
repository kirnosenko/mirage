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

		public Machine()
			: this(64 * 1024)
		{
		}
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

		public void IncPointers()
		{
			pointerHi++;
			pointerLo++;
		}
		public void DecPointers()
		{
			pointerHi--;
			pointerLo--;
		}
		public void IncHiPointer()
		{
			pointerHi++;
		}
		public void DecHiPointer()
		{
			pointerHi--;
		}
		public void ReflectHiPointer()
		{
			pointerHi = pointerLo - (pointerHi - pointerLo);
		}
		public void LoadHiPointer()
		{
			if (pointerHi == pointerLo)
			{
				pointerHi = 0;
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

		public void Inc()
		{
			byte[] word = GetWord();

			int counter = 0;
			int carry = 1;
			while (carry > 0 && counter < word.Length)
			{
				int sum = word[counter] + carry;
				carry = sum >= 256 ? 1 : 0;
				word[counter] = (byte)(sum & 0xFF);
				counter++;
			}

			SetWord(word);
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

		public void And()
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
		public void Add()
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

		public void Output(Action<byte[]> output)
		{
			if (output != null)
			{
				byte[] word = GetWord();
				output(word);
			}
		}
		public void Input(Action<byte[]> input)
		{
			if (input != null)
			{
				byte[] word = new byte[Math.Abs(pointerHi - pointerLo)];
				input(word);
				SetWord(word);
			}
		}
		public void LoadData(byte[] word)
		{
			int sizeDelta = word.Length - Math.Abs(pointerHi - pointerLo);

			pointerHi += pointerHi >= pointerLo ? sizeDelta : -sizeDelta;

			SetWord(word);
		}

		public bool Jmp()
		{
			return ((pointerHi == pointerLo) || WordIsZero());
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
