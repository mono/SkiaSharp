using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SkiaSharpSample
{
	public class Startup : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder) =>
			appBuilder
				.UseSkiaSharp(true)
				.UseMauiApp<App>();
	}
}
