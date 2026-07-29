using System;
using System.Threading.Tasks;
using SkiaSharp.Tests.Visual;
using Xunit;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// Shared behaviour tests for the Graphite wrap-backend-texture release
	/// callbacks (<see cref="SKSurface.Create(SKGraphiteRecorder, SKGraphiteBackendTexture, SKColorType, SKColorSpace, SKSurfaceProperties, SKGraphiteReleaseDelegate)"/>
	/// and <see cref="SKImage.FromTexture(SKGraphiteRecorder, SKGraphiteBackendTexture, SKColorType, SKAlphaType, SKColorSpace, SKGraphiteReleaseDelegate)"/>).
	///
	/// <para>
	/// The test bodies are backend-agnostic; a derived class provides the two
	/// backend-specific pieces — bringing up a context/recorder
	/// (<see cref="CreateHarnessAsync"/>) and handing back a wrappable backend
	/// texture (<see cref="GraphiteReleaseHarness.CreateBackendTexture"/>). One
	/// derived class per Graphite backend (Metal, Vulkan, Dawn) runs the same
	/// assertions on the hosts that can drive it.
	/// </para>
	/// </summary>
	public abstract class SKGraphiteReleaseTestsBase : SKTest
	{
		protected const int Width = 64;
		protected const int Height = 64;

		/// <summary>Colour type matching the backend texture the harness creates.</summary>
		protected abstract SKColorType ColorType { get; }

		/// <summary>Some backends (Dawn on WASM) cannot submit synchronously.</summary>
		protected virtual bool CanSubmitSync => true;

		/// <summary>
		/// The GPU backend these tests drive. Whether it runs on the current host
		/// is decided centrally by <see cref="GpuPolicy"/>, so a derived class
		/// never gates itself on the platform.
		/// </summary>
		protected abstract string Backend { get; }

		/// <summary>
		/// Brings up the backend and returns a live harness. Called only when the
		/// backend is required on this host, so it must <b>not</b> catch a failed
		/// bring-up: a missing device or driver is a red test, not a skip.
		/// </summary>
		protected abstract Task<GraphiteReleaseHarness> CreateHarnessAsync();

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

		/// <summary>
		/// Runs the GPU scenario. GPU work is serialized across the whole assembly
		/// by the GPU rendering test collection each derived class joins (xUnit
		/// <c>DisableParallelization</c>), so no in-test lock is needed; the body is
		/// simply awaited (harness creation completes synchronously for the desktop
		/// backends and genuinely awaits on single-threaded WASM for Dawn).
		/// </summary>
		protected Task RunGuardedAsync(Func<Task> body) => body();

		private void SkipIfUnsupported() => GpuPolicy.RequireOrSkip(Backend);

		private void SnapAndSubmit(GraphiteReleaseHarness harness)
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
		protected abstract class GraphiteReleaseHarness : IDisposable
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
