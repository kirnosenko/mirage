using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Mirage.Tests
{
	[TestFixture]
	public class MachineTest
	{
		public class DebugOutput : IByteOutput
		{
			private List<byte> buffer = new List<byte>(64 * 1024);

			public void Output(byte value)
			{
				buffer.Add(value);
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
		}

		Machine m;
		DebugOutput output;

		[SetUp]
		public void SetUp()
		{
			m = new Machine(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
			output = new DebugOutput();
			m.ByteOutput = output;
		}
		[Test]
		public void Should_reset_pointers()
		{
			m.Run("]]");
			m.Reset();
			m.Run("!");

			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { });
		}
		[Test]
		public void Output_size_is_equel_to_word_size()
		{
			m.Run("!");
			output.GetAndClear.Length.Should().Be(0);

			m.Run("]!");
			output.GetAndClear.Length.Should().Be(1);

			m.Run("]!");
			output.GetAndClear.Length.Should().Be(2);

			m.Run("]!");
			output.GetAndClear.Length.Should().Be(3);
		}
		[Test]
		public void Word_is_byte_sequence_from_lo_pointer_to_hi_one()
		{
			m = new Machine(new byte[] { 0, 1, 2 });
			m.ByteOutput = output;

			m.Run("]!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0 });

			m.Run("]!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 1 });

			m.Run("]!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 1, 2 });

			m.Run("%!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 2, 1, 0 });
		}
		[Test]
		public void Should_reflect_hi_pointer_relative_to_lo_pointer()
		{
			m.Run("]]]~%");

			m.Run("#!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 0, 0 });

			m.Run("#!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 255, 255, 255 });
		}
		[Test]
		public void Shloud_load_hi_pointer_from_word()
		{
			byte[] data = new byte[600];
			data[512] = 255;
			data[513] = 255;

			m = new Machine(data);
			m.ByteOutput = output;
			m.Run("]])<<<<<<<<<$=]]!");

			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 255, 255 });
		}
		[Test]
		public void Should_increment_word()
		{
			m.Run("])!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1 });

			m.Run(")!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 2 });

			m.Run("]");
			for (int i = 0; i < 256; i++)
			{
				m.Run(")");
			}
			m.Run("!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 2, 1 });
		}
		[Test]
		public void Should_decrement_word()
		{
			m = new Machine(new byte[] { 0, 0, 1 });
			m.ByteOutput = output;

			m.Run("]]](!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 255, 255, 0 });

			m.Run("%(!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 255, 254, 255 });
		}
		[Test]
		public void Should_clear_the_word()
		{
			m = new Machine(new byte[] { 255, 100, 10 });
			m.ByteOutput = output;

			m.Run("]]]_!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 0, 0 });
		}
		[Test]
		public void Should_invert_the_word()
		{
			m.Run("])~!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 254 });

			m.Run("]~!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1, 255 });
		}
		[Test]
		public void Can_exchange_word_and_argument()
		{
			m = new Machine(new byte[] { 1, 2, 3, 4 });
			m.ByteOutput = output;

			m.Run("]]=]]^!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1, 2 });

			m.Run("#!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 4, 3 });
		}
		[Test]
		public void Should_do_logic_shift_of_the_word()
		{
			m.Run("])<<<!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 8 });

			m.Run("]<<<<<<!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 2 });

			m.Run(">>>!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 64, 0 });
		}
		[Test]
		public void Should_do_logic_and()
		{
			m = new Machine(new byte[] { 0x0F, 0x7A, 0x38, 0xF2 });
			m.ByteOutput = output;

			m.Run("]]=]]&!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x08, 0x72 });
		}
		[Test]
		public void Should_do_logic_or()
		{
			m = new Machine(new byte[] { 0x0F, 0x7A, 0x38, 0xF1 });
			m.ByteOutput = output;

			m.Run("]]=]]|!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x3F, 0xFB });
		}
		[Test]
		public void Can_run_in_cycle()
		{
			m.Run("])<<){(]})!");

			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1, 0, 0, 0, 0, 0 });
		}
	}
}
