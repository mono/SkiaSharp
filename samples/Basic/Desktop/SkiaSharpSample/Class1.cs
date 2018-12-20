using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample
{
	public class Class1 : SKDrawable
	{
		public Class1()
		{ }

		protected override void OnDraw(SKCanvas canvas)
		{
			var path = new SKPath();
			var paint = new SKPaint();
			path.MoveTo(20, 20);
			path.LineTo(100, 100);
			path.Close();
			paint.Color = SKColors.Red;
			paint.StrokeWidth = 2;
			paint.Style = SKPaintStyle.Stroke;
			canvas.DrawPath(path, paint);
		}

		protected override SKRect OnGetBounds()
		{
			return new SKRect(20, 20, 100, 100);
		}
	}
}
