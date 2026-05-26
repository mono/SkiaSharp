using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKFontManagerTest : SKTest
	{
		// https://github.com/mono/SkiaSharp/issues/3817
		// When SKFontManager.Default is the FIRST SkiaSharp type touched in a process,
		// the static cctor chain (SKFontManager -> SKObject base -> SKTypeface) re-enters
		// SKFontManager.Default while its cctor is still running, so defaultManager is
		// still null and SKTypeface.cctor throws NullReferenceException, leaving the
		// whole chain in a faulted TypeInitializationException state.
		// Other tests in the suite usually warm static state via a different entry
		// point, so we run the access in an isolated AssemblyLoadContext to get a
		// fresh managed-init state.
		[SkippableFact]
		public void DefaultDoesNotThrowOnFirstAccess()
		{
			var alc = new IsolatedSkiaSharpLoadContext(typeof(SKFontManager).Assembly);
			try
			{
				var asm = alc.LoadFromAssemblyName(typeof(SKFontManager).Assembly.GetName());
				var type = asm.GetType("SkiaSharp.SKFontManager", throwOnError: true);
				var prop = type.GetProperty(nameof(SKFontManager.Default), BindingFlags.Public | BindingFlags.Static);
				Assert.NotNull(prop);

				object value;
				try
				{
					value = prop.GetValue(null);
				}
				catch (TargetInvocationException ex)
				{
					throw ex.InnerException;
				}

				Assert.NotNull(value);
			}
			finally
			{
				alc.Unload();
			}
		}

		private sealed class IsolatedSkiaSharpLoadContext : AssemblyLoadContext
		{
			private readonly AssemblyDependencyResolver _resolver;

			public IsolatedSkiaSharpLoadContext(Assembly hostAssembly)
				: base("SkiaSharp.Issue3817", isCollectible: true)
			{
				_resolver = new AssemblyDependencyResolver(hostAssembly.Location);
			}

			protected override Assembly Load(AssemblyName assemblyName)
			{
				var path = _resolver.ResolveAssemblyToPath(assemblyName);
				return path != null ? LoadFromAssemblyPath(path) : null;
			}

			protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
			{
				var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
				return path != null ? LoadUnmanagedDllFromPath(path) : IntPtr.Zero;
			}
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.MatchCharacter)]
		[SkippableFact]
		public void TestFontManagerMatchCharacter()
		{
			SkipOnPlatform(IsBrowser, "WASM has no system font manager");

			var fonts = SKFontManager.Default;
			var emoji = "🚀";
			var emojiChar = StringUtilities.GetUnicodeCharacterCode(emoji, SKTextEncoding.Utf32);
			using var typeface = fonts.MatchCharacter(emojiChar);
			Assert.NotNull(typeface);

			var familyName = typeface.FamilyName;
			if (typeface.FamilyName.EndsWith("##fallback"))
			{
				using var stream = typeface.OpenStream();
				using var temp = SKTypeface.FromStream(stream);

				familyName = temp.FamilyName;
			}

			Assert.Contains(familyName, UnicodeFontFamilies);
		}

		[SkippableFact]
		public void TestCreateDefault()
		{
			Assert.NotNull(SKFontManager.CreateDefault());
		}

		[SkippableFact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
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
			SkipOnPlatform(IsBrowser, "WASM has no system font manager");

			var fonts = SKFontManager.Default;

			var set = fonts.GetFontStyles(DefaultFontFamily);
			Assert.NotNull(set);

			Assert.True(set.Count > 0);
		}

		[SkippableFact]
		public void TestMatchFamilyStyle()
		{
			SkipOnPlatform(IsBrowser, "WASM has no system font manager");

			var fonts = SKFontManager.Default;

			var tf = fonts.MatchFamily(DefaultFontFamily, SKFontStyle.Bold);
			Assert.NotNull(tf);

			Assert.Equal((int)SKFontStyleWeight.Bold, tf.FontWeight);
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
		public void StreamIsAccessibleFromNativeType()
		{
			SkipOnMono();

			var paint = CreateFont(out var typefaceHandle);

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKTypeface>(typefaceHandle, out _));

			var tf = paint.Typeface;

			Assert.Equal("Roboto2", tf.FamilyName);
			Assert.True(tf.TryGetTableTags(out var tags));
			Assert.NotEmpty(tags);

			SKFont CreateFont(out IntPtr handle)
			{
				var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
				var dotnet = new MemoryStream(bytes);
				var stream = new SKManagedStream(dotnet, true);

				var typeface = SKFontManager.Default.CreateTypeface(stream);
				handle = typeface.Handle;

				return new SKFont
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

		[SkippableFact]
		public unsafe void FromPathReturnsDifferentObject()
		{
			var fonts = SKFontManager.Default;

			var path = Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf");

			using var tf1 = fonts.CreateTypeface(path);
			using var tf2 = fonts.CreateTypeface(path);

			Assert.NotSame(tf1, tf2);
		}

		[SkippableFact]
		public unsafe void FromStreamReturnsDifferentObject()
		{
			var fonts = SKFontManager.Default;

			using var stream1 = File.OpenRead(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
			using var tf1 = fonts.CreateTypeface(stream1);

			using var stream2 = File.OpenRead(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));
			using var tf2 = fonts.CreateTypeface(stream2);

			Assert.NotSame(tf1, tf2);
		}

		[SkippableFact]
		public unsafe void FromDataReturnsDifferentObject()
		{
			var fonts = SKFontManager.Default;

			using var data = SKData.Create(Path.Combine(PathToFonts, "Roboto2-Regular_NoEmbed.ttf"));

			using var tf1 = fonts.CreateTypeface(data);
			using var tf2 = fonts.CreateTypeface(data);

			Assert.NotSame(tf1, tf2);
		}

		[SkippableFact]
		public unsafe void FromFamilyReturnsSameObject()
		{
			var fonts = SKFontManager.Default;

			var tf1 = fonts.MatchFamily(DefaultFontFamily, SKFontStyle.Normal);
			var tf2 = fonts.MatchFamily(DefaultFontFamily, SKFontStyle.Normal);

			Assert.Same(tf1, tf2);
		}

		[SkippableFact]
		public unsafe void FromFamilyDisposeDoesNotDispose()
		{
			SkipOnPlatform(IsBrowser, "WASM has no system font manager");

			var fonts = SKFontManager.Default;

			var tf1 = fonts.MatchFamily(DefaultFontFamily, SKFontStyle.Normal);
			var tf2 = fonts.MatchFamily(DefaultFontFamily, SKFontStyle.Normal);

			Assert.Same(tf1, tf2);

			tf1.Dispose();

			Assert.NotEqual(IntPtr.Zero, tf1.Handle);
			Assert.False(tf1.IsDisposed);
		}

		[SkippableFact]
		public unsafe void TypefaceAndFontManagerReturnsSameObject()
		{
			SkipOnPlatform(IsBrowser, "WASM has no system font manager");

			var fonts = SKFontManager.Default;

			// Use a font family known to exist on the current platform
			var familyName = DefaultFontFamily;

			var tf1 = fonts.MatchFamily(familyName);
			var tf2 = SKTypeface.FromFamilyName(familyName);

			Assert.NotNull(tf1);
			Assert.Same(tf1, tf2);
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
				var fonts = SKFontManager.Default;

				var tf1 = fonts.MatchFamily("Times New Roman", SKFontStyle.Normal);
				var tf2 = fonts.MatchFamily("Times New Roman", SKFontStyle.Normal);
				Assert.Same(tf1, tf2);

				var tf3 = fonts.CreateTypeface(@"C:\Windows\Fonts\times.ttf");
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

		[SkippableFact]
		public void FontManagerDefaultHasValidHandle()
		{
			Assert.NotEqual(IntPtr.Zero, SKFontManager.Default.Handle);
		}
	}
}
