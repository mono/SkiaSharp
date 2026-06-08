using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
#if !NETFRAMEWORK
using System.Runtime.Loader;
#endif
using Xunit;

namespace SkiaSharp.Tests
{
	// Serialized: SameColorSpaceCreatedDifferentWaysAreTheSameObject asserts the EXACT native refcount
	// of the process-wide srgb-linear singleton, which any parallel test creating/decoding into
	// srgb-linear would transiently perturb. Running in the DisableParallelization phase removes that
	// interference. The remaining identity/flag/cold-start tests here are race-immune and microsecond-fast,
	// so serializing the whole (small) class costs nothing meaningful.
	[Collection (HandleDictionaryThreadingCollection.Name)]
	public class SKSingletonInitTest : SKTest
	{
		// --- Singleton identity + dispose-protected flag ---

		[SkippableFact]
		public void SKColorSpaceSrgbReturnsSameInstanceAndIsDisposeProtected()
		{
			var a = SKColorSpace.CreateSrgb();
			var b = SKColorSpace.CreateSrgb();
			Assert.Same(a, b);
			Assert.True(a.IgnorePublicDispose);
		}

		[SkippableFact]
		public void SKColorSpaceSrgbLinearReturnsSameInstanceAndIsDisposeProtected()
		{
			var a = SKColorSpace.CreateSrgbLinear();
			var b = SKColorSpace.CreateSrgbLinear();
			Assert.Same(a, b);
			Assert.True(a.IgnorePublicDispose);
		}

		[SkippableFact]
		public void SameColorSpaceCreatedDifferentWaysAreTheSameObject()
		{
			// Drain pending finalizers first so a srgb-linear holder created during the earlier parallel
			// phase cannot decrement the native refcount below baseline mid-test. Combined with the
			// serialized collection (no concurrent test increments), the count is then deterministic.
			CollectGarbage();

			// get the first instance of the sRGB Linear and capture the steady-state refcount
			var colorspace1 = SKColorSpace.CreateSrgbLinear();
			Assert.True(colorspace1.IgnorePublicDispose);
			var baselineRefCount = colorspace1.GetReferenceCount();

			// create a new one with the same parameters, which will return the same instance
			var colorspace2 = SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Linear, SKColorSpaceXyz.Srgb);
			Assert.True(colorspace2.IgnorePublicDispose);
			Assert.Same(colorspace1, colorspace2);
			Assert.Equal(baselineRefCount, colorspace1.GetReferenceCount());
			Assert.Equal(baselineRefCount, colorspace2.GetReferenceCount());

			// create a different one manually, which will return a new instance
			var colorspace3 = SKColorSpace.CreateRgb(
				new SKColorSpaceTransferFn { A = 0.6f, B = 0.5f, C = 0.4f, D = 0.3f, E = 0.2f, F = 0.1f },
				SKColorSpaceXyz.Identity);
			Assert.NotSame(colorspace1, colorspace3);
			Assert.Equal(baselineRefCount, colorspace1.GetReferenceCount());
			Assert.Equal(baselineRefCount, colorspace2.GetReferenceCount());

			colorspace3.Dispose();
			Assert.True(colorspace3.IsDisposed);
			Assert.Equal(baselineRefCount, colorspace1.GetReferenceCount());

			colorspace2.Dispose();
			Assert.False(colorspace2.IsDisposed);
			Assert.Equal(baselineRefCount, colorspace1.GetReferenceCount());

			colorspace1.Dispose();
			Assert.False(colorspace1.IsDisposed);
			Assert.Equal(baselineRefCount, colorspace1.GetReferenceCount());
		}

		[SkippableFact]
		public void SKDataEmptyReturnsSameInstanceAndIsDisposeProtected()
		{
			var a = SKData.Empty;
			var b = SKData.Empty;
			Assert.Same(a, b);
			Assert.True(a.IgnorePublicDispose);
		}

		[SkippableFact]
		public void SKFontManagerDefaultReturnsSameInstanceAndIsDisposeProtected()
		{
			var a = SKFontManager.Default;
			var b = SKFontManager.Default;
			Assert.Same(a, b);
			Assert.True(a.IgnorePublicDispose);
		}

		[SkippableFact]
		public void SKTypefaceDefaultReturnsSameInstanceAndIsDisposeProtected()
		{
			var a = SKTypeface.Default;
			var b = SKTypeface.Default;
			Assert.Same(a, b);
			Assert.True(a.IgnorePublicDispose);
		}

		[SkippableFact]
		public void SKTypefaceEmptyReturnsSameInstanceAndIsDisposeProtected()
		{
			var a = SKTypeface.Empty;
			var b = SKTypeface.Empty;
			Assert.Same(a, b);
			Assert.True(a.IgnorePublicDispose);
		}

		[SkippableFact]
		public void SKBlenderForModeReturnsSameInstanceAndIsDisposeProtected()
		{
			var a = SKBlender.CreateBlendMode(SKBlendMode.SrcOver);
			var b = SKBlender.CreateBlendMode(SKBlendMode.SrcOver);
			Assert.Same(a, b);
			Assert.True(a.IgnorePublicDispose);
		}

