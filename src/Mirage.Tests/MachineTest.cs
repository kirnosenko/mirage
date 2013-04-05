using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Mirage.Tests
{
	[TestFixture]
	public class MachineTest
	{
		Machine m;
		byte[] input;
		byte[] output;

		[SetUp]
		public void SetUp()
		{
			input = null;
			output = null;
			m = new Machine(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
			m.Input = () => input;
			m.Output = (o) => output = o;
		}
		[Test]
		public void Should_reset_pointers()
		{
			m.Run("]]");
			m.Reset();
			m.Run("!");
			output.Should().Have.SameSequenceAs(new byte[] { });
		}
		[Test]
		public void Output_size_is_equel_to_word_size()
		{
			m.Run("!");
			output.Length.Should().Be(0);

			m.Run("]!");
			output.Length.Should().Be(1);

			m.Run("]!");
			output.Length.Should().Be(2);

			m.Run("]!");
			output.Length.Should().Be(3);
		}
		[Test]
		public void Word_is_byte_sequence_from_lo_pointer_to_hi_one()
		{
			m = new Machine(new byte[] { 0, 1, 2 });
			m.Output = (o) => output = o;

			m.Run("]!");
			output.Should().Have.SameSequenceAs(new byte[] { 0 });

			m.Run("]!");
			output.Should().Have.SameSequenceAs(new byte[] { 0, 1 });

			m.Run("]!");
			output.Should().Have.SameSequenceAs(new byte[] { 0, 1, 2 });

			m.Run("%!");

			output.Should().Have.SameSequenceAs(new byte[] { 2, 1, 0 });
		}
		[Test]
		public void Shloud_load_hi_pointer_from_word()
		{
			byte[] data = new byte[600];
			data[512] = 255;
			data[513] = 255;

			m = new Machine(data);
			m.Output = (o) => output = o;
			m.Run("]]+<<<<<<<<<$=]]!");

			output.Should().Have.SameSequenceAs(new byte[] { 255, 255 });
		}
		[Test]
		public void Should_increment_word()
		{
			m.Run("]+!");
			output.Should().Have.SameSequenceAs(new byte[] { 1 });

			m.Run("+!");
			output.Should().Have.SameSequenceAs(new byte[] { 2 });

			m.Run("]");
			for (int i = 0; i < 256; i++)
			{
				m.Run("+");
			}
			m.Run("!");
			output.Should().Have.SameSequenceAs(new byte[] { 2, 1 });
		}
		[Test]
		public void Should_decrement_word()
		{
			m = new Machine(new byte[] { 0, 0, 1 });
			m.Output = (o) => output = o;

			m.Run("]]]-!");
			output.Should().Have.SameSequenceAs(new byte[] { 255, 255, 0 });

			m.Run("%-!");
			output.Should().Have.SameSequenceAs(new byte[] { 255, 254, 255 });
		}
		[Test]
		public void Should_invert_the_word()
		{
			m.Run("]+~!");
			output.Should().Have.SameSequenceAs(new byte[] { 254 });

			m.Run("]~!");
			output.Should().Have.SameSequenceAs(new byte[] { 1, 255 });
		}
		[Test]
		public void Should_do_logic_shift_of_the_word()
		{
			m.Run("]+<<<!");
			output.Should().Have.SameSequenceAs(new byte[] { 8 });

			m.Run("]<<<<<<!");
			output.Should().Have.SameSequenceAs(new byte[] { 0, 2 });

			m.Run(">>>!");
			output.Should().Have.SameSequenceAs(new byte[] { 64, 0 });
		}
		[Test]
		public void Can_run_in_cycle()
		{
			m.Run("]+<<+{-]}+!");

			output.Should().Have.SameSequenceAs(new byte[] { 1, 0, 0, 0, 0, 0 });
		}
	}
}
