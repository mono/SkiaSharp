using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SkiaSharp.Views.Blazor;

/// <summary>
/// Dependency-injection helpers for SkiaSharp Blazor views.
/// </summary>
public static class SKBlazorServiceCollectionExtensions
{
	/// <summary>
	/// Registers global defaults for SkiaSharp Blazor views. Calling this is optional; when
	/// it is not called, sensible per-host defaults are used.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="configure">An optional callback to configure the defaults.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddSkiaSharpViewsBlazor(
		this IServiceCollection services,
		Action<SKBlazorOptions>? configure = null)
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		var options = new SKBlazorOptions();
		configure?.Invoke(options);

		services.TryAddSingleton(options);

		return services;
	}
}
