#nullable enable

using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp.Views.Maui;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <summary>
	/// Extension methods to convert MAUI brush types to SkiaSharp paints and shaders.
	/// </summary>
	public static class BrushExtensions
	{
		/// <summary>
		/// Creates an <see cref="SKPaint"/> from a MAUI <see cref="Brush"/>.
		/// </summary>
		/// <param name="brush">The MAUI brush to convert.</param>
		/// <param name="bounds">The bounds to use for gradient calculations.</param>
		/// <returns>An SKPaint configured with the brush properties, or null if brush is null.</returns>
		public static SKPaint? ToSKPaint(this Brush? brush, SKRect bounds)
		{
			if (brush == null)
				return null;

			var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill
			};

			switch (brush)
			{
				case SolidColorBrush solidBrush:
					paint.Color = solidBrush.Color.ToSKColor();
					break;

				case LinearGradientBrush linearBrush:
					paint.Shader = CreateLinearGradientShader(linearBrush, bounds);
					break;

				case RadialGradientBrush radialBrush:
					paint.Shader = CreateRadialGradientShader(radialBrush, bounds);
					break;

				default:
					// For unsupported brush types, use transparent
					paint.Color = SKColors.Transparent;
					break;
			}

			return paint;
		}

		/// <summary>
		/// Creates an <see cref="SKPaint"/> from a MAUI <see cref="Brush"/> configured for stroking.
		/// </summary>
		/// <param name="brush">The MAUI brush to convert.</param>
		/// <param name="bounds">The bounds to use for gradient calculations.</param>
		/// <param name="strokeWidth">The stroke width.</param>
		/// <param name="strokeLineCap">The stroke line cap style.</param>
		/// <param name="strokeLineJoin">The stroke line join style.</param>
		/// <param name="strokeDashArray">The stroke dash pattern.</param>
		/// <param name="strokeDashOffset">The stroke dash offset.</param>
		/// <param name="strokeMiterLimit">The stroke miter limit.</param>
		/// <returns>An SKPaint configured for stroking, or null if brush is null.</returns>
		public static SKPaint? ToSKStrokePaint(
			this Brush? brush,
			SKRect bounds,
			float strokeWidth,
			Microsoft.Maui.Controls.Shapes.PenLineCap strokeLineCap = Microsoft.Maui.Controls.Shapes.PenLineCap.Flat,
			Microsoft.Maui.Controls.Shapes.PenLineJoin strokeLineJoin = Microsoft.Maui.Controls.Shapes.PenLineJoin.Miter,
			DoubleCollection? strokeDashArray = null,
			float strokeDashOffset = 0,
			float strokeMiterLimit = 10)
		{
			var paint = brush.ToSKPaint(bounds);
			if (paint == null)
				return null;

			paint.Style = SKPaintStyle.Stroke;
			paint.StrokeWidth = strokeWidth;
			paint.StrokeCap = strokeLineCap.ToSKStrokeCap();
			paint.StrokeJoin = strokeLineJoin.ToSKStrokeJoin();
			paint.StrokeMiter = strokeMiterLimit;

			if (strokeDashArray != null && strokeDashArray.Count > 0)
			{
				var dashPattern = strokeDashArray.Select(d => (float)(d * strokeWidth)).ToArray();
				
				// Ensure even number of elements
				if (dashPattern.Length % 2 != 0)
				{
					var newPattern = new float[dashPattern.Length * 2];
					Array.Copy(dashPattern, 0, newPattern, 0, dashPattern.Length);
					Array.Copy(dashPattern, 0, newPattern, dashPattern.Length, dashPattern.Length);
					dashPattern = newPattern;
				}

				paint.PathEffect = SKPathEffect.CreateDash(dashPattern, strokeDashOffset * strokeWidth);
			}

			return paint;
		}

		/// <summary>
		/// Converts a MAUI <see cref="Microsoft.Maui.Controls.Shapes.PenLineCap"/> to <see cref="SKStrokeCap"/>.
		/// </summary>
		public static SKStrokeCap ToSKStrokeCap(this Microsoft.Maui.Controls.Shapes.PenLineCap lineCap)
		{
			return lineCap switch
			{
				Microsoft.Maui.Controls.Shapes.PenLineCap.Flat => SKStrokeCap.Butt,
				Microsoft.Maui.Controls.Shapes.PenLineCap.Round => SKStrokeCap.Round,
				Microsoft.Maui.Controls.Shapes.PenLineCap.Square => SKStrokeCap.Square,
				_ => SKStrokeCap.Butt
			};
		}

		/// <summary>
		/// Converts a MAUI <see cref="Microsoft.Maui.Controls.Shapes.PenLineJoin"/> to <see cref="SKStrokeJoin"/>.
		/// </summary>
		public static SKStrokeJoin ToSKStrokeJoin(this Microsoft.Maui.Controls.Shapes.PenLineJoin lineJoin)
		{
			return lineJoin switch
			{
				Microsoft.Maui.Controls.Shapes.PenLineJoin.Miter => SKStrokeJoin.Miter,
				Microsoft.Maui.Controls.Shapes.PenLineJoin.Bevel => SKStrokeJoin.Bevel,
				Microsoft.Maui.Controls.Shapes.PenLineJoin.Round => SKStrokeJoin.Round,
				_ => SKStrokeJoin.Miter
			};
		}

		private static SKShader CreateLinearGradientShader(LinearGradientBrush brush, SKRect bounds)
		{
			var startPoint = new SKPoint(
				bounds.Left + (float)(brush.StartPoint.X * bounds.Width),
				bounds.Top + (float)(brush.StartPoint.Y * bounds.Height));

			var endPoint = new SKPoint(
				bounds.Left + (float)(brush.EndPoint.X * bounds.Width),
				bounds.Top + (float)(brush.EndPoint.Y * bounds.Height));

			var colors = brush.GradientStops
				.OrderBy(gs => gs.Offset)
				.Select(gs => gs.Color.ToSKColor())
				.ToArray();

			var positions = brush.GradientStops
				.OrderBy(gs => gs.Offset)
				.Select(gs => (float)gs.Offset)
				.ToArray();

			if (colors.Length == 0)
			{
				return SKShader.CreateColor(SKColors.Transparent);
			}

			return SKShader.CreateLinearGradient(
				startPoint,
				endPoint,
				colors,
				positions,
				SKShaderTileMode.Clamp);
		}

		private static SKShader CreateRadialGradientShader(RadialGradientBrush brush, SKRect bounds)
		{
			var center = new SKPoint(
				bounds.Left + (float)(brush.Center.X * bounds.Width),
				bounds.Top + (float)(brush.Center.Y * bounds.Height));

			var radius = (float)(brush.Radius * Math.Max(bounds.Width, bounds.Height));

			var colors = brush.GradientStops
				.OrderBy(gs => gs.Offset)
				.Select(gs => gs.Color.ToSKColor())
				.ToArray();

			var positions = brush.GradientStops
				.OrderBy(gs => gs.Offset)
				.Select(gs => (float)gs.Offset)
				.ToArray();

			if (colors.Length == 0)
			{
				return SKShader.CreateColor(SKColors.Transparent);
			}

			return SKShader.CreateRadialGradient(
				center,
				radius,
				colors,
				positions,
				SKShaderTileMode.Clamp);
		}
	}
}
