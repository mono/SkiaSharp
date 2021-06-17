using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Compatibility;

namespace SkiaSharpSample
{
	public class Startup : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder) =>
			appBuilder
				.UseSkiaSharpCompatibilityRenderers()
				.UseSkiaSharpHandlers()
				.UseMauiApp<App>();
	}
}
