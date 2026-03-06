using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaSharpSample;

[Register("DrawingViewController")]
public class DrawingViewController : UIViewController
{
	private readonly record struct Stroke(SKPath Path, SKColor Color, float Width);

	private static readonly (string Name, SKColor Light, SKColor Dark)[] colorPalette =
	{
		("Black", SKColors.Black, SKColors.White),
		("Red", new SKColor(0xE5, 0x39, 0x35), new SKColor(0xEF, 0x53, 0x50)),
		("Blue", new SKColor(0x1E, 0x88, 0xE5), new SKColor(0x42, 0xA5, 0xF5)),
		("Green", new SKColor(0x43, 0xA0, 0x47), new SKColor(0x66, 0xBB, 0x6A)),
		("Orange", new SKColor(0xFB, 0x8C, 0x00), new SKColor(0xFF, 0xA7, 0x26)),
		("Purple", new SKColor(0x8E, 0x24, 0xAA), new SKColor(0xAB, 0x47, 0xBC)),
	};

	private readonly List<Stroke> strokes = new();
	private SKPath? currentPath;
	private SKColor currentColor;
	private float brushSize = 4f;
	private bool isDrawing;

	private SKCanvasView? skiaView;

	private UILabel? brushLabel;
	private UIView? selectedSwatch;
	private readonly List<(UIView View, SKColor Light, SKColor Dark)> swatchViews = new();

	bool IsDarkMode => TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark;
	SKColor CanvasBackground => IsDarkMode ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;
	SKColor CurrentColor => currentColor;

	public DrawingViewController(IntPtr handle) : base(handle)
	{
		currentColor = SKColors.Black;
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		Title = "Drawing";
		View!.BackgroundColor = UIColor.SystemBackground;

		currentColor = IsDarkMode ? SKColors.White : SKColors.Black;

		skiaView = new SKCanvasView
		{
			TranslatesAutoresizingMaskIntoConstraints = false,
			IgnorePixelScaling = true,
		};
		View.AddSubview(skiaView);
		skiaView.PaintSurface += OnPaintSurface;

		// Bottom toolbar
		var toolbar = new UIView
		{
			TranslatesAutoresizingMaskIntoConstraints = false,
			BackgroundColor = UIColor.SecondarySystemBackground,
		};
		View.AddSubview(toolbar);

		var stack = new UIStackView
		{
			TranslatesAutoresizingMaskIntoConstraints = false,
			Axis = UILayoutConstraintAxis.Horizontal,
			Spacing = 8,
			Alignment = UIStackViewAlignment.Center,
		};
		toolbar.AddSubview(stack);

		// Color swatches
		foreach (var (name, light, dark) in colorPalette)
		{
			var color = IsDarkMode ? dark : light;
			var swatch = new UIView
			{
				BackgroundColor = new UIColor(color.Red / 255f, color.Green / 255f, color.Blue / 255f, 1f),
			};
			swatch.Layer.CornerRadius = 16;
			swatch.TranslatesAutoresizingMaskIntoConstraints = false;
			swatch.WidthAnchor.ConstraintEqualTo(32).Active = true;
			swatch.HeightAnchor.ConstraintEqualTo(32).Active = true;

			var capturedLight = light;
			var capturedDark = dark;
			var tap = new UITapGestureRecognizer(() =>
			{
				currentColor = IsDarkMode ? capturedDark : capturedLight;
				UpdateSwatchSelection(swatch);
			});
			swatch.AddGestureRecognizer(tap);
			swatch.UserInteractionEnabled = true;
			stack.AddArrangedSubview(swatch);
			swatchViews.Add((swatch, light, dark));

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
			Font = UIFont.MonospacedDigitSystemFontOfSize(14, UIFontWeight.Regular),
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
			skiaView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
			skiaView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
			skiaView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
			skiaView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

			toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
			toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
			toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
			toolbar.HeightAnchor.ConstraintEqualTo(52),

			stack.LeadingAnchor.ConstraintEqualTo(toolbar.LeadingAnchor, 16),
			stack.TrailingAnchor.ConstraintEqualTo(toolbar.TrailingAnchor, -16),
			stack.CenterYAnchor.ConstraintEqualTo(toolbar.CenterYAnchor),
		});

		// Pinch gesture for brush size
		var pinch = new UIPinchGestureRecognizer(HandlePinch);
		pinch.CancelsTouchesInView = false;
		skiaView.AddGestureRecognizer(pinch);
	}

	private void UpdateSwatchSelection(UIView swatch)
	{
		if (selectedSwatch != null)
			selectedSwatch.Layer.BorderWidth = 0;
		swatch.Layer.BorderWidth = 3;
		swatch.Layer.BorderColor = UIColor.SystemBlue.CGColor;
		selectedSwatch = swatch;
	}

	private void UpdateSwatchColors()
	{
		foreach (var (view, light, dark) in swatchViews)
		{
			var c = IsDarkMode ? dark : light;
			view.BackgroundColor = new UIColor(c.Red / 255f, c.Green / 255f, c.Blue / 255f, 1f);
		}
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
		canvas.Clear(CanvasBackground);

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

	public override void ViewWillDisappear(bool animated)
	{
		base.ViewWillDisappear(animated);
		foreach (var stroke in strokes)
			stroke.Path.Dispose();
		strokes.Clear();
		currentPath?.Dispose();
		currentPath = null;
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

	public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
	{
		base.TraitCollectionDidChange(previousTraitCollection);
		if (previousTraitCollection?.UserInterfaceStyle != TraitCollection.UserInterfaceStyle)
		{
			if (currentColor == SKColors.Black && IsDarkMode)
				currentColor = SKColors.White;
			else if (currentColor == SKColors.White && !IsDarkMode)
				currentColor = SKColors.Black;
			UpdateSwatchColors();
			skiaView?.SetNeedsDisplay();
		}
	}
}
