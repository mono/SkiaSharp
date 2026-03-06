using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaSharpSample;

public class DrawingViewController : UIViewController
{
	private readonly record struct Stroke(SKPath Path, SKColor Color, float Width);

	private static readonly (string Name, SKColor Color)[] colorPalette =
	{
		("Black", SKColors.Black),
		("Red", new SKColor(0xE5, 0x39, 0x35)),
		("Blue", new SKColor(0x1E, 0x88, 0xE5)),
		("Green", new SKColor(0x43, 0xA0, 0x47)),
		("Orange", new SKColor(0xFB, 0x8C, 0x00)),
		("Purple", new SKColor(0x8E, 0x24, 0xAA)),
	};

	private readonly List<Stroke> strokes = new();
	private SKPath? currentPath;
	private SKColor currentColor = SKColors.Black;
	private float brushSize = 4f;
	private bool isDrawing;

	private SKCanvasView? skiaView;
	private UILabel? brushLabel;
	private UIView? selectedSwatch;

	public override void LoadView()
	{
		var container = new UIView { BackgroundColor = UIColor.SystemBackground };

		// Canvas
		skiaView = new SKCanvasView { TranslatesAutoresizingMaskIntoConstraints = false };
		skiaView.IgnorePixelScaling = true;
		skiaView.PaintSurface += OnPaintSurface;
		container.AddSubview(skiaView);

		// Bottom toolbar
		var toolbar = new UIView
		{
			TranslatesAutoresizingMaskIntoConstraints = false,
			BackgroundColor = UIColor.SecondarySystemBackground,
		};
		container.AddSubview(toolbar);

		var stack = new UIStackView
		{
			TranslatesAutoresizingMaskIntoConstraints = false,
			Axis = UILayoutConstraintAxis.Horizontal,
			Spacing = 8,
			Alignment = UIStackViewAlignment.Center,
		};
		toolbar.AddSubview(stack);

		// Color swatches
		foreach (var (name, color) in colorPalette)
		{
			var swatch = new UIView
			{
				BackgroundColor = new UIColor(color.Red / 255f, color.Green / 255f, color.Blue / 255f, 1f),
			};
			swatch.Layer.CornerRadius = 16;
			swatch.TranslatesAutoresizingMaskIntoConstraints = false;
			swatch.WidthAnchor.ConstraintEqualTo(32).Active = true;
			swatch.HeightAnchor.ConstraintEqualTo(32).Active = true;

			var capturedColor = color;
			var tap = new UITapGestureRecognizer(() => SelectColor(capturedColor, swatch));
			swatch.AddGestureRecognizer(tap);
			swatch.UserInteractionEnabled = true;
			stack.AddArrangedSubview(swatch);

			if (name == "Black")
			{
				swatch.Layer.BorderWidth = 3;
				swatch.Layer.BorderColor = UIColor.SystemBlue.CGColor;
				selectedSwatch = swatch;
			}
		}

		// Flexible spacer
		var spacer = new UIView();
		spacer.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);
		stack.AddArrangedSubview(spacer);

		// Brush size label
		brushLabel = new UILabel
		{
			Text = $"{brushSize:F0}px",
			Font = UIFont.MonospacedSystemFontOfSize(14, UIFontWeight.Regular),
			TextColor = UIColor.Label,
		};
		stack.AddArrangedSubview(brushLabel);

		// Clear button
		var clearBtn = new UIButton(UIButtonType.System);
		clearBtn.SetImage(UIImage.GetSystemImage("trash"), UIControlState.Normal);
		clearBtn.TouchUpInside += (_, _) => ClearCanvas();
		stack.AddArrangedSubview(clearBtn);

