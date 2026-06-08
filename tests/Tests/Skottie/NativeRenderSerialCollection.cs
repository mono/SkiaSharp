using System;
using Xunit;

namespace SkiaSharp.Tests
{
// Containment for a pre-existing, intermittent native crash in the test harness.
//
// Symptom: on the net48 / x86 CI leg only, the suite intermittently (~1 in 5 runs) throws an
// AccessViolationException from inside the native Skottie build/render call
// (skottie_animation_builder_make_from_data). The victim is ALWAYS a Skottie native-render
// test (the base64 tests, which decode embedded PNG/JPEG via a native resource provider, and
// the render tests, which lay out and rasterize text). The test classes running concurrently
// at crash time VARY from build to build, and the identical crash reproduces on `main` — which
// does not contain this PR's singleton rework — so this is a pre-existing test-harness
// concurrency hazard, not a product regression introduced here.
//
// What the evidence supports (and what it does not): the net48 leg runs every test class
// concurrently in one process (xUnit Parallelism=All, NoAppDomain=true). The Skottie native
// build/render path is the heaviest single-call user of process-global native Skia state
// (image codecs, colour management, the default font manager and its glyph caches). The
// consistent Skottie victim with varying neighbours points to a Skottie-specific native path
// that is sensitive to the concurrent native allocation/free churn from the rest of the suite,
// rather than a permanently-corrupted global (which would crash unrelated tests at random too).
// We have NOT root-caused the native fault itself; it does not reproduce on the dev platform
// (macOS arm64 netcore) even under Guard Malloc across 40 full-suite and 25 targeted runs.
//
// Containment: placing the Skottie native-render classes in a DisableParallelization collection
// runs them sequentially in xUnit's non-parallel phase, after the parallel phase has drained,
// so the decode/render no longer overlaps the rest of the suite's native churn. The shared
// fixture additionally forces a GC + finalizer drain before the first Skottie test, so native
// objects finalized from the parallel phase are torn down before the serialized render begins
// rather than racing it. This is a test-harness isolation fix, not a product thread-safety
// guarantee. Coverage is unchanged: every Skottie assertion still runs.
public sealed class NativeRenderSerialFixture
{
public NativeRenderSerialFixture ()
{
// Drain anything finalizable left over from the parallel phase so the serialized
// Skottie render starts against a quiescent native heap.
GC.Collect ();
GC.WaitForPendingFinalizers ();
GC.Collect ();
}
}

[CollectionDefinition (NativeRenderSerialCollection.Name, DisableParallelization = true)]
public sealed class NativeRenderSerialCollection : ICollectionFixture<NativeRenderSerialFixture>
{
public const string Name = "Native render (serialized)";
}
}
