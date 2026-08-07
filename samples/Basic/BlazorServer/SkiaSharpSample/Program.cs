using Microsoft.AspNetCore.Components;
using MudBlazor.Services;
using SkiaSharpSample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// Mirror the WebAssembly sample: an HttpClient scoped to the app root (used by the Home page
// to load the bundled font) and MudBlazor.
builder.Services.AddScoped(sp =>
{
	var navigation = sp.GetRequiredService<NavigationManager>();
	return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
});
builder.Services.AddMudServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();

/// <summary>
/// Partial program class exposing the <see cref="DefaultPage"/> setting.
/// </summary>
public partial class Program
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;
}
