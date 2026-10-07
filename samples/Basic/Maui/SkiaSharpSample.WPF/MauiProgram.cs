using Microsoft.Maui.Controls.Hosting.WPF;
using Microsoft.Maui.Handlers.WPF;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.Windows.WPF.Essentials;
using SkiaSharp.Views.Maui.Controls.Hosting;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent.WPF;
#endif

namespace SkiaSharpSample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// The preview Shell handler reads Page without realizing its ContentTemplate.
		ShellHandler.Mapper.PrependToMapping(nameof(Shell.Items), (_, shell) =>
		{
			foreach (var item in shell.Items)
				foreach (var section in item.Items)
					foreach (var content in section.Items)
						((IShellContentController)content).GetOrCreateContent();
		});

		var builder = MauiApp.CreateBuilder()
			.UseMauiAppWPF<App>()
			.UseWPFEssentials()
			.UseSkiaSharpWPF();
#if DEBUG
		// The preview agent shares one log file across apps.
		builder.AddMauiDevFlowAgent(options => options.EnableFileLogging = false);
#endif
		return builder.Build();
	}
}
