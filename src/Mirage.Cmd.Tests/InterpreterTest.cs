using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;

namespace Mirage.Cmd
{
	[TestFixture]
	public class InterpreterTest
	{
		private Interpreter i;
		private DebugOutput output;

		[SetUp]
		public void SetUp()
		{
			output = new DebugOutput();
			i = new Interpreter(64 * 1024, null, output);
		}
		[Test]
		public void Can_run_program()
		{
			i.Run("]]~!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 255, 255 });
		}
		[Test]
		public void Can_deal_with_cycles()
		{
			i.Run("]^<<^{~^~]}^!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1, 0, 0, 0, 0, 0 });
		}
		[Test]
		public void Should_parse_text_to_word()
		{
			i.Run("\"text\"!");
			output.GetAndClear.Should().Have.SameSequenceAs(Encoding.UTF8.GetBytes("text"));
		}
		[Test]
		public void Should_parse_hex_to_word()
		{
			i.Run("\"0x0AFF\"!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0xFF, 0x0A });

			i.Run("\"0xaff\"!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0xFF, 0x0A });

			i.Run("\"0x000f\"!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 0x0F, 0x00 });

			i.Run("\"\"!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] {});
		}
	}
}
