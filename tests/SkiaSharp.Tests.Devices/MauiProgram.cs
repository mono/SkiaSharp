using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Compatibility.Hosting;

namespace SkiaSharp.Tests
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp() =>
			MauiApp
				.CreateBuilder()
				.UseSkiaSharp(true)
				.UseMauiApp<App>()
				.Build();
	}
}
