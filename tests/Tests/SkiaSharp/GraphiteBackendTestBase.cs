using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp.Tests.Visual;
using Xunit;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// Shared backend-agnostic Graphite test scenarios. A derived class provides
	/// backend-specific bring-up (context + recorder from a live GPU) and, if the
	/// scenario needs one, a wrappable backend texture. Every test body defined
	/// on this class runs on every backend whose subclass supports it.
	///
	/// <para>
	/// Currently covered:
	///   * <see cref="SKSurface.Create(SKGraphiteRecorder, SKGraphiteBackendTexture, SKColorType, SKColorSpace, SKSurfaceProperties, SKGraphiteReleaseDelegate)"/>
	///     / <see cref="SKImage.FromTexture(SKGraphiteRecorder, SKGraphiteBackendTexture, SKColorType, SKAlphaType, SKColorSpace, SKGraphiteReleaseDelegate)"/>
	///     release-callback lifetime (fires exactly once on dispose, null callback safe);
	///   * shader-error handler installation via <see cref="SKGraphiteContextOptions.ShaderErrorHandler"/>
	///     (delivery + no-spurious-fire; #4555).
	/// </para>
	///
	/// <para>
	/// New scenarios that need the same bring-up (backend context, options,
	/// deferred-work pumping) belong here rather than in new bespoke test files —
	/// see the review discussion on #4586.
	/// </para>
	/// </summary>
	public abstract class GraphiteBackendTestBase : SKTest
	{
		protected const int Width = 64;
		protected const int Height = 64;

		/// <summary>Colour type matching the backend texture the harness creates.</summary>
		protected abstract SKColorType ColorType { get; }

		/// <summary>Some backends (Dawn on WASM) cannot submit synchronously.</summary>
		protected virtual bool CanSubmitSync => true;

		/// <summary>The GPU backend these tests drive.</summary>
		protected abstract string Backend { get; }

		/// <summary>
		/// True when <see cref="CreateHarnessAsync(SKGraphiteContextOptions)"/> honors
		/// the options for each call. False for backends that share a process-wide
		/// context (Dawn on WASM): those cannot install a fresh shader-error handler
		/// per test, so the shader-handler scenario is skipped.
		/// </summary>
		protected virtual bool SupportsPerHarnessOptions => true;

		/// <summary>
		/// Brings up the backend with default options and returns a live harness.
		/// Must not catch a failed bring-up.
		/// </summary>
		protected Task<GraphiteBackendHarness> CreateHarnessAsync() =>
			CreateHarnessAsync(new SKGraphiteContextOptions());

		/// <summary>
		/// Options-aware bring-up. Subclasses on backends that build a fresh Context
		/// per harness (Metal, Vulkan) override this to pass the supplied options into
		/// <see cref="SKGraphiteContext.CreateMetal(SKGraphiteMtlBackendContext, SKGraphiteContextOptions)"/>
		/// / <see cref="SKGraphiteContext.CreateVulkan(SKGraphiteVkBackendContext, SKGraphiteContextOptions)"/>.
		/// Subclasses that share a Context across tests (Dawn on WASM) may leave this
		/// unimplemented and set <see cref="SupportsPerHarnessOptions"/> to false.
		/// </summary>
		protected abstract Task<GraphiteBackendHarness> CreateHarnessAsync(SKGraphiteContextOptions options);

		[Fact]
		public Task SurfaceWrapReleaseProcFiresExactlyOnceOnDispose()
		{
			SkipIfUnsupported();
			return RunGuardedAsync(async () =>
			{
				using var harness = await CreateHarnessAsync();
				var (backendTexture, textureOwner) = harness.CreateBackendTexture(Width, Height);
				try
				{
					var released = 0;
					var surface = SKSurface.Create(
						harness.Recorder, backendTexture, ColorType, null, null,
						() => released++);
					Assert.NotNull(surface);

					surface.Canvas.Clear(SKColors.Red);
					SnapAndSubmit(harness);

					// Alive: the wrapper still holds the texture, callback must not fire.
					Assert.Equal(0, released);

					surface.Dispose();
					PumpToCompletion(harness.Context);

					// Destroyed exactly once => released exactly once.
					Assert.Equal(1, released);
				}
				finally
				{
					textureOwner.Dispose();
				}
			});
		}

		[Fact]
		public Task SurfaceWrapNullReleaseProcDoesNotCrash()
		{
			SkipIfUnsupported();
			return RunGuardedAsync(async () =>
			{
				using var harness = await CreateHarnessAsync();
				var (backendTexture, textureOwner) = harness.CreateBackendTexture(Width, Height);
				try
				{
					using var surface = SKSurface.Create(
						harness.Recorder, backendTexture, ColorType, null, null,
						(SKGraphiteReleaseDelegate)null);
					Assert.NotNull(surface);

					surface.Canvas.Clear(SKColors.Green);
					SnapAndSubmit(harness);
				}
				finally
				{
					PumpToCompletion(harness.Context);
					textureOwner.Dispose();
				}
			});
		}

		[Fact]
		public Task ImageWrapReleaseProcFiresExactlyOnceOnDispose()
		{
			SkipIfUnsupported();
			return RunGuardedAsync(async () =>
			{
				using var harness = await CreateHarnessAsync();
				var (backendTexture, textureOwner) = harness.CreateBackendTexture(Width, Height);
				try
				{
					var released = 0;
					var image = SKImage.FromTexture(
						harness.Recorder, backendTexture, ColorType, SKAlphaType.Premul, null,
						() => released++);
					Assert.NotNull(image);

					Assert.Equal(0, released);

					image.Dispose();
					PumpToCompletion(harness.Context);

					Assert.Equal(1, released);
				}
				finally
				{
					textureOwner.Dispose();
				}
			});
		}

		// End-to-end wiring for SKGraphiteContextOptions.ShaderErrorHandler.
		// Callback argument forwarding is unit-tested in SKGraphiteShaderErrorHandlerTest;
		// deterministic driver-rejection isn't reachable on our CI drivers (Lavapipe,
		// SwiftShader — both accept every SkSL shader Skia emits), so this test proves
		// the *wiring* holds under a real Context: handler installed via the property
		// path, Snap/Insert/Submit round-trip completes without spuriously firing the
		// handler, and Dispose tears down bridge + pinned delegate without crashing.
		// End-to-end firing under a real driver rejection is verified by the iOS
		// simulator integration run documented in the #4555 PR body.
		[Fact]
		public Task ShaderErrorHandlerWiringSurvivesFullRoundTrip()
		{
			Assert.SkipUnless(SupportsPerHarnessOptions,
				$"{Backend} backend uses a process-shared Context; per-test options are not honored.");
			SkipIfUnsupported();
			return RunGuardedAsync(async () =>
			{
				var fired = 0;
				var options = new SKGraphiteContextOptions
				{
					ShaderErrorHandler = (s, e, c) => Interlocked.Increment(ref fired),
				};
				using var harness = await CreateHarnessAsync(options);

				using var surface = SKSurface.Create(harness.Recorder, new SKImageInfo(Width, Height), false, null);
				Assert.NotNull(surface);
				surface.Canvas.Clear(SKColors.Cyan);
				SnapAndSubmit(harness);
				PumpToCompletion(harness.Context);

				// Skia's built-in pipelines compile cleanly on Lavapipe / SwiftShader / a
				// real vendor driver. A spurious fire here means the caps machinery drifted
				// (e.g. #4555-style: caps claim a driver feature the driver actually rejects).
				Assert.Equal(0, fired);
			});
		}

		/// <summary>
		/// Runs the GPU scenario. GPU work is serialized across the whole assembly
		/// by the GPU rendering test collection each derived class joins (xUnit
		/// <c>DisableParallelization</c>), so no in-test lock is needed; the body is
		/// simply awaited (harness creation completes synchronously for the desktop
		/// backends and genuinely awaits on single-threaded WASM for Dawn).
		/// </summary>
		protected Task RunGuardedAsync(Func<Task> body) => body();

		private void SkipIfUnsupported() => GpuPolicy.RequireOrSkip(Backend);

		private void SnapAndSubmit(GraphiteBackendHarness harness)
		{
			using var recording = harness.Recorder.Snap();
			Assert.Equal(SKGraphiteInsertStatus.Success, harness.Context.InsertRecording(recording));
			harness.Context.Submit(new SKGraphiteSubmitInfo { Sync = CanSubmitSync });
		}

		// Drains any deferred GPU work/resource release so a release callback tied
		// to resource cleanup is guaranteed to have fired before we assert.
		private void PumpToCompletion(SKGraphiteContext context)
		{
			context.Submit(new SKGraphiteSubmitInfo { Sync = CanSubmitSync });
			for (var i = 0; i < 100; i++)
				context.CheckAsyncWorkCompletion();
			context.FreeGpuResources();
		}

		/// <summary>
		/// A live Graphite context + recorder plus a way to mint a wrappable
		/// backend texture. Implemented once per backend.
		/// </summary>
		protected abstract class GraphiteBackendHarness : IDisposable
		{
			public abstract SKGraphiteContext Context { get; }

			public abstract SKGraphiteRecorder Recorder { get; }

			/// <summary>
			/// Creates a backend texture to wrap. The returned disposable owns the
			/// whole texture lifetime — both the managed wrapper and the underlying
			/// native/Skia-allocated GPU texture — and is disposed by the test after
			/// the wrapping surface/image and any pending GPU work are gone.
			/// </summary>
			public abstract (SKGraphiteBackendTexture texture, IDisposable owner) CreateBackendTexture(int width, int height);

			public abstract void Dispose();
		}
	}
}
