using SkiaSharp;
using SkiaSharpSample;
using SkiaSharpSample.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Image endpoint: serves SkiaSharp-rendered images by name
// Supports ?w=WIDTH&h=HEIGHT query params for dynamic sizing
app.MapGet("/api/skia/{name}", (string name, int? w, int? h) =>
{
    var width = Math.Clamp(w ?? 800, 50, 4000);
    var height = Math.Clamp(h ?? 300, 50, 4000);
    var info = new SKImageInfo(width, height);
    using var surface = SKSurface.Create(info);
    var canvas = surface.Canvas;

    SkiaRenderer.Draw(canvas, info, name);

    using var image = surface.Snapshot();
    using var data = image.Encode(SKEncodedImageFormat.Webp, 90);

    return Results.File(data.ToArray(), "image/webp");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SkiaSharpSample.Client._Imports).Assembly);

app.Run();
