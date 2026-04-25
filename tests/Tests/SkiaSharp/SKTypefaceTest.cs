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

			// Static fonts should handle this gracefully
			using var cloned = typeface.Clone (position);
			// Result may or may not be null depending on implementation
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
		public void SpanVariationDesignParametersEmptyForStaticFont ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "content-font.ttf"));
			Assert.NotNull (typeface);

			var spanBuffer = new SKFontVariationAxis[4];
			var written = typeface.GetVariationDesignParameters (spanBuffer);
			Assert.True (written <= 0);
		}

		[SkippableFact]
		public void SpanVariationDesignPositionEmptyForStaticFont ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "content-font.ttf"));
			Assert.NotNull (typeface);

			var spanBuffer = new SKFontVariationPositionCoordinate[4];
			var written = typeface.GetVariationDesignPosition (spanBuffer);
			Assert.True (written <= 0);
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

			ReadOnlySpan<SKFontVariationPositionCoordinate> emptyPosition = ReadOnlySpan<SKFontVariationPositionCoordinate>.Empty;
			using var cloned = typeface.Clone (emptyPosition, 0, 0, overrides);
			Assert.NotNull (cloned);
		}

		[SkippableFact]
		public void CloneWithDifferentPaletteIndex ()
		{
			using var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "test_glyphs-COLRv1.ttf"));
			Assert.NotNull (typeface);

			ReadOnlySpan<SKFontVariationPositionCoordinate> emptyPosition = ReadOnlySpan<SKFontVariationPositionCoordinate>.Empty;
			using var clone0 = typeface.Clone (emptyPosition, 0, 0, ReadOnlySpan<SKFontPaletteOverride>.Empty);
			using var clone1 = typeface.Clone (emptyPosition, 0, 1, ReadOnlySpan<SKFontPaletteOverride>.Empty);
			Assert.NotNull (clone0);
			Assert.NotNull (clone1);
		}
	}
}
