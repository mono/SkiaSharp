using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Xunit;
using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	[Collection(VulkanGpuRenderingCollection.Name)]
	public unsafe class GRVkDeviceLostTest : VKTest
	{
		private static IntPtr realQueueSubmit;
		private static bool failQueueSubmit;

		#if NET5_0_OR_GREATER
		[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
		private static int QueueSubmitShimStdcall(IntPtr queue, uint submitCount, void* pSubmits, ulong fence)
		{
			if (failQueueSubmit)
				return (int)Result.ErrorDeviceLost;

			return ((delegate* unmanaged[Stdcall]<IntPtr, uint, void*, ulong, int>)realQueueSubmit)(queue, submitCount, pSubmits, fence);
		}

		[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
		private static int QueueSubmitShimCdecl(IntPtr queue, uint submitCount, void* pSubmits, ulong fence)
		{
			if (failQueueSubmit)
				return (int)Result.ErrorDeviceLost;

			return ((delegate* unmanaged[Cdecl]<IntPtr, uint, void*, ulong, int>)realQueueSubmit)(queue, submitCount, pSubmits, fence);
		}

		private static IntPtr GetQueueSubmitShimPointer() =>
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
				? (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, uint, void*, ulong, int>)&QueueSubmitShimStdcall
				: (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, uint, void*, ulong, int>)&QueueSubmitShimCdecl;
		#else
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int QueueSubmitDelegate(IntPtr queue, uint submitCount, void* pSubmits, ulong fence);

		private static readonly QueueSubmitDelegate QueueSubmitShimInstance = (queue, submitCount, pSubmits, fence) =>
		{
			if (failQueueSubmit)
				return (int)Result.ErrorDeviceLost;

			var del = Marshal.GetDelegateForFunctionPointer<QueueSubmitDelegate>(realQueueSubmit);
			return del(queue, submitCount, pSubmits, fence);
		};

		private static IntPtr GetQueueSubmitShimPointer() =>
			Marshal.GetFunctionPointerForDelegate(QueueSubmitShimInstance);
		#endif

		[Fact]
		public void DeviceLostPropertyRoundTripsOnGRVkBackendContext()
		{
			using var backend = new GRVkBackendContext();
			Assert.Null(backend.DeviceLost);

			GRVkDeviceLostDelegate first = _ => { };
			GRVkDeviceLostDelegate second = _ => { };

			backend.DeviceLost = first;
			Assert.Same(first, backend.DeviceLost);

			backend.DeviceLost = second;
			Assert.Same(second, backend.DeviceLost);

			backend.DeviceLost = null;
			Assert.Null(backend.DeviceLost);
		}

		[Fact]
		public void DeviceLostPropertyRoundTripsOnSKGraphiteVkBackendContext()
		{
			using var backend = new SKGraphiteVkBackendContext();
			Assert.Null(backend.DeviceLost);

			GRVkDeviceLostDelegate first = _ => { };
			GRVkDeviceLostDelegate second = _ => { };

			backend.DeviceLost = first;
			Assert.Same(first, backend.DeviceLost);

			backend.DeviceLost = second;
			Assert.Same(second, backend.DeviceLost);

			backend.DeviceLost = null;
			Assert.Null(backend.DeviceLost);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GRContextAcceptsDeviceLostHandlerAndSurvivesDispose()
		{
			using var ctx = CreateSilkVkContext();
			using var extensions = new GRVkExtensions();
			extensions.Initialize((name, instance, device) => ctx.BaseGetProc(name, instance, device),
				ctx.Instance.Handle, ctx.PhysicalDevice.Handle);

			var fired = false;
			using var backend = new GRVkBackendContext
			{
				VkInstance = ctx.Instance.Handle,
				VkPhysicalDevice = ctx.PhysicalDevice.Handle,
				VkDevice = ctx.Device.Handle,
				VkQueue = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxAPIVersion = SilkVkContext.ApiVersion,
				Extensions = extensions,
				GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
				DeviceLost = _ => fired = true,
			};

			using var grContext = GRContext.CreateVulkan(backend);
			Assert.NotNull(grContext);

			using var surface = SKSurface.Create(grContext, budgeted: true, new SKImageInfo(64, 64));
			Assert.NotNull(surface);
			surface.Canvas.Clear(SKColors.Purple);
			grContext.Flush(submit: true, synchronous: true);

			Assert.False(fired);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void SKGraphiteContextAcceptsDeviceLostHandlerAndSurvivesDispose()
		{
			using var ctx = CreateSilkVkContext();

			var fired = false;
			using var backend = new SKGraphiteVkBackendContext
			{
				VkInstance = ctx.Instance.Handle,
				VkPhysicalDevice = ctx.PhysicalDevice.Handle,
				VkDevice = ctx.Device.Handle,
				VkQueue = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxApiVersion = SilkVkContext.ApiVersion,
				GetProcedureAddress = (name, instance, device) => ctx.BaseGetProc(name, instance, device),
				DeviceLost = _ => fired = true,
			};

			using var graphiteContext = SKGraphiteContext.CreateVulkan(backend);
			Assert.NotNull(graphiteContext);

			using var recorder = graphiteContext.CreateRecorder();
			using var surface = SKSurface.Create(recorder, new SKImageInfo(64, 64), false, null);
			Assert.NotNull(surface);
			surface.Canvas.Clear(SKColors.Cyan);
			using var recording = recorder.Snap();
			Assert.NotNull(recording);
			Assert.Equal(SKGraphiteInsertStatus.Success, graphiteContext.InsertRecording(recording));
			Assert.True(graphiteContext.Submit(new SKGraphiteSubmitInfo { Sync = true }));

			Assert.False(fired);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GRContextInvokesDeviceLostCallbackWhenQueueSubmitReturnsDeviceLost()
		{
			using var ctx = CreateSilkVkContext();
			using var extensions = new GRVkExtensions();
			extensions.Initialize((name, instance, device) => ctx.BaseGetProc(name, instance, device),
				ctx.Instance.Handle, ctx.PhysicalDevice.Handle);

			string description = null;
			GRVkDeviceFaultAddressInfo[] addressInfos = null;
			GRVkDeviceFaultVendorInfo[] vendorInfos = null;
			byte[] vendorBinaryData = null;
			var callCount = 0;

			failQueueSubmit = false;
			try
			{
				using var backend = new GRVkBackendContext
				{
					VkInstance = ctx.Instance.Handle,
					VkPhysicalDevice = ctx.PhysicalDevice.Handle,
					VkDevice = ctx.Device.Handle,
					VkQueue = ctx.GraphicsQueue.Handle,
					GraphicsQueueIndex = ctx.GraphicsFamily,
					MaxAPIVersion = SilkVkContext.ApiVersion,
					Extensions = extensions,
					GetProcedureAddress = (name, instance, device) =>
					{
						var addr = ctx.BaseGetProc(name, instance, device);
						if (name == "vkQueueSubmit")
						{
							realQueueSubmit = addr;
							return GetQueueSubmitShimPointer();
						}
						return addr;
					},
					DeviceLost = info =>
					{
						callCount++;
						description = info.Description;
						addressInfos = info.AddressInfos;
						vendorInfos = info.VendorInfos;
						vendorBinaryData = info.VendorBinaryData;
					},
				};

				using var grContext = GRContext.CreateVulkan(backend);
				Assert.NotNull(grContext);

				using var surface = SKSurface.Create(grContext, budgeted: true, new SKImageInfo(64, 64));
				Assert.NotNull(surface);
				surface.Canvas.Clear(SKColors.Red);

				failQueueSubmit = true;
				grContext.Flush(submit: true, synchronous: false);
			}
			finally
			{
				failQueueSubmit = false;
			}

			Assert.Equal(1, callCount);
			Assert.False(string.IsNullOrEmpty(description));
			Assert.NotNull(addressInfos);
			Assert.NotNull(vendorInfos);
			Assert.NotNull(vendorBinaryData);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void SKGraphiteContextInvokesDeviceLostCallbackWhenQueueSubmitReturnsDeviceLost()
		{
			using var ctx = CreateSilkVkContext();

			string description = null;
			var callCount = 0;

			failQueueSubmit = false;
			try
			{
				using var backend = new SKGraphiteVkBackendContext
				{
					VkInstance = ctx.Instance.Handle,
					VkPhysicalDevice = ctx.PhysicalDevice.Handle,
					VkDevice = ctx.Device.Handle,
					VkQueue = ctx.GraphicsQueue.Handle,
					GraphicsQueueIndex = ctx.GraphicsFamily,
					MaxApiVersion = SilkVkContext.ApiVersion,
					GetProcedureAddress = (name, instance, device) =>
					{
						var addr = ctx.BaseGetProc(name, instance, device);
						if (name == "vkQueueSubmit")
						{
							realQueueSubmit = addr;
							return GetQueueSubmitShimPointer();
						}
						return addr;
					},
					DeviceLost = info =>
					{
						callCount++;
						description = info.Description;
					},
				};

				using var graphiteContext = SKGraphiteContext.CreateVulkan(backend);
				Assert.NotNull(graphiteContext);

				using var recorder = graphiteContext.CreateRecorder();
				using var surface = SKSurface.Create(recorder, new SKImageInfo(64, 64), false, null);
				Assert.NotNull(surface);
				surface.Canvas.Clear(SKColors.Blue);
				using var recording = recorder.Snap();
				Assert.NotNull(recording);
				graphiteContext.InsertRecording(recording);

				failQueueSubmit = true;
				Assert.False(graphiteContext.Submit(new SKGraphiteSubmitInfo { Sync = false }));

				Assert.Equal(1, callCount);
				Assert.False(string.IsNullOrEmpty(description));
				Assert.True(graphiteContext.IsDeviceLost);
			}
			finally
			{
				failQueueSubmit = false;
			}
		}

		// Skia keeps the device-lost user data for the context's lifetime, so the
		// context must own it: clearing the handler, disposing the backend and
		// collecting it must not invalidate the handler an existing context uses.
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void GRContextKeepsDeviceLostHandlerAfterBackendIsClearedAndDisposed()
		{
			using var ctx = CreateSilkVkContext();
			using var extensions = new GRVkExtensions();
			extensions.Initialize((name, instance, device) => ctx.BaseGetProc(name, instance, device),
				ctx.Instance.Handle, ctx.PhysicalDevice.Handle);

			var callCount = 0;

			failQueueSubmit = false;
			try
			{
				using var grContext = CreateGaneshContextAndDropBackend(ctx, extensions, () => callCount++);
				Assert.NotNull(grContext);

				CollectGarbage();

				using var surface = SKSurface.Create(grContext, budgeted: true, new SKImageInfo(64, 64));
				Assert.NotNull(surface);
				surface.Canvas.Clear(SKColors.Red);

				failQueueSubmit = true;
				grContext.Flush(submit: true, synchronous: false);
			}
			finally
			{
				failQueueSubmit = false;
			}

			Assert.Equal(1, callCount);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void SKGraphiteContextKeepsDeviceLostHandlerAfterBackendIsClearedAndDisposed()
		{
			using var ctx = CreateSilkVkContext();

			var callCount = 0;

			failQueueSubmit = false;
			try
			{
				using var graphiteContext = CreateGraphiteContextAndDropBackend(ctx, () => callCount++);
				Assert.NotNull(graphiteContext);

				CollectGarbage();

				using var recorder = graphiteContext.CreateRecorder();
				using var surface = SKSurface.Create(recorder, new SKImageInfo(64, 64), false, null);
				Assert.NotNull(surface);
				surface.Canvas.Clear(SKColors.Blue);
				using var recording = recorder.Snap();
				Assert.NotNull(recording);
				graphiteContext.InsertRecording(recording);

				failQueueSubmit = true;
				Assert.False(graphiteContext.Submit(new SKGraphiteSubmitInfo { Sync = false }));
			}
			finally
			{
				failQueueSubmit = false;
			}

			Assert.Equal(1, callCount);
		}

		// Kept out of line so the backend and the original lambda are unreachable
		// once it returns; only the context can keep the handler alive.
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static GRContext CreateGaneshContextAndDropBackend(SilkVkContext ctx, GRVkExtensions extensions, Action onDeviceLost)
		{
			var backend = new GRVkBackendContext
			{
				VkInstance = ctx.Instance.Handle,
				VkPhysicalDevice = ctx.PhysicalDevice.Handle,
				VkDevice = ctx.Device.Handle,
				VkQueue = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxAPIVersion = SilkVkContext.ApiVersion,
				Extensions = extensions,
				GetProcedureAddress = (name, instance, device) => ShimQueueSubmit(ctx, name, instance, device),
				DeviceLost = _ => onDeviceLost(),
			};

			var grContext = GRContext.CreateVulkan(backend);

			backend.DeviceLost = null;
			backend.Dispose();

			return grContext;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static SKGraphiteContext CreateGraphiteContextAndDropBackend(SilkVkContext ctx, Action onDeviceLost)
		{
			var backend = new SKGraphiteVkBackendContext
			{
				VkInstance = ctx.Instance.Handle,
				VkPhysicalDevice = ctx.PhysicalDevice.Handle,
				VkDevice = ctx.Device.Handle,
				VkQueue = ctx.GraphicsQueue.Handle,
				GraphicsQueueIndex = ctx.GraphicsFamily,
				MaxApiVersion = SilkVkContext.ApiVersion,
				GetProcedureAddress = (name, instance, device) => ShimQueueSubmit(ctx, name, instance, device),
				DeviceLost = _ => onDeviceLost(),
			};

			var graphiteContext = SKGraphiteContext.CreateVulkan(backend);

			backend.DeviceLost = null;
			backend.Dispose();

			return graphiteContext;
		}

		private static IntPtr ShimQueueSubmit(SilkVkContext ctx, string name, IntPtr instance, IntPtr device)
		{
			var addr = ctx.BaseGetProc(name, instance, device);
			if (name == "vkQueueSubmit")
			{
				realQueueSubmit = addr;
				return GetQueueSubmitShimPointer();
			}
			return addr;
		}
	}
}
