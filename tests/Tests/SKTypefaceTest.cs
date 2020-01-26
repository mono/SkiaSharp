using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKTypefaceTest : SKTest
	{
		private static readonly string[] ExpectedTablesSpiderFont = new string[] {
			"FFTM", "GDEF", "OS/2", "cmap", "cvt ", "gasp", "glyf", "head",
			"hhea", "hmtx", "loca", "maxp", "name", "post",
		};

		private static readonly int[] ExpectedTableLengthsReallyBigAFont = new int[] {
			28, 30, 96, 330, 4, 8, 236, 54, 36, 20, 12, 32, 13665, 44,
		};

		private static readonly byte[] ExpectedTableDataPostReallyBigAFont = new byte[] {
			0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xF1, 0x00, 0x06, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x01, 0x00,
			0x02, 0x00, 0x24, 0x00, 0x44,
		};

		[SkippableFact]
		public void NullWithMissingFile()
		{
			var path = Path.Combine(PathToFonts, "font that doesn't exist.ttf");
			var typeface = SKTypeface.FromFile(path);

			Assert.Null(typeface);
		}

		[SkippableFact]
		public void TestFamilyName()
		{
			var path = Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf");
			using var typeface = SKTypeface.FromFile(path);

			Assert.Equal("Roboto2", typeface.FamilyName);
		}

		[SkippableFact]
		public void TestIsNotFixedPitch()
		{
			var path = Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf");
			using var typeface = SKTypeface.FromFile(path);

			Assert.False(typeface.IsFixedPitch);
		}

		[SkippableFact]
		public void TestIsFixedPitch()
		{
			var path = Path.Combine(PathToFonts, "CourierNew.ttf");
			using var typeface = SKTypeface.FromFile(path);

			Assert.True(typeface.IsFixedPitch);
		}

		[SkippableFact]
		public void CanReadNonASCIIFile()
		{
			var path = Path.Combine(PathToFonts, "上田雅美.ttf");
			using var typeface = SKTypeface.FromFile(path);

			Assert.Equal("Roboto2", typeface.FamilyName);
		}

		private static string GetReadableTag(uint v) =>
			((char)((v >> 24) & 0xFF)).ToString() +
			((char)((v >> 16) & 0xFF)).ToString() +
			((char)((v >> 08) & 0xFF)).ToString() +
			((char)((v >> 00) & 0xFF)).ToString();

		private static uint GetIntTag(string v) =>
			(uint)(v[0]) << 24 |
			(uint)(v[1]) << 16 |
			(uint)(v[2]) << 08 |
			(uint)(v[3]) << 00;

		[SkippableFact]
		public void TestGetTableTags()
		{
			var path = Path.Combine(PathToFonts, "SpiderSymbol.ttf");
			using var typeface = SKTypeface.FromFile(path);

			Assert.Equal("SpiderSymbol", typeface.FamilyName);
			var tables = typeface.GetTableTags();
			Assert.Equal(ExpectedTablesSpiderFont.Length, tables.Length);

			for (var i = 0; i < tables.Length; i++)
			{
				Assert.Equal(ExpectedTablesSpiderFont[i], GetReadableTag(tables[i]));
			}
		}

		[SkippableFact]
		public void TestGetTableData()
		{
			var path = Path.Combine(PathToFonts, "ReallyBigA.ttf");
			using var typeface = SKTypeface.FromFile(path);

			Assert.Equal("ReallyBigA", typeface.FamilyName);
			var tables = typeface.GetTableTags();

			for (var i = 0; i < tables.Length; i++)
			{
				var tableData = typeface.GetTableData(tables[i]);
				Assert.Equal(ExpectedTableLengthsReallyBigAFont[i], tableData.Length);
			}

			Assert.Equal(ExpectedTableDataPostReallyBigAFont, typeface.GetTableData(GetIntTag("post")));
		}

		[SkippableFact]
		public void ExceptionInInvalidGetTableData()
		{
			var path = Path.Combine(PathToFonts, "Distortable.ttf");
			using var typeface = SKTypeface.FromFile(path);

			Assert.Throws<System.Exception>(() => typeface.GetTableData(8));
		}

		[SkippableFact]
		public void TestTryGetTableData()
		{
			var path = Path.Combine(PathToFonts, "ReallyBigA.ttf");
			using var typeface = SKTypeface.FromFile(path);

			var tables = typeface.GetTableTags();
			for (var i = 0; i < tables.Length; i++)
			{
				Assert.True(typeface.TryGetTableData(tables[i], out var tableData));
				Assert.Equal(ExpectedTableLengthsReallyBigAFont[i], tableData.Length);
			}

			Assert.Equal(ExpectedTableDataPostReallyBigAFont, typeface.GetTableData(GetIntTag("post")));
		}

		[SkippableFact]
		public void InvalidTryGetTableData()
		{
			var path = Path.Combine(PathToFonts, "Distortable.ttf");
			using var typeface = SKTypeface.FromFile(path);

			Assert.False(typeface.TryGetTableData(8, out var tableData));
			Assert.Null(tableData);
		}

		[SkippableFact]
		public void CanReadData()
		{
			var path = Path.Combine(PathToFonts, "Distortable.ttf");
			var bytes = File.ReadAllBytes(path);

			using var data = SKData.CreateCopy(bytes);
			using var typeface = SKTypeface.FromData(data);

			Assert.NotNull(typeface);
		}

		[SkippableFact]
		public void CanReadNonSeekableStream()
		{
			var path = Path.Combine(PathToFonts, "Distortable.ttf");
			using var stream = File.OpenRead(path);
			using var nonSeekable = new NonSeekableReadOnlyStream(stream);
			using var typeface = SKTypeface.FromStream(nonSeekable);

			Assert.NotNull(typeface);
		}

		[SkippableFact]
		public void CanDisposeDefault()
		{
			// get the fist
			var typeface = SKTypeface.Default;
			Assert.NotNull(typeface);

			// dispose and make sure that we didn't kill it
			typeface.Dispose();
			typeface = SKTypeface.Default;
			Assert.NotNull(typeface);

			// dispose and make sure that we didn't kill it again
			typeface.Dispose();
			typeface = SKTypeface.Default;
			Assert.NotNull(typeface);
		}

		[SkippableFact]
		public void PlainGlyphsReturnsTheCorrectNumberOfCharacters()
		{
			const string text = "Hello World!";

			var typeface = SKTypeface.Default;

			Assert.True(typeface.CountGlyphs(text) > 0);
			Assert.True(typeface.GetGlyphs(text).Length > 0);
		}

		[Trait(CategoryKey, MatchCharacterCategory)]
		[SkippableFact]
		public void UnicodeGlyphsReturnsTheCorrectNumberOfCharacters()
		{
			const string text = "🚀";
			var emojiChar = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf32);

			var typeface = SKFontManager.Default.MatchCharacter(emojiChar);
			Assert.NotNull(typeface);

			Assert.True(typeface.CountGlyphs(text) > 0);
			Assert.True(typeface.GetGlyphs(text).Length > 0);
		}

		[SkippableFact]
		public unsafe void ReleaseDataWasInvokedOnlyAfterTheTypefaceWasFinished()
		{
			if (IsMac)
				throw new SkipException("macOS does not release the data when the typeface is disposed.");

			var path = Path.Combine(PathToFonts, "Distortable.ttf");
			var bytes = File.ReadAllBytes(path);

			var released = false;

			fixed (byte* b = bytes)
			{
				var data = SKData.Create((IntPtr)b, bytes.Length, (addr, ctx) => released = true);

				var typeface = SKTypeface.FromData(data);
				Assert.Equal("Distortable", typeface.FamilyName);

				data.Dispose();
				Assert.False(released, "The SKDataReleaseDelegate was called too soon.");

				typeface.Dispose();
				Assert.True(released, "The SKDataReleaseDelegate was not called at all.");
			}
		}

		[SkippableFact]
		public unsafe void StreamLosesOwnershipAndCanBeDisposedButIsNotActually()
		{
			var path = Path.Combine(PathToFonts, "Distortable.ttf");
			var stream = new SKMemoryStream(File.ReadAllBytes(path));
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.False(stream.IgnorePublicDispose);
			Assert.True(SKObject.GetInstance<SKMemoryStream>(handle, out _));

			var typeface = SKTypeface.FromStream(stream);
			Assert.False(stream.OwnsHandle);
			Assert.True(stream.IgnorePublicDispose);

			stream.Dispose();
			Assert.True(SKObject.GetInstance<SKMemoryStream>(handle, out var inst));
			Assert.Same(stream, inst);

			Assert.NotEmpty(typeface.GetTableTags());

			typeface.Dispose();
			Assert.False(SKObject.GetInstance<SKMemoryStream>(handle, out _));
		}

		[SkippableFact]
		public unsafe void InvalidStreamIsDisposedImmediately()
		{
			var stream = CreateTestSKStream();
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.False(stream.IgnorePublicDispose);
			Assert.True(SKObject.GetInstance<SKStream>(handle, out _));

			Assert.Null(SKTypeface.FromStream(stream));

			Assert.False(stream.OwnsHandle);
			Assert.True(stream.IgnorePublicDispose);
			Assert.False(SKObject.GetInstance<SKStream>(handle, out _));
		}

		[SkippableFact]
		public void ManagedStreamIsAccessableFromNativeType()
		{
			var paint = CreateFont();

			CollectGarbage();

			var tf = paint.Typeface;

			Assert.Equal("Roboto2", tf.FamilyName);
			Assert.True(tf.TryGetTableTags(out var tags));
			Assert.NotEmpty(tags);

			static SKFont CreateFont()
			{
				var path = Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf");
				var bytes = File.ReadAllBytes(path);
				var dotnet = new MemoryStream(bytes);
				var stream = new SKManagedStream(dotnet, true);

				var typeface = SKTypeface.FromStream(stream);

				return new SKFont
				{
					Typeface = typeface
				};
			}
		}

		[SkippableFact]
		public void StreamIsAccessableFromNativeType()
		{
			VerifyImmediateFinalizers();

			var paint = CreateFont(out var typefaceHandle);

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKTypeface>(typefaceHandle, out _));

			var tf = paint.Typeface;

			Assert.Equal("Roboto2", tf.FamilyName);
			Assert.True(tf.TryGetTableTags(out var tags));
			Assert.NotEmpty(tags);

			static SKFont CreateFont(out IntPtr handle)
			{
				var path = Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf");
				var bytes = File.ReadAllBytes(path);
				var dotnet = new MemoryStream(bytes);
				var stream = new SKManagedStream(dotnet, true);

				var typeface = SKTypeface.FromStream(stream);
				handle = typeface.Handle;

				return new SKFont
				{
					Typeface = typeface
				};
			}
		}

		[SkippableFact]
		public unsafe void ManagedStreamIsCollectedWhenTypefaceIsDisposed()
		{
			var path = Path.Combine(PathToFonts, "Distortable.ttf");
			var bytes = File.ReadAllBytes(path);
			var dotnet = new MemoryStream(bytes);
			var stream = new SKManagedStream(dotnet, true);
			var handle = stream.Handle;

			var typeface = SKTypeface.FromStream(stream);

			typeface.Dispose();

			Assert.False(SKObject.GetInstance<SKManagedStream>(handle, out _));
			Assert.Throws<ObjectDisposedException>(() => dotnet.Position);
		}

		[SkippableFact]
		public unsafe void ManagedStreamIsCollectedWhenCollected()
		{
			var path = Path.Combine(PathToFonts, "Distortable.ttf");
			var bytes = File.ReadAllBytes(path);
			var dotnet = new MemoryStream(bytes);

			var handle = DoWork();

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKManagedStream>(handle, out _));
			Assert.Throws<ObjectDisposedException>(() => dotnet.Position);

			IntPtr DoWork()
			{
				var stream = new SKManagedStream(dotnet, true);
				var typeface = SKTypeface.FromStream(stream);
				return stream.Handle;
			}
		}

		[SkippableFact]
		public unsafe void StreamLosesOwnershipAndCanBeGarbageCollected()
		{
			VerifyImmediateFinalizers();

			var path = Path.Combine(PathToFonts, "Distortable.ttf");
			var bytes = File.ReadAllBytes(path);

			DoWork(out var typefaceH, out var streamH);

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKMemoryStream>(streamH, out _));
			Assert.False(SKObject.GetInstance<SKTypeface>(typefaceH, out _));

			void DoWork(out IntPtr typefaceHandle, out IntPtr streamHandle)
			{
				var typeface = CreateTypeface(out streamHandle);
				typefaceHandle = typeface.Handle;

				CollectGarbage();

				Assert.NotEmpty(typeface.GetTableTags());

				Assert.True(SKObject.GetInstance<SKMemoryStream>(streamHandle, out var stream));
				Assert.False(stream.OwnsHandle);
				Assert.True(stream.IgnorePublicDispose);
			}

			SKTypeface CreateTypeface(out IntPtr streamHandle)
			{
				var stream = new SKMemoryStream(bytes);
				streamHandle = stream.Handle;

				Assert.True(stream.OwnsHandle);
				Assert.False(stream.IgnorePublicDispose);
				Assert.True(SKObject.GetInstance<SKMemoryStream>(streamHandle, out _));

				return SKTypeface.FromStream(stream);
			}
		}
	}
}
