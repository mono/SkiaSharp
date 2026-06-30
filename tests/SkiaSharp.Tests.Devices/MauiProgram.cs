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
					.AddCliConfiguration()
					.AddConsoleResultChannel()
					.AddTestAssemblies(testAssemblies)
					.AddXunit3());

#if WINDOWS
			builder.Logging.AddDebug();
#else
			builder.Logging.AddConsole();
#endif

			return builder.Build();
		}
	}
}
