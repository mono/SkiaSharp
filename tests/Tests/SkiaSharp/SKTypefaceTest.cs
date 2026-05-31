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
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
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
		public void TestPostScriptName()
		{
			using (var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "CourierNew.ttf")))
			{
				Assert.Equal("CourierNewPSMT", typeface.PostScriptName);
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
		public unsafe void UnicharCountReturnsCorrectCount()
		{
			var text = new uint[] { 79 };
			var count = text.Length;

			var typeface = SKTypeface.Default;

			fixed (uint* t = text)
			{
				Assert.Equal(1, typeface.CountGlyphs((IntPtr)t, count, SKTextEncoding.Utf32));

				var glyphs = typeface.GetGlyphs((IntPtr)t, count, SKTextEncoding.Utf32);
				Assert.Single(glyphs);
			}
		}

		[SkippableFact]
		public void PlainGlyphsReturnsTheCorrectNumberOfCharacters()
		{
			const string text = "Hello World!";

			var typeface = SKTypeface.Default;

			Assert.True(typeface.CountGlyphs(text) > 0);
			Assert.True(typeface.GetGlyphs(text).Length > 0);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.MatchCharacter)]
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

		[SkippableFact]
		public void ContainsGlyphsWithByteSpanDoesNotStackOverflow ()
		{
			using var typeface = SKTypeface.Default;
			var text = System.Text.Encoding.UTF8.GetBytes ("Hello");
			ReadOnlySpan<byte> span = text;

			// This overload had infinite recursion (called itself instead of GetFont())
			// It should throw StackOverflowException if still broken, or work correctly if fixed
			var result = typeface.ContainsGlyphs (span, SKTextEncoding.Utf8);
			Assert.True (result);
		}

		[SkippableFact]
		public unsafe void ReleaseDataWasInvokedOnlyAfterTheTypefaceWasFinished()
		{
			SkipOnPlatform(IsMac, "macOS does not release the data when the typeface is disposed");

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
		public void ManagedStreamIsAccessibleFromNativeType()
		{
			var font = CreateFont();

			CollectGarbage();

			var tf = font.Typeface;

			Assert.Equal("Roboto2", tf.FamilyName);
			Assert.True(tf.TryGetTableTags(out var tags));
			Assert.NotEmpty(tags);

			SKFont CreateFont()
			{
				var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
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
		public void StreamIsAccessibleFromNativeType()
		{
			SkipOnMono();

			var font = CreateFont(out var typefaceHandle);

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKTypeface>(typefaceHandle, out _));

			var tf = font.Typeface;

			Assert.Equal("Roboto2", tf.FamilyName);
			Assert.True(tf.TryGetTableTags(out var tags));
			Assert.NotEmpty(tags);

			SKFont CreateFont(out IntPtr handle)
			{
				var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
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
			SkipOnMono();

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
			SkipOnNonWindows("Test uses Windows-specific font path");

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

		// Empty / IsEmpty

		[SkippableFact]
		public void DefaultHasValidNativeHandle()
		{
			Assert.NotEqual(IntPtr.Zero, SKTypeface.Default.Handle);
		}

		[SkippableFact]
		public void DefaultFamilyNameIsNotNullOrEmpty()
		{
			Assert.NotNull(SKTypeface.Default.FamilyName);
			Assert.NotEmpty(SKTypeface.Default.FamilyName);
		}

		[SkippableFact]
		public void DefaultIsNotEmpty()
		{
			Assert.False(SKTypeface.Default.IsEmpty);
			Assert.True(SKTypeface.Default.GlyphCount > 0);
		}

		[SkippableFact]
		public void CreateDefaultHasValidHandle()
		{
			using var tf = SKTypeface.CreateDefault();
			Assert.NotNull(tf);
			Assert.NotEqual(IntPtr.Zero, tf.Handle);
		}

		[SkippableFact]
		public void CreateDefaultAndDefaultSameFamily()
		{
			using var created = SKTypeface.CreateDefault();
			Assert.Equal(SKTypeface.Default.FamilyName, created.FamilyName);
		}

		[SkippableFact]
		public void EmptyTypefaceIsEmpty()
		{
			Assert.True(SKTypeface.Empty.IsEmpty);
			Assert.Equal(0, SKTypeface.Empty.GlyphCount);
		}

		[SkippableFact]
		public void EmptyTypefaceHasEmptyFamilyName()
		{
			Assert.NotNull(SKTypeface.Empty.FamilyName);
			Assert.Empty(SKTypeface.Empty.FamilyName);
		}

		[SkippableFact]
		public void CanDisposeEmpty()
		{
			SKTypeface.Empty.Dispose();
			Assert.NotNull(SKTypeface.Empty);
			Assert.NotEqual(IntPtr.Zero, SKTypeface.Empty.Handle);
		}

		// FromFamilyName behavioral changes

		[SkippableFact]
		public void FromFamilyNameNonExistentReturnsNonNull()
		{
			var tf = SKTypeface.FromFamilyName("NonExistentFontFamilyXYZ12345");
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void FromFamilyNameNullReturnsNonNull()
		{
			var tf = SKTypeface.FromFamilyName(null);
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void FromFamilyNameEmptyStringReturnsNonNull()
		{
			var tf = SKTypeface.FromFamilyName("");
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void FromFamilyNameNonExistentReturnsFallback()
		{
			var tf = SKTypeface.FromFamilyName("NonExistentFontFamilyXYZ12345");
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void FromFamilyNameNonExistentConsistentWithMatchFamily()
		{
			var fromFamily = SKTypeface.FromFamilyName("NonExistentFontFamilyXYZ12345");
			var matched = SKFontManager.Default.MatchFamily("NonExistentFontFamilyXYZ12345");

			// FromFamilyName always returns non-null (falls back to Default)
			Assert.NotNull(fromFamily);

			// MatchFamily is the strict API — may return null on some platforms
			if (matched != null)
			{
				// If both returned a result, they should be the same cached object
				Assert.Same(matched, fromFamily);
			}
		}

		[SkippableFact]
		public void FromFileNullThrows()
		{
			Assert.Throws<ArgumentNullException>(() => SKTypeface.FromFile(null));
		}

		// ZeroGlyphs font

		[SkippableFact]
		public void ZeroGlyphFontIsEmpty()
		{
			using var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "ZeroGlyphs.ttf"));
			if (tf == null)
				return;

			Assert.Equal(0, tf.GlyphCount);
			Assert.True(tf.IsEmpty);
		}

		[SkippableFact]
		public void ZeroGlyphFontHasFamilyName()
		{
			using var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "ZeroGlyphs.ttf"));
			if (tf == null)
				return;

			Assert.Equal("ZeroGlyphs", tf.FamilyName);
		}

		// Variable font tests

		[SkippableFact]
		public void CanGetVariationDesignParameters ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			var axes = typeface.VariationDesignParameters;
			Assert.NotEmpty (axes);

			var axis = axes[0];
			Assert.NotEqual (default(SKFourByteTag), axis.Tag);
			Assert.True (axis.Min <= axis.Default);
			Assert.True (axis.Default <= axis.Max);
		}

		[SkippableFact]
		public void VariationDesignParametersEmptyForStaticFont ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "content-font.ttf"));
			Assert.NotNull (typeface);

			var axes = typeface.VariationDesignParameters;
			Assert.Empty (axes);
		}

		[SkippableFact]
		public void CanGetVariationDesignPosition ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			var position = typeface.VariationDesignPosition;
			Assert.NotEmpty (position);
		}

		[SkippableFact]
		public void CanClone ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			var axes = typeface.VariationDesignParameters;
			Assert.NotEmpty (axes);

			var position = new SKFontVariationPositionCoordinate[] {
				new SKFontVariationPositionCoordinate { Axis = axes[0].Tag, Value = axes[0].Max }
			};

			using var cloned = typeface.Clone (position);
			Assert.NotNull (cloned);

			// Verify the cloned typeface has the new position
			var clonedPosition = cloned.VariationDesignPosition;
			Assert.NotEmpty (clonedPosition);
			Assert.Equal (axes[0].Max, clonedPosition[0].Value);
		}

		[SkippableFact]
		public void CloneOnStaticFontReturnsTypeface ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "content-font.ttf"));
			Assert.NotNull (typeface);

			var position = new SKFontVariationPositionCoordinate[] {
				new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse ("wght"), Value = 400 }
			};

			// Static fonts should handle this gracefully — Skia returns a valid typeface
			using var cloned = typeface.Clone (position);
			Assert.NotNull (cloned);
		}

		[SkippableFact]
		public void SpanGetVariationDesignParametersMatchesProperty ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			var arrayResult = typeface.VariationDesignParameters;
			Assert.NotEmpty (arrayResult);

			var spanBuffer = new SKFontVariationAxis[arrayResult.Length];
			var written = typeface.GetVariationDesignParameters (spanBuffer);
			Assert.Equal (arrayResult.Length, written);

			for (int i = 0; i < arrayResult.Length; i++) {
				Assert.Equal (arrayResult[i].Tag, spanBuffer[i].Tag);
				Assert.Equal (arrayResult[i].Min, spanBuffer[i].Min);
				Assert.Equal (arrayResult[i].Default, spanBuffer[i].Default);
				Assert.Equal (arrayResult[i].Max, spanBuffer[i].Max);
			}
		}

		[SkippableFact]
		public void SpanGetVariationDesignPositionMatchesProperty ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			var arrayResult = typeface.VariationDesignPosition;
			Assert.NotEmpty (arrayResult);

			var spanBuffer = new SKFontVariationPositionCoordinate[arrayResult.Length];
			var written = typeface.GetVariationDesignPosition (spanBuffer);
			Assert.Equal (arrayResult.Length, written);

			for (int i = 0; i < arrayResult.Length; i++) {
				Assert.Equal (arrayResult[i].Axis, spanBuffer[i].Axis);
				Assert.Equal (arrayResult[i].Value, spanBuffer[i].Value);
			}
		}

		[SkippableFact]
		public void SpanGetVariationDesignParametersWithUndersizedBuffer ()
		{
			// InterVariable has multiple axes (wght, opsz) — tests undersized buffer behavior
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "InterVariable.ttf"));
			Assert.NotNull (typeface);

			var total = typeface.VariationDesignParameterCount;
			Assert.True (total >= 2, $"Need a multi-axis font, got {total} axes");

			// span=0: nothing to write, returns 0
			var buf0 = new SKFontVariationAxis[0];
			Assert.Equal (0, typeface.GetVariationDesignParameters (buf0));

			// span=1 (undersized): should fill 1 and return 1
			var buf1 = new SKFontVariationAxis[1];
			var ret1 = typeface.GetVariationDesignParameters (buf1);
			Assert.Equal (1, ret1);
			Assert.NotEqual (default, buf1[0].Tag);

			// span=exact: should fill all and return total
			var bufExact = new SKFontVariationAxis[total];
			Assert.Equal (total, typeface.GetVariationDesignParameters (bufExact));

			// The partial fill should match the first element of the full fill
			Assert.Equal (bufExact[0].Tag, buf1[0].Tag);
			Assert.Equal (bufExact[0].Min, buf1[0].Min);
		}

		[SkippableFact]
		public void SpanGetVariationDesignPositionWithUndersizedBuffer ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "InterVariable.ttf"));
			Assert.NotNull (typeface);

			var total = typeface.VariationDesignPositionCount;
			Assert.True (total >= 2, $"Need a multi-axis font, got {total} axes");

			// span=0: nothing to write, returns 0
			var buf0 = new SKFontVariationPositionCoordinate[0];
			Assert.Equal (0, typeface.GetVariationDesignPosition (buf0));

			// span=1 (undersized): should fill 1 and return 1
			var buf1 = new SKFontVariationPositionCoordinate[1];
			var ret1 = typeface.GetVariationDesignPosition (buf1);
			Assert.Equal (1, ret1);
			Assert.NotEqual (default, buf1[0].Axis);

			// span=exact: should fill all
			var bufExact = new SKFontVariationPositionCoordinate[total];
			Assert.Equal (total, typeface.GetVariationDesignPosition (bufExact));

			// Partial fill matches first element of full fill
			Assert.Equal (bufExact[0].Axis, buf1[0].Axis);
			Assert.Equal (bufExact[0].Value, buf1[0].Value);
		}

		[SkippableFact]
		public void SpanVariationDesignParametersEmptyForStaticFont ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "content-font.ttf"));
			Assert.NotNull (typeface);

			var spanBuffer = new SKFontVariationAxis[4];
			var written = typeface.GetVariationDesignParameters (spanBuffer);
			Assert.True (written <= 0, $"Static font should return 0 or -1, got {written}");
		}

		[SkippableFact]
		public void SpanVariationDesignPositionEmptyForStaticFont ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "content-font.ttf"));
			Assert.NotNull (typeface);

			var spanBuffer = new SKFontVariationPositionCoordinate[4];
			var written = typeface.GetVariationDesignPosition (spanBuffer);
			Assert.True (written <= 0, $"Static font should return 0 or -1, got {written}");
		}

		[SkippableFact]
		public void SpanGetVariationDesignParametersWithOversizedBuffer ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			var total = typeface.VariationDesignParameterCount;
			Assert.True (total > 0);

			var spanBuffer = new SKFontVariationAxis[total + 5];
			var written = typeface.GetVariationDesignParameters (spanBuffer);
			Assert.Equal (total, written);
		}

		[SkippableFact]
		public void SpanGetVariationDesignPositionWithOversizedBuffer ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			var total = typeface.VariationDesignPositionCount;
			Assert.True (total > 0);

			var spanBuffer = new SKFontVariationPositionCoordinate[total + 5];
			var written = typeface.GetVariationDesignPosition (spanBuffer);
			Assert.Equal (total, written);
		}

		[SkippableFact]
		public void CloneWithReadOnlySpan ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			var axes = typeface.VariationDesignParameters;
			Assert.NotEmpty (axes);

			ReadOnlySpan<SKFontVariationPositionCoordinate> position = new[] {
				new SKFontVariationPositionCoordinate { Axis = axes[0].Tag, Value = axes[0].Min }
			};

			using var cloned = typeface.Clone (position);
			Assert.NotNull (cloned);

			var clonedPosition = cloned.VariationDesignPosition;
			Assert.NotEmpty (clonedPosition);
			Assert.Equal (axes[0].Min, clonedPosition[0].Value);
		}

		[SkippableFact]
		public void CloneWithPaletteOverride ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "test_glyphs-COLRv1.ttf"));
			Assert.NotNull (typeface);

			var overrides = new SKFontPaletteOverride[] {
				new SKFontPaletteOverride { Index = 0, Color = 0xFFFF0000 }
			};

			using var cloned = typeface.Clone (new SKFontArguments {
				PaletteOverrides = overrides,
			});
			Assert.NotNull (cloned);
		}

		[SkippableFact]
		public void CloneWithDifferentPaletteIndex ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "test_glyphs-COLRv1.ttf"));
			Assert.NotNull (typeface);

			using var clone0 = typeface.Clone (0);
			using var clone1 = typeface.Clone (1);
			Assert.NotNull (clone0);
			Assert.NotNull (clone1);
		}

		[SkippableFact]
		public void CloneWithNegativePaletteIndexThrows ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "test_glyphs-COLRv1.ttf"));
			Assert.NotNull (typeface);

			Assert.Throws<ArgumentOutOfRangeException> (() => typeface.Clone (-1));
		}

