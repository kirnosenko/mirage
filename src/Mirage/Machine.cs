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
		private byte[] GetArgument()
		{
			return null;
		}
	}
}
