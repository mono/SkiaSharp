using System;
using System.Linq;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Controls;

namespace SkiaSharpSample;

public sealed class GraphitePage : GpuPage
{
	private SKGraphiteRecorder? recorder;
	private SKImage? badge;

	public GraphitePage() : base(true)
	{
		var view = ((Grid)Content).Children.OfType<SKGraphiteView>().Single();
		view.ContextCreated += (_, _) =>
		{
			recorder = view.CreateRecorder();
			using var surface = SKSurface.Create(recorder, new SKImageInfo(96, 32))
				?? throw new InvalidOperationException("Could not create the recording demo surface.");
			surface.Canvas.Clear(SKColors.Transparent);
			using var paint = new SKPaint { Color = SKColors.White.WithAlpha(160), IsAntialias = true };
			surface.Canvas.DrawRoundRect(new SKRect(0, 0, 96, 32), 8, 8, paint);
			using var recording = view.SnapRecording(recorder);
			view.InsertRecording(recording);
			badge = surface.Snapshot();
		};
		view.PaintSurface += (_, e) =>
		{
			if (badge is not null) e.Surface.Canvas.DrawImage(badge, 16, e.Info.Height - 48);
		};
		view.ContextDestroying += (_, _) =>
		{
			badge?.Dispose(); badge = null;
			recorder?.Dispose(); recorder = null;
		};
	}
}
