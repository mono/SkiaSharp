using System;
using Xunit;

namespace SkiaSharp.Tests;

public unsafe class SKGraphiteContextOptionsTest : SKTest
{
	[Theory]
	[InlineData(-2L)]
	[InlineData(-1L)]
	[InlineData(0L)]
	[InlineData(1L)]
	[InlineData(268435456L)]
	[InlineData(long.MaxValue)]
	public void ToNativePreservesRequestedGpuBudget(long budget)
	{
		var options = new SKGraphiteContextOptions { GpuBudgetInBytes = budget };

		var native = options.ToNative(out var pinnedHandler, out var nativeHandler);

		Assert.Equal(budget, native.fGpuBudgetInBytes);
		Assert.False(pinnedHandler.IsAllocated);
		Assert.Equal(IntPtr.Zero, nativeHandler);
		Assert.Equal(IntPtr.Zero, native.fShaderErrorHandler);
	}

	[Fact]
	public void DefaultInitializedOptionsPreserveZeroByteBudget()
	{
		var options = default(SKGraphiteContextOptions);

		var native = options.ToNative(out _, out _);

		Assert.Equal(0L, options.GpuBudgetInBytes);
		Assert.Equal(0L, native.fGpuBudgetInBytes);
	}

	[Fact]
	public void ConstructedOptionsPreserveDefaultBudgetSentinel()
	{
		var options = new SKGraphiteContextOptions();

		var native = options.ToNative(out _, out _);

		Assert.Equal(-1L, options.GpuBudgetInBytes);
		Assert.Equal(-1L, native.fGpuBudgetInBytes);
	}
}
