using System;
using System.Text;

using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBBufferAddUtf8Test : HBTest
	{
		public static TheoryData<string> Utf8Strings => new TheoryData<string>
		{
			// ASCII
			"1234",
			"Hello, World!",
			"The quick brown fox jumps over the lazy dog.",
			// whitespace / control
			"line1\nline2\ttabbed",
			// 2-byte UTF-8 sequences
			"café",
			"naïve façade",
			"Ærøskøbing",
			// 3-byte UTF-8 sequences (CJK)
			"日本語のテキスト",
			"中文字符",
			// combining marks (decomposed)
			"e\u0301a\u0300",
			// 4-byte UTF-8 sequences (surrogate pairs / emoji)
			"a😀b",
			"🎉🎊🥳",
			// mixed scripts + emoji
			"Hi 日本 🎉 café!",
			// long string (exercises larger pooled buffers)
			new string('x', 5000),
			LongMixed(),
		};

		private static string LongMixed()
		{
			var sb = new StringBuilder();
			for (var i = 0; i < 400; i++)
				sb.Append("The quick brown fox — 素早い狐 🦊 café ");
			return sb.ToString();
		}

		[Theory]
		[MemberData(nameof(Utf8Strings))]
		public void AddUtf8StringMatchesByteArrayPath(string text)
		{
			// Reference = the exact original implementation of AddUtf8(string):
			//   AddUtf8(Encoding.UTF8.GetBytes(text), 0, -1)
			var (expectedCodepoints, expectedClusters) = AddViaBytes(text);

			// New = the shipped AddUtf8(string) overload under test.
			var (actualCodepoints, actualClusters) = AddViaString(text);

			Assert.Equal(expectedCodepoints, actualCodepoints);
			Assert.Equal(expectedClusters, actualClusters);
		}

		[Theory]
		[MemberData(nameof(Utf8Strings))]
		public void AddUtf8StringShapesIdentically(string text)
		{
			var expected = ShapeViaBytes(text);
			var actual = ShapeViaString(text);

			Assert.Equal(expected.Codepoints, actual.Codepoints);
			Assert.Equal(expected.Clusters, actual.Clusters);
			Assert.Equal(expected.XAdvances, actual.XAdvances);
			Assert.Equal(expected.YAdvances, actual.YAdvances);
		}

		[Fact]
		public void EmptyStringIsANoOp()
		{
			// main's behaviour: an empty string adds nothing. The empty string now
			// flows through the pooled path (no ArgumentNullException, Length stays 0),
			// matching a fresh buffer.
			using var buffer = new Buffer();
			Assert.Equal(0, buffer.Length);

			buffer.AddUtf8("");

			Assert.Equal(0, buffer.Length);
			Assert.Empty(buffer.GlyphInfos);
		}

		[Fact]
		public void NullStringThrowsArgumentNullException()
		{
			// main's behaviour: a null string throws ArgumentNullException before
			// adding anything (originally from Encoding.UTF8.GetBytes((string)null)).
			using var buffer = new Buffer();
			Assert.Throws<ArgumentNullException>(() => buffer.AddUtf8((string)null));
		}

		[Fact]
		public void RealStringProducesExpectedGlyphInfos()
		{
			// "Teeth" guard: proves the equivalence/shaping tests are not vacuously
			// comparing empty output. Pre-shape, HarfBuzz adds one item per Unicode
			// scalar carrying the raw codepoint, with clusters as UTF-8 byte offsets.
			var (codepoints, clusters) = AddViaString("Hello");

			Assert.Equal(new uint[] { 'H', 'e', 'l', 'l', 'o' }, codepoints);
			Assert.Equal(new uint[] { 0, 1, 2, 3, 4 }, clusters);
		}

		private static (uint[] Codepoints, uint[] Clusters) AddViaString(string text)
		{
			using var buffer = new Buffer();
			buffer.AddUtf8(text);
			return Snapshot(buffer);
		}

		private static (uint[] Codepoints, uint[] Clusters) AddViaBytes(string text)
		{
			using var buffer = new Buffer();
			buffer.AddUtf8(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(text)), 0, -1);
			return Snapshot(buffer);
		}

		private static (uint[] Codepoints, uint[] Clusters) Snapshot(Buffer buffer)
		{
			var infos = buffer.GlyphInfos;
			var codepoints = new uint[infos.Length];
			var clusters = new uint[infos.Length];
			for (var i = 0; i < infos.Length; i++)
			{
				codepoints[i] = infos[i].Codepoint;
				clusters[i] = infos[i].Cluster;
			}
			return (codepoints, clusters);
		}

		private ShapeResult ShapeViaString(string text)
		{
			using var buffer = new Buffer();
			buffer.AddUtf8(text);
			return Shape(buffer);
		}

		private ShapeResult ShapeViaBytes(string text)
		{
			using var buffer = new Buffer();
			buffer.AddUtf8(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(text)), 0, -1);
			return Shape(buffer);
		}

		private ShapeResult Shape(Buffer buffer)
		{
			buffer.GuessSegmentProperties();
			Font.Shape(buffer);

			var infos = buffer.GlyphInfos;
			var positions = buffer.GlyphPositions;

			var result = new ShapeResult
			{
				Codepoints = new uint[infos.Length],
				Clusters = new uint[infos.Length],
				XAdvances = new int[positions.Length],
				YAdvances = new int[positions.Length],
			};
			for (var i = 0; i < infos.Length; i++)
			{
				result.Codepoints[i] = infos[i].Codepoint;
				result.Clusters[i] = infos[i].Cluster;
			}
			for (var i = 0; i < positions.Length; i++)
			{
				result.XAdvances[i] = positions[i].XAdvance;
				result.YAdvances[i] = positions[i].YAdvance;
			}
			return result;
		}

		private class ShapeResult
		{
			public uint[] Codepoints;
			public uint[] Clusters;
			public int[] XAdvances;
			public int[] YAdvances;
		}
	}
}
