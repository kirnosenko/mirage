using System;
using System.Text;
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
		public void Should_not_move_pointers_outside_memory_space()
		{
			m = new Machine(new byte[] { 1, 2, 3, 4, 5 });

			m.IncHiPointer();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1 });
			m.DecPointers();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] {});
			m.IncHiPointer();
			m.IncPointers();
			m.IncPointers();
			m.IncPointers();
			m.IncPointers();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 5 });
			m.IncPointers();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] {});
			m.IncPointers();
			m.DecHiPointer();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 5 });
			m.ReflectHiPointer();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] {});
			m.Reset();
			m.IncHiPointer();
			m.IncHiPointer();
			m.LoadHiPointer();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1, 2, 3, 4, 5 });
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
			m.LoadData("01");
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { (byte)'0', (byte)'1' });

			m.LoadData("3");
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { (byte)'3' });

			m.LoadData("555");
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { (byte)'5', (byte)'5', (byte)'5' });

			m.XchPointers();
			m.LoadData("89");
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { (byte)'8', (byte)'9' });
		}
		[Test]
		public void Should_parse_loaded_data_format()
		{
			m.LoadData("some text");
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(Encoding.UTF8.GetBytes("some text"));

			m.LoadData("0x0F");
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x0F });

			m.LoadData("0xa5f");
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x5F, 0x0A });

			m.LoadData("0x0AFF");
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0xFF, 0x0A });

			m.LoadData("0xaff");
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0xFF, 0x0A });

			m.LoadData("0x000f");
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x0F, 0x00 });

			m.LoadData("");
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] {});
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
		public void Should_decrement_word()
		{
			m = new Machine(new byte[] { 0, 0, 1 });

			m.IncHiPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			m.Dec();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 255, 255, 0 });

			m.XchPointers();
			m.Dec();
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
			m.Not();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0xFF });

			m.IncHiPointer();
			m.Not();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 0xFF });
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
		public void Should_do_logic_xor()
		{
			m = new Machine(new byte[] { 0x0F, 0x7A, 0x38, 0xF1 });

			m.IncHiPointer();
			m.IncHiPointer();
			m.DragLoPointer();
			m.IncHiPointer();
			m.IncHiPointer();
			m.Xor();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x37, 0x8B });

			m.Xor();
			m.ReflectHiPointer();
			m.Xor();
			m.ReflectHiPointer();
			m.Xor();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x0F, 0x7A });

			m.ReflectHiPointer();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x8B, 0x37 });
		}
		[Test]
		public void Should_do_arithmetic_shift_of_the_word()
		{
			m.LoadData("0x01");
			m.Sal();
			m.Sal();
			m.Sal();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 8 });

			m.IncHiPointer();
			for (int i = 0; i < 12; i++)
			{
				m.Sal();
			}
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 128 });

			m.Sar();
			m.Sar();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 224 });
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

			m.Not();
			m.Add();
			m.Not();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0xFF, 0xFF });

			m.Not();
			m.Dec();
			m.Not();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0, 0 });

			m.Dec();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0xFF, 0xFF });
		}
		[Test]
		public void Should_do_nothing_when_cant_get_argument()
		{
			m.LoadData("0x0101");
			m.DecHiPointer();
			
			m.Add();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01 });

			m.And();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01 });

			m.Or();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01 });

			m.Xor();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01 });

			m.IncPointers();
			m.IncHiPointer();

			m.Add();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01, 0x00 });

			m.And();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01, 0x00 });

			m.Or();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01, 0x00 });

			m.Xor();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01, 0x00 });

			m.LoadData("0xFF");
			m.LoadHiPointer();
			m.DragLoPointer();
			m.DecHiPointer();
			m.LoadData("0x01");

			m.Add();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01 });

			m.And();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01 });

			m.Or();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01 });

			m.Xor();
			m.Output(output);
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x01 });

		}
	}
}