		// Layout
		NSLayoutConstraint.ActivateConstraints(new[]
		{
			skiaView.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor),
			skiaView.TrailingAnchor.ConstraintEqualTo(container.TrailingAnchor),
			skiaView.TopAnchor.ConstraintEqualTo(container.TopAnchor),
			skiaView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

			toolbar.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor),
			toolbar.TrailingAnchor.ConstraintEqualTo(container.TrailingAnchor),
			toolbar.BottomAnchor.ConstraintEqualTo(container.SafeAreaLayoutGuide.BottomAnchor),
			toolbar.HeightAnchor.ConstraintEqualTo(52),

			stack.LeadingAnchor.ConstraintEqualTo(toolbar.LeadingAnchor, 16),
			stack.TrailingAnchor.ConstraintEqualTo(toolbar.TrailingAnchor, -16),
			stack.CenterYAnchor.ConstraintEqualTo(toolbar.CenterYAnchor),
		});

		// Pinch gesture for brush size
		var pinch = new UIPinchGestureRecognizer(HandlePinch);
		pinch.CancelsTouchesInView = false;
		skiaView.AddGestureRecognizer(pinch);

		View = container;
		Title = "Drawing";
	}

	private void SelectColor(SKColor color, UIView swatch)
	{
		currentColor = color;
		if (selectedSwatch != null)
			selectedSwatch.Layer.BorderWidth = 0;
		swatch.Layer.BorderWidth = 3;
		swatch.Layer.BorderColor = UIColor.SystemBlue.CGColor;
		selectedSwatch = swatch;
	}

	private void HandlePinch(UIPinchGestureRecognizer gesture)
	{
		if (gesture.State == UIGestureRecognizerState.Changed)
		{
			brushSize = Math.Clamp(brushSize * (float)gesture.Scale, 1f, 50f);
			gesture.Scale = 1;
			brushLabel!.Text = $"{brushSize:F0}px";
			skiaView?.SetNeedsDisplay();
		}
	}

	private void ClearCanvas()
	{
		foreach (var stroke in strokes)
			stroke.Path.Dispose();
		strokes.Clear();

		currentPath?.Dispose();
		currentPath = null;
		isDrawing = false;

		skiaView?.SetNeedsDisplay();
	}

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
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

		foreach (var stroke in strokes)
		{
			paint.Color = stroke.Color;
			paint.StrokeWidth = stroke.Width;
			canvas.DrawPath(stroke.Path, paint);
		}

		if (currentPath != null)
		{
			paint.Color = currentColor;
			paint.StrokeWidth = brushSize;
			canvas.DrawPath(currentPath, paint);
		}
	}

	public override void TouchesBegan(NSSet touches, UIEvent? evt)
	{
		base.TouchesBegan(touches, evt);
		if (touches.AnyObject is UITouch touch && skiaView != null)
		{
			isDrawing = true;
			var loc = touch.LocationInView(skiaView);
			currentPath = new SKPath();
			currentPath.MoveTo((float)loc.X, (float)loc.Y);
			skiaView.SetNeedsDisplay();
		}
	}

	public override void TouchesMoved(NSSet touches, UIEvent? evt)
	{
		base.TouchesMoved(touches, evt);
		if (isDrawing && currentPath != null && touches.AnyObject is UITouch touch && skiaView != null)
		{
			var loc = touch.LocationInView(skiaView);
			currentPath.LineTo((float)loc.X, (float)loc.Y);
			skiaView.SetNeedsDisplay();
		}
	}

	public override void TouchesEnded(NSSet touches, UIEvent? evt)
	{
		base.TouchesEnded(touches, evt);
		if (isDrawing && currentPath != null)
		{
			isDrawing = false;
			strokes.Add(new Stroke(currentPath, currentColor, brushSize));
			currentPath = null;
			skiaView?.SetNeedsDisplay();
		}
	}

	public override void TouchesCancelled(NSSet touches, UIEvent? evt)
	{
		base.TouchesCancelled(touches, evt);
		currentPath?.Dispose();
		currentPath = null;
		isDrawing = false;
		skiaView?.SetNeedsDisplay();
	}
}
