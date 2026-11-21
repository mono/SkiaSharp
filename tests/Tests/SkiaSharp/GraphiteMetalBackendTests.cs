using System;
using System.Collections.Generic;
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

		// Forces [MTLDevice newRenderPipelineStateWithDescriptor:error:] to fail, as the
		// iOS simulator does for framebuffer fetch (#4555). The handler's shader argument
		// must carry the Metal Shading Language source, not just the pipeline label.
		[Fact]
		public void PipelineStateFailureReportsMetalShaderSource()
		{
			GpuPolicy.RequireOrSkip(Backend);

			var reports = new List<(string Shader, string Errors, bool Cached)>();
			var options = new SKGraphiteContextOptions
			{
				ShaderErrorHandler = (shader, errors, cached) => reports.Add((shader, errors, cached)),
			};

			using (PipelineStateFailureInjection.Install())
			{
				using var harness = CreateHarness(options);
				using var surface = SKSurface.Create(harness.Recorder, new SKImageInfo(Width, Height), false, null);
				Assert.NotNull(surface);
				using var paint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
				surface.Canvas.DrawCircle(Width / 2f, Height / 2f, Width / 3f, paint);
				// Pipelines compile while snapping. The recording is deliberately not
				// inserted: Graphite dereferences the null pipeline when it inspects a
				// failed recording during insertion.
				using var recording = harness.Recorder.Snap();
			}

			var report = Assert.Single(reports, r => r.Errors.Contains(PipelineStateFailureInjection.Domain));
			Assert.False(report.Cached);
			Assert.Contains("#include <metal_stdlib>", report.Shader);
			Assert.Contains("fragmentMain", report.Shader);
		}

		private sealed class PipelineStateFailureInjection : IDisposable
		{
			public const string Domain = "SkiaSharpInjectedPipelineStateFailure";

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate IntPtr NewPipelineStateDelegate(IntPtr self, IntPtr sel, IntPtr descriptor, IntPtr error);

			private static readonly NewPipelineStateDelegate failingImplementation = FailNewPipelineState;

			private readonly IntPtr method;
			private readonly IntPtr originalImplementation;

			private PipelineStateFailureInjection(IntPtr method, IntPtr originalImplementation)
			{
				this.method = method;
				this.originalImplementation = originalImplementation;
			}

			public static PipelineStateFailureInjection Install()
			{
				var device = MTLCreateSystemDefaultDevice();
				Assert.NotEqual(IntPtr.Zero, device);
				try
				{
					var method = class_getInstanceMethod(
						object_getClass(device), sel_registerName("newRenderPipelineStateWithDescriptor:error:"));
					Assert.NotEqual(IntPtr.Zero, method);
					var original = method_setImplementation(
						method, Marshal.GetFunctionPointerForDelegate(failingImplementation));
					return new PipelineStateFailureInjection(method, original);
				}
				finally
				{
					ObjcRelease(device);
				}
			}

			public void Dispose() => method_setImplementation(method, originalImplementation);

			private static IntPtr FailNewPipelineState(IntPtr self, IntPtr sel, IntPtr descriptor, IntPtr error)
			{
				if (error != IntPtr.Zero)
				{
					var domain = objc_msgSend_string(
						objc_getClass("NSString"), sel_registerName("stringWithUTF8String:"), Domain);
					var nsError = objc_msgSend_error(
						objc_getClass("NSError"), sel_registerName("errorWithDomain:code:userInfo:"), domain, 1, IntPtr.Zero);
					Marshal.WriteIntPtr(error, nsError);
				}
				return IntPtr.Zero;
			}
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

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend", CharSet = CharSet.Ansi)]
		private static extern IntPtr objc_msgSend_string(IntPtr receiver, IntPtr sel, string value);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_error(IntPtr receiver, IntPtr sel, IntPtr domain, nint code, IntPtr userInfo);

		[DllImport("/usr/lib/libobjc.dylib")]
		private static extern IntPtr object_getClass(IntPtr obj);

		[DllImport("/usr/lib/libobjc.dylib")]
		private static extern IntPtr class_getInstanceMethod(IntPtr cls, IntPtr sel);

		[DllImport("/usr/lib/libobjc.dylib")]
		private static extern IntPtr method_setImplementation(IntPtr method, IntPtr imp);
	}
}
