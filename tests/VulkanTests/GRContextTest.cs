using System;
using SharpVk.Interop;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Vulkan.Tests
{
	public class GRContextTest : VKTest
	{
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CreateVkContextIsValid()
		{
			var nativeLibrary = new NativeLibrary();

			using (var ctx = CreateVkContext())
			using (var vkInterface = GRVkInterface.Create(
				nativeLibrary.GetProcedureAddress("vkGetInstanceProcAddr"),
				nativeLibrary.GetProcedureAddress("vkGetDeviceProcAddr"),
				(IntPtr)ctx.Instance.RawHandle.ToUInt64(),
				(IntPtr)ctx.Device.RawHandle.ToUInt64(),
				0))
			{
				Assert.NotNull(vkInterface);
				Assert.True(vkInterface.Validate(0));

				using (var grVkBackendContext = GRVkBackendContext.Assemble(
					(IntPtr)ctx.Instance.RawHandle.ToUInt64(),
					(IntPtr)ctx.PhysicalDevice.RawHandle.ToUInt64(),
					(IntPtr)ctx.Device.RawHandle.ToUInt64(),
					(IntPtr)ctx.GraphicsQueue.RawHandle.ToUInt64(),
					ctx.GraphicsFamily,
					0,
					0,
					0,
					vkInterface))
				{
					Assert.NotNull(grVkBackendContext);

					using (var grContext = GRContext.CreateVulkan(grVkBackendContext))
					{
						Assert.NotNull(grContext);
					}
				}
			}
		}
	}
}
