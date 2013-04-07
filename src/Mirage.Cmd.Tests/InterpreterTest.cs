using System;
using System.Collections.Generic;
using System.Linq;
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
			i.Run("])<<){(]})!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1, 0, 0, 0, 0, 0 });
		}
	}
}
