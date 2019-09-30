using System;
using Xunit;
using System.IO;

namespace SkiaSharp.Tests
{
	public class SKFontManagerTest : SKTest
	{
		[Trait(CategoryKey, MatchCharacterCategory)]
		[SkippableFact]
		public void TestFontManagerMatchCharacter()
		{
			var fonts = SKFontManager.Default;
			var emoji = "🚀";
			var emojiChar = StringUtilities.GetUnicodeCharacterCode(emoji, SKTextEncoding.Utf32);
			using (var typeface = fonts.MatchCharacter(emojiChar))
			{
				Assert.NotNull(typeface);
				if (IsLinux)
					Assert.Equal("Symbola", typeface.FamilyName);
				else if (IsMac)
					Assert.Equal("Apple Color Emoji", typeface.FamilyName);
				else if (IsWindows)
					Assert.Contains(typeface.FamilyName, new[] { "Segoe UI Emoji", "Segoe UI Symbol" });
			}
		}

		[SkippableFact]
		public void TestCreateDefault()
		{
			Assert.NotNull(SKFontManager.CreateDefault());
		}

		[SkippableFact]
		public void TestFamilyCount()
		{
			var fonts = SKFontManager.Default;
			Assert.True(fonts.FontFamilyCount > 0);

			var families = fonts.GetFontFamilies();
			Assert.True(families.Length > 0);
			Assert.Equal(fonts.FontFamilyCount, families.Length);
		}

		[SkippableFact]
		public void TestGetFontStyles()
		{
			var fonts = SKFontManager.Default;

			var set = fonts.GetFontStyles(DefaultFontFamily);
			Assert.NotNull(set);

			Assert.True(set.Count > 0);
		}

		[SkippableFact]
		public void TestMatchFamilyStyle()
		{
			var fonts = SKFontManager.Default;

			var tf = fonts.MatchFamily(DefaultFontFamily, SKFontStyle.Bold);
			Assert.NotNull(tf);

			Assert.Equal((int)SKFontStyleWeight.Bold, tf.FontWeight);
		}

		[SkippableFact]
		public void TestMatchTypeface()
		{
			VerifySupportsMatchingTypefaces();

			var fonts = SKFontManager.Default;

			var normal = fonts.MatchFamily(DefaultFontFamily, SKFontStyle.Normal);
			Assert.NotNull(normal);
			Assert.Equal((int)SKFontStyleWeight.Normal, normal.FontWeight);

			var bold = fonts.MatchTypeface(normal, SKFontStyle.Bold);
			Assert.NotNull(bold);
			Assert.Equal((int)SKFontStyleWeight.Bold, bold.FontWeight);

			Assert.Equal(normal.FamilyName, bold.FamilyName);
		}

		[SkippableFact]
		public void TestMatchTypefaceFromStream()
		{
			VerifySupportsMatchingTypefaces();
			VerifySupportsMatchingTypefacesFromStreams();

			var fonts = SKFontManager.Default;

			var typeface = fonts.CreateTypeface(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
			Assert.Equal("Roboto2", typeface.FamilyName);

			var match = fonts.MatchTypeface(typeface, SKFontStyle.Bold);
			Assert.NotNull(match);
		}

		[SkippableFact]
		public void NullWithMissingFile()
		{
			var fonts = SKFontManager.Default;

			Assert.Null(fonts.CreateTypeface(Path.Combine(PathToFonts, "font that doesn't exist.ttf")));
		}

		[SkippableFact]
		public void TestFamilyName()
		{
			var fonts = SKFontManager.Default;

			using (var typeface = fonts.CreateTypeface(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf")))
			{
				Assert.Equal("Roboto2", typeface.FamilyName);
			}
		}


		[SkippableFact]
		public void CanReadNonASCIIFile()
		{
			var fonts = SKFontManager.Default;

			using (var typeface = fonts.CreateTypeface(Path.Combine(PathToFonts, "上田雅美.ttf")))
			{
				Assert.Equal("Roboto2", typeface.FamilyName);
			}
		}

		[SkippableFact]
		public void CanReadData()
		{
			var fonts = SKFontManager.Default;

			var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Distortable.ttf"));
			using (var data = SKData.CreateCopy(bytes))
			using (var typeface = fonts.CreateTypeface(data))
			{
				Assert.NotNull(typeface);
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

				var typeface = SKFontManager.Default.CreateTypeface(stream);
				handle = typeface.Handle;

				return new SKPaint
				{
					Typeface = typeface
				};
			}
		}

		[SkippableFact]
		public void CanReadNonSeekableStream()
		{
			var fonts = SKFontManager.Default;

			using (var stream = File.OpenRead(Path.Combine(PathToFonts, "Distortable.ttf")))
			using (var nonSeekable = new NonSeekableReadOnlyStream(stream))
			using (var typeface = fonts.CreateTypeface(nonSeekable))
			{
				Assert.NotNull(typeface);
			}
		}

		[SkippableFact]
		public void CanGetFontStyles()
		{
			var fonts = SKFontManager.Default;

			Assert.NotNull(fonts.GetFontStyles(0));
		}

		[SkippableFact]
		public void CanDisposeDefault()
		{
			// get the fist
			var fonts = SKFontManager.Default;
			Assert.NotNull(fonts);

			// dispose and make sure that we didn't kill it
			fonts.Dispose();
			fonts = SKFontManager.Default;
			Assert.NotNull(fonts);

			// dispose and make sure that we didn't kill it again
			fonts.Dispose();
			fonts = SKFontManager.Default;
			Assert.NotNull(fonts);
		}
	}
}
