using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using SkiaSharp;
using SkiaSharp.Views.Mac;

namespace SkiaSharpSample
{
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
		NSTextField? sizeLabel;
		NSSlider? sizeSlider;
		SKPath? currentPath;
		int colorIndex;
		float brushSize = 4f;

		bool IsDarkMode =>
			NSApplication.SharedApplication.EffectiveAppearance.Name.ToString()
				.Contains("Dark", StringComparison.OrdinalIgnoreCase);
		SKColor CanvasBackground => IsDarkMode ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;
		SKColor ResolveColor(SKColor light, SKColor dark) => IsDarkMode ? dark : light;

		SKCanvasView? skiaView;

		public override void LoadView()
		{
			View = new NSView();
			skiaView = new SKCanvasView { TranslatesAutoresizingMaskIntoConstraints = false };
			View.AddSubview(skiaView);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			if (skiaView == null) return;

			skiaView.IgnorePixelScaling = true;
			skiaView.PaintSurface += OnPaintSurface;

			// Full-screen canvas
			NSLayoutConstraint.ActivateConstraints(new[]
			{
				skiaView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
				skiaView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
				skiaView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
				skiaView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
			});

			// Floating Clear button (top-right, translucent pill)
			var clearBtn = new NSButton
			{
				Title = "Clear",
				BezelStyle = NSBezelStyle.Rounded,
				WantsLayer = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			clearBtn.Layer!.BackgroundColor = new CGColor(0, 0, 0, 0.5f);
			clearBtn.Layer.CornerRadius = 14;
			clearBtn.ContentTintColor = NSColor.White;
			clearBtn.Bordered = false;
			clearBtn.Activated += (s, e) =>
			{
				foreach (var stroke in strokes)
					stroke.Path.Dispose();
				strokes.Clear();
				currentPath?.Dispose();
				currentPath = null;
				skiaView.NeedsDisplay = true;
			};
			View.AddSubview(clearBtn);

			NSLayoutConstraint.ActivateConstraints(new[]
			{
				clearBtn.TopAnchor.ConstraintEqualTo(View.TopAnchor, 12),
				clearBtn.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, -12),
				clearBtn.HeightAnchor.ConstraintEqualTo(28),
				clearBtn.WidthAnchor.ConstraintGreaterThanOrEqualTo(60),
			});

			// Floating toolbox (centered at bottom)
			var toolbox = new NSVisualEffectView
			{
				BlendingMode = NSVisualEffectBlendingMode.WithinWindow,
				Material = NSVisualEffectMaterial.HudWindow,
				State = NSVisualEffectState.Active,
				WantsLayer = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			toolbox.Layer!.CornerRadius = 24;
			toolbox.Layer.MasksToBounds = true;
			View.AddSubview(toolbox);

			NSLayoutConstraint.ActivateConstraints(new[]
			{
				toolbox.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
				toolbox.BottomAnchor.ConstraintEqualTo(View.BottomAnchor, -16),
			});

			// Horizontal stack: [swatches] [separator] [slider] [label]
			var stack = new NSStackView
			{
				Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
				Spacing = 10,
				EdgeInsets = new NSEdgeInsets(10, 16, 10, 16),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			toolbox.AddSubview(stack);

			NSLayoutConstraint.ActivateConstraints(new[]
			{
				stack.LeadingAnchor.ConstraintEqualTo(toolbox.LeadingAnchor),
				stack.TrailingAnchor.ConstraintEqualTo(toolbox.TrailingAnchor),
				stack.TopAnchor.ConstraintEqualTo(toolbox.TopAnchor),
				stack.BottomAnchor.ConstraintEqualTo(toolbox.BottomAnchor),
			});

			// Color swatches (circular, 32pt)
			for (int i = 0; i < palette.Length; i++)
			{
				var idx = i;
				var (light, dark) = palette[i];
				var resolved = ResolveColor(light, dark);
				var swatch = new NSView
				{
					WantsLayer = true,
					TranslatesAutoresizingMaskIntoConstraints = false,
				};
				swatch.Layer!.BackgroundColor = new CGColor(resolved.Red / 255f, resolved.Green / 255f, resolved.Blue / 255f);
				swatch.Layer.CornerRadius = 16;
				swatch.WidthAnchor.ConstraintEqualTo(32).Active = true;
				swatch.HeightAnchor.ConstraintEqualTo(32).Active = true;

				var click = new NSClickGestureRecognizer(() =>
				{
					colorIndex = idx;
					UpdateSwatchSelection(swatch);
					skiaView.NeedsDisplay = true;
				});
				swatch.AddGestureRecognizer(click);

				stack.AddArrangedSubview(swatch);
				paletteSwatches.Add(swatch);

				if (i == 0)
				{
					swatch.Layer.BorderWidth = 3;
					swatch.Layer.BorderColor = NSColor.SystemBlue.CGColor;
					selectedSwatch = swatch;
				}
			}

			// Separator
			var separator = new NSView
			{
				WantsLayer = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			separator.Layer!.BackgroundColor = NSColor.Separator.CGColor;
			separator.WidthAnchor.ConstraintEqualTo(1).Active = true;
			separator.HeightAnchor.ConstraintEqualTo(24).Active = true;
			stack.AddArrangedSubview(separator);

			// Brush size slider
			sizeSlider = new NSSlider
			{
				MinValue = 1,
				MaxValue = 50,
				DoubleValue = brushSize,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			sizeSlider.WidthAnchor.ConstraintEqualTo(120).Active = true;
			sizeSlider.Activated += (s, e) =>
			{
				brushSize = (float)sizeSlider!.DoubleValue;
				sizeLabel!.StringValue = $"{brushSize:F0}px";
			};
			stack.AddArrangedSubview(sizeSlider);

			// Size label
			sizeLabel = new NSTextField
			{
				StringValue = $"{brushSize:F0}px",
				Editable = false,
				Bordered = false,
				DrawsBackground = false,
				TextColor = NSColor.White,
				Font = NSFont.MonospacedDigitSystemFontOfSize(13, NSFontWeight.Regular),
			};
			stack.AddArrangedSubview(sizeLabel);
		}

		void UpdateSwatchSelection(NSView swatch)
		{
			if (selectedSwatch != null)
				selectedSwatch.Layer!.BorderWidth = 0;
			swatch.Layer!.BorderWidth = 3;
			swatch.Layer.BorderColor = NSColor.SystemBlue.CGColor;
			selectedSwatch = swatch;
		}

		public override void ViewWillDisappear()
		{
			base.ViewWillDisappear();
			if (skiaView != null)
				skiaView.PaintSurface -= OnPaintSurface;
		}

		public override void MouseDown(NSEvent theEvent)
		{
			if (skiaView == null) return;
			var pt = CanvasPoint(theEvent);
			if (pt == null) return;
			currentPath = new SKPath();
			currentPath.MoveTo(pt.Value);
			skiaView.NeedsDisplay = true;
		}

		public override void MouseDragged(NSEvent theEvent)
		{
			if (currentPath == null || skiaView == null) return;
			var pt = CanvasPoint(theEvent);
			if (pt == null) return;
			currentPath.LineTo(pt.Value);
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
			if (skiaView != null) skiaView.NeedsDisplay = true;
		}

		public override void ScrollWheel(NSEvent theEvent)
		{
			brushSize = Math.Clamp(brushSize + (float)theEvent.ScrollingDeltaY * 0.5f, 1f, 50f);
			sizeSlider!.DoubleValue = brushSize;
			sizeLabel!.StringValue = $"{brushSize:F0}px";
			if (skiaView != null) skiaView.NeedsDisplay = true;
		}

		SKPoint? CanvasPoint(NSEvent theEvent)
		{
			if (skiaView == null) return null;
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
}
