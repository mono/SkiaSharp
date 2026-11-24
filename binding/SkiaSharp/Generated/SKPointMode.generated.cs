using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_point_mode_t
	/// <summary>Possible values to interpret the incoming array of points for the <see cref="M:SkiaSharp.SKCanvas.DrawPoints(SkiaSharp.SKPointMode,SkiaSharp.SKPoint[],SkiaSharp.SKPaint)" /> method.</summary>
	/// <remarks />
	public enum SKPointMode {
		// POINTS_SK_POINT_MODE = 0
		/// <summary>Interpret the data as coordinates for points.</summary>
		Points = 0,
		// LINES_SK_POINT_MODE = 1
		/// <summary>Interpret the data as coordinates for lines.</summary>
		Lines = 1,
		// POLYGON_SK_POINT_MODE = 2
		/// <summary>Interpret the data as coordinates for polygons.</summary>
		Polygon = 2,
	}
}