// Exact axis value tests — verify interop produces correct values

		[SkippableFact]
		public void DistortableFontHasExactAxisValues ()
		{
			// Distortable.ttf has 1 axis: wght min=0.5 default=1.0 max=2.0
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			var axes = typeface.VariationDesignParameters;
			Assert.Single (axes);

			var axis = axes[0];
			Assert.Equal (SKFourByteTag.Parse ("wght"), axis.Tag);
			Assert.Equal ("wght", axis.Tag.ToString ());
			Assert.Equal (0.5f, axis.Min);
			Assert.Equal (1.0f, axis.Default);
			Assert.Equal (2.0f, axis.Max);
			Assert.False (axis.IsHidden);
		}

		[SkippableFact]
		public void DistortableFontDefaultPositionMatchesAxis ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			var position = typeface.VariationDesignPosition;
			Assert.Single (position);
			Assert.Equal (SKFourByteTag.Parse ("wght"), position[0].Axis);
			Assert.Equal (1.0f, position[0].Value); // default value
		}

		[SkippableFact]
		public void ClonePreservesExactDesignPosition ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			// Clone with min value
			var position = new[] {
				new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse ("wght"), Value = 0.5f }
			};
			using var cloned = typeface.Clone (position);
			Assert.NotNull (cloned);

			var clonedPos = cloned.VariationDesignPosition;
			Assert.Single (clonedPos);
			Assert.Equal (SKFourByteTag.Parse ("wght"), clonedPos[0].Axis);
			Assert.Equal (0.5f, clonedPos[0].Value);
		}

		[SkippableFact]
		public void VariationDesignParameterCountMatchesArray ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			Assert.Equal (1, typeface.VariationDesignParameterCount);
			Assert.Equal (typeface.VariationDesignParameterCount, typeface.VariationDesignParameters.Length);
		}

		[SkippableFact]
		public void VariationDesignPositionCountMatchesArray ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			Assert.Equal (1, typeface.VariationDesignPositionCount);
			Assert.Equal (typeface.VariationDesignPositionCount, typeface.VariationDesignPosition.Length);
		}

		// Interop safety tests — verify struct layout through native round-trip

		[SkippableFact]
		public void VariationAxisStructLayoutMatchesNative ()
		{
			// Verify that SKFontVariationAxis fields are in the correct order
			// by reading known values from Distortable.ttf
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			// Use Span overload to verify the same data comes through
			var axes = new SKFontVariationAxis[1];
			var count = typeface.GetVariationDesignParameters (axes);
			Assert.Equal (1, count);

			// All fields must have survived the P/Invoke
			Assert.Equal (SKFourByteTag.Parse ("wght"), axes[0].Tag);
			Assert.Equal (0.5f, axes[0].Min);
			Assert.Equal (1.0f, axes[0].Default);
			Assert.Equal (2.0f, axes[0].Max);
			Assert.False (axes[0].IsHidden);
		}

		// SKFontArguments tests

		[SkippableFact]
		public void CloneWithFontArguments ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			using var cloned = typeface.Clone (new SKFontArguments {
				VariationDesignPosition = new[] {
					new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse ("wght"), Value = 0.5f }
				}
			});
			Assert.NotNull (cloned);

			var pos = cloned.VariationDesignPosition;
			Assert.Single (pos);
			Assert.Equal (0.5f, pos[0].Value);
		}

		[SkippableFact]
		public void CloneWithFontArgumentsDefaultIsNoOp ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			// Empty args — should clone with defaults
			using var cloned = typeface.Clone (new SKFontArguments ());
			Assert.NotNull (cloned);
		}

		[SkippableFact]
		public void CloneWithFontArgumentsCollectionIndex ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			using var cloned = typeface.Clone (new SKFontArguments {
				CollectionIndex = 0,
				VariationDesignPosition = new[] {
					new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse ("wght"), Value = 2.0f }
				}
			});
			Assert.NotNull (cloned);

			var pos = cloned.VariationDesignPosition;
			Assert.Single (pos);
			Assert.Equal (2.0f, pos[0].Value);
		}

		[SkippableFact]
		public void CloneWithFontArgumentsStackalloc ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "Distortable.ttf"));
			Assert.NotNull (typeface);

			Span<SKFontVariationPositionCoordinate> pos = stackalloc SKFontVariationPositionCoordinate[] {
				new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse ("wght"), Value = 1.5f }
			};

			using var cloned = typeface.Clone (new SKFontArguments {
				VariationDesignPosition = pos
			});
			Assert.NotNull (cloned);

			var result = cloned.VariationDesignPosition;
			Assert.Single (result);
			Assert.Equal (1.5f, result[0].Value);
		}
	
	}
}
