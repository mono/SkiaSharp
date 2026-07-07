using SkiaSharp.Views.Blazor;
using SkiaSharpSample.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// Optional: configure global defaults for SkiaSharp Blazor views.
builder.Services.AddSkiaSharpViewsBlazor(options =>
{
	// Server streams frames over SignalR, so JPEG keeps payloads small.
	options.TransferFormat = SKBlazorTransferFormat.Jpeg;
	options.Quality = 80;
});

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
