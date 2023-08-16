using System;
using System.Text;

using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBBufferTest : HBTest
	{
		private const string SimpleText = "1234";

		private const string SerializedSimpleText = "[gid25=0+772|gid26=1+772|gid27=2+772|gid28=3+772]";

		[SkippableFact]
		public void ShouldHaveCorrectContentType()
		{
			using (var buffer = new Buffer())
			{
				buffer.Direction = Direction.LeftToRight;

				Assert.Equal(ContentType.Invalid, buffer.ContentType);

				buffer.AddUtf8(SimpleText);

				Assert.Equal(ContentType.Unicode, buffer.ContentType);

				Font.Shape(buffer);

				Assert.Equal(ContentType.Glyphs, buffer.ContentType);
			}
		}

		[SkippableFact]
		public void ShouldHaveDefaultStateAfterReset()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf8(SimpleText);

				buffer.Reset();

				Assert.Equal(ContentType.Invalid, buffer.ContentType);

				Assert.Equal(0, buffer.Length);
			}
		}

		[SkippableFact]
		public void ShouldClearContents()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf8(SimpleText);

				Assert.Equal(SimpleText.Length, buffer.GlyphInfos.Length);

				buffer.ClearContents();

				Assert.Empty(buffer.GlyphInfos);
			}
		}

		[SkippableFact]
		public void ShouldAdd()
		{
			using (var buffer = new Buffer())
			{
				buffer.ContentType = ContentType.Unicode;

				buffer.Add(55, 1337);

				Assert.Equal(1, buffer.Length);

				Assert.Equal(55u, buffer.GlyphInfos[0].Codepoint);

				Assert.Equal(1337u, buffer.GlyphInfos[0].Cluster);
			}
		}

		[SkippableFact]
		public void ShouldAddUtfByString()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf8("A");

				Assert.Equal(1, buffer.Length);

				buffer.AddUtf16("B");

				Assert.Equal(2, buffer.Length);

				buffer.AddUtf32("C");

				Assert.Equal(3, buffer.Length);
			}
		}

		[SkippableFact]
		public void ShouldAddUtfBySpan()
		{
			using (var buffer = new Buffer())
			{
				var bytes = Encoding.UTF8.GetBytes("A").AsSpan();

				buffer.AddUtf8(bytes);

				Assert.Equal(1, buffer.Length);

				var utf16 = "B".AsSpan();

				buffer.AddUtf16(utf16);

				Assert.Equal(2, buffer.Length);

				bytes = Encoding.UTF32.GetBytes("C");

				var utf32 = new int[] { bytes[0] };

				buffer.AddUtf32(utf32);

				Assert.Equal(3, buffer.Length);
			}
		}

		[SkippableFact]
		public void ShouldThrowInvalidOperationExceptionOnAddUtfWhenBufferIsShaped()
		{
			using (var buffer = new Buffer())
			{
				buffer.Direction = Direction.LeftToRight;

				buffer.AddUtf8(SimpleText);

				Font.Shape(buffer);

				Assert.Throws<InvalidOperationException>(() => { buffer.AddUtf8("A"); });
			}
		}

		[SkippableFact]
		public void ShouldThrowInvalidOperationExceptionOnSerializeGlyphsWhenBufferIsEmpty()
		{
			using (var buffer = new Buffer())
			{
				Assert.Throws<InvalidOperationException>(() => { buffer.SerializeGlyphs(); });
			}
		}

		[SkippableFact]
		public void ShouldThrowInvalidOperationExceptionOnSerializeGlyphsWhenBufferIsUnShaped()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf8("A");

				Assert.Throws<InvalidOperationException>(() => { buffer.SerializeGlyphs(); });
			}
		}

		[SkippableFact]
		public void ShouldSerializeGlyphs()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf16(SimpleText);

				buffer.GuessSegmentProperties();

				Font.Shape(buffer);

				var serializedGlyphs = buffer.SerializeGlyphs();

				Assert.Equal(SerializedSimpleText, serializedGlyphs);
			}
		}

		[SkippableFact]
		public void ShouldReverse()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf8("12");

				buffer.Reverse();

				Assert.Equal(50u, buffer.GlyphInfos[0].Codepoint);
				Assert.Equal(1u, buffer.GlyphInfos[0].Cluster);

				Assert.Equal(49u, buffer.GlyphInfos[1].Codepoint);
				Assert.Equal(0u, buffer.GlyphInfos[1].Cluster);
			}
		}

		[SkippableFact]
		public void ShouldReverseClusters()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf8("12");

				buffer.ReverseClusters();

				Assert.Equal(50u, buffer.GlyphInfos[0].Codepoint);
				Assert.Equal(1u, buffer.GlyphInfos[0].Cluster);

				Assert.Equal(49u, buffer.GlyphInfos[1].Codepoint);
				Assert.Equal(0u, buffer.GlyphInfos[1].Cluster);
			}
		}

		[SkippableFact]
		public void ShouldReverseRange()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf8("123");

				buffer.ReverseRange(0, 2);

				Assert.Equal(50u, buffer.GlyphInfos[0].Codepoint);
				Assert.Equal(1u, buffer.GlyphInfos[0].Cluster);

				Assert.Equal(49u, buffer.GlyphInfos[1].Codepoint);
				Assert.Equal(0u, buffer.GlyphInfos[1].Cluster);

				Assert.Equal(51u, buffer.GlyphInfos[2].Codepoint);
				Assert.Equal(2u, buffer.GlyphInfos[2].Cluster);
			}
		}

		[SkippableFact]
		public void ShouldAppendBuffer()
		{
			using (var buffer = new Buffer())
			using (var source = new Buffer())
			{
				buffer.ContentType = ContentType.Unicode;

				source.AddUtf8("123");

				buffer.Append(source, 0, source.Length);
			}
		}

		[SkippableFact]
		public void ShouldNormalize()
		{
			using (var buffer = new Buffer())
			{
				buffer.Direction = Direction.LeftToRight;

				buffer.AddUtf16("Â̶");

				Font.Shape(buffer);

				buffer.NormalizeGlyphs();

				Assert.Equal(0, buffer.GlyphPositions[0].XOffset);
				Assert.Equal(0, buffer.GlyphPositions[0].YOffset);
				Assert.Equal(-1135, buffer.GlyphPositions[1].XOffset);
				Assert.Equal(0, buffer.GlyphPositions[1].YOffset);
			}
		}

		[SkippableFact]
		public void ShouldThrowInvalidOperationExceptionOnDeserializeGlyphsWhenBufferIsNonEmpty()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf8("A");

				Assert.Throws<InvalidOperationException>(() => { buffer.DeserializeGlyphs(SerializedSimpleText); });
			}
		}

		[SkippableFact]
		public void ShouldDeserializeGlyphs()
		{
			using (var buffer = new Buffer())
			{
				buffer.DeserializeGlyphs(SerializedSimpleText);

				Assert.Equal(SimpleText.Length, buffer.Length);

				Assert.Equal(0u, buffer.GlyphInfos[0].Cluster);
				Assert.Equal(25u, buffer.GlyphInfos[0].Codepoint);

				Assert.Equal(1u, buffer.GlyphInfos[1].Cluster);
				Assert.Equal(26u, buffer.GlyphInfos[1].Codepoint);

				Assert.Equal(2u, buffer.GlyphInfos[2].Cluster);
				Assert.Equal(27u, buffer.GlyphInfos[2].Codepoint);

				Assert.Equal(3u, buffer.GlyphInfos[3].Cluster);
				Assert.Equal(28u, buffer.GlyphInfos[3].Codepoint);
			}
		}
	}
}
