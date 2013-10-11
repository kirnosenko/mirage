using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;

namespace Mirage.Cmd
{
	[TestFixture]
	public class InterpreterTest
	{
		private Interpreter i;
		private DebugInput input;
		private DebugOutput output;

		[SetUp]
		public void SetUp()
		{
			input = new DebugInput();
			output = new DebugOutput();
			i = new Interpreter(64 * 1024, input, output);
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
			i.Run("]~-~<<~-~{-]}~-~!");
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

		[Test]
		public void Should_run_hello_world_program()
		{
			RunFromFile("../../../../doc/Samples/Hello world.txt");
			output.GetAndClear.Should().Have.SameSequenceAs(Encoding.UTF8.GetBytes("hello world!"));
		}
		[Test]
		public void Should_run_cat_program()
		{
			input.Add(Encoding.UTF8.GetBytes("some text"));
			RunFromFile("../../../../doc/Samples/Copy input to output.txt");
			output.GetAndClear.Should().Have.SameSequenceAs(Encoding.UTF8.GetBytes("some text\0"));
		}
		[Test]
		public void Should_run_reverse_program()
		{
			RunFromFile("../../../../doc/Samples/Reverse text.txt");
			output.GetAndClear.Should().Have.SameSequenceAs(Encoding.UTF8.GetBytes("esrever ot txet"));
		}
		[Test]
		public void Should_run_rot13_program()
		{
			input.Add(Encoding.UTF8.GetBytes("HELLO world! 123 [] {}"));
			RunFromFile("../../../../doc/Samples/ROT13.txt");
			output.GetAndClear.Should().Have.SameSequenceAs(Encoding.UTF8.GetBytes("URYYB jbeyq! 123 [] {}\0"));
		}
		
		private void RunFromFile(string fileName)
		{
			using (TextReader file = new StreamReader(fileName))
			{
				string src = file.ReadToEnd();
				i.Run(src);
			}
		}
	}
}
