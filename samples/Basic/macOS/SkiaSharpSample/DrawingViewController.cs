using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using SkiaSharp;
using SkiaSharp.Views.Mac;

namespace SkiaSharpSample
{
	public class DrawingViewController : NSViewController
	{
		static readonly SKColor[] palette =
		{
			SKColors.Black,
			new SKColor(0xFFE53935),  // red
			new SKColor(0xFF1E88E5),  // blue
			new SKColor(0xFF43A047),  // green
			new SKColor(0xFFFF8F00),  // amber
			new SKColor(0xFF8E24AA),  // purple
		};

		readonly List<Stroke> strokes = new();
		SKPath? currentPath;
		int colorIndex;
		float brushSize = 4f;
		SKCanvasView? canvasView;

		public override void LoadView()
		{
			var container = new NSView();

			canvasView = new SKCanvasView { IgnorePixelScaling = true };
			canvasView.TranslatesAutoresizingMaskIntoConstraints = false;
			canvasView.PaintSurface += OnPaintSurface;
			container.AddSubview(canvasView);

			var toolbar = CreateToolbar();
			toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
			container.AddSubview(toolbar);

			NSLayoutConstraint.ActivateConstraints(new[]
			{
				canvasView.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor),
				canvasView.TrailingAnchor.ConstraintEqualTo(container.TrailingAnchor),
				canvasView.TopAnchor.ConstraintEqualTo(container.TopAnchor),
				canvasView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

				toolbar.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor),
				toolbar.TrailingAnchor.ConstraintEqualTo(container.TrailingAnchor),
				toolbar.BottomAnchor.ConstraintEqualTo(container.BottomAnchor),
				toolbar.HeightAnchor.ConstraintEqualTo(40),
			});

			View = container;
		}

		NSView CreateToolbar()
		{
			var toolbar = new NSView();
			toolbar.WantsLayer = true;
			toolbar.Layer!.BackgroundColor = NSColor.WindowBackground.CGColor;

			var stack = new NSStackView
			{
				Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
				Spacing = 6,
				EdgeInsets = new NSEdgeInsets(4, 8, 4, 8),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			for (int i = 0; i < palette.Length; i++)
			{
				var idx = i;
				var color = palette[i];
				var btn = new NSButton
				{
					Title = "",
					BezelStyle = NSBezelStyle.SmallSquare,
					WantsLayer = true,
					Bordered = false,
				};
				btn.Layer!.BackgroundColor = new CGColor(color.Red / 255f, color.Green / 255f, color.Blue / 255f);
				btn.Layer.CornerRadius = 4;
				btn.WidthAnchor.ConstraintEqualTo(28).Active = true;
				btn.HeightAnchor.ConstraintEqualTo(28).Active = true;
				btn.Activated += (s, e) =>
				{
					colorIndex = idx;
					canvasView!.NeedsDisplay = true;
				};
				stack.AddArrangedSubview(btn);
			}

			var separator = new NSView();
			separator.WidthAnchor.ConstraintEqualTo(1).Active = true;
			separator.WantsLayer = true;
			separator.Layer!.BackgroundColor = NSColor.Separator.CGColor;
			stack.AddArrangedSubview(separator);

			var sizeLabel = new NSTextField
			{
				StringValue = $"Size: {brushSize:F0}",
				Editable = false,
				Bordered = false,
				DrawsBackground = false,
			};
			stack.AddArrangedSubview(sizeLabel);

			var sizeSlider = new NSSlider
			{
				MinValue = 1,
				MaxValue = 50,
				DoubleValue = brushSize,
			};
			sizeSlider.WidthAnchor.ConstraintEqualTo(120).Active = true;
			sizeSlider.Activated += (s, e) =>
			{
				brushSize = (float)sizeSlider.DoubleValue;
				sizeLabel.StringValue = $"Size: {brushSize:F0}";
			};
			stack.AddArrangedSubview(sizeSlider);

			var spacer = new NSView();
			stack.AddArrangedSubview(spacer);
			spacer.SetContentHuggingPriorityForOrientation(1, NSLayoutConstraintOrientation.Horizontal);

			var clearBtn = new NSButton
			{
				Title = "Clear",
				BezelStyle = NSBezelStyle.Rounded,
			};
			clearBtn.Activated += (s, e) =>
			{
				strokes.Clear();
				canvasView!.NeedsDisplay = true;
			};
			stack.AddArrangedSubview(clearBtn);

			toolbar.AddSubview(stack);
			NSLayoutConstraint.ActivateConstraints(new[]
			{
				stack.LeadingAnchor.ConstraintEqualTo(toolbar.LeadingAnchor),
				stack.TrailingAnchor.ConstraintEqualTo(toolbar.TrailingAnchor),
				stack.TopAnchor.ConstraintEqualTo(toolbar.TopAnchor),
				stack.BottomAnchor.ConstraintEqualTo(toolbar.BottomAnchor),
			});

			return toolbar;
		}

		public override void MouseDown(NSEvent theEvent)
		{
			if (canvasView == null) return;
			var pt = CanvasPoint(theEvent);
			if (pt == null) return;
			currentPath = new SKPath();
			currentPath.MoveTo(pt.Value);
			canvasView.NeedsDisplay = true;
		}

		public override void MouseDragged(NSEvent theEvent)
		{
			if (currentPath == null || canvasView == null) return;
			var pt = CanvasPoint(theEvent);
			if (pt == null) return;
			currentPath.LineTo(pt.Value);
			canvasView.NeedsDisplay = true;
		}

		public override void MouseUp(NSEvent theEvent)
		{
			if (currentPath == null) return;
			strokes.Add(new Stroke
			{
				Path = currentPath,
				Color = palette[colorIndex],
				Width = brushSize,
			});
			currentPath = null;
			canvasView!.NeedsDisplay = true;
		}

		public override void ScrollWheel(NSEvent theEvent)
		{
			brushSize = Math.Clamp(brushSize + (float)theEvent.ScrollingDeltaY * 0.5f, 1f, 50f);
			canvasView!.NeedsDisplay = true;
		}

		SKPoint? CanvasPoint(NSEvent theEvent)
		{
			if (canvasView == null) return null;
			var loc = canvasView.ConvertPointFromView(theEvent.LocationInWindow, null);
			// NSView origin is bottom-left; SkiaSharp canvas origin is top-left
			return new SKPoint((float)loc.X, (float)(canvasView.Bounds.Height - loc.Y));
		}

		void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			canvas.Clear(SKColors.White);

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
				paint.Color = palette[colorIndex];
				paint.StrokeWidth = brushSize;
				canvas.DrawPath(currentPath, paint);
			}

			// Hint text when empty
			if (strokes.Count == 0 && currentPath == null)
			{
				using var textPaint = new SKPaint
				{
					Color = new SKColor(0, 0, 0, 80),
					IsAntialias = true,
				};
				using var font = new SKFont { Size = 20 };
				canvas.DrawText("Draw here — use scroll wheel to change brush size",
					new SKPoint(e.Info.Width / 2f, e.Info.Height / 2f),
					SKTextAlign.Center, font, textPaint);
			}
		}

		class Stroke
		{
			public SKPath Path { get; init; } = null!;
			public SKColor Color { get; init; }
			public float Width { get; init; }
		}
	}
}
