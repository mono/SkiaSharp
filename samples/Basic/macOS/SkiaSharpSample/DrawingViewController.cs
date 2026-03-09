using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using SkiaSharp;
using SkiaSharp.Views.Mac;

namespace SkiaSharpSample;

[Register("DrawingViewController")]
public class DrawingViewController : NSViewController
{
	static readonly (SKColor Light, SKColor Dark)[] palette =
	{
		(SKColors.Black, SKColors.White),
		(new SKColor(0xE5, 0x39, 0x35), new SKColor(0xEF, 0x53, 0x50)),  // red
		(new SKColor(0x1E, 0x88, 0xE5), new SKColor(0x42, 0xA5, 0xF5)),  // blue
		(new SKColor(0x43, 0xA0, 0x47), new SKColor(0x66, 0xBB, 0x6A)),  // green
		(new SKColor(0xFB, 0x8C, 0x00), new SKColor(0xFF, 0xA7, 0x26)),  // orange
		(new SKColor(0x8E, 0x24, 0xAA), new SKColor(0xAB, 0x47, 0xBC)),  // purple
	};

	readonly List<Stroke> strokes = new();
	readonly List<NSView> paletteSwatches = new();
	NSView? selectedSwatch;
	SKPath? currentPath;
	int colorIndex;
	float brushSize = 4f;

	bool IsDarkMode =>
		NSApplication.SharedApplication.EffectiveAppearance.Name.ToString()
			.Contains("Dark", StringComparison.OrdinalIgnoreCase);
	SKColor CanvasBackground => IsDarkMode ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;
	SKColor ResolveColor(SKColor light, SKColor dark) => IsDarkMode ? dark : light;

	[Outlet("skiaView")]
	SKCanvasView skiaView { get; set; } = null!;

	[Outlet("clearButton")]
	NSButton clearButton { get; set; } = null!;

	[Outlet("brushSlider")]
	NSSlider brushSlider { get; set; } = null!;

	[Outlet("brushLabel")]
	NSTextField brushLabel { get; set; } = null!;

	[Outlet("swatchStack")]
	NSStackView swatchStack { get; set; } = null!;

	[Outlet("toolbox")]
	NSBox toolbox { get; set; } = null!;

	public DrawingViewController(NativeHandle handle) : base(handle) { }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		skiaView.IgnorePixelScaling = true;
		skiaView.PaintSurface += OnPaintSurface;

		// Style toolbox with translucent background
		toolbox.WantsLayer = true;
		toolbox.Layer!.BackgroundColor = new CGColor(0.5f, 0.5f, 0.5f, 0.3f);
		toolbox.Layer.CornerRadius = 16;
		toolbox.Layer.MasksToBounds = true;

		clearButton.Activated += (s, e) =>
		{
			foreach (var stroke in strokes)
				stroke.Path.Dispose();
			strokes.Clear();
			currentPath?.Dispose();
			currentPath = null;
			skiaView.NeedsDisplay = true;
		};

		brushSlider.Activated += (s, e) =>
		{
			brushSize = (float)brushSlider.DoubleValue;
			brushLabel.StringValue = $"{brushSize:F0}px";
		};

		var arrangedSubviews = swatchStack.ArrangedSubviews;
		for (int i = 0; i < arrangedSubviews.Length && i < palette.Length; i++)
		{
			var idx = i;
			var swatch = arrangedSubviews[i];
			var (light, dark) = palette[i];
			var resolved = ResolveColor(light, dark);
			swatch.WantsLayer = true;
			swatch.Layer!.BackgroundColor = new CGColor(resolved.Red / 255f, resolved.Green / 255f, resolved.Blue / 255f);
			swatch.Layer.CornerRadius = 16;

			var click = new NSClickGestureRecognizer(() =>
			{
				colorIndex = idx;
				UpdateSwatchSelection(swatch);
				skiaView.NeedsDisplay = true;
			});
			swatch.AddGestureRecognizer(click);

			paletteSwatches.Add(swatch);

			if (i == 0)
			{
				swatch.Layer.BorderWidth = 3;
				swatch.Layer.BorderColor = NSColor.SystemBlue.CGColor;
				selectedSwatch = swatch;
			}
		}
	}

	void UpdateSwatchSelection(NSView swatch)
	{
		if (selectedSwatch != null)
			selectedSwatch.Layer!.BorderWidth = 0;
		swatch.Layer!.BorderWidth = 3;
		swatch.Layer.BorderColor = NSColor.SystemBlue.CGColor;
		selectedSwatch = swatch;
	}

	public override void MouseDown(NSEvent theEvent)
	{
		var pt = CanvasPoint(theEvent);
		currentPath = new SKPath();
		currentPath.MoveTo(pt);
		skiaView.NeedsDisplay = true;
	}

	public override void MouseDragged(NSEvent theEvent)
	{
		if (currentPath == null) return;
		var pt = CanvasPoint(theEvent);
		currentPath.LineTo(pt);
		skiaView.NeedsDisplay = true;
	}

	public override void MouseUp(NSEvent theEvent)
	{
		if (currentPath == null) return;
		var (light, dark) = palette[colorIndex];
		strokes.Add(new Stroke
		{
			Path = currentPath,
			Color = ResolveColor(light, dark),
			Width = brushSize,
		});
		currentPath = null;
		skiaView.NeedsDisplay = true;
	}

	public override void ScrollWheel(NSEvent theEvent)
	{
		brushSize = Math.Clamp(brushSize + (float)theEvent.ScrollingDeltaY * 0.5f, 1f, 50f);
		brushSlider.DoubleValue = brushSize;
		brushLabel.StringValue = $"{brushSize:F0}px";
		skiaView.NeedsDisplay = true;
	}

	SKPoint CanvasPoint(NSEvent theEvent)
	{
		var loc = skiaView.ConvertPointFromView(theEvent.LocationInWindow, null);
		// NSView origin is bottom-left; SkiaSharp canvas origin is top-left
		return new SKPoint((float)loc.X, (float)(skiaView.Bounds.Height - loc.Y));
	}

	void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		// Update palette swatch colors for current appearance
		for (int i = 0; i < paletteSwatches.Count && i < palette.Length; i++)
		{
			var (l, d) = palette[i];
			var c = ResolveColor(l, d);
			paletteSwatches[i].Layer!.BackgroundColor = new CGColor(c.Red / 255f, c.Green / 255f, c.Blue / 255f);
		}

		var canvas = e.Surface.Canvas;
		canvas.Clear(CanvasBackground);

		using var paint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			StrokeCap = SKStrokeCap.Round,
			StrokeJoin = SKStrokeJoin.Round,
		};

		// Draw completed strokes
		foreach (var stroke in strokes)
		{
			paint.Color = stroke.Color;
			paint.StrokeWidth = stroke.Width;
			canvas.DrawPath(stroke.Path, paint);
		}

		// Draw current in-progress stroke
		if (currentPath != null)
		{
			var (cl, cd) = palette[colorIndex];
			paint.Color = ResolveColor(cl, cd);
			paint.StrokeWidth = brushSize;
			canvas.DrawPath(currentPath, paint);
		}
	}

	class Stroke
	{
		public SKPath Path { get; init; } = null!;
		public SKColor Color { get; init; }
		public float Width { get; init; }
	}
}
