#nullable enable

using System;

namespace SkiaSharp.Views.Maui
{
	/// <summary>Provides data for the <see cref="E:SkiaSharp.Views.Maui.Controls.SKGraphiteView.PaintSurface" /> event.</summary>
	/// <remarks />
	public class SKPaintGraphiteSurfaceEventArgs : SKPaintSurfaceEventArgs
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKPaintGraphiteSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="context">The Graphite GPU context used to render the surface.</param>
		/// <param name="recorder">The Graphite recorder used to record drawing commands for the surface.</param>
		/// <param name="backendTexture">The backend texture that is currently being drawn.</param>
		/// <param name="info">The image information describing the surface.</param>
		/// <remarks />
		public SKPaintGraphiteSurfaceEventArgs(
			SKSurface surface,
			SKGraphiteContext? context,
			SKGraphiteRecorder? recorder,
			SKGraphiteBackendTexture? backendTexture,
			SKImageInfo info)
			: this(surface, context, recorder, backendTexture, info, info)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKPaintGraphiteSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="context">The Graphite GPU context used to render the surface.</param>
		/// <param name="recorder">The Graphite recorder used to record drawing commands for the surface.</param>
		/// <param name="backendTexture">The backend texture that is currently being drawn.</param>
		/// <param name="info">The image information describing the surface.</param>
		/// <param name="rawInfo">The raw image information of the underlying surface.</param>
		/// <remarks />
		public SKPaintGraphiteSurfaceEventArgs(
			SKSurface surface,
			SKGraphiteContext? context,
			SKGraphiteRecorder? recorder,
			SKGraphiteBackendTexture? backendTexture,
			SKImageInfo info,
			SKImageInfo rawInfo)
			: base(surface, info, rawInfo)
		{
			Context = context;
			Recorder = recorder;
			BackendTexture = backendTexture;
		}

		/// <summary>Gets the Graphite GPU context used to render the surface.</summary>
		/// <value>The Graphite GPU context, or <see langword="null" /> if unavailable.</value>
		/// <remarks />
		public SKGraphiteContext? Context { get; }

		/// <summary>Gets the Graphite recorder used to record drawing commands for the surface.</summary>
		/// <value>The Graphite recorder, or <see langword="null" /> if unavailable.</value>
		/// <remarks />
		public SKGraphiteRecorder? Recorder { get; }

		/// <summary>Gets the backend texture that is currently being drawn.</summary>
		/// <value>The current backend texture, or <see langword="null" /> if unavailable.</value>
		/// <remarks />
		public SKGraphiteBackendTexture? BackendTexture { get; }
	}
}
