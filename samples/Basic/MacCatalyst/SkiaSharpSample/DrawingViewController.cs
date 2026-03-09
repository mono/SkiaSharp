using ObjCRuntime;
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
	private SKColor currentColorLight = SKColors.Black;
	private SKColor currentColorDark = SKColors.White;
	private float brushSize = 4f;
	private UIView? selectedSwatch;

	[Outlet("skiaView")] SKCanvasView skiaView { get; set; } = null!;
	[Outlet("clearButton")] UIButton clearButton { get; set; } = null!;
	[Outlet("brushSlider")] UISlider brushSlider { get; set; } = null!;
	[Outlet("brushLabel")] UILabel brushLabel { get; set; } = null!;
	[Outlet("swatchStack")] UIStackView swatchStack { get; set; } = null!;

	bool IsDarkMode => TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark;
	SKColor CanvasBackground => IsDarkMode ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;
	SKColor ResolveColor(SKColor light, SKColor dark) => IsDarkMode ? dark : light;
	SKColor CurrentColor => ResolveColor(currentColorLight, currentColorDark);

	public DrawingViewController(NativeHandle handle) : base(handle) { }

	public override UIRectEdge PreferredScreenEdgesDeferringSystemGestures => UIRectEdge.Bottom;

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		skiaView.IgnorePixelScaling = true;
		skiaView.PaintSurface += OnPaintSurface;
		clearButton.TouchUpInside += (_, _) => ClearCanvas();
		brushSlider.ValueChanged += (_, _) =>
		{
			brushSize = brushSlider.Value;
			brushLabel.Text = $"{brushSize:F0}px";
			skiaView.SetNeedsDisplay();
		};

		// Wire swatch tap gestures (swatches are defined in storyboard)
		var swatches = swatchStack.ArrangedSubviews;
		for (int i = 0; i < swatches.Length && i < colorPalette.Length; i++)
		{
			var swatch = swatches[i];
			var (_, light, dark) = colorPalette[i];
			var capturedLight = light;
			var capturedDark = dark;
			swatch.UserInteractionEnabled = true;
			swatch.AddGestureRecognizer(new UITapGestureRecognizer(() => SelectColor(capturedLight, capturedDark, swatch)));

			if (i == 0)
			{
				swatch.Layer.BorderWidth = 3;
				swatch.Layer.BorderColor = UIColor.SystemBlue.CGColor;
				selectedSwatch = swatch;
			}
		}

		var draw = new UIPanGestureRecognizer(HandleDraw);
		draw.MaximumNumberOfTouches = 1;
		skiaView.AddGestureRecognizer(draw);

		var pinch = new UIPinchGestureRecognizer(HandlePinch);
		pinch.CancelsTouchesInView = false;
		skiaView.AddGestureRecognizer(pinch);

		UpdateSwatchColors();
	}

	public override void ViewDidAppear(bool animated)
	{
		base.ViewDidAppear(animated);
		skiaView.SetNeedsDisplay();
	}

	private void SelectColor(SKColor light, SKColor dark, UIView swatch)
	{
		currentColorLight = light;
		currentColorDark = dark;
		if (selectedSwatch != null)
			selectedSwatch.Layer.BorderWidth = 0;
		swatch.Layer.BorderWidth = 3;
		swatch.Layer.BorderColor = UIColor.SystemBlue.CGColor;
		selectedSwatch = swatch;
	}

	private void HandleDraw(UIPanGestureRecognizer gesture)
	{
		var loc = gesture.LocationInView(skiaView);
		switch (gesture.State)
		{
			case UIGestureRecognizerState.Began:
				currentPath = new SKPath();
				currentPath.MoveTo((float)loc.X, (float)loc.Y);
				break;
			case UIGestureRecognizerState.Changed:
				currentPath?.LineTo((float)loc.X, (float)loc.Y);
				break;
			case UIGestureRecognizerState.Ended:
				if (currentPath != null)
				{
					strokes.Add(new Stroke(currentPath, CurrentColor, brushSize));
					currentPath = null;
				}
				break;
			case UIGestureRecognizerState.Cancelled:
				currentPath?.Dispose();
				currentPath = null;
				break;
		}
		skiaView.SetNeedsDisplay();
	}

	private void HandlePinch(UIPinchGestureRecognizer gesture)
	{
		if (gesture.State == UIGestureRecognizerState.Changed)
		{
			brushSize = Math.Clamp(brushSize * (float)gesture.Scale, 1f, 50f);
			gesture.Scale = 1;
			brushLabel.Text = $"{brushSize:F0}px";
			brushSlider.Value = brushSize;
			skiaView.SetNeedsDisplay();
		}
	}

	private void ClearCanvas()
	{
		foreach (var stroke in strokes)
			stroke.Path.Dispose();
		strokes.Clear();
		currentPath?.Dispose();
		currentPath = null;
		skiaView.SetNeedsDisplay();
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
			paint.Color = CurrentColor;
			paint.StrokeWidth = brushSize;
			canvas.DrawPath(currentPath, paint);
		}
	}

#pragma warning disable CA1422
	public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
	{
		base.TraitCollectionDidChange(previousTraitCollection);
		if (previousTraitCollection?.UserInterfaceStyle != TraitCollection.UserInterfaceStyle)
		{
			UpdateSwatchColors();
			skiaView.SetNeedsDisplay();
		}
	}
#pragma warning restore CA1422

	private void UpdateSwatchColors()
	{
		var swatches = swatchStack.ArrangedSubviews;
		for (int i = 0; i < swatches.Length && i < colorPalette.Length; i++)
		{
			var resolved = ResolveColor(colorPalette[i].Light, colorPalette[i].Dark);
			swatches[i].BackgroundColor = new UIColor(resolved.Red / 255f, resolved.Green / 255f, resolved.Blue / 255f, 1f);
		}
	}
}
