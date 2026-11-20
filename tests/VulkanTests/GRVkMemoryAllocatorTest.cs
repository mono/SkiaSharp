using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Xunit;
using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	[Collection(VulkanGpuRenderingCollection.Name)]
	public class GRVkMemoryAllocatorTest : VKTest
	{
		// SkiaSharp's GetReferenceCount() is internal, and this unsigned test
		// assembly cannot be granted InternalsVisibleTo by the strong-named
		// SkiaSharp, so read the native SkRefCnt count directly.
		[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
		private static extern int sk_refcnt_get_ref_count(IntPtr refcnt);

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void CreateDefaultRejectsNullHandlesQueueAndGetProc()
		{
			Assert.Throws<ArgumentException>(() => GRVkMemoryAllocator.CreateDefault(
				vkInstance: IntPtr.Zero, vkPhysicalDevice: (IntPtr)1, vkDevice: (IntPtr)2, vkQueue: (IntPtr)4,
				maxApiVersion: 0, getProcedureAddress: (n, i, d) => IntPtr.Zero));

			Assert.Throws<ArgumentException>(() => GRVkMemoryAllocator.CreateDefault(
				vkInstance: (IntPtr)1, vkPhysicalDevice: IntPtr.Zero, vkDevice: (IntPtr)2, vkQueue: (IntPtr)4,
				maxApiVersion: 0, getProcedureAddress: (n, i, d) => IntPtr.Zero));

			Assert.Throws<ArgumentException>(() => GRVkMemoryAllocator.CreateDefault(
				vkInstance: (IntPtr)1, vkPhysicalDevice: (IntPtr)2, vkDevice: IntPtr.Zero, vkQueue: (IntPtr)4,
				maxApiVersion: 0, getProcedureAddress: (n, i, d) => IntPtr.Zero));

			// The queue is required: skgpu::VulkanMemoryAllocators::Make asserts it is set, and a
			// missing one used to abort Vulkan-enabled debug builds (#4585 review).
			Assert.Throws<ArgumentException>(() => GRVkMemoryAllocator.CreateDefault(
				vkInstance: (IntPtr)1, vkPhysicalDevice: (IntPtr)2, vkDevice: (IntPtr)3, vkQueue: IntPtr.Zero,
				maxApiVersion: 0, getProcedureAddress: (n, i, d) => IntPtr.Zero));

			Assert.Throws<ArgumentNullException>(() => GRVkMemoryAllocator.CreateDefault(
				vkInstance: (IntPtr)1, vkPhysicalDevice: (IntPtr)2, vkDevice: (IntPtr)3, vkQueue: (IntPtr)4,
				maxApiVersion: 0, getProcedureAddress: null));
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void CreateDefaultReturnsAllocator()
		{
			using var ctx = CreateSilkVkContext();

			using var allocator = GRVkMemoryAllocator.CreateDefault(
				ctx.Instance.Handle,
				ctx.PhysicalDevice.Handle,
				ctx.Device.Handle,
				ctx.GraphicsQueue.Handle,
				SilkVkContext.ApiVersion,
				(name, instance, device) => ctx.BaseGetProc(name, instance, device));

			Assert.NotNull(allocator);
			Assert.NotEqual(IntPtr.Zero, allocator.Handle);
		}

		// The Ganesh factory ref_sp's whatever comes through GRVkBackendContext.MemoryAllocator,
		// so the Context outlives the managed wrapper's Dispose. Verify the wrapper can be
		// disposed immediately after GRContext.CreateVulkan and the Context still functions.
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GaneshContextRetainsAllocatorAfterWrapperDispose()
		{
			using var ctx = CreateSilkVkContext();
			var allocator = GRVkMemoryAllocator.CreateDefault(
				ctx.Instance.Handle,
				ctx.PhysicalDevice.Handle,
				ctx.Device.Handle,
				ctx.GraphicsQueue.Handle,
				SilkVkContext.ApiVersion,
				(name, instance, device) => ctx.BaseGetProc(name, instance, device));
			Assert.NotNull(allocator);

			using var extensions = new GRVkExtensions();
			extensions.Initialize((name, instance, device) => ctx.BaseGetProc(name, instance, device),
				ctx.Instance.Handle, ctx.PhysicalDevice.Handle);

			using var backend = new GRVkBackendContext
			{
				VkInstance         = ctx.Instance.Handle,
				VkPhysicalDevice   = ctx.PhysicalDevice.Handle,
				VkDevice           = ctx.Device.Handle,
				VkQueue            = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxAPIVersion      = SilkVkContext.ApiVersion,
				Extensions         = extensions,
				GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
				MemoryAllocator    = allocator,
			};

			// Without a forwarded allocator Skia silently creates its own default
			// one and the draw below still succeeds, so prove the Context took a
			// ref on this specific allocator.
			var refsBeforeCreate = sk_refcnt_get_ref_count(allocator.Handle);

			using var grContext = GRContext.CreateVulkan(backend);
			Assert.NotNull(grContext);
			Assert.True(sk_refcnt_get_ref_count(allocator.Handle) > refsBeforeCreate,
				"The Ganesh context did not retain the supplied allocator.");

			// Drop the managed wrapper's ref. The Context's own ref must keep the allocator alive.
			allocator.Dispose();

			// Exercise the allocator via a texture-backed surface + draw.
			using var surface = SKSurface.Create(grContext, budgeted: true, new SKImageInfo(64, 64));
			Assert.NotNull(surface);
			surface.Canvas.Clear(SKColors.Purple);
			grContext.Flush(submit: true, synchronous: true);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GraphiteContextAcceptsCustomAllocator()
		{
			using var ctx = CreateSilkVkContext();

			var allocator = GRVkMemoryAllocator.CreateDefault(
				ctx.Instance.Handle,
				ctx.PhysicalDevice.Handle,
				ctx.Device.Handle,
				ctx.GraphicsQueue.Handle,
				SilkVkContext.ApiVersion,
				(name, instance, device) => ctx.BaseGetProc(name, instance, device));
			Assert.NotNull(allocator);

			using var backend = new SKGraphiteVkBackendContext
			{
				VkInstance         = ctx.Instance.Handle,
				VkPhysicalDevice   = ctx.PhysicalDevice.Handle,
				VkDevice           = ctx.Device.Handle,
				VkQueue            = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxApiVersion      = SilkVkContext.ApiVersion,
				GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
				MemoryAllocator    = allocator,
			};

			// See GaneshContextRetainsAllocatorAfterWrapperDispose: rendering alone
			// cannot tell the supplied allocator from Skia's default fallback.
			var refsBeforeCreate = sk_refcnt_get_ref_count(allocator.Handle);

			using var graphiteContext = SKGraphiteContext.CreateVulkan(backend);
			Assert.NotNull(graphiteContext);
			Assert.True(sk_refcnt_get_ref_count(allocator.Handle) > refsBeforeCreate,
				"The Graphite context did not retain the supplied allocator.");

			// The Context has retained the allocator; managed wrapper can go.
			allocator.Dispose();

			using var recorder = graphiteContext.CreateRecorder();
			Assert.NotNull(recorder);
			using var surface = SKSurface.Create(recorder, new SKImageInfo(64, 64), false, null);
			Assert.NotNull(surface);
			surface.Canvas.Clear(SKColors.Cyan);
			using var recording = recorder.Snap();
			Assert.NotNull(recording);
			Assert.Equal(SKGraphiteInsertStatus.Success, graphiteContext.InsertRecording(recording));
			Assert.True(graphiteContext.Submit(new SKGraphiteSubmitInfo { Sync = true }));
		}
	
		// The allocator resolves every Vulkan entry point while it is being created and keeps
		// only the resolved function pointers. Prove that the getProc delegate is never called
		// after CreateDefault returns: not while drawing, not while reading back, and not while
		// the context and allocator are torn down.
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void AllocatorGetProcIsOnlyUsedDuringCreation()
		{
			using var ctx = CreateSilkVkContext();

			var allocatorCalls = 0;
			var allocator = GRVkMemoryAllocator.CreateDefault(
				ctx.Instance.Handle,
				ctx.PhysicalDevice.Handle,
				ctx.Device.Handle,
				ctx.GraphicsQueue.Handle,
				SilkVkContext.ApiVersion,
				(name, instance, device) =>
				{
					Interlocked.Increment(ref allocatorCalls);
					return ctx.BaseGetProc(name, instance, device);
				});
			Assert.NotNull(allocator);

			var callsAfterCreate = Volatile.Read(ref allocatorCalls);
			Assert.True(callsAfterCreate > 0);

			using (var extensions = CreateExtensions(ctx))
			using (var backend = CreateBackendContext(ctx, extensions, allocator))
			using (var grContext = GRContext.CreateVulkan(backend))
			{
				Assert.NotNull(grContext);
				allocator.Dispose();

				DrawAndAssertPixel(grContext, SKColors.Red);
			}

			Assert.Equal(callsAfterCreate, Volatile.Read(ref allocatorCalls));
		}

		// The getProc delegate's GCHandle is released when CreateDefault returns. Make the
		// delegate unreachable and collect it before the allocator is used for real work.
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void AllocatorSurvivesGetProcCollection()
		{
			using var ctx = CreateSilkVkContext();

			var (allocator, getProcRef) = CreateAllocatorWithCollectableGetProc(ctx);
			Assert.NotNull(allocator);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			Assert.False(getProcRef.IsAlive);

			using (var extensions = CreateExtensions(ctx))
			using (var backend = CreateBackendContext(ctx, extensions, allocator))
			using (var grContext = GRContext.CreateVulkan(backend))
			{
				Assert.NotNull(grContext);
				allocator.Dispose();

				DrawAndAssertPixel(grContext, SKColors.Green);
			}
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void ThreadSafeAllocatorCanBeSharedByContexts()
		{
			using var ctx = CreateSilkVkContext();

			using var allocator = GRVkMemoryAllocator.CreateDefault(
				ctx.Instance.Handle,
				ctx.PhysicalDevice.Handle,
				ctx.Device.Handle,
				ctx.GraphicsQueue.Handle,
				SilkVkContext.ApiVersion,
				(name, instance, device) => ctx.BaseGetProc(name, instance, device),
				threadSafe: true);
			Assert.NotNull(allocator);

			using var extensions = CreateExtensions(ctx);
			using var backend1 = CreateBackendContext(ctx, extensions, allocator);
			using var backend2 = CreateBackendContext(ctx, extensions, allocator);
			using var grContext1 = GRContext.CreateVulkan(backend1);
			using var grContext2 = GRContext.CreateVulkan(backend2);
			Assert.NotNull(grContext1);
			Assert.NotNull(grContext2);

			DrawAndAssertPixel(grContext1, SKColors.Blue);
			DrawAndAssertPixel(grContext2, SKColors.Yellow);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static (GRVkMemoryAllocator Allocator, WeakReference GetProc) CreateAllocatorWithCollectableGetProc(SilkVkContext ctx)
		{
			var baseGetProc = ctx.BaseGetProc;
			GRVkGetProcedureAddressDelegate getProc = (name, instance, device) => baseGetProc(name, instance, device);
			var allocator = GRVkMemoryAllocator.CreateDefault(
				ctx.Instance.Handle,
				ctx.PhysicalDevice.Handle,
				ctx.Device.Handle,
				ctx.GraphicsQueue.Handle,
				SilkVkContext.ApiVersion,
				getProc);
			return (allocator, new WeakReference(getProc));
		}

		private static GRVkExtensions CreateExtensions(SilkVkContext ctx)
		{
			var extensions = new GRVkExtensions();
			extensions.Initialize((name, instance, device) => ctx.BaseGetProc(name, instance, device),
				ctx.Instance.Handle, ctx.PhysicalDevice.Handle);
			return extensions;
		}

		private static GRVkBackendContext CreateBackendContext(SilkVkContext ctx, GRVkExtensions extensions, GRVkMemoryAllocator allocator) =>
			new GRVkBackendContext
			{
				VkInstance          = ctx.Instance.Handle,
				VkPhysicalDevice    = ctx.PhysicalDevice.Handle,
				VkDevice            = ctx.Device.Handle,
				VkQueue             = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex  = ctx.GraphicsFamily,
				MaxAPIVersion       = SilkVkContext.ApiVersion,
				Extensions          = extensions,
				GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
				MemoryAllocator     = allocator,
			};

		private static void DrawAndAssertPixel(GRContext grContext, SKColor color)
		{
			var info = new SKImageInfo(64, 64, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create(grContext, budgeted: true, info);
			Assert.NotNull(surface);

			surface.Canvas.Clear(color);
			grContext.Flush(submit: true, synchronous: true);

			using var bitmap = new SKBitmap(info);
			Assert.True(surface.ReadPixels(info, bitmap.GetPixels(), info.RowBytes, 0, 0));
			Assert.Equal(color, bitmap.GetPixel(32, 32));
		}
}
}
