using SkiaSharp.Views.Blazor;
using SkiaSharp.Views.Blazor.Internal;
using Xunit;

namespace SkiaSharp.Tests.Blazor;

public class HostTests
{
	[Theory]
	[InlineData("WebAssembly", "WebAssembly")]
	[InlineData("Server", "Server")]
	[InlineData("WebView", "Hybrid")]
	[InlineData("Static", "StaticSsr")]
	public void ResolvesKnownRendererNames(string name, string expected) =>
		Assert.Equal(expected, Host.Resolve(name).ToString());

	[Fact]
	public void ResolvesUnknownRendererNameOffBrowserToHybrid() =>
		// Host detection is only consulted once interactive; an unrecognized non-browser host
		// is treated as a native WebView (Hybrid) so the bridged path still activates.
		Assert.Equal(HostKind.Hybrid, Host.Resolve("something-else"));

	[Theory]
	[InlineData("Server", true)]
	[InlineData("WebView", true)]
	[InlineData("Static", true)]
	[InlineData("WebAssembly", false)]
	public void IsBridgedIsCorrect(string rendererName, bool expected) =>
		Assert.Equal(expected, Host.IsBridged(Host.Resolve(rendererName)));

	[Fact]
	public void PerControlFormatWinsOverGlobalAndHostDefault()
	{
		var options = new SKBlazorOptions { TransferFormat = SKBlazorTransferFormat.Png };

		var resolved = Host.ResolveTransferFormat(
			HostKind.Hybrid,
			SKBlazorTransferFormat.Jpeg,
			options);

		Assert.Equal(SKBlazorTransferFormat.Jpeg, resolved);
	}

	[Fact]
	public void GlobalOptionUsedWhenNoPerControlValue()
	{
		var options = new SKBlazorOptions { TransferFormat = SKBlazorTransferFormat.Png };

		var resolved = Host.ResolveTransferFormat(HostKind.Server, null, options);

		Assert.Equal(SKBlazorTransferFormat.Png, resolved);
	}

	[Fact]
	public void HybridDefaultsToJpeg() =>
		Assert.Equal(
			SKBlazorTransferFormat.Jpeg,
			Host.ResolveTransferFormat(HostKind.Hybrid, null, new SKBlazorOptions()));

	[Fact]
	public void ServerDefaultsToJpeg() =>
		Assert.Equal(
			SKBlazorTransferFormat.Jpeg,
			Host.ResolveTransferFormat(HostKind.Server, null, new SKBlazorOptions()));
}
