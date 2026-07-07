using SkiaSharp.Views.Blazor;
using SkiaSharp.Views.Blazor.Internal;
using Xunit;

namespace SkiaSharp.Tests.Blazor;

public class SKBlazorHostTests
{
	[Theory]
	[InlineData("WebAssembly", "WebAssembly")]
	[InlineData("Server", "Server")]
	[InlineData("WebView", "Hybrid")]
	[InlineData("Static", "StaticSsr")]
	public void ResolvesKnownRendererNames(string name, string expected) =>
		Assert.Equal(expected, SKBlazorHost.Resolve(name).ToString());

	[Fact]
	public void ResolvesUnknownRendererNameOffBrowserToHybrid() =>
		// Host detection is only consulted once interactive; an unrecognized non-browser host
		// is treated as a native WebView (Hybrid) so the bridged path still activates.
		Assert.Equal(SKBlazorHostKind.Hybrid, SKBlazorHost.Resolve("something-else"));

	[Theory]
	[InlineData("Server", true)]
	[InlineData("WebView", true)]
	[InlineData("Static", true)]
	[InlineData("WebAssembly", false)]
	public void IsBridgedIsCorrect(string rendererName, bool expected) =>
		Assert.Equal(expected, SKBlazorHost.IsBridged(SKBlazorHost.Resolve(rendererName)));

	[Fact]
	public void PerControlFormatWinsOverGlobalAndHostDefault()
	{
		var options = new SKBlazorOptions { TransferFormat = SKBlazorTransferFormat.Png };

		var resolved = SKBlazorHost.ResolveTransferFormat(
			SKBlazorHostKind.Hybrid,
			SKBlazorTransferFormat.Jpeg,
			options);

		Assert.Equal(SKBlazorTransferFormat.Jpeg, resolved);
	}

	[Fact]
	public void GlobalOptionUsedWhenNoPerControlValue()
	{
		var options = new SKBlazorOptions { TransferFormat = SKBlazorTransferFormat.Png };

		var resolved = SKBlazorHost.ResolveTransferFormat(SKBlazorHostKind.Server, null, options);

		Assert.Equal(SKBlazorTransferFormat.Png, resolved);
	}

	[Fact]
	public void HybridDefaultsToPut() =>
		Assert.Equal(
			SKBlazorTransferFormat.Put,
			SKBlazorHost.ResolveTransferFormat(SKBlazorHostKind.Hybrid, null, new SKBlazorOptions()));

	[Fact]
	public void ServerDefaultsToJpeg() =>
		Assert.Equal(
			SKBlazorTransferFormat.Jpeg,
			SKBlazorHost.ResolveTransferFormat(SKBlazorHostKind.Server, null, new SKBlazorOptions()));
}
