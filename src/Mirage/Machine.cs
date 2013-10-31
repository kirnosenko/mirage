/*
 * Mirage programming language
 * 
 * Copyright (C) 2013  Semyon Kirnosenko
 */

using System;
using System.Text;

namespace Mirage
{
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
			int newHiPointer = 0;
			WordGet((b) => {
				newHiPointer = newHiPointer << 8;
				newHiPointer |= b;
			}, false);
			
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
			WordSet(ClearProcessor, true);
		}
		public void Add()
		{
			WordModify(null, AddProcessor, true, 0);
		}
		public void Dec()
		{
			WordModify(DecProcessor, null, true, 1);
		}

		public void Not()
		{
			WordModify(NotProcessor, null, true, 0);
		}
		public void And()
		{
			WordModify(null, AndProcessor, true, 0);
		}
		public void Or()
		{
			WordModify(null, OrProcessor, true, 0);
		}
		public void Xor()
		{
			WordModify(null, XorProcessor, true, 0);
		}
		
		public void Sal()
		{
			WordModify(SalProcessor, null, true, 0);
		}
		public void Sar()
		{
			byte hiByte = (byte)(memory[PointerHi < PointerLo ? PointerHi : PointerHi - 1] & 128);
			WordModify(SarProcessor, null, false, hiByte);
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
				int i = 0;
				WordSet(() => {
					return word[i++];
				}, true);
			}
		}
		public void Output(Action<byte[]> output)
		{
			if (output != null)
			{
				byte[] word = new byte[Math.Abs(PointerHi - PointerLo)];
				int i = 0;
				WordGet((b) => {
					word[i++] = b;
				}, true);
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
		protected bool WordIsZero()
		{
			bool isZero = true;

			WordGet((b) => {
				if (b != 0)
				{
					isZero = false;
				}
			}, true);
			
			return isZero;
		}
		protected void LoadData(byte[] word)
		{
			int sizeDelta = word.Length - Math.Abs(PointerHi - PointerLo);

			PointerHi += PointerHi >= PointerLo ? sizeDelta : -sizeDelta;

			int i = 0;
			WordSet(() =>
			{
				return word[i++];
			}, true);
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

		protected class ByteProcessingData
		{
			public byte Byte { get; set; }
			public byte Flag { get; set; }
		}

		protected void WordGet(Action<byte> wordGetter, bool direct)
		{
			WordProcessing(wordGetter, null, null, null, direct, 0);
		}
		protected void WordSet(Func<byte> wordSetter, bool direct)
		{
			WordProcessing(null, wordSetter, null, null, direct, 0);
		}
		protected void WordModify(
			Action<byte,byte,ByteProcessingData> wordModifier,
			Action<byte,byte,byte,ByteProcessingData> wordModifierWithArgument,
			bool direct,
			byte initFlag
		)
		{
			WordProcessing(null, null, wordModifier, wordModifierWithArgument, direct, initFlag);
		}
		protected void WordProcessing(
			Action<byte> wordGetter,
			Func<byte> wordSetter,
			Action<byte,byte,ByteProcessingData> wordModifier,
			Action<byte,byte,byte,ByteProcessingData> wordModifierWithArgument,
			bool direct,
			byte initFlag
		)
		{
			if (PointerHi == PointerLo)
			{
				return;
			}
			
			int pointerArgLo = pointerHi - 2 * (pointerHi - pointerLo);
			if ((wordModifierWithArgument != null) && ((pointerArgLo < 0) || (pointerArgLo > memory.Length)))
			{
				return;
			}

			int wordPosition = direct ? pointerLo : pointerHi;
			int wordFinishPosition = direct ? pointerHi : pointerLo;
			int argumentPosition = direct ? pointerArgLo : pointerLo;
			
			int delta = wordFinishPosition > wordPosition ? 1 : -1;
			if (delta < 0)
			{
				wordPosition--;
				wordFinishPosition--;
				argumentPosition--;
			}

			ByteProcessingData processingData = new ByteProcessingData() { Byte = 0, Flag = initFlag };
			
			while (wordPosition != wordFinishPosition)
			{
				if (wordSetter == null)
				{
					processingData.Byte = memory[wordPosition];
				}

				if (wordGetter != null)
				{
					wordGetter(processingData.Byte);
				}
				else if (wordSetter != null)
				{
					processingData.Byte = wordSetter();
				}
				else if (wordModifier != null)
				{
					wordModifier(processingData.Byte, processingData.Flag, processingData);
				}
				else
				{
					wordModifierWithArgument(processingData.Byte, memory[argumentPosition], processingData.Flag, processingData);
					argumentPosition += delta;
				}

				if (wordGetter == null)
				{
					memory[wordPosition] = processingData.Byte;
				}
				wordPosition += delta;
			}
		}

		protected byte ClearProcessor()
		{
			return 0;
		}
		protected void AddProcessor(byte wordByte, byte argumentByte, byte flag, ByteProcessingData result)
		{
			int sum = wordByte + argumentByte + flag;
			result.Byte = (byte)(sum & 0xFF);
			result.Flag = (byte)(sum >> 8);
		}
		protected void DecProcessor(byte wordByte, byte flag, ByteProcessingData result)
		{
			int sum = wordByte - flag;
			result.Byte = (byte)(sum < 0 ? 0xFF : sum);
			result.Flag = (byte)(sum < 0 ? 1 : 0);
		}
		protected void NotProcessor(byte wordByte, byte flag, ByteProcessingData result)
		{
			result.Byte = (byte)(~wordByte);
		}
		protected void AndProcessor(byte wordByte, byte argumentByte, byte flag, ByteProcessingData result)
		{
			result.Byte = (byte)(wordByte & argumentByte);
		}
		protected void OrProcessor(byte wordByte, byte argumentByte, byte flag, ByteProcessingData result)
		{
			result.Byte = (byte)(wordByte | argumentByte);
		}
		protected void XorProcessor(byte wordByte, byte argumentByte, byte flag, ByteProcessingData result)
		{
			result.Byte = (byte)(wordByte ^ argumentByte);
		}
		protected void SalProcessor(byte wordByte, byte flag, ByteProcessingData result)
		{
			result.Byte = (byte)(((wordByte << 1) & 0xFF) | flag);
			result.Flag = (byte)((wordByte & 0x80) >> 7);
		}
		protected void SarProcessor(byte wordByte, byte flag, ByteProcessingData result)
		{
			result.Byte = (byte)(((wordByte >> 1) & 0xFF) | flag);
			result.Flag = (byte)((wordByte & 0x01) << 7);
		}
	}
}
