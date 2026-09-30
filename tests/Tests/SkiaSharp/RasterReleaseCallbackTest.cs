using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	public class RasterReleaseCallbackTest : SKTest
	{
		private sealed class ReleaseState
		{
			public int Calls;
			public IntPtr Address;
		}

		[Fact]
		public async Task FailedImageCreationReleasesDelegateHandleWithoutCallingRelease()
		{
			var references = FailImageCreations();
			await AssertEx.EventuallyGC(references);
		}

		[Fact]
		public async Task FailedSurfaceCreationReleasesDelegateHandleWithoutCallingRelease()
		{
			var references = FailSurfaceCreations();
			await AssertEx.EventuallyGC(references);
		}

		[Fact]
		public void SuccessfulRasterCreationsCallReleaseOnce()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			var pixels = Marshal.AllocCoTaskMem(info.BytesSize);
			try
			{
				var imageState = new ReleaseState();
				using (var pixmap = new SKPixmap(info, pixels))
				{
					var image = SKImage.FromPixels(pixmap, (addr, context) =>
					{
						var state = (ReleaseState)context;
						state.Address = addr;
						state.Calls++;
					}, imageState);
					Assert.NotNull(image);
					Assert.Equal(0, imageState.Calls);
					image.Dispose();
					image.Dispose();
					Assert.Equal(1, imageState.Calls);
					Assert.Equal(pixels, imageState.Address);
				}

				var surfaceState = new ReleaseState();
				var surface = SKSurface.Create(info, pixels, info.RowBytes, (addr, context) =>
				{
					var state = (ReleaseState)context;
					state.Address = addr;
					state.Calls++;
				}, surfaceState);
				Assert.NotNull(surface);
				Assert.Equal(0, surfaceState.Calls);
				surface.Dispose();
				surface.Dispose();
				Assert.Equal(1, surfaceState.Calls);
				Assert.Equal(pixels, surfaceState.Address);
			}
			finally
			{
				Marshal.FreeCoTaskMem(pixels);
			}
		}

		[Fact]
		public void NullDelegatesAndImageArgumentValidationAreUnchanged()
		{
			var error = Assert.Throws<ArgumentNullException>(() =>
				SKImage.FromPixels(null, (addr, context) => { }, new object()));
			Assert.Equal("pixmap", error.ParamName);

			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			var pixels = Marshal.AllocCoTaskMem(info.BytesSize);
			try
			{
				using var invalidPixmap = new SKPixmap(
					new SKImageInfo(4, 4, SKColorType.Unknown, SKAlphaType.Unknown), pixels);
				Assert.Null(SKImage.FromPixels(invalidPixmap, null, new object()));
				Assert.Null(SKSurface.Create(info, pixels, 1, null, new object()));

				using var pixmap = new SKPixmap(info, pixels);
				using var image = SKImage.FromPixels(pixmap, null, null);
				using var surface = SKSurface.Create(info, pixels, info.RowBytes, null, null);
				Assert.NotNull(image);
				Assert.NotNull(surface);
			}
			finally
			{
				Marshal.FreeCoTaskMem(pixels);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static WeakReference[] FailImageCreations()
		{
			var pixels = Marshal.AllocCoTaskMem(4 * 4 * 4);
			try
			{
				var references = new WeakReference[8];
				var info = new SKImageInfo(4, 4, SKColorType.Unknown, SKAlphaType.Unknown);
				using var pixmap = new SKPixmap(info, pixels);
				for (var i = 0; i < references.Length; i++)
				{
					var state = new ReleaseState();
					references[i] = new WeakReference(state);
					using var image = SKImage.FromPixels(pixmap, (addr, context) =>
						((ReleaseState)context).Calls++, state);
					Assert.Null(image);
					Assert.Equal(0, state.Calls);
				}
				return references;
			}
			finally
			{
				Marshal.FreeCoTaskMem(pixels);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static WeakReference[] FailSurfaceCreations()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
			var pixels = Marshal.AllocCoTaskMem(info.BytesSize);
			try
			{
				var references = new WeakReference[8];
				for (var i = 0; i < references.Length; i++)
				{
					var state = new ReleaseState();
					references[i] = new WeakReference(state);
					using var surface = SKSurface.Create(info, pixels, 1, (addr, context) =>
						((ReleaseState)context).Calls++, state);
					Assert.Null(surface);
					Assert.Equal(0, state.Calls);
				}
				return references;
			}
			finally
			{
				Marshal.FreeCoTaskMem(pixels);
			}
		}
	}
}
