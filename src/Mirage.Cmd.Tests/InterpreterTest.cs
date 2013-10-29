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
			i.Run("(0x05){-]}~-~!");
			output.GetAndClear.Should().Have.SameSequenceAs(new byte[] { 1, 0, 0, 0, 0, 0 });
		}
		[Test]
		public void Should_parse_text_to_word()
		{
			i.Run("(text)!");
			output.GetAndClear.Should().Have.SameSequenceAs(Encoding.UTF8.GetBytes("text"));
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
		[Test]
		public void Should_run_99bottles_program()
		{
			StringBuilder fullSongText = new StringBuilder();
			int count = 99;

			Func<int,string> countToText = (c) =>
			{
				if (count > 9)
				{
					return count.ToString();
				}
				else if (count > 0)
				{
					return "\b" + count.ToString();
				}
				else
				{
					return "No";
				}
			};

			while (count > 0)
			{
				fullSongText.AppendLine(countToText(count) + " bottles of beer on the wall,");
				fullSongText.AppendLine(countToText(count) + " bottles of beer.");
				fullSongText.AppendLine("Take one down, pass it around,");
				count--;
				fullSongText.AppendLine(countToText(count) + " bottles of beer on the wall.");
				fullSongText.AppendLine();
			}
			
			RunFromFile("../../../../doc/Samples/99 bottles of beer.txt");
			string outputSongText = Encoding.UTF8.GetString(output.GetAndClear);
			outputSongText.Should().Be(fullSongText.ToString());
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
