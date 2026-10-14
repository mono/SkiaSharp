using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SkiaSharp.Tests.Visual;
using Xunit;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// <see cref="GraphiteBackendTestBase"/> harness over Apple Metal — the one backend
	/// a dev host can drive without a swapchain. Creates a real <c>MTLTexture</c> via
	/// the Objective-C runtime and wraps it. (The Metal objc here is duplicated
	/// with <c>GraphiteMetalRenderer</c> for now; a shared Metal test vehicle is a
	/// later cleanup.)
	/// </summary>
	[Collection(GpuRenderingCollection.Name)]
	public sealed class GraphiteMetalBackendTests : GraphiteBackendTestBase
	{
		protected override SKColorType ColorType => SKColorType.Bgra8888;

		protected override string Backend => GpuBackends.GraphiteMetal;

		protected override Task<GraphiteBackendHarness> CreateHarnessAsync(SKGraphiteContextOptions options) =>
			Task.FromResult(CreateHarness(options));

		private GraphiteBackendHarness CreateHarness(SKGraphiteContextOptions options)
		{
			var device = MTLCreateSystemDefaultDevice();
			if (device == IntPtr.Zero)
				throw new InvalidOperationException(
					"MTLCreateSystemDefaultDevice returned null; no Metal device on this host.");
			if (!MetalCanDriveGraphite(device))
			{
				ObjcRelease(device);
				throw new InvalidOperationException(
					"MTLDevice does not support a Skia-Graphite MTLGPUFamily (Apple7+, Mac2). " +
					"This is usually a virtualized/software Metal driver.");
			}

			var queue = ObjcSend(device, "newCommandQueue");
			if (queue == IntPtr.Zero)
			{
				ObjcRelease(device);
				throw new InvalidOperationException("[MTLDevice newCommandQueue] returned null.");
			}

			var backendContext = new SKGraphiteMtlBackendContext { MtlDevice = device, MtlQueue = queue };
			var context = SKGraphiteContext.CreateMetal(backendContext, options)
				?? throw new InvalidOperationException("SKGraphiteContext.CreateMetal returned null.");
			var recorder = context.CreateRecorder()
				?? throw new InvalidOperationException("SKGraphiteContext.CreateRecorder returned null.");

			return new MetalHarness(device, queue, context, recorder);
		}

		private sealed class MetalHarness : GraphiteBackendHarness
		{
			private readonly IntPtr device;
			private readonly IntPtr queue;
			private readonly SKGraphiteContext context;
			private readonly SKGraphiteRecorder recorder;

			public MetalHarness(IntPtr device, IntPtr queue, SKGraphiteContext context, SKGraphiteRecorder recorder)
			{
				this.device = device;
				this.queue = queue;
				this.context = context;
				this.recorder = recorder;
			}

			public override SKGraphiteContext Context => context;

			public override SKGraphiteRecorder Recorder => recorder;

			public override (SKGraphiteBackendTexture texture, IDisposable owner) CreateBackendTexture(int width, int height)
			{
				var mtlTexture = CreateMetalTexture(device, width, height);
				if (mtlTexture == IntPtr.Zero)
					throw new InvalidOperationException("[MTLDevice newTextureWithDescriptor:] returned null.");

				var backendTexture = SKGraphiteBackendTexture.CreateMetal(width, height, mtlTexture)
					?? throw new InvalidOperationException("SKGraphiteBackendTexture.CreateMetal returned null.");

				return (backendTexture, new MetalTextureOwner(backendTexture, mtlTexture));
			}

			public override void Dispose()
			{
				recorder.Dispose();
				context.Dispose();
				ObjcRelease(queue);
				ObjcRelease(device);
			}

			private sealed class MetalTextureOwner : IDisposable
			{
				private readonly SKGraphiteBackendTexture backendTexture;
				private IntPtr mtlTexture;

				public MetalTextureOwner(SKGraphiteBackendTexture backendTexture, IntPtr mtlTexture)
				{
					this.backendTexture = backendTexture;
					this.mtlTexture = mtlTexture;
				}

				public void Dispose()
				{
					backendTexture.Dispose();
					if (mtlTexture != IntPtr.Zero)
					{
						ObjcRelease(mtlTexture);
						mtlTexture = IntPtr.Zero;
					}
				}
			}
		}

		// Metal.framework enum values (MTLPixelFormat.h / MTLTexture.h).
		private const nuint MTLPixelFormatBGRA8Unorm = 80;
		private const nuint MTLTextureUsageShaderRead = 1;
		private const nuint MTLTextureUsageRenderTarget = 4;
		private const nuint MTLStorageModePrivate = 2;

		private const ulong MTLGPUFamilyApple7 = 1007;
		private const ulong MTLGPUFamilyApple8 = 1008;
		private const ulong MTLGPUFamilyApple9 = 1009;
		private const ulong MTLGPUFamilyMac2 = 2002;

		private static IntPtr CreateMetalTexture(IntPtr device, int width, int height)
		{
			var descClass = objc_getClass("MTLTextureDescriptor");
			var descSel = sel_registerName("texture2DDescriptorWithPixelFormat:width:height:mipmapped:");
			var descriptor = objc_msgSend_texDesc(descClass, descSel, MTLPixelFormatBGRA8Unorm, (nuint)width, (nuint)height, 0);
			if (descriptor == IntPtr.Zero)
				return IntPtr.Zero;

			objc_msgSend_setNUInt(descriptor, sel_registerName("setUsage:"),
				MTLTextureUsageRenderTarget | MTLTextureUsageShaderRead);
			objc_msgSend_setNUInt(descriptor, sel_registerName("setStorageMode:"), MTLStorageModePrivate);

			return objc_msgSend_arg(device, sel_registerName("newTextureWithDescriptor:"), descriptor);
		}

		private static bool MetalHasGraphiteCapableFamily(IntPtr device)
		{
			var sel = sel_registerName("supportsFamily:");
			foreach (var f in new[] { MTLGPUFamilyApple9, MTLGPUFamilyApple8, MTLGPUFamilyApple7, MTLGPUFamilyMac2 })
			{
				if (objc_msgSend_supportsFamily(device, sel, f) != 0)
					return true;
			}
			return false;
		}

		// Whether this MTLDevice can drive Skia Graphite. Real hardware must
		// advertise Apple7+ or Mac2 (below that, Skia's Metal init SK_ABORTs). The
		// Apple simulator under-reports its GPU family (typically Apple1/Apple2/
		// Common1) yet is backed by the host Apple Silicon GPU and drives Graphite
		// Metal correctly, so it is whitelisted.
		private static bool MetalCanDriveGraphite(IntPtr device) =>
			MetalHasGraphiteCapableFamily(device) || IsRunningOnAppleSimulator;

		// The iOS/tvOS simulator sets SIMULATOR_* in the app's environment.
		private static bool IsRunningOnAppleSimulator =>
			!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SIMULATOR_UDID"))
			|| !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SIMULATOR_DEVICE_NAME"));

		private static IntPtr ObjcSend(IntPtr obj, string selector) =>
			objc_msgSend(obj, sel_registerName(selector));

		private static void ObjcRelease(IntPtr obj)
		{
			if (obj != IntPtr.Zero)
				objc_msgSend_void(obj, sel_registerName("release"));
		}

		[DllImport("/usr/lib/libobjc.dylib", CharSet = CharSet.Ansi)]
		private static extern IntPtr objc_getClass(string name);

		[DllImport("/usr/lib/libobjc.dylib", CharSet = CharSet.Ansi)]
		private static extern IntPtr sel_registerName(string name);

		[DllImport("/System/Library/Frameworks/Metal.framework/Metal")]
		private static extern IntPtr MTLCreateSystemDefaultDevice();

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr sel);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_void(IntPtr receiver, IntPtr sel);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern byte objc_msgSend_supportsFamily(IntPtr receiver, IntPtr sel, ulong family);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_texDesc(IntPtr cls, IntPtr sel, nuint pixelFormat, nuint width, nuint height, byte mipmapped);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_setNUInt(IntPtr receiver, IntPtr sel, nuint value);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_arg(IntPtr receiver, IntPtr sel, IntPtr arg);
	}
}
