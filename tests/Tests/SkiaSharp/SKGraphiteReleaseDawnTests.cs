#if !NETFRAMEWORK
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using SkiaSharp.Tests.Visual;
using Xunit;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// Graphite release-callback tests over Dawn (WebGPU) for the Blazor WASM
	/// host. Reuses the <c>SKWebGpu</c> JS interop that <c>GraphiteDawnRenderer</c>
	/// uses to obtain a WebGPU device and mint textures. Only runs in a browser;
	/// every other host skips.
	///
	/// <para>
	/// Dawn on WASM is a <em>non-yielding</em> context: it cannot submit
	/// synchronously and the context must outlive any in-flight GPU work, so the
	/// context/recorder are created once and kept for the process lifetime (never
	/// disposed). Bring-up is asynchronous; the single-threaded WASM host needs no
	/// GPU gate, and blocking on the async setup would deadlock the JS event loop,
	/// so <see cref="SKGraphiteReleaseTestsBase.RunGuardedAsync"/> simply awaits.
	/// </para>
	/// </summary>
	[Collection(GpuRenderingCollection.Name)]
	public sealed class SKGraphiteReleaseDawnTests : SKGraphiteReleaseTestsBase
	{
		private static SKGraphiteContext s_context;
		private static SKGraphiteRecorder s_recorder;
		private static JSObject s_device;
		private static bool s_ready;

		protected override SKColorType ColorType => SKColorType.Rgba8888;

		protected override bool CanSubmitSync => false;

		protected override string Backend => GpuBackends.GraphiteDawn;

		protected override async Task<GraphiteReleaseHarness> CreateHarnessAsync()
		{
			if (!s_ready)
			{
				var adapter = await SKWebGpu.RequestAdapter()
					?? throw new InvalidOperationException(
						"navigator.gpu.requestAdapter returned null — WebGPU is unavailable in this browser.");
				var device = await SKWebGpu.RequestDevice(adapter)
					?? throw new InvalidOperationException(
						"adapter.requestDevice returned null.");
				s_device = device;

				// Create a real WGPUInstance first: emdawnwebgpu tags each imported
				// object's events with the parent EventSource's InstanceID, so the
				// queue/device must be registered under this instance or
				// EventManager::WaitAny asserts on the first async wait.
				var instanceId = SKWebGpu.CreateInstance();
				if (instanceId == 0)
					throw new InvalidOperationException("Module._wgpuCreateInstance not exported — cannot obtain a real WGPUInstance.");

				var queue = SKWebGpu.GetDeviceQueue(device);
				var queueId = SKWebGpu.RegisterQueue(queue, instanceId);
				var deviceId = SKWebGpu.RegisterDevice(device, instanceId);

				var backendContext = new SKGraphiteDawnBackendContext
				{
					WgpuInstance = (IntPtr)instanceId,
					WgpuDevice = (IntPtr)deviceId,
					WgpuQueue = (IntPtr)queueId,
				};
				s_context = SKGraphiteContext.CreateDawn(backendContext)
					?? throw new InvalidOperationException("SKGraphiteContext.CreateDawn returned null.");
				s_recorder = s_context.CreateRecorder()
					?? throw new InvalidOperationException("SKGraphiteContext.CreateRecorder returned null.");
				s_ready = true;
			}

			return new DawnHarness(s_context, s_recorder, s_device);
		}

		private sealed class DawnHarness : GraphiteReleaseHarness
		{
			private readonly JSObject device;

			public DawnHarness(SKGraphiteContext context, SKGraphiteRecorder recorder, JSObject device)
			{
				Context = context;
				Recorder = recorder;
				this.device = device;
			}

			public override SKGraphiteContext Context { get; }

			public override SKGraphiteRecorder Recorder { get; }

			public override (SKGraphiteBackendTexture texture, IDisposable owner) CreateBackendTexture(int width, int height)
			{
				var texture = SKWebGpu.CreateTexture(device, width, height);
				var textureId = SKWebGpu.RegisterTexture(texture);
				var backendTexture = SKGraphiteBackendTexture.CreateDawn((IntPtr)textureId)
					?? throw new InvalidOperationException("SKGraphiteBackendTexture.CreateDawn returned null.");
				return (backendTexture, new DawnTextureOwner(backendTexture, textureId));
			}

			// The non-yielding Dawn context is process-lived; never disposed here.
			public override void Dispose()
			{
			}

			private sealed class DawnTextureOwner : IDisposable
			{
				private readonly SKGraphiteBackendTexture backendTexture;
				private readonly int textureId;
				private bool disposed;

				public DawnTextureOwner(SKGraphiteBackendTexture backendTexture, int textureId)
				{
					this.backendTexture = backendTexture;
					this.textureId = textureId;
				}

				public void Dispose()
				{
					if (disposed)
						return;
					disposed = true;
					backendTexture.Dispose();
					SKWebGpu.ReleaseTexture(textureId);
				}
			}
		}
	}
}
#endif
