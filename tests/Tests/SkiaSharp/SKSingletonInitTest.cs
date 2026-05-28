using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
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

		// --- Concurrent init smoke test ---

		[SkippableFact]
		public void ConcurrentSingletonAccessReturnsSameInstance()
		{
			const int threadCount = 32;
			using var barrier = new Barrier(threadCount);
			var results = new SKColorSpace[threadCount];

			Parallel.For(0, threadCount, i =>
			{
				barrier.SignalAndWait();
				results[i] = SKColorSpace.CreateSrgb();
			});

			for (int i = 1; i < threadCount; i++)
				Assert.Same(results[0], results[i]);
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
	}
}
