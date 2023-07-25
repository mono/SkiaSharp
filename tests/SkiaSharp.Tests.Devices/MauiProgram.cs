using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;
using DeviceRunners.XHarness;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

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
				.ConfigureUITesting()
				.UseXHarnessTestRunner(conf => conf
					.AddTestAssemblies(testAssemblies)
					.AddXunit())
				.UseVisualTestRunner(conf => conf
					.AddTestAssemblies(testAssemblies)
					.AddXunit());

#if WINDOWS
			builder.Logging.AddDebug();
#else
			builder.Logging.AddConsole();
#endif

			return builder.Build();
		}
	}
}
