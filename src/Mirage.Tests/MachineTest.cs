using System;
using NUnit.Framework;
using SharpTestsEx;

namespace Mirage.Tests
{
	[TestFixture]
	public class MachineTest
	{
		Machine m;

		[SetUp]
		public void SetUp()
		{
			m = new Machine(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
		}
		[Test]
		public void Output_size_is_equel_to_word_size()
		{
			m.Output().Length
				.Should().Be(0);

			m.PointerInc();
			m.Output().Length
				.Should().Be(1);

			m.PointerInc();
			m.Output().Length
				.Should().Be(2);

			m.PointerInc();
			m.Output().Length
				.Should().Be(3);
		}
		[Test]
		public void Word_is_byte_sequence_from_lo_pointer_to_hi_one()
		{
			m = new Machine(new byte[] { 0, 1, 2 });

			m.PointerInc();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 0 });

			m.PointerInc();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 0, 1 });

			m.PointerInc();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 0, 1, 2 });

			m.XchPointers();

			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 2, 1, 0 });
		}
		[Test]
		public void Shloud_load_hi_pointer_from_word()
		{
			byte[] data = new byte[600];
			data[512] = 0xFF;
			data[513] = 0xFF;

			m = new Machine(data);
			m.PointerInc();
			m.PointerInc();
			m.Inc();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.LoadPointer();
			m.DragPointer();
			m.PointerInc();
			m.PointerInc();

			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 255, 255 });
		}
		[Test]
		public void Should_increment_word()
		{
			m.PointerInc();
			
			m.Inc();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 1 });

			m.Inc();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 2 });

			m.PointerInc();
			for (int i = 0; i < 256; i++)
			{
				m.Inc();
			}
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 2, 1 });
		}
		[Test]
		public void Should_decrement_word()
		{
			m = new Machine(new byte[] { 0, 0, 1 });

			m.PointerInc();
			m.PointerInc();
			m.PointerInc();
			m.Dec();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 255, 255, 0 });

			m.XchPointers();
			m.Dec();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 255, 254, 255 });
		}
		[Test]
		public void Should_invert_the_word()
		{
			m.PointerInc();
			m.Inc();
			m.Not();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 254 });

			m.PointerInc();
			m.Not();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 1, 255 });
		}
		[Test]
		public void Shlould_do_logic_shift_of_the_word()
		{
			m.PointerInc();
			m.Inc();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 8 });

			m.PointerInc();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Shl();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 0, 2 });

			m.Shr();
			m.Shr();
			m.Shr();
			m.Output()
				.Should().Have.SameSequenceAs(new byte[] { 64, 0 });
		}
	}
}
