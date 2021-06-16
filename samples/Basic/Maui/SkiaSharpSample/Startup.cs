using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace SkiaSharpSample
{
	public class Startup : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder) =>
			appBuilder.UseMauiApp<App>();
	}
}
