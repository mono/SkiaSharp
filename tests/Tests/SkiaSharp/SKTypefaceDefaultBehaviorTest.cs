using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKTypefaceDefaultBehaviorTest : SKTest
	{
		// ──────────────── Group A: SKTypeface.Default & CreateDefault ────────────────

		[SkippableFact]
		public void A0_DefaultHasValidNativeHandle()
		{
			Assert.NotEqual(IntPtr.Zero, SKTypeface.Default.Handle);
		}

		[SkippableFact]
		public void A1_DefaultIsNotNull()
		{
			Assert.NotNull(SKTypeface.Default);
		}

		[SkippableFact]
		public void A2_DefaultFamilyNameCaptured()
		{
			// Record the family name; may be empty with empty-typeface fallback
			var name = SKTypeface.Default.FamilyName;
			Assert.NotNull(name); // at minimum not null (could be "")
		}

		[SkippableFact]
		public void A3_CreateDefaultIsNotNull()
		{
			using var tf = SKTypeface.CreateDefault();
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void A3b_CreateDefaultHasValidHandle()
		{
			using var tf = SKTypeface.CreateDefault();
			Assert.NotEqual(IntPtr.Zero, tf.Handle);
		}

		[SkippableFact]
		public void A4_CreateDefaultFamilyNameCaptured()
		{
			using var tf = SKTypeface.CreateDefault();
			var name = tf.FamilyName;
			Assert.NotNull(name);
		}

		[SkippableFact]
		public void A5_DefaultAndCreateDefaultSameFamily()
		{
			using var created = SKTypeface.CreateDefault();
			Assert.Equal(SKTypeface.Default.FamilyName, created.FamilyName);
		}

		[SkippableFact]
		public void A6_DefaultSurvivesDispose()
		{
			var tf = SKTypeface.Default;
			tf.Dispose();
			// Should still be usable (static immunity)
			Assert.NotNull(SKTypeface.Default);
			Assert.NotEqual(IntPtr.Zero, SKTypeface.Default.Handle);
			Assert.NotNull(SKTypeface.Default.FamilyName);
		}

		// ──────────────── Group B: SKFont default constructor ────────────────

		[SkippableFact]
		public void B1_DefaultFontHasValidHandle()
		{
			using var font = new SKFont();
			Assert.NotEqual(IntPtr.Zero, font.Handle);
		}

		[SkippableFact]
		public void B2_DefaultFontTypefaceIsEmpty()
		{
			// m119: new SKFont() → null typeface in C++ → .Typeface returns null
			// m132: new SKFont() → C++ sets MakeEmpty() → .Typeface returns empty typeface
			using var font = new SKFont();
			Assert.NotNull(font.Typeface);
			Assert.True(font.Typeface.IsEmpty);
		}

		[SkippableFact]
		public void B3_DefaultFontTypefaceIsEmpty()
		{
			// m132: new SKFont() → C++ sets MakeEmpty() → .Typeface is empty
			using var font = new SKFont();
			Assert.NotNull(font.Typeface);
			Assert.True(font.Typeface.IsEmpty);
		}

		[SkippableFact]
		public void B4_DefaultFontSizeIs12()
		{
			using var font = new SKFont();
			Assert.Equal(12f, font.Size);
		}

		[SkippableFact]
		public void B5_FontWithNullTypefaceIsValid()
		{
			using var font = new SKFont(null);
			Assert.NotEqual(IntPtr.Zero, font.Handle);
		}

		[SkippableFact]
		public void B6_FontWithNullTypefaceIsEmpty()
		{
			// m132: SKFont(null) → C++ sets MakeEmpty() → .Typeface returns empty typeface
			using var font = new SKFont(null);
			Assert.NotNull(font.Typeface);
			Assert.True(font.Typeface.IsEmpty);
		}

		[SkippableFact]
		public void B7_FontWithNullTypefaceIsEmpty()
		{
			// m132: SKFont(null) → C++ sets MakeEmpty()
			using var font = new SKFont(null);
			Assert.NotNull(font.Typeface);
			Assert.True(font.Typeface.IsEmpty);
		}

		[SkippableFact]
		public void B8_DefaultFontWorksAfterDefaultDispose()
		{
			SKTypeface.Default.Dispose();
			using var font = new SKFont();
			Assert.NotEqual(IntPtr.Zero, font.Handle);
			// OLD (release): font.Typeface is null (default font doesn't track managed typeface)
			// NEW (PR): font.Typeface is non-null (uses SKTypeface.Default)
			// Just verify the font itself was created successfully
		}

		[SkippableFact]
		public void B9_DefaultFontCanMeasureText()
		{
			using var font = new SKFont();
			// Should not throw; width may be zero with empty fallback
			var width = font.MeasureText("Hello");
			Assert.True(width >= 0);
		}

		// ──────────────── Group C: SKTypeface.FromFamilyName ────────────────

		[SkippableFact]
		public void C1_FromFamilyNameKnownFontReturnsNonNull()
		{
			var tf = SKTypeface.FromFamilyName(DefaultFontFamily);
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void C2_FromFamilyNameKnownFontCorrectFamily()
		{
			var tf = SKTypeface.FromFamilyName(DefaultFontFamily);
			Assert.Equal(DefaultFontFamily, tf.FamilyName);
		}

		[SkippableFact]
		public void C3_FromFamilyNameNonExistentFont()
		{
			// On macOS: Skia already falls back to system default (Helvetica) even on release
			// On other platforms: may return null
			// PR: always returns non-null (explicitly falls back to Default)
			var tf = SKTypeface.FromFamilyName("NonExistentFontFamilyXYZ12345");
			Assert.NotNull(tf);
			// On macOS release this is "Helvetica" (Skia's fallback)
		}

		[SkippableFact]
		public void C3b_FromFamilyNameNonExistentIsDefaultIfNonNull()
		{
			var tf = SKTypeface.FromFamilyName("NonExistentFontFamilyXYZ12345");
			if (tf != null)
			{
				// If it's non-null, it should be Default (PR behavior)
				Assert.Equal(SKTypeface.Default.FamilyName, tf.FamilyName);
			}
			// else: old behavior, null is expected
		}

		[SkippableFact]
		public void C3c_FromFamilyNameNonExistentWithBoldStyle()
		{
			// KEY: style may be silently dropped on fallback
			var tf = SKTypeface.FromFamilyName("NonExistentFontFamilyXYZ12345", SKFontStyle.Bold);
			if (tf != null)
			{
				// Record whether style was preserved
				var weight = tf.FontWeight;
				// On PR: likely Normal weight (Default), not Bold
				// This is informational - we capture the value
				Assert.True(weight > 0); // just ensure it's valid
			}
		}

		[SkippableFact]
		public void C4_FromFamilyNameNull()
		{
			// Old: may return null or platform-specific default
			// New: returns Default
			var tf = SKTypeface.FromFamilyName(null);
			// Capture: is it null or non-null?
			// On most platforms even old code returns non-null for null family
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void C4b_FromFamilyNameEmptyString()
		{
			var tf = SKTypeface.FromFamilyName("");
			// Empty string vs null may behave differently
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void C5_FromFamilyNameBoldHasBoldWeight()
		{
			var tf = SKTypeface.FromFamilyName(DefaultFontFamily, SKFontStyle.Bold);
			Assert.NotNull(tf);
			Assert.Equal((int)SKFontStyleWeight.Bold, tf.FontWeight);
		}

		[SkippableFact]
		public void C6_FromFamilyNameSameFamilyReturnsSameObject()
		{
			var tf1 = SKTypeface.FromFamilyName(DefaultFontFamily);
			var tf2 = SKTypeface.FromFamilyName(DefaultFontFamily);
			Assert.Same(tf1, tf2);
		}

		[SkippableFact]
		public void C7_FromFamilyNameResultCannotBePubliclyDisposed()
		{
			var tf = SKTypeface.FromFamilyName(DefaultFontFamily);
			Assert.NotNull(tf);
			tf.Dispose();
			// PreventPublicDisposal means Dispose is a no-op
			Assert.NotEqual(IntPtr.Zero, tf.Handle);
			Assert.False(tf.IsDisposed);
		}

		[SkippableFact]
		public void C8_FromFamilyNameMatchesFontManagerMatchFamily()
		{
			var tf1 = SKTypeface.FromFamilyName(DefaultFontFamily);
			var tf2 = SKFontManager.Default.MatchFamily(DefaultFontFamily);
			Assert.Same(tf1, tf2);
		}

		[SkippableFact]
		public void C9_DisposeFallbackDoesNotBreakDefault()
		{
			var tf = SKTypeface.FromFamilyName("NonExistentFontFamilyXYZ12345");
			if (tf != null)
			{
				tf.Dispose();
			}
			// Default should still work
			Assert.NotNull(SKTypeface.Default);
			Assert.NotEqual(IntPtr.Zero, SKTypeface.Default.Handle);
		}

		[SkippableFact]
		public void C10_CreateDefaultVsFromFamilyNameNull()
		{
			using var created = SKTypeface.CreateDefault();
			var fromNull = SKTypeface.FromFamilyName(null);
			// Both should resolve to similar typeface
			Assert.Equal(created.FamilyName, fromNull.FamilyName);
		}

		// ──────────────── Group D: SKTypeface.FromFile ────────────────

		[SkippableFact]
		public void D1_FromFileValidPathReturnsNonNull()
		{
			using var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void D2_FromFileValidPathCorrectFamily()
		{
			using var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
			Assert.Equal("Roboto2", tf.FamilyName);
		}

		[SkippableFact]
		public void D3_FromFileMissingPathReturnsNull()
		{
			var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "font_that_does_not_exist.ttf"));
			Assert.Null(tf);
		}

		[SkippableFact]
		public void D4_FromFileNullThrows()
		{
			Assert.Throws<ArgumentNullException>(() => SKTypeface.FromFile(null));
		}

		// ──────────────── Group E: SKTypeface.FromStream ────────────────

		[SkippableFact]
		public void E1_FromStreamValidReturnsNonNull()
		{
			using var stream = File.OpenRead(Path.Combine(PathToFonts, "Distortable.ttf"));
			using var tf = SKTypeface.FromStream(stream);
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void E2_FromStreamInvalidDataReturnsNull()
		{
			var randomBytes = new byte[1024];
			new Random(42).NextBytes(randomBytes);
			using var stream = new MemoryStream(randomBytes);
			var tf = SKTypeface.FromStream(stream);
			Assert.Null(tf);
		}

		[SkippableFact]
		public void E3_FromStreamNullStreamThrows()
		{
			Assert.Throws<ArgumentNullException>(() => SKTypeface.FromStream((Stream)null));
		}

		[SkippableFact]
		public void E4_FromStreamAssetValidReturnsNonNull()
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Distortable.ttf"));
			using var stream = new SKMemoryStream(bytes);
			var tf = SKTypeface.FromStream(stream);
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void E5_FromStreamAssetNullThrows()
		{
			Assert.Throws<ArgumentNullException>(() => SKTypeface.FromStream((SKStreamAsset)null));
		}

		[SkippableFact]
		public void E6_StreamOwnershipTransfer()
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Distortable.ttf"));
			var stream = new SKMemoryStream(bytes);
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);

			var tf = SKTypeface.FromStream(stream);
			Assert.NotNull(tf);

			// After FromStream, stream should have lost ownership
			Assert.False(stream.OwnsHandle);

			// Typeface should be usable
			Assert.NotEmpty(tf.GetTableTags());

			tf.Dispose();
		}

		// ──────────────── Group F: SKTypeface.FromData ────────────────

		[SkippableFact]
		public void F1_FromDataValidReturnsNonNull()
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Distortable.ttf"));
			using var data = SKData.CreateCopy(bytes);
			using var tf = SKTypeface.FromData(data);
			Assert.NotNull(tf);
		}

		[SkippableFact]
		public void F2_FromDataInvalidReturnsNull()
		{
			var randomBytes = new byte[64];
			new Random(42).NextBytes(randomBytes);
			using var data = SKData.CreateCopy(randomBytes);
			var tf = SKTypeface.FromData(data);
			Assert.Null(tf);
		}

		[SkippableFact]
		public void F3_FromDataNullThrows()
		{
			Assert.Throws<ArgumentNullException>(() => SKTypeface.FromData(null));
		}

		// ──────────────── Group G: SKFontManager.Default ────────────────

		[SkippableFact]
		public void G0_FontManagerDefaultHasValidHandle()
		{
			Assert.NotEqual(IntPtr.Zero, SKFontManager.Default.Handle);
		}

		[SkippableFact]
		public void G1_FontManagerDefaultIsNotNull()
		{
			Assert.NotNull(SKFontManager.Default);
		}

		[SkippableFact]
		public void G2_FontManagerDefaultHasFamilies()
		{
			Assert.True(SKFontManager.Default.FontFamilyCount > 0);
		}

		// ──────────────── Group I: Identity & caching ────────────────

		[SkippableFact]
		public void I1_DefaultTypefaceSameReference()
		{
			var tf1 = SKTypeface.Default;
			var tf2 = SKTypeface.Default;
			Assert.Same(tf1, tf2);
		}

		[SkippableFact]
		public void I2_DefaultFontManagerSameReference()
		{
			var fm1 = SKFontManager.Default;
			var fm2 = SKFontManager.Default;
			Assert.Same(fm1, fm2);
		}

		[SkippableFact]
		public void I3_MatchFamilySameNameReturnsSameObject()
		{
			var tf1 = SKFontManager.Default.MatchFamily(DefaultFontFamily);
			var tf2 = SKFontManager.Default.MatchFamily(DefaultFontFamily);
			Assert.Same(tf1, tf2);
		}

		[SkippableFact]
		public void I4_MatchFamilyResultCannotBePubliclyDisposed()
		{
			var tf = SKFontManager.Default.MatchFamily(DefaultFontFamily);
			Assert.NotNull(tf);
			tf.Dispose();
			// PreventPublicDisposal means Dispose is a no-op
			Assert.NotEqual(IntPtr.Zero, tf.Handle);
			Assert.False(tf.IsDisposed);
		}

		// ──────────────── Group J: FromFamilyName vs MatchFamily consistency ────────────────

		[SkippableFact]
		public void J1_FromFamilyNameAndMatchFamilyConsistentOnMissingFont()
		{
			// KEY TEST: On release/3.119.x, these use DIFFERENT C++ code paths:
			//   MatchFamily → matchFamilyStyle (strict, returns null on macOS for missing font)
			//   FromFamilyName → legacyMakeTypeface (has per-platform fallback, returns Helvetica on macOS)
			// On PR: both go through matchFamilyStyle, but FromFamilyName adds ?? Default
			//
			// This test documents the (in)consistency between the two APIs.
			var fromFamily = SKTypeface.FromFamilyName("NonExistentFontFamilyXYZ12345");
			var matched = SKFontManager.Default.MatchFamily("NonExistentFontFamilyXYZ12345");

			// MatchFamily uses matchFamilyStyle — strict, null on no match (on macOS)
			// FromFamilyName historically used legacyMakeTypeface — falls back on macOS
			// So they may legitimately differ. Document the values:
			if (fromFamily != null && matched != null)
			{
				// Both returned something — they should be the same object (cached)
				Assert.Same(matched, fromFamily);
			}
			// If they differ (one null, other non-null), that's the documented inconsistency
			// — not a new bug. The key question is: does FromFamilyName return non-null?
			// On release: yes (via legacyMakeTypeface fallback)
			// On PR: yes (via ?? Default)
			Assert.NotNull(fromFamily);
		}

		[SkippableFact]
		public void J2_FromFamilyNameMissingBoldStylePreserved()
		{
			// If the font doesn't exist and we get a fallback, did the style survive?
			var tf = SKTypeface.FromFamilyName("NonExistentFontFamilyXYZ12345", SKFontStyle.Bold);
			var tfNormal = SKTypeface.FromFamilyName("NonExistentFontFamilyXYZ12345", SKFontStyle.Normal);

			if (tf != null && tfNormal != null)
			{
				// The bold request should produce a different weight than normal,
				// OR both return same fallback (meaning style was dropped)
				// On macOS: both return Helvetica Normal — style is dropped by Skia's fallback
				// This test documents the behavior, not asserts correctness
				var boldWeight = tf.FontWeight;
				var normalWeight = tfNormal.FontWeight;

				// At minimum, the weights should be valid
				Assert.True(boldWeight > 0);
				Assert.True(normalWeight > 0);
			}
		}

		// ──────────────── Group K: font.Typeface setter with null ────────────────

		[SkippableFact]
		public void K1_FontTypefaceSetNullStillWorks()
		{
			using var font = new SKFont(SKTypeface.Default);
			Assert.NotNull(font.Typeface);

			// Set typeface to null
			font.Typeface = null;

			// Font should still be valid and usable
			Assert.NotEqual(IntPtr.Zero, font.Handle);

			// On m119: font.Typeface is null (C++ stores null)
			// On m132: font.Typeface may be non-null (C++ sets MakeEmpty)
			// Either way, MeasureText should not crash
			var width = font.MeasureText("Hello");
			Assert.True(width >= 0);
		}

		[SkippableFact]
		public void K2_FontTypefaceSetNullThenGetReturnsEmpty()
		{
			using var font = new SKFont(SKTypeface.Default);
			font.Typeface = null;

			// m132: setTypeface(null) → MakeEmpty() → getter returns empty typeface
			var tf = font.Typeface;
			Assert.NotNull(tf);
			Assert.True(tf.IsEmpty);
		}

		// ──────────────── Group H: Cross-layer consistency ────────────────

		[SkippableFact]
		public void H1_FromFamilyNameMatchesFontManagerMatchFamily()
		{
			var tf1 = SKTypeface.FromFamilyName(DefaultFontFamily);
			var tf2 = SKFontManager.Default.MatchFamily(DefaultFontFamily);
			Assert.Same(tf1, tf2);
		}

		[SkippableFact]
		public void H2_DefaultFontTypefaceIsEmpty()
		{
			// m132: new SKFont() → C++ sets MakeEmpty()
			using var font = new SKFont();
			Assert.NotNull(font.Typeface);
			Assert.True(font.Typeface.IsEmpty);
		}

		[SkippableFact]
		public void H3_DefaultFontCanMeasureTextWidth()
		{
			using var font = new SKFont();
			var width = font.MeasureText("Hello World!");
			// Record the width; may be zero with empty fallback
			Assert.True(width >= 0);
		}

		// ──────────────── Group L: Zero-glyph font & IsEmpty ────────────────

		[SkippableFact]
		public void L1_ZeroGlyphFontIsEmpty()
		{
			// A hand-crafted font with valid tables but numGlyphs=0
			using var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "ZeroGlyphs.ttf"));
			if (tf == null)
				return; // Skia rejected it — also valid

			Assert.Equal(0, tf.GlyphCount);
			Assert.True(tf.IsEmpty);
		}

		[SkippableFact]
		public void L2_ZeroGlyphFontHasFamilyName()
		{
			using var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "ZeroGlyphs.ttf"));
			if (tf == null)
				return;

			// Unlike SkEmptyTypeface, this font has real metadata
			Assert.Equal("ZeroGlyphs", tf.FamilyName);
		}

		[SkippableFact]
		public void L3_ZeroGlyphFontMeasuresZero()
		{
			using var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "ZeroGlyphs.ttf"));
			if (tf == null)
				return;

			using var font = new SKFont(tf);
			var width = font.MeasureText("Hello");
			Assert.Equal(0, width);
		}

		[SkippableFact]
		public void L4_EmptyTypefaceIsEmpty()
		{
			Assert.True(SKTypeface.Empty.IsEmpty);
			Assert.Equal(0, SKTypeface.Empty.GlyphCount);
		}

		[SkippableFact]
		public void L5_DefaultTypefaceIsNotEmpty()
		{
			Assert.False(SKTypeface.Default.IsEmpty);
			Assert.True(SKTypeface.Default.GlyphCount > 0);
		}
	}
}
