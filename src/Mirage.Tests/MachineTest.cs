using System;
using NUnit.Framework;
using SharpTestsEx;

namespace Mirage
{
	[TestFixture]
	public class MachineTest
	{
		private Machine m;
		private DebugOutput output;

		[SetUp]
		public void SetUp()
		{
			m = new Machine(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
			output = new DebugOutput();
		}
		[Test]
		public void Should_reset_pointers()
		{
			m.IncHiPointer();
			m.IncHiPointer();
			m.Reset();
			m.Output(output);

			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] {});
		}
		[Test]
		public void Output_size_is_equel_to_word_size()
		{
			m.Output(output);
			output.GetAndClear.Length.Should().Be(0);

			m.IncHiPointer();
			m.Output(output);
			output.GetAndClear.Length.Should().Be(1);

			m.IncHiPointer();
			m.Output(output);
			output.GetAndClear.Length.Should().Be(2);

			m.IncHiPointer();
			m.Output(output);
			output.GetAndClear.Length.Should().Be(3);
		}
		[Test]
		public void Input_size_is_equel_to_word_size()
		{
			int size = 0;
			Action<byte[]> input = (word) =>
			{
				size = word.Length;
			};

			m.Input(input);
			size.Should().Be(0);

			m.IncHiPointer();
			m.IncHiPointer();
			m.Input(input);
			size.Should().Be(2);

			m.XchPointers();
			m.Input(input);
			size.Should().Be(2);
		}
		[Test]
		public void Should_set_word_size_equal_to_loaded_data_size()
		{
			m.LoadData(new byte[] { 1, 2 });
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1, 2 });

			m.LoadData(new byte[] { 3 });
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 3 });

			m.LoadData(new byte[] { 5, 5, 5 });
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 5, 5, 5 });

			m.XchPointers();
			m.LoadData(new byte[] { 8, 9 });
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 8, 9 });
		}
		[Test]
		public void Word_is_byte_sequence_from_lo_pointer_to_hi_one()
		{
			m = new Machine(new byte[] { 0, 1, 2 });

			m.IncHiPointer();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0 });

			m.IncHiPointer();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 1 });

			m.IncHiPointer();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 1, 2 });

			m.XchPointers();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 2, 1, 0 });
		}
		[Test]
		public void Should_reflect_hi_pointer_relative_to_lo_pointer()
		{
			m = new Machine(new byte[] { 0, 1, 2, 3 });

			m.IncHiPointer();
			m.IncHiPointer();
			m.DragLoPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			m.Output(output);
			
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 2, 3 });

			m.ReflectHiPointer();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1, 0 });
		}
		[Test]
		public void Shloud_load_hi_pointer_from_word()
		{
			byte[] data = new byte[600];
			data[0] = 0;
			data[1] = 2;
			data[512] = 254;
			data[513] = 255;

			m = new Machine(data);

			m.IncHiPointer();
			m.IncHiPointer();
			m.LoadHiPointer();
			m.DragLoPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			m.Output(output);

			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 254, 255 });

			m.DragLoPointer();
			m.LoadHiPointer();
			m.DragLoPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			m.Output(output);
			
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 2 });
		}
		[Test]
		public void Should_increment_word()
		{
			m.IncHiPointer();
			m.Inc();
			m.Output(output);
			
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1 });

			m.Inc();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 2 });

			m.IncHiPointer();
			for (int i = 0; i < 256; i++)
			{
				m.Inc();
			}
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 2, 1 });
		}
		[Test]
		public void Can_make_decrement_from_increment()
		{
			m = new Machine(new byte[] { 0, 0, 1 });

			m.IncHiPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			m.Not();
			m.Inc();
			m.Not();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 255, 255, 0 });

			m.XchPointers();
			m.Not();
			m.Inc();
			m.Not();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 255, 254, 255 });
		}
		[Test]
		public void Should_clear_the_word()
		{
			m = new Machine(new byte[] { 255, 100, 10 });

			m.IncHiPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			m.Clear();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 0, 0 });
		}
		[Test]
		public void Should_invert_the_word()
		{
			m.IncHiPointer();
			m.Inc();
			m.Not();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 254 });

			m.IncHiPointer();
			m.Not();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1, 255 });
		}
		[Test]
		public void Should_do_logic_shift_of_the_word()
		{
			m.IncHiPointer();
			m.Inc();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 8 });

			m.IncHiPointer();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 2 });

			m.Shr();
			m.Shr();
			m.Shr();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 64, 0 });
		}
		[Test]
		public void Should_do_logic_and()
		{
			m = new Machine(new byte[] { 0x0F, 0x7A, 0x38, 0xF2 });

			m.IncHiPointer();
			m.IncHiPointer();
			m.DragLoPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			m.And();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x08, 0x72 });
		}
		[Test]
		public void Should_do_logic_or()
		{
			m = new Machine(new byte[] { 0x0F, 0x7A, 0x38, 0xF1 });

			m.IncHiPointer();
			m.IncHiPointer();
			m.DragLoPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			m.Or();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x3F, 0xFB });
		}
		[Test]
		public void Should_add_operand_to_word()
		{
			m = new Machine(new byte[] { 200, 20, 200, 40 });

			m.IncHiPointer();
			m.IncHiPointer();
			m.DragLoPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			m.Add();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 144, 61 });

			m.ReflectHiPointer();
			m.Add();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 81, 88});
		}
		[Test]
		public void Should_sub_operand_from_word()
		{
			m = new Machine(new byte[] { 200, 20, 200, 50 });

			m.IncHiPointer();
			m.IncHiPointer();
			m.DragLoPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			m.Sub();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 30 });

			m.ReflectHiPointer();
			m.Sub();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 246, 199 });
		}
		[Test]
		public void Should_work_correctly_with_overflow()
		{
			m = new Machine(new byte[] { 1, 0, 0xFF, 0xFF });

			m.IncHiPointer();
			m.IncHiPointer();
			m.DragLoPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			
			m.Add();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 0 });

			m.Sub();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0xFF, 0xFF });

			m.Inc();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 0 });

			m.Not();
			m.Inc();
			m.Not();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0xFF, 0xFF });
		}
	}
}
