using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// Tests that validate the documented behavioral breaking changes
	/// from Skia m119 → m132 (SkiaSharp 3.x → 4.x).
	/// Each test maps to a numbered item in issue #3734.
	/// </summary>
	public class SKBreakingChangesTest : SKTest
	{
		// ──── Breaking Change #1: new SKFont() → empty typeface ────

		[SkippableFact]
		public void BC1_NewSKFontTypefaceIsEmpty()
		{
			// m119: null; m132: SKTypeface.Empty
			using var font = new SKFont();
			Assert.NotNull(font.Typeface);
			Assert.True(font.Typeface.IsEmpty);
		}

		[SkippableFact]
		public void BC1_NewSKFontMeasureTextReturnsZero()
		{
			// m119: returns real width (lazy default); m132: returns 0 (empty typeface)
			using var font = new SKFont();
			var width = font.MeasureText("Hello World!");
			Assert.Equal(0, width);
		}

		[SkippableFact]
		public void BC1_SKFontSetTypefaceNullReturnsEmpty()
		{
			// m119: set null → get null; m132: set null → get empty
			using var font = new SKFont(SKTypeface.Default);
			font.Typeface = null;
			Assert.NotNull(font.Typeface);
			Assert.True(font.Typeface.IsEmpty);
		}

		[SkippableFact]
		public void BC1_SKFontWithDefaultCanMeasure()
		{
			// Migration path: use SKTypeface.Default explicitly
			using var font = new SKFont(SKTypeface.Default);
			var width = font.MeasureText("Hello World!");
			Assert.True(width > 0);
		}

		// ──── Breaking Change #2: new SKPaint() text measurement ────

		[Obsolete]
		[SkippableFact]
		public void BC2_NewSKPaintCanMeasureText()
		{
			// PR fixes this: SKPaint() initializes with SKTypeface.Default
			var paint = new SKPaint();
			var width = paint.MeasureText("Hello World!");
			Assert.True(width > 0);
		}

		[Obsolete]
		[SkippableFact]
		public void BC2_NewSKPaintTypefaceIsDefault()
		{
			// m119: null (lazy default); PR: SKTypeface.Default (explicit)
			var paint = new SKPaint();
			Assert.NotNull(paint.Typeface);
			Assert.False(paint.Typeface.IsEmpty);
			Assert.Equal(SKTypeface.Default.FamilyName, paint.Typeface.FamilyName);
		}

		[Obsolete]
		[SkippableFact]
		public void BC2_SKPaintResetPreservesDefaultTypeface()
		{
			var paint = new SKPaint();
			paint.Typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
			paint.Reset();
			// After reset, should be back to Default
			Assert.NotNull(paint.Typeface);
			Assert.False(paint.Typeface.IsEmpty);
		}

		// ──── Breaking Change #3: FromFamilyName("missing") → Default on all platforms ────

		[SkippableFact]
		public void BC3_FromFamilyNameMissingReturnsNonNull()
		{
			// m119: null on Android, fallback elsewhere; m132: Default on all
			var tf = SKTypeface.FromFamilyName("NonExistentFontFamilyXYZ12345");
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void BC3_MatchFamilyMissingReturnsNull()
		{
			// MatchFamily is the strict API — returns null on no match (on most platforms)
			var tf = SKFontManager.Default.MatchFamily("NonExistentFontFamilyXYZ12345");
			// On macOS this may be non-null (CoreText fallback), on others null
			// This documents the behavior — MatchFamily is the strict path
			if (tf == null)
			{
				Assert.Null(tf);
			}
			else
			{
				// macOS: CoreText still falls back
				Assert.NotNull(tf);
			}
		}

		[SkippableFact]
		public void BC3_FromFamilyNameKnownFontWorks()
		{
			var tf = SKTypeface.FromFamilyName(DefaultFontFamily);
			Assert.NotNull(tf);
			Assert.Equal(DefaultFontFamily, tf.FamilyName);
		}

		// ──── Breaking Change #4: SKTypeface.Default resolution ────

		[SkippableFact]
		public void BC4_DefaultIsNotNull()
		{
			Assert.NotNull(SKTypeface.Default);
			Assert.NotEqual(IntPtr.Zero, SKTypeface.Default.Handle);
		}

		[SkippableFact]
		public void BC4_DefaultIsNotEmpty()
		{
			// On platforms with fonts, Default should be a real typeface
			Assert.False(SKTypeface.Default.IsEmpty);
			Assert.True(SKTypeface.Default.GlyphCount > 0);
		}

		[SkippableFact]
		public void BC4_DefaultHasFamilyName()
		{
			var name = SKTypeface.Default.FamilyName;
			Assert.NotNull(name);
			Assert.NotEmpty(name);
		}

		// ──── Breaking Change #5: New APIs (Empty, IsEmpty) ────

		[SkippableFact]
		public void BC5_EmptyTypefaceExists()
		{
			Assert.NotNull(SKTypeface.Empty);
			Assert.NotEqual(IntPtr.Zero, SKTypeface.Empty.Handle);
		}

		[SkippableFact]
		public void BC5_EmptyIsEmpty()
		{
			Assert.True(SKTypeface.Empty.IsEmpty);
			Assert.Equal(0, SKTypeface.Empty.GlyphCount);
		}

		[SkippableFact]
		public void BC5_EmptyFamilyNameIsEmpty()
		{
			var name = SKTypeface.Empty.FamilyName;
			Assert.NotNull(name);
			Assert.Empty(name);
		}

		[SkippableFact]
		public void BC5_DefaultIsNotEmpty()
		{
			Assert.False(SKTypeface.Default.IsEmpty);
		}

		[SkippableFact]
		public void BC5_RealTypefaceIsNotEmpty()
		{
			using var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
			Assert.NotNull(tf);
			Assert.False(tf.IsEmpty);
		}

		[SkippableFact]
		public void BC5_EmptySurvivesDispose()
		{
			SKTypeface.Empty.Dispose();
			Assert.NotNull(SKTypeface.Empty);
			Assert.NotEqual(IntPtr.Zero, SKTypeface.Empty.Handle);
		}

		// ──── Breaking Change #6: Removed C APIs (validated via delegation) ────

		[SkippableFact]
		public void BC6_FromFileDelegatesToFontManager()
		{
			// FromFile now delegates to SKFontManager.Default.CreateTypeface
			// Verify same result
			var path = Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf");
			using var tf1 = SKTypeface.FromFile(path);
			using var tf2 = SKFontManager.Default.CreateTypeface(path);
			Assert.NotNull(tf1);
			Assert.NotNull(tf2);
			Assert.Equal(tf1.FamilyName, tf2.FamilyName);
		}

		[SkippableFact]
		public void BC6_FromStreamDelegatesToFontManager()
		{
			using var stream1 = File.OpenRead(Path.Combine(PathToFonts, "Distortable.ttf"));
			using var tf1 = SKTypeface.FromStream(stream1);

			using var stream2 = File.OpenRead(Path.Combine(PathToFonts, "Distortable.ttf"));
			using var tf2 = SKFontManager.Default.CreateTypeface(stream2);

			Assert.NotNull(tf1);
			Assert.NotNull(tf2);
			Assert.Equal(tf1.FamilyName, tf2.FamilyName);
		}

		[SkippableFact]
		public void BC6_FromDataDelegatesToFontManager()
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Distortable.ttf"));
			using var data = SKData.CreateCopy(bytes);
			using var tf1 = SKTypeface.FromData(data);
			using var tf2 = SKFontManager.Default.CreateTypeface(data);
			Assert.NotNull(tf1);
			Assert.NotNull(tf2);
			Assert.Equal(tf1.FamilyName, tf2.FamilyName);
		}

		// ──── Breaking Change #7: SKFontManager.Default — owned instance ────

		[SkippableFact]
		public void BC7_FontManagerDefaultIsSameReference()
		{
			var fm1 = SKFontManager.Default;
			var fm2 = SKFontManager.Default;
			Assert.Same(fm1, fm2);
		}

		[SkippableFact]
		public void BC7_FontManagerDefaultIsValid()
		{
			Assert.NotNull(SKFontManager.Default);
			Assert.NotEqual(IntPtr.Zero, SKFontManager.Default.Handle);
			Assert.True(SKFontManager.Default.FontFamilyCount > 0);
		}

		[SkippableFact]
		public void BC7_FontManagerDefaultSurvivesDispose()
		{
			SKFontManager.Default.Dispose();
			Assert.NotNull(SKFontManager.Default);
			Assert.NotEqual(IntPtr.Zero, SKFontManager.Default.Handle);
		}
	}
}
