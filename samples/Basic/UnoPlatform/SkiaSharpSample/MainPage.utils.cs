using System.Reflection;
using HarfBuzzSharp;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace SkiaSharpSample;

public partial class MainPage
{
	public void InitializeVersionsContextMenu()
	{
		if (Content is not UIElement rootControl)
			return;

		MenuFlyoutSubItem modules;
		rootControl.ContextFlyout = new MenuFlyout
		{
			Items =
			{
				new MenuFlyoutItem { Text = GetTextV("libSkiaSharp", SkiaSharpVersion.Native) },
				new MenuFlyoutItem { Text = GetText<SKPaint>("SkiaSharp") },
				new MenuFlyoutItem { Text = GetText<SKXamlCanvas>("Views") },
				new MenuFlyoutItem { Text = GetText<Face>("HarfBuzz") },
				new MenuFlyoutSeparator(),
				(modules = new MenuFlyoutSubItem
				{
					Text = "Loaded Modules",
					Items = { new MenuFlyoutItem { Text = "Loading..." } }
				})
			}
		};
		rootControl.ContextFlyout.Opening += (s, e) =>
		{
			modules.Items.Clear();

			var assemblies = AppDomain.CurrentDomain
				.GetAssemblies()
				.Select(a => a.GetName().Name)
				.OrderBy(n => n);
			foreach (var ass in assemblies)
			{
				modules.Items.Add(new MenuFlyoutItem { Text = ass });
			}
		};

		static string? GetText<T>(string name) =>
			GetTextS(name, typeof(T).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

		static string? GetTextV(string name, Version version) =>
			GetTextS(name, version.ToString());

		static string? GetTextS(string name, string? version) =>
			$"{name} Version: {version}";
	}
}
