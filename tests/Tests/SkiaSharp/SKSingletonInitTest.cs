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

		// --- Process-global singletons are latched immortal ---

		// IgnorePublicDispose alone only guards the PUBLIC Dispose() entry point. The lifecycle rework
		// made singletons normal owns:true wrappers and moved the isDisposed CAS out of Dispose(bool)
		// into the three entry points, so DisposeInternal() and the finalizer — neither of which consults
		// IgnorePublicDispose — could still free the shared native singleton. The immortal latch closes
		// those paths. These tests assert every singleton sets it.

		[SkippableFact]
		public void AllSingletonsAreImmortal()
		{
			Assert.True(SKColorSpace.CreateSrgb().IsImmortalSingleton);
			Assert.True(SKColorSpace.CreateSrgbLinear().IsImmortalSingleton);
			Assert.True(SKData.Empty.IsImmortalSingleton);
			Assert.True(SKFontManager.Default.IsImmortalSingleton);
			Assert.True(SKTypeface.Default.IsImmortalSingleton);
			Assert.True(SKTypeface.Empty.IsImmortalSingleton);
			Assert.True(SKBlender.CreateBlendMode(SKBlendMode.SrcOver).IsImmortalSingleton);
			Assert.True(SKColorFilter.CreateSrgbToLinearGamma().IsImmortalSingleton);
			Assert.True(SKColorFilter.CreateLinearToSrgbGamma().IsImmortalSingleton);
			Assert.True(SKFontStyle.Normal.IsImmortalSingleton);
			Assert.True(SKFontStyle.Bold.IsImmortalSingleton);
			Assert.True(SKFontStyle.Italic.IsImmortalSingleton);
			Assert.True(SKFontStyle.BoldItalic.IsImmortalSingleton);
		}

		// DisposeInternal() is THE path the rework reopened (owned-child teardown, ownership handoff and
		// dict replacement all funnel through it). On a real process-global singleton it must be a no-op:
		// isDisposed stays false, the handle is unchanged, and the singleton accessor keeps returning the
		// same live instance. Without the immortal guard this CAS+Dispose(true) would unref/free the
		// shared native object and corrupt every other consumer of it.

		[SkippableFact]
		public void DisposeInternalOnRefCountedSingletonIsNoOp()
		{
			var srgb = SKColorSpace.CreateSrgb();
			var handleBefore = srgb.Handle;
			var refCountBefore = srgb.GetReferenceCount();

			srgb.DisposeInternal();

			Assert.False(srgb.IsDisposed);
			Assert.Equal(handleBefore, srgb.Handle);
			Assert.Equal(refCountBefore, srgb.GetReferenceCount());
			Assert.Same(srgb, SKColorSpace.CreateSrgb());
			Assert.Equal(handleBefore, SKColorSpace.CreateSrgb().Handle);
		}

		[SkippableFact]
		public void DisposeInternalOnNonVirtualRefCountedSingletonIsNoOp()
		{
			// SKData is ISKNonVirtualReferenceCounted — a different unref path than SKColorSpace — so
			// cover SKData.Empty independently.
			var empty = SKData.Empty;
			var handleBefore = empty.Handle;

			empty.DisposeInternal();

			Assert.False(empty.IsDisposed);
			Assert.Equal(handleBefore, empty.Handle);
			Assert.Same(empty, SKData.Empty);
			Assert.Equal(handleBefore, SKData.Empty.Handle);
		}

		[SkippableFact]
		public void DisposeInternalOnDefaultTypefaceSingletonIsNoOp()
		{
			// The default typeface handle can ALSO be reached mortally via SKFontManager.MatchFamily,
			// so its wrapper is the promote-to-immortal case. Guard it explicitly.
			var typeface = SKTypeface.Default;
			var handleBefore = typeface.Handle;

			typeface.DisposeInternal();

			Assert.False(typeface.IsDisposed);
			Assert.Equal(handleBefore, typeface.Handle);
			Assert.Same(typeface, SKTypeface.Default);
		}

		// --- Re-running the init chain after a (simulated) partial-init failure is idempotent ---

		// SkiaSharpStatics resets its state to Uninitialized when any InitializeStatics throws, so a later
		// touch re-runs the WHOLE chain. Every InitializeStatics must therefore be idempotent: it must
		// reuse the wrappers it already created and never construct a second immortal wrapper. This matters
		// most for two types:
		//   * SKFontStyle bypasses the HandleDictionary dedup (ISKSkipObjectRegistration), so a second
		//     construction would leak four immortal native font styles whose finalizers never run.
		//   * SKBlender roots its immortal blenders only through its static dictionary, so rebuilding the
		//     dictionary would drop the sole strong references to the previously-created wrappers.
		// This test forces a re-run by resetting the orchestrator's private state field and asserts every
		// singleton is the SAME managed instance with the SAME native handle afterwards.
		[SkippableFact]
		public void ReinitializingStaticsReusesSameSingletonInstances()
		{
			var srgb = SKColorSpace.CreateSrgb();
			var srgbLinear = SKColorSpace.CreateSrgbLinear();
			var data = SKData.Empty;
			var srgbToLinear = SKColorFilter.CreateSrgbToLinearGamma();
			var linearToSrgb = SKColorFilter.CreateLinearToSrgbGamma();
			var fontManager = SKFontManager.Default;
			var defaultTypeface = SKTypeface.Default;
			var emptyTypeface = SKTypeface.Empty;
			var normalStyle = SKFontStyle.Normal;
			var blender = SKBlender.CreateBlendMode(SKBlendMode.SrcOver);

			var staticsType = typeof(SKColorSpace).Assembly.GetType("SkiaSharp.SkiaSharpStatics", throwOnError: true);
			var stateField = staticsType.GetField("state", BindingFlags.NonPublic | BindingFlags.Static);
			var ensure = staticsType.GetMethod("EnsureInitialized", BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(stateField);
			Assert.NotNull(ensure);

			// Reset to Uninitialized (0) and re-run the whole init chain.
			stateField.SetValue(null, 0);
			ensure.Invoke(null, null);

			Assert.Same(srgb, SKColorSpace.CreateSrgb());
			Assert.Same(srgbLinear, SKColorSpace.CreateSrgbLinear());
			Assert.Same(data, SKData.Empty);
			Assert.Same(srgbToLinear, SKColorFilter.CreateSrgbToLinearGamma());
			Assert.Same(linearToSrgb, SKColorFilter.CreateLinearToSrgbGamma());
			Assert.Same(fontManager, SKFontManager.Default);
			Assert.Same(defaultTypeface, SKTypeface.Default);
			Assert.Same(emptyTypeface, SKTypeface.Empty);
			Assert.Same(normalStyle, SKFontStyle.Normal);
			Assert.Same(blender, SKBlender.CreateBlendMode(SKBlendMode.SrcOver));

			// Still alive and immortal after the re-run.
			Assert.False(srgb.IsDisposed);
			Assert.False(data.IsDisposed);
			Assert.False(defaultTypeface.IsDisposed);
			Assert.True(srgb.IsImmortalSingleton);
			Assert.True(blender.IsImmortalSingleton);
			Assert.True(normalStyle.IsImmortalSingleton);
		}

		// Targeted coverage for SKBlender's incremental-fill idempotency (the dictionary is rooted up-front
		// and only missing modes are (re)created). Simulate a partial-init failure by reflectively
		// removing ONE mode and clearing the completion latch, then re-run: the removed mode is recreated
		// while every other blender stays the exact same immortal instance (no wholesale rebuild that
		// would drop the roots of the already-created wrappers and leak their native refs).
		[SkippableFact]
		public void ReinitializingBlendersAfterPartialResetFillsOnlyMissingModes()
		{
			var srcOverBefore = SKBlender.CreateBlendMode(SKBlendMode.SrcOver);
			var screenBefore = SKBlender.CreateBlendMode(SKBlendMode.Screen);

			var blenderType = typeof(SKBlender);
			var dictField = blenderType.GetField("blendModeBlenders", BindingFlags.NonPublic | BindingFlags.Static);
			var doneField = blenderType.GetField("blendModeBlendersInitialized", BindingFlags.NonPublic | BindingFlags.Static);
			Assert.NotNull(dictField);
			Assert.NotNull(doneField);

			var dict = dictField.GetValue(null);
			var remove = dict.GetType().GetMethod("Remove", new[] { typeof(SKBlendMode) });
			Assert.NotNull(remove);

			// Evict SrcOver and clear the latch so the next init re-creates only that mode.
			remove.Invoke(dict, new object[] { SKBlendMode.SrcOver });
			doneField.SetValue(null, false);

			var staticsType = typeof(SKColorSpace).Assembly.GetType("SkiaSharp.SkiaSharpStatics", throwOnError: true);
			staticsType.GetField("state", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, 0);
			staticsType.GetMethod("EnsureInitialized", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);

			// Screen was never evicted -> same instance (proves we don't rebuild the whole dictionary).
			var screenAfter = SKBlender.CreateBlendMode(SKBlendMode.Screen);
			Assert.Same(screenBefore, screenAfter);

			// SrcOver was evicted -> recreated as a fresh immortal, and stable on subsequent calls.
			var srcOverAfter = SKBlender.CreateBlendMode(SKBlendMode.SrcOver);
			Assert.NotNull(srcOverAfter);
			Assert.True(srcOverAfter.IsImmortalSingleton);
			Assert.Same(srcOverAfter, SKBlender.CreateBlendMode(SKBlendMode.SrcOver));
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
