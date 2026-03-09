using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SkiaSharp.Tests
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			TestConfig.Current = new DevicesTestConfig();

			AssetCopier.CopyAssets();

			var builder = MauiApp.CreateBuilder();

			var testAssemblies = new[]
			{
				typeof(MauiProgram).Assembly,
				typeof(BaseTest).Assembly
			};

			builder
				.UseSkiaSharp()
				.ConfigureUITesting()
				.UseVisualTestRunner(conf => conf
#if MODE_NON_INTERACTIVE_VISUAL
					.EnableAutoStart(true)
					.AddTcpResultChannel(new TcpResultChannelOptions
					{
						HostNames = ["localhost", "10.0.2.2"],
						Port = 16384,
						Formatter = new TextResultChannelFormatter(),
						Required = false,
						Retries = 3,
						RetryTimeout = TimeSpan.FromSeconds(5),
						Timeout = TimeSpan.FromSeconds(30)
					})
#endif
					.AddConsoleResultChannel()
					.AddTestAssemblies(testAssemblies)
					.AddXunit()
					.SkipCategory(Traits.FailingOn.Key, Traits.FailingOn.Values.GetCurrent())
					.SkipCategory(Traits.SkipOn.Key, Traits.SkipOn.Values.GetCurrent()));

#if WINDOWS
			builder.Logging.AddDebug();
#else
			builder.Logging.AddConsole();
#endif

			return builder.Build();
		}
	}
}
