using System;
using Xunit;

namespace SkiaSharp.Tests
{
	// Equivalence / parity coverage for the HandleDictionary "skip global registration?" fast path.
	//
	// GetInstance / GetOrAddObject decide, on EVERY native-object wrap, whether the wrapper type
	// implements ISKSkipObjectRegistration. That decision is a compile-time invariant, but it used to
	// be recomputed with a reflection call (Type.IsAssignableFrom) on every call. It is now cached per
	// type in HandleDictionary.SkipObjectRegistration<T>.Value.
	//
	// These tests are the behaviour-parity proof for that optimization: the cached value must be
	// bit-identical to what the original reflection check returned, for skip AND non-skip types, and
	// the observable GetInstance / GetOrAddObject tracking behaviour must be unchanged. The oracle is
	// the exact original expression, evaluated live here and compared to the cached value.
	public class HandleDictionarySkipRegistrationParityTest : SKTest
	{
		private static readonly Type SkipInterface = typeof (ISKSkipObjectRegistration);

		// The original, pre-optimization predicate — used verbatim as the oracle.
		private static bool ReflectionOracle<T> ()
			where T : SKObject =>
			SkipInterface.IsAssignableFrom (typeof (T));

		private static void AssertParity<T> ()
			where T : SKObject =>
			Assert.Equal (ReflectionOracle<T> (), HandleDictionary.SkipObjectRegistration<T>.Value);

		[Fact]
		public void CachedValueMatchesReflectionForSkipRegistrationTypes ()
		{
			// Types that implement ISKSkipObjectRegistration — oracle is true.
			AssertParity<SKPaint> ();
			AssertParity<SKPath> ();
			AssertParity<SKBitmap> ();
			AssertParity<SKRegion> ();
			AssertParity<SKPictureRecorder> ();
			AssertParity<SKVertices> ();
			AssertParity<SKRoundRect> ();
			AssertParity<SKTextBlob> ();

			// And confirm the oracle really is true for these (guards against a vacuous test).
			Assert.True (HandleDictionary.SkipObjectRegistration<SKPaint>.Value);
			Assert.True (HandleDictionary.SkipObjectRegistration<SKPath>.Value);
		}

		[Fact]
		public void CachedValueMatchesReflectionForTrackedTypes ()
		{
			// Types that do NOT skip registration — oracle is false.
			AssertParity<SKImage> ();
			AssertParity<SKShader> ();
			AssertParity<SKColorSpace> ();
			AssertParity<SKData> ();
			AssertParity<SKColorFilter> ();
			AssertParity<SKImageFilter> ();
			AssertParity<SKSurface> ();
			AssertParity<SKCanvas> ();

			// And confirm the oracle really is false for these.
			Assert.False (HandleDictionary.SkipObjectRegistration<SKImage>.Value);
			Assert.False (HandleDictionary.SkipObjectRegistration<SKShader>.Value);
		}

		[Fact]
		public void TrackedTypeStillDeduplicatesThroughHandleDictionary ()
		{
			// End-to-end behaviour parity for a non-skip type: GetOrAddObject must register the wrapper
			// and hand back the SAME instance on a second lookup, and GetInstance must find it. A wrong
			// cached value (true) for this type would bypass registration and break both.
			var handle = SKHandleDictionaryTestHelpers.NextHandle ();

			var first = HandleDictionary.GetOrAddObject<ParityTrackedObject> (
				handle, owns: false, unrefExisting: false,
				(h, o) => new ParityTrackedObject (h));
			try
			{
				Assert.NotNull (first);

				var second = HandleDictionary.GetOrAddObject<ParityTrackedObject> (
					handle, owns: false, unrefExisting: false,
					(h, o) => new ParityTrackedObject (h));
				Assert.Same (first, second);

				Assert.True (HandleDictionary.GetInstance<ParityTrackedObject> (handle, out var fetched));
				Assert.Same (first, fetched);
			}
			finally
			{
				first.Dispose ();
			}

			Assert.False (HandleDictionary.GetInstance<ParityTrackedObject> (handle, out _));
		}

		[Fact]
		public void SkipRegistrationTypeShortCircuitsGetInstance ()
		{
			// A skip-registration type is never stored, so GetInstance must return false without ever
			// touching the dictionary — this is the branch the cached value gates.
			var handle = SKHandleDictionaryTestHelpers.NextHandle ();
			Assert.False (HandleDictionary.GetInstance<ParitySkippedObject> (handle, out var instance));
			Assert.Null (instance);
		}

		// A non-owning, native-memory-free wrapper that does NOT skip registration.
		private sealed class ParityTrackedObject : SKObject
		{
			public ParityTrackedObject (IntPtr handle)
				: base (handle, owns: false)
			{
			}

			protected override void DisposeNative ()
			{
			}
		}

		// A non-owning wrapper that DOES skip registration.
		private sealed class ParitySkippedObject : SKObject, ISKSkipObjectRegistration
		{
			public ParitySkippedObject (IntPtr handle)
				: base (handle, owns: false)
			{
			}

			protected override void DisposeNative ()
			{
			}
		}
	}
}
