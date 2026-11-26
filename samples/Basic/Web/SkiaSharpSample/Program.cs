var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// ASP.NET Core Razor Pages and MVC controllers are not trim-annotated; suppress IL2026 on their registration
// while keeping AOT/trim analysis enabled for the sample's own controller and SkiaSharp rendering code.
#pragma warning disable IL2026
builder.Services.AddRazorPages();
builder.Services.AddControllers();
#pragma warning restore IL2026

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();

app.Run();
