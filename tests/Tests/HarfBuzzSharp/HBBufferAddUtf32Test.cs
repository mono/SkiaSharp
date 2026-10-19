using System;
using System.Text;

using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBBufferAddUtf32Test : HBTest
	{
		public static TheoryData<string> Utf32Strings => new TheoryData<string>
		{
			"",
			"Hello, World!",
			"line1\nline2\ttabbed",
			"日本語のテキスト",
			"e\u0301a\u0300",
			"a😀b",
			"Hi 日本 🎉 café!",
			"\ud800",
			"\udc00",
			new string('x', 5000),
			LongMixed(),
		};

		private static string LongMixed()
		{
			var text = new StringBuilder();
			for (var i = 0; i < 400; i++)
				text.Append("The quick brown fox — 素早い狐 🦊 café ");
			return text.ToString();
		}

		[Theory]
		[MemberData(nameof(Utf32Strings))]
		public void AddUtf32StringMatchesByteArrayPath(string text)
		{
			var expected = AddViaBytes(text);
			var actual = AddViaString(text);

			Assert.Equal(expected.Codepoints, actual.Codepoints);
			Assert.Equal(expected.Clusters, actual.Clusters);
		}

		[Theory]
		[MemberData(nameof(Utf32Strings))]
		public void AddUtf32StringShapesIdentically(string text)
		{
			var expected = ShapeViaBytes(text);
			var actual = ShapeViaString(text);

			Assert.Equal(expected.Codepoints, actual.Codepoints);
			Assert.Equal(expected.Clusters, actual.Clusters);
			Assert.Equal(expected.XAdvances, actual.XAdvances);
			Assert.Equal(expected.YAdvances, actual.YAdvances);
		}

		[Fact]
		public void NullStringThrowsArgumentNullException()
		{
			using var buffer = new Buffer();

			Assert.Throws<ArgumentNullException>(() => buffer.AddUtf32((string)null));
		}

		[Fact]
		public void RealStringProducesExpectedGlyphInfos()
		{
			var result = AddViaString("a😀b");

			Assert.Equal(new uint[] { 'a', 0x1f600, 'b' }, result.Codepoints);
			Assert.Equal(new uint[] { 0, 1, 2 }, result.Clusters);
		}

		private static BufferResult AddViaString(string text)
		{
			using var buffer = new Buffer();
			buffer.AddUtf32(text);
			return Snapshot(buffer);
		}

		private static BufferResult AddViaBytes(string text)
		{
			using var buffer = new Buffer();
			buffer.AddUtf32(Encoding.UTF32.GetBytes(text));
			return Snapshot(buffer);
		}

		private ShapeResult ShapeViaString(string text)
		{
			using var buffer = new Buffer();
			buffer.AddUtf32(text);
			return Shape(buffer);
		}

		private ShapeResult ShapeViaBytes(string text)
		{
			using var buffer = new Buffer();
			buffer.AddUtf32(Encoding.UTF32.GetBytes(text));
			return Shape(buffer);
		}

		private static BufferResult Snapshot(Buffer buffer)
		{
			var infos = buffer.GlyphInfos;
			var result = new BufferResult
			{
				Codepoints = new uint[infos.Length],
				Clusters = new uint[infos.Length],
			};
			for (var i = 0; i < infos.Length; i++)
			{
				result.Codepoints[i] = infos[i].Codepoint;
				result.Clusters[i] = infos[i].Cluster;
			}
			return result;
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

		private class BufferResult
		{
			public uint[] Codepoints;
			public uint[] Clusters;
		}

		private class ShapeResult : BufferResult
		{
			public int[] XAdvances;
			public int[] YAdvances;
		}
	}
}
