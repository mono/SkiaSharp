using System;
using Xunit;

namespace SkiaSharp.Vulkan.Tests
{
	public class GRContextTest : VKTest
	{
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CreateVkContextIsValid()
		{
			using (var ctx = CreateVkContext())
			{
				using (var grVkBackendContext = GRVkBackendContext.Assemble(
					(IntPtr)ctx.Instance.RawHandle.ToUInt64(),
					(IntPtr)ctx.PhysicalDevice.RawHandle.ToUInt64(),
					(IntPtr)ctx.Device.RawHandle.ToUInt64(),
					(IntPtr)ctx.GraphicsQueue.RawHandle.ToUInt64(),
					ctx.GraphicsFamily,
					0,
					0,
					0,
					ctx.GetProc))
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
