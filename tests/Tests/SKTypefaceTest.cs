using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKTypefaceTest : SKTest
	{
		static readonly string[] ExpectedTablesSpiderFont = new string[] {
			"FFTM", "GDEF", "OS/2", "cmap", "cvt ", "gasp", "glyf", "head",
			"hhea", "hmtx", "loca", "maxp", "name", "post",
		};

		static readonly int[] ExpectedTableLengthsReallyBigAFont = new int[] {
			28, 30, 96, 330, 4, 8, 236, 54, 36, 20, 12, 32, 13665, 44,
		};

		static readonly byte[] ExpectedTableDataPostReallyBigAFont = new byte[] {
			0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xF1, 0x00, 0x06, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x01, 0x00,
			0x02, 0x00, 0x24, 0x00, 0x44,
		};

		[SkippableFact]
		public void NullWithMissingFile()
		{
			Assert.Null(SKTypeface.FromFile(Path.Combine(PathToFonts, "font that doesn't exist.ttf")));
		}

		[SkippableFact]
		public void TestFamilyName()
		{
			using (var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf")))
			{
				Assert.Equal("Roboto2", typeface.FamilyName);
			}
		}

		[SkippableFact]
		public void TestIsNotFixedPitch()
		{
			using (var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf")))
			{
				Assert.False(typeface.IsFixedPitch);
			}
		}

		[SkippableFact]
		public void TestIsFixedPitch()
		{
			using (var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "CourierNew.ttf")))
			{
				Assert.True(typeface.IsFixedPitch);
			}
		}

		[SkippableFact]
		public void CanReadNonASCIIFile()
		{
			using (var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "上田雅美.ttf")))
			{
				Assert.Equal("Roboto2", typeface.FamilyName);
			}
		}

		private static string GetReadableTag(uint v)
		{
			return
				((char)((v >> 24) & 0xFF)).ToString() +
				((char)((v >> 16) & 0xFF)).ToString() +
				((char)((v >> 08) & 0xFF)).ToString() +
				((char)((v >> 00) & 0xFF)).ToString();
		}

		private static uint GetIntTag(string v)
		{
			return
				(UInt32)(v[0]) << 24 |
				(UInt32)(v[1]) << 16 |
				(UInt32)(v[2]) << 08 |
				(UInt32)(v[3]) << 00;
		}

		[SkippableFact]
		public void TestGetTableTags()
		{
			using (var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "SpiderSymbol.ttf")))
			{
				Assert.Equal("SpiderSymbol", typeface.FamilyName);
				var tables = typeface.GetTableTags();
				Assert.Equal(ExpectedTablesSpiderFont.Length, tables.Length);

				for (int i = 0; i < tables.Length; i++)
				{
					Assert.Equal(ExpectedTablesSpiderFont[i], GetReadableTag(tables[i]));
				}
			}
		}

		[SkippableFact]
		public void TestGetTableData()
		{
			using (var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "ReallyBigA.ttf")))
			{
				Assert.Equal("ReallyBigA", typeface.FamilyName);
				var tables = typeface.GetTableTags();

				for (int i = 0; i < tables.Length; i++)
				{
					byte[] tableData = typeface.GetTableData(tables[i]);
					Assert.Equal(ExpectedTableLengthsReallyBigAFont[i], tableData.Length);
				}

				Assert.Equal(ExpectedTableDataPostReallyBigAFont, typeface.GetTableData(GetIntTag("post")));

			}
		}

		[SkippableFact]
		public void ExceptionInInvalidGetTableData()
		{
			using (var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "Distortable.ttf")))
			{
				Assert.Throws<System.Exception>(() => typeface.GetTableData(8));
			}
		}

		[SkippableFact]
		public void TestTryGetTableData()
		{
			using (var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "ReallyBigA.ttf")))
			{
				var tables = typeface.GetTableTags();
				for (int i = 0; i < tables.Length; i++)
				{
					byte[] tableData;
					Assert.True(typeface.TryGetTableData(tables[i], out tableData));
					Assert.Equal(ExpectedTableLengthsReallyBigAFont[i], tableData.Length);
				}

				Assert.Equal(ExpectedTableDataPostReallyBigAFont, typeface.GetTableData(GetIntTag("post")));

			}
		}

		[SkippableFact]
		public void InvalidTryGetTableData()
		{
			using (var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "Distortable.ttf")))
			{
				byte[] tableData;
				Assert.False(typeface.TryGetTableData(8, out tableData));
				Assert.Null(tableData);
			}
		}

		[SkippableFact]
		public void CanReadData()
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Distortable.ttf"));
			using (var data = SKData.CreateCopy(bytes))
			using (var typeface = SKTypeface.FromData(data))
			{
				Assert.NotNull(typeface);
			}
		}

		[SkippableFact]
		public void CanReadNonSeekableStream()
		{
			using (var stream = File.OpenRead(Path.Combine(PathToFonts, "Distortable.ttf")))
			using (var nonSeekable = new NonSeekableReadOnlyStream(stream))
			using (var typeface = SKTypeface.FromStream(nonSeekable))
			{
				Assert.NotNull(typeface);
			}
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

			var count = typeface.CountGlyphs(text);
			var glyphs = typeface.GetGlyphs(text);

			Assert.True(count > 0);
			Assert.True(glyphs.Length > 0);
			Assert.DoesNotContain((ushort)0, glyphs);
		}

		[Obsolete]
		[Trait(CategoryKey, MatchCharacterCategory)]
		[SkippableFact]
		public void UnicodeGlyphsReturnsTheCorrectNumberOfCharactersObsolete()
		{
			const string text = "🚀";
			var emojiChar = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf32);

			var typeface = SKFontManager.Default.MatchCharacter(emojiChar);
			Assert.NotNull(typeface);

			Assert.True(typeface.CountGlyphs(text) > 0);
			Assert.True(typeface.CountGlyphs(text, SKEncoding.Utf32) > 0);
			Assert.True(typeface.GetGlyphs(text).Length > 0);
			Assert.True(typeface.GetGlyphs(text, SKEncoding.Utf32).Length > 0);
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
			var paint = CreatePaint();

			CollectGarbage();

			var tf = paint.Typeface;

			Assert.Equal("Roboto2", tf.FamilyName);
			Assert.True(tf.TryGetTableTags(out var tags));
			Assert.NotEmpty(tags);

			SKPaint CreatePaint()
			{
				var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
				var dotnet = new MemoryStream(bytes);
				var stream = new SKManagedStream(dotnet, true);

				var typeface = SKTypeface.FromStream(stream);

				return new SKPaint
				{
					Typeface = typeface
				};
			}
		}

		[SkippableFact]
		public void StreamIsAccessableFromNativeType()
		{
			VerifyImmediateFinalizers();

			var paint = CreatePaint(out var typefaceHandle);

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKTypeface>(typefaceHandle, out _));

			var tf = paint.Typeface;

			Assert.Equal("Roboto2", tf.FamilyName);
			Assert.True(tf.TryGetTableTags(out var tags));
			Assert.NotEmpty(tags);

			SKPaint CreatePaint(out IntPtr handle)
			{
				var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
				var dotnet = new MemoryStream(bytes);
				var stream = new SKManagedStream(dotnet, true);

				var typeface = SKTypeface.FromStream(stream);
				handle = typeface.Handle;

				return new SKPaint
				{
					Typeface = typeface
				};
			}
		}

		[SkippableFact]
		public unsafe void ManagedStreamIsCollectedWhenTypefaceIsDisposed()
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Distortable.ttf"));
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
			var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Distortable.ttf"));
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

			var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Distortable.ttf"));

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

		[SkippableFact]
		public unsafe void FromFileReturnsDifferentObject()
		{
			var path = Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf");

			using var tf1 = SKTypeface.FromFile(path);
			using var tf2 = SKTypeface.FromFile(path);

			Assert.NotSame(tf1, tf2);
		}

		[SkippableFact]
		public unsafe void FromStreamReturnsDifferentObject()
		{
			using var stream1 = File.OpenRead(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
			using var tf1 = SKTypeface.FromStream(stream1);

			using var stream2 = File.OpenRead(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
			using var tf2 = SKTypeface.FromStream(stream2);

			Assert.NotSame(tf1, tf2);
		}

		[SkippableFact]
		public unsafe void FromDataReturnsDifferentObject()
		{
			using var data = SKData.Create(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));

			using var tf1 = SKTypeface.FromData(data);
			using var tf2 = SKTypeface.FromData(data);

			Assert.NotSame(tf1, tf2);
		}

		[Obsolete]
		[SkippableFact]
		public unsafe void FromTypefaceReturnsSameObject()
		{
			VerifySupportsMatchingTypefaces();

			var tf = SKTypeface.FromFamilyName(DefaultFontFamily);

			var tf1 = SKTypeface.FromTypeface(tf, SKTypefaceStyle.Normal);
			var tf2 = SKTypeface.FromTypeface(tf, SKTypefaceStyle.Normal);

			Assert.Same(tf, tf1);
			Assert.Same(tf1, tf2);
		}

		[SkippableFact]
		public unsafe void FromFamilyReturnsSameObject()
		{
			var tf1 = SKTypeface.FromFamilyName(DefaultFontFamily);
			var tf2 = SKTypeface.FromFamilyName(DefaultFontFamily);

			Assert.Same(tf1, tf2);
		}

		[SkippableFact]
		public unsafe void FontStyleIsNotTheSame()
		{
			var tf = SKTypeface.FromFamilyName(DefaultFontFamily);
			
			var fs1 = tf.FontStyle;
			var fs2 = tf.FontStyle;

			Assert.NotSame(fs1, fs2);
			Assert.NotEqual(fs1.Handle, fs2.Handle);
		}

		[SkippableFact]
		public unsafe void FromFamilyDisposeDoesNotDispose()
		{
			var tf1 = SKTypeface.FromFamilyName(DefaultFontFamily);
			var tf2 = SKTypeface.FromFamilyName(DefaultFontFamily);

			Assert.Same(tf1, tf2);

			tf1.Dispose();

			Assert.NotEqual(IntPtr.Zero, tf1.Handle);
			Assert.False(tf1.IsDisposed);
		}

		[SkippableFact]
		public unsafe void GCStillCollectsTypeface()
		{
			if (!IsWindows)
				throw new SkipException("Test designed for Windows.");

			var handle = DoWork();

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKTypeface>(handle, out _));

			static IntPtr DoWork()
			{
				var tf1 = SKTypeface.FromFamilyName("Times New Roman");
				var tf2 = SKTypeface.FromFamilyName("Times New Roman");
				Assert.Same(tf1, tf2);

				var tf3 = SKTypeface.FromFile(@"C:\Windows\Fonts\times.ttf");
				Assert.NotSame(tf1, tf3);

				tf1.Dispose();
				tf2.Dispose();
				tf3.Dispose();

				Assert.NotEqual(IntPtr.Zero, tf1.Handle);
				Assert.False(tf1.IsDisposed);

				Assert.NotEqual(IntPtr.Zero, tf2.Handle);
				Assert.False(tf2.IsDisposed);

				Assert.Equal(IntPtr.Zero, tf3.Handle);
				Assert.True(tf3.IsDisposed);

				return tf1.Handle;
			}
		}
	}
}
