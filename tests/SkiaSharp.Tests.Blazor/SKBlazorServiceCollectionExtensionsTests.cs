using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Views.Blazor;
using Xunit;

namespace SkiaSharp.Tests.Blazor;

public class SKBlazorServiceCollectionExtensionsTests
{
	[Fact]
	public void AddRegistersConfiguredOptions()
	{
		var services = new ServiceCollection();

		services.AddSkiaSharpViewsBlazor(o =>
		{
			o.TransferFormat = SKBlazorTransferFormat.Png;
			o.Quality = 42;
		});

		using var provider = services.BuildServiceProvider();
		var options = provider.GetService<SKBlazorOptions>();

		Assert.NotNull(options);
		Assert.Equal(SKBlazorTransferFormat.Png, options!.TransferFormat);
		Assert.Equal(42, options.Quality);
	}

	[Fact]
	public void AddWithoutConfigureUsesDefaults()
	{
		var services = new ServiceCollection();

		services.AddSkiaSharpViewsBlazor();

		using var provider = services.BuildServiceProvider();
		var options = provider.GetService<SKBlazorOptions>();

		Assert.NotNull(options);
		Assert.Null(options!.TransferFormat);
		Assert.Equal(85, options.Quality);
	}
}
