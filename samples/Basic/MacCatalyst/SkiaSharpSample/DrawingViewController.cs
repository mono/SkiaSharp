using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaSharpSample;

public class DrawingViewController : UIViewController
{
	SKCanvasView? skiaView;

	readonly List<List<SKPoint>> completedStrokes = new();
	List<SKPoint>? currentStroke;

	readonly SKColor[] strokeColors =
	{
		new(0x20, 0x60, 0xE0),
		new(0xE0, 0x40, 0x40),
		new(0x40, 0xB0, 0x40),
		new(0xE0, 0x90, 0x20),
		new(0x90, 0x40, 0xD0),
	};

	public DrawingViewController()
	{
		Title = "Drawing";
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		View!.BackgroundColor = UIColor.SystemBackground;

		NavigationItem.RightBarButtonItem = new UIBarButtonItem(
			UIBarButtonSystemItem.Trash,
			(s, e) =>
			{
				completedStrokes.Clear();
				currentStroke = null;
				skiaView?.SetNeedsDisplay();
			});

		skiaView = new SKCanvasView
		{
			BackgroundColor = UIColor.White,
			TranslatesAutoresizingMaskIntoConstraints = false,
		};
		skiaView.PaintSurface += OnPaintSurface;

		View.AddSubview(skiaView);
		NSLayoutConstraint.ActivateConstraints(new[]
		{
			skiaView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
			skiaView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
			skiaView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
			skiaView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
		});
	}

	void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var info = e.Info;
		canvas.Clear(SKColors.White);

		float scaleX = info.Width / (float)skiaView!.Bounds.Width;
		float scaleY = info.Height / (float)skiaView.Bounds.Height;

		// Hint text when empty
		if (completedStrokes.Count == 0 && currentStroke == null)
		{
			using var hintPaint = new SKPaint
			{
				Color = new SKColor(0xAA, 0xAA, 0xAA),
				IsAntialias = true,
			};
			using var hintFont = new SKFont { Size = 32 };
			canvas.DrawText("Touch or click and drag to draw",
				new SKPoint(info.Width / 2f, info.Height / 2f),
				SKTextAlign.Center, hintFont, hintPaint);
		}

		using var paint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 4,
			StrokeCap = SKStrokeCap.Round,
			StrokeJoin = SKStrokeJoin.Round,
		};

		// Draw completed strokes
		for (int i = 0; i < completedStrokes.Count; i++)
		{
			paint.Color = strokeColors[i % strokeColors.Length];
			DrawStroke(canvas, completedStrokes[i], paint, scaleX, scaleY);
		}

		// Draw current stroke
		if (currentStroke is { Count: >= 2 })
		{
			paint.Color = strokeColors[completedStrokes.Count % strokeColors.Length];
			DrawStroke(canvas, currentStroke, paint, scaleX, scaleY);
		}

		// Stroke counter
		int total = completedStrokes.Count + (currentStroke != null ? 1 : 0);
		if (total > 0)
		{
			using var counterPaint = new SKPaint
			{
				Color = new SKColor(0x66, 0x66, 0x66),
				IsAntialias = true,
			};
			using var counterFont = new SKFont { Size = 24 };
			canvas.DrawText($"{completedStrokes.Count} stroke{(completedStrokes.Count == 1 ? "" : "s")}",
				new SKPoint(20, info.Height - 20), SKTextAlign.Left, counterFont, counterPaint);
		}
	}

	static void DrawStroke(SKCanvas canvas, List<SKPoint> points, SKPaint paint, float scaleX, float scaleY)
	{
		if (points.Count < 2)
			return;

		using var path = new SKPath();
		var first = points[0];
		path.MoveTo(first.X * scaleX, first.Y * scaleY);

		for (int i = 1; i < points.Count; i++)
		{
			var pt = points[i];
			path.LineTo(pt.X * scaleX, pt.Y * scaleY);
		}

		canvas.DrawPath(path, paint);
	}

	public override void TouchesBegan(NSSet touches, UIEvent? evt)
	{
		base.TouchesBegan(touches, evt);
		if (touches.AnyObject is UITouch touch && skiaView != null)
		{
			var loc = touch.LocationInView(skiaView);
			currentStroke = new List<SKPoint> { new((float)loc.X, (float)loc.Y) };
			skiaView.SetNeedsDisplay();
		}
	}

	public override void TouchesMoved(NSSet touches, UIEvent? evt)
	{
		base.TouchesMoved(touches, evt);
		if (touches.AnyObject is UITouch touch && skiaView != null && currentStroke != null)
		{
			var loc = touch.LocationInView(skiaView);
			currentStroke.Add(new SKPoint((float)loc.X, (float)loc.Y));
			skiaView.SetNeedsDisplay();
		}
	}

	public override void TouchesEnded(NSSet touches, UIEvent? evt)
	{
		base.TouchesEnded(touches, evt);
		FinishStroke();
	}

	public override void TouchesCancelled(NSSet touches, UIEvent? evt)
	{
		base.TouchesCancelled(touches, evt);
		FinishStroke();
	}

	void FinishStroke()
	{
		if (currentStroke is { Count: >= 2 })
			completedStrokes.Add(currentStroke);
		currentStroke = null;
		skiaView?.SetNeedsDisplay();
	}
}