		[SkippableFact]
		public void SKColorFilterSrgbToLinearGammaReturnsSameInstanceAndIsDisposeProtected()
		{
			var a = SKColorFilter.CreateSrgbToLinearGamma();
			var b = SKColorFilter.CreateSrgbToLinearGamma();
			Assert.Same(a, b);
			Assert.True(a.IgnorePublicDispose);
		}

		[SkippableFact]
		public void SKColorFilterLinearToSrgbGammaReturnsSameInstanceAndIsDisposeProtected()
		{
			var a = SKColorFilter.CreateLinearToSrgbGamma();
			var b = SKColorFilter.CreateLinearToSrgbGamma();
			Assert.Same(a, b);
			Assert.True(a.IgnorePublicDispose);
		}

		[SkippableFact]
		public void SKFontStyleStaticsAreDisposeProtected()
		{
			Assert.True(SKFontStyle.Normal.IgnorePublicDispose);
			Assert.True(SKFontStyle.Bold.IgnorePublicDispose);
			Assert.True(SKFontStyle.Italic.IgnorePublicDispose);
			Assert.True(SKFontStyle.BoldItalic.IgnorePublicDispose);
		}

		// --- Dispose() is a no-op on dispose-protected wrappers ---

		[SkippableFact]
		public void DisposeOnDisposeProtectedSingletonIsNoOp()
		{
			var srgb = SKColorSpace.CreateSrgb();
			var handleBefore = srgb.Handle;

			srgb.Dispose();

			Assert.False(srgb.IsDisposed);
			Assert.Equal(handleBefore, SKColorSpace.CreateSrgb().Handle);
			Assert.Same(srgb, SKColorSpace.CreateSrgb());
		}

		// --- SKTypeface.CreateDefault never returns null even on a cold backing field ---

		[SkippableFact]
		public void SKTypefaceCreateDefaultIsNotNull()
		{
			// Regression net for an earlier draft of this PR where CreateDefault
			// returned the (now lazy-initialized) `empty` backing field on match
			// failure rather than the Empty property — so the call could observe
			// null if Empty hadn't been accessed yet.
			Assert.NotNull(SKTypeface.CreateDefault());
		}

		// --- SKPaint construction touches the DefaultFont lazy initializer ---

		[SkippableFact]
		public void SKPaintConstructionDoesNotThrow()
		{
			// Smoke test: exercises the SKPaint -> DefaultFont path. The factory
			// inside DefaultFont's LazyInitializer references SKTypeface.Default,
			// SKFont.DefaultSize, SKFont.DefaultScaleX, SKFont.DefaultSkewX —
			// any compile-time typo in those identifiers would fail to build,
			// any runtime cycle would throw here.
			using var paint = new SKPaint();
			Assert.NotNull(paint);
		}

#if !NETFRAMEWORK
		// --- #3817 cold-start cctor cycle (best-effort) ---

		// When SKFontManager.Default is the FIRST SkiaSharp managed type touched
		// in a freshly loaded SkiaSharp assembly, the pre-fix cctor chain
		// (SKFontManager -> SKObject base -> SKTypeface) re-entered
		// SKFontManager.Default while its cctor was still running, leaving
		// defaultManager null and throwing TypeInitializationException out of
		// the chain.
		//
		// This test exercises the path via an isolated AssemblyLoadContext so
		// SkiaSharp's cctors run from cold. The result is reliable against the
		// fix that landed in this PR; if a future refactor re-introduces a
		// similar cycle, that runtime would land here and throw.
		//
		// Caveat: cctor ordering inside the isolated assembly depends on the
		// runtime's choice of which dependent cctor to trigger first. We
		// minimize that by accessing SKFontManager.Default *via reflection*
		// and not touching any other SkiaSharp type from inside the ALC.
		[SkippableFact]
		public void Issue3817_SKFontManagerDefaultDoesNotThrowFromColdStart()
		{
			SkipOnPlatform(IsBrowser || IsAndroid || IsIOS || IsMacCatalyst, "AssemblyDependencyResolver is not supported on this platform (browser WASM, Android, iOS, Mac Catalyst); cold-start ALC isolation is a desktop-only technique.");

			var alc = new IsolatedSkiaSharpLoadContext(typeof(SKFontManager).Assembly);
			try
			{
				var asm = alc.LoadFromAssemblyName(typeof(SKFontManager).Assembly.GetName());
				var fmType = asm.GetType(typeof(SKFontManager).FullName, throwOnError: true);
				var defaultProp = fmType.GetProperty(nameof(SKFontManager.Default), BindingFlags.Public | BindingFlags.Static);
				Assert.NotNull(defaultProp);

				object value;
				try
				{
					value = defaultProp.GetValue(null);
				}
				catch (TargetInvocationException ex)
				{
					// Surface the real cold-start failure (the Issue #3817 cctor
					// cycle) with its original stack intact, rather than throw
					// ex.InnerException; which resets the stack and NREs when
					// InnerException is null.
					ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
					throw; // unreachable; satisfies definite-assignment of value
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
#endif
	}
}
