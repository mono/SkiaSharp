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

			// The Vulkan tests live in their own unsigned, platform-restricted library
			// (SkiaSharp.Vulkan.Tests). Register its assembly on the platforms where it
			// is referenced so both the test runner and the renderer catalog
			// (CatalogReflection scans loaded SkiaSharp.* assemblies) discover it. A
			// future Direct3D library would be added the same way.
			var testAssemblies = new[]
			{
				typeof(MauiProgram).Assembly,
				typeof(BaseTest).Assembly,
#if ANDROID || WINDOWS
				typeof(SkiaSharp.Vulkan.Tests.VKTest).Assembly,
#endif
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
