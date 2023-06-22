﻿using System;

namespace SkiaSharp
{
	public partial struct SKPoint
	{
		public static readonly SKPoint Empty;

		public SKPoint (float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public readonly bool IsEmpty => this == Empty;

		public readonly float Length => (float)Math.Sqrt (x * x + y * y);

		public readonly float LengthSquared => x * x + y * y;

		public void Offset (SKPoint p)
		{
			x += p.x;
			y += p.y;
		}

		public void Offset (float dx, float dy)
		{
			x += dx;
			y += dy;
		}

		public readonly override string ToString () => $"{{X={x}, Y={y}}}";

		public static SKPoint Normalize (SKPoint point)
		{
			var ls = point.x * point.x + point.y * point.y;
			var invNorm = 1.0 / Math.Sqrt (ls);
			return new SKPoint ((float)(point.x * invNorm), (float)(point.y * invNorm));
		}

		public static float Distance (SKPoint point, SKPoint other)
		{
			var dx = point.x - other.x;
			var dy = point.y - other.y;
			var ls = dx * dx + dy * dy;
			return (float)Math.Sqrt (ls);
		}

		public static float DistanceSquared (SKPoint point, SKPoint other)
		{
			var dx = point.x - other.x;
			var dy = point.y - other.y;
			return dx * dx + dy * dy;
		}

		public static SKPoint Reflect (SKPoint point, SKPoint normal)
		{
			var dot = point.x * point.x + point.y * point.y;
			return new SKPoint (
				point.x - 2.0f * dot * normal.x,
				point.y - 2.0f * dot * normal.y);
		}

		public static SKPoint Add (SKPoint pt, SKSizeI sz) => pt + sz;
		public static SKPoint Add (SKPoint pt, SKSize sz) => pt + sz;
		public static SKPoint Add (SKPoint pt, SKPointI sz) => pt + sz;
		public static SKPoint Add (SKPoint pt, SKPoint sz) => pt + sz;

		public static SKPoint Subtract (SKPoint pt, SKSizeI sz) => pt - sz;
		public static SKPoint Subtract (SKPoint pt, SKSize sz) => pt - sz;
		public static SKPoint Subtract (SKPoint pt, SKPointI sz) => pt - sz;
		public static SKPoint Subtract (SKPoint pt, SKPoint sz) => pt - sz;

		public static SKPoint operator + (SKPoint pt, SKSizeI sz) =>
			new SKPoint (pt.x + sz.Width, pt.y + sz.Height);
		public static SKPoint operator + (SKPoint pt, SKSize sz) =>
			new SKPoint (pt.x + sz.Width, pt.y + sz.Height);
		public static SKPoint operator + (SKPoint pt, SKPointI sz) =>
			new SKPoint (pt.x + sz.X, pt.y + sz.Y);
		public static SKPoint operator + (SKPoint pt, SKPoint sz) =>
			new SKPoint (pt.x + sz.X, pt.y + sz.Y);

		public static SKPoint operator - (SKPoint pt, SKSizeI sz) =>
			new SKPoint (pt.X - sz.Width, pt.Y - sz.Height);
		public static SKPoint operator - (SKPoint pt, SKSize sz) =>
			new SKPoint (pt.X - sz.Width, pt.Y - sz.Height);
		public static SKPoint operator - (SKPoint pt, SKPointI sz) =>
			new SKPoint (pt.X - sz.X, pt.Y - sz.Y);
		public static SKPoint operator - (SKPoint pt, SKPoint sz) =>
			new SKPoint (pt.X - sz.X, pt.Y - sz.Y);
	}

	public partial struct SKPointI
	{
		public static readonly SKPointI Empty;

		public SKPointI (SKSizeI sz)
		{
			x = sz.Width;
			y = sz.Height;
		}

		public SKPointI (int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public readonly bool IsEmpty => this == Empty;

		public readonly int Length => (int)Math.Sqrt (x * x + y * y);

		public readonly int LengthSquared => x * x + y * y;

		public void Offset (SKPointI p)
		{
			x += p.X;
			y += p.Y;
		}

		public void Offset (int dx, int dy)
		{
			x += dx;
			y += dy;
		}

		public readonly override string ToString () => $"{{X={x},Y={y}}}";

		public static SKPointI Normalize (SKPointI point)
		{
			var ls = point.x * point.x + point.y * point.y;
			var invNorm = 1.0 / Math.Sqrt (ls);
			return new SKPointI ((int)(point.x * invNorm), (int)(point.y * invNorm));
		}

		public static float Distance (SKPointI point, SKPointI other)
		{
			var dx = point.x - other.x;
			var dy = point.y - other.y;
			var ls = dx * dx + dy * dy;
			return (float)Math.Sqrt (ls);
		}

		public static float DistanceSquared (SKPointI point, SKPointI other)
		{
			var dx = point.x - other.x;
			var dy = point.y - other.y;
			return dx * dx + dy * dy;
		}

		public static SKPointI Reflect (SKPointI point, SKPointI normal)
		{
			var dot = point.x * point.x + point.y * point.y;
			return new SKPointI (
				(int)(point.x - 2.0f * dot * normal.x),
				(int)(point.y - 2.0f * dot * normal.y));
		}

		public static SKPointI Ceiling (SKPoint value)
		{
			int x, y;
			checked {
				x = (int)Math.Ceiling (value.X);
				y = (int)Math.Ceiling (value.Y);
			}

			return new SKPointI (x, y);
		}

		public static SKPointI Round (SKPoint value)
		{
			int x, y;
			checked {
				x = (int)Math.Round (value.X);
				y = (int)Math.Round (value.Y);
			}

			return new SKPointI (x, y);
		}

		public static SKPointI Truncate (SKPoint value)
		{
			int x, y;
			checked {
				x = (int)value.X;
				y = (int)value.Y;
			}

			return new SKPointI (x, y);
		}

		public static SKPointI Add (SKPointI pt, SKSizeI sz) => pt + sz;
		public static SKPointI Add (SKPointI pt, SKPointI sz) => pt + sz;

		public static SKPointI Subtract (SKPointI pt, SKSizeI sz) => pt - sz;
		public static SKPointI Subtract (SKPointI pt, SKPointI sz) => pt - sz;

		public static SKPointI operator + (SKPointI pt, SKSizeI sz) =>
			new SKPointI (pt.X + sz.Width, pt.Y + sz.Height);
		public static SKPointI operator + (SKPointI pt, SKPointI sz) =>
			new SKPointI (pt.X + sz.X, pt.Y + sz.Y);

		public static SKPointI operator - (SKPointI pt, SKSizeI sz) =>
			new SKPointI (pt.X - sz.Width, pt.Y - sz.Height);
		public static SKPointI operator - (SKPointI pt, SKPointI sz) =>
			new SKPointI (pt.X - sz.X, pt.Y - sz.Y);

		public static explicit operator SKSizeI (SKPointI p) =>
			new SKSizeI (p.X, p.Y);
		public static implicit operator SKPoint (SKPointI p) =>
			new SKPoint (p.X, p.Y);
	}

	public partial struct SKPoint3
	{
		public static readonly SKPoint3 Empty;

		public SKPoint3 (float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public readonly bool IsEmpty => this == Empty;

		public readonly override string ToString () => $"{{X={x}, Y={y}, Z={z}}}";

		public static SKPoint3 Add (SKPoint3 pt, SKPoint3 sz) => pt + sz;

		public static SKPoint3 Subtract (SKPoint3 pt, SKPoint3 sz) => pt - sz;

		public static SKPoint3 operator + (SKPoint3 pt, SKPoint3 sz) =>
			new SKPoint3 (pt.X + sz.X, pt.Y + sz.Y, pt.Z + sz.Z);

		public static SKPoint3 operator - (SKPoint3 pt, SKPoint3 sz) =>
			new SKPoint3 (pt.X - sz.X, pt.Y - sz.Y, pt.Z - sz.Z);
	}

	public partial struct SKSize
	{
		public static readonly SKSize Empty;

		public SKSize (float width, float height)
		{
			w = width;
			h = height;
		}

		public SKSize (SKPoint pt)
		{
			w = pt.X;
			h = pt.Y;
		}

		public readonly bool IsEmpty => this == Empty;

		public readonly SKPoint ToPoint () =>
			new SKPoint (w, h);

		public readonly SKSizeI ToSizeI ()
		{
			int w, h;
			checked {
				w = (int)this.w;
				h = (int)this.h;
			}

			return new SKSizeI (w, h);
		}

		public readonly override string ToString () =>
			$"{{Width={w}, Height={h}}}";

		public static SKSize Add (SKSize sz1, SKSize sz2) => sz1 + sz2;

		public static SKSize Subtract (SKSize sz1, SKSize sz2) => sz1 - sz2;

		public static SKSize operator + (SKSize sz1, SKSize sz2) =>
			new SKSize (sz1.Width + sz2.Width, sz1.Height + sz2.Height);

		public static SKSize operator - (SKSize sz1, SKSize sz2) =>
			new SKSize (sz1.Width - sz2.Width, sz1.Height - sz2.Height);

		public static explicit operator SKPoint (SKSize size) =>
			new SKPoint (size.Width, size.Height);
		public static implicit operator SKSize (SKSizeI size) =>
			new SKSize (size.Width, size.Height);
	}

	public partial struct SKSizeI
	{
		public static readonly SKSizeI Empty;

		public SKSizeI (int width, int height)
		{
			w = width;
			h = height;
		}

		public SKSizeI (SKPointI pt)
		{
			w = pt.X;
			h = pt.Y;
		}

		public readonly bool IsEmpty => this == Empty;

		public readonly SKPointI ToPointI () => new SKPointI (w, h);

		public readonly override string ToString () =>
			$"{{Width={w}, Height={h}}}";

		public static SKSizeI Add (SKSizeI sz1, SKSizeI sz2) => sz1 + sz2;

		public static SKSizeI Subtract (SKSizeI sz1, SKSizeI sz2) => sz1 - sz2;

		public static SKSizeI operator + (SKSizeI sz1, SKSizeI sz2) =>
			new SKSizeI (sz1.Width + sz2.Width, sz1.Height + sz2.Height);

		public static SKSizeI operator - (SKSizeI sz1, SKSizeI sz2) =>
			new SKSizeI (sz1.Width - sz2.Width, sz1.Height - sz2.Height);

		public static explicit operator SKPointI (SKSizeI size) =>
			new SKPointI (size.Width, size.Height);
	}

	public partial struct SKRect
	{
		public static readonly SKRect Empty;

		public SKRect (float left, float top, float right, float bottom)
		{
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}

		public readonly float MidX => left + (Width / 2f);

		public readonly float MidY => top + (Height / 2f);

		public readonly float Width => right - left;

		public readonly float Height => bottom - top;

		public readonly bool IsEmpty => this == Empty;

		public SKSize Size {
			readonly get => new SKSize (Width, Height);
			set {
				right = left + value.Width;
				bottom = top + value.Height;
			}
		}

		public SKPoint Location {
			readonly get => new SKPoint (left, top);
			set => this = SKRect.Create (value, Size);
		}

		public readonly SKRect Standardized {
			get {
				if (left > right) {
					if (top > bottom) {
						return new SKRect (right, bottom, left, top);
					} else {
						return new SKRect (right, top, left, bottom);
					}
				} else {
					if (top > bottom) {
						return new SKRect (left, bottom, right, top);
					} else {
						return new SKRect (left, top, right, bottom);
					}
				}
			}
		}

		public readonly SKRect AspectFit (SKSize size) => AspectResize (size, true);

		public readonly SKRect AspectFill (SKSize size) => AspectResize (size, false);

		private readonly SKRect AspectResize (SKSize size, bool fit)
		{
			if (size.Width == 0 || size.Height == 0 || Width == 0 || Height == 0)
				return Create (MidX, MidY, 0, 0);

			var aspectWidth = size.Width;
			var aspectHeight = size.Height;
			var imgAspect = aspectWidth / aspectHeight;
			var fullRectAspect = Width / Height;

			var compare = fit ? (fullRectAspect > imgAspect) : (fullRectAspect < imgAspect);
			if (compare) {
				aspectHeight = Height;
				aspectWidth = aspectHeight * imgAspect;
			} else {
				aspectWidth = Width;
				aspectHeight = aspectWidth / imgAspect;
			}
			var aspectLeft = MidX - (aspectWidth / 2f);
			var aspectTop = MidY - (aspectHeight / 2f);

			return Create (aspectLeft, aspectTop, aspectWidth, aspectHeight);
		}

		public static SKRect Inflate (SKRect rect, float x, float y)
		{
			var r = new SKRect (rect.left, rect.top, rect.right, rect.bottom);
			r.Inflate (x, y);
			return r;
		}

		public void Inflate (SKSize size) =>
			Inflate (size.Width, size.Height);

		public void Inflate (float x, float y)
		{
			left -= x;
			top -= y;
			right += x;
			bottom += y;
		}

		public static SKRect Intersect (SKRect a, SKRect b)
		{
			if (!a.IntersectsWithInclusive (b)) {
				return Empty;
			}
			return new SKRect (
				Math.Max (a.left, b.left),
				Math.Max (a.top, b.top),
				Math.Min (a.right, b.right),
				Math.Min (a.bottom, b.bottom));
		}

		public void Intersect (SKRect rect) =>
			this = Intersect (this, rect);

		public static SKRect Union (SKRect a, SKRect b) =>
			new SKRect (
				Math.Min (a.left, b.left),
				Math.Min (a.top, b.top),
				Math.Max (a.right, b.right),
				Math.Max (a.bottom, b.bottom));

		public void Union (SKRect rect) =>
			this = Union (this, rect);

		public static implicit operator SKRect (SKRectI r) =>
			new SKRect (r.Left, r.Top, r.Right, r.Bottom);

		public readonly bool Contains (float x, float y) =>
			(x >= left) && (x < right) && (y >= top) && (y < bottom);

		public readonly bool Contains (SKPoint pt) =>
			Contains (pt.X, pt.Y);

		public readonly bool Contains (SKRect rect) =>
			(left <= rect.left) && (right >= rect.right) &&
			(top <= rect.top) && (bottom >= rect.bottom);

		public readonly bool IntersectsWith (SKRect rect) =>
			(left < rect.right) && (right > rect.left) && (top < rect.bottom) && (bottom > rect.top);

		public readonly bool IntersectsWithInclusive (SKRect rect) =>
			(left <= rect.right) && (right >= rect.left) && (top <= rect.bottom) && (bottom >= rect.top);

		public void Offset (float x, float y)
		{
			left += x;
			top += y;
			right += x;
			bottom += y;
		}

		public void Offset (SKPoint pos) => Offset (pos.X, pos.Y);

		public readonly override string ToString () =>
			$"{{Left={Left},Top={Top},Width={Width},Height={Height}}}";

		public static SKRect Create (SKPoint location, SKSize size) =>
			Create (location.X, location.Y, size.Width, size.Height);

		public static SKRect Create (SKSize size) =>
			Create (SKPoint.Empty, size);

		public static SKRect Create (float width, float height) =>
			new SKRect (SKPoint.Empty.X, SKPoint.Empty.Y, width, height);

		public static SKRect Create (float x, float y, float width, float height) =>
			new SKRect (x, y, x + width, y + height);
	}

	public partial struct SKRectI
	{
		public static readonly SKRectI Empty;

		public SKRectI (int left, int top, int right, int bottom)
		{
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}

		public readonly int MidX => left + (Width / 2);

		public readonly int MidY => top + (Height / 2);

		public readonly int Width => right - left;

		public readonly int Height => bottom - top;

		public readonly bool IsEmpty => this == Empty;

		public SKSizeI Size {
			readonly get => new SKSizeI (Width, Height);
			set {
				right = left + value.Width;
				bottom = top + value.Height;
			}
		}

		public SKPointI Location {
			readonly get => new SKPointI (left, top);
			set => this = SKRectI.Create (value, Size);
		}

		public readonly SKRectI Standardized {
			get {
				if (left > right) {
					if (top > bottom) {
						return new SKRectI (right, bottom, left, top);
					} else {
						return new SKRectI (right, top, left, bottom);
					}
				} else {
					if (top > bottom) {
						return new SKRectI (left, bottom, right, top);
					} else {
						return new SKRectI (left, top, right, bottom);
					}
				}
			}
		}

		public readonly SKRectI AspectFit (SKSizeI size) =>
			Truncate (((SKRect)this).AspectFit (size));

		public readonly SKRectI AspectFill (SKSizeI size) =>
			Truncate (((SKRect)this).AspectFill (size));

		public static SKRectI Ceiling (SKRect value) =>
			Ceiling (value, false);

		public static SKRectI Ceiling (SKRect value, bool outwards)
		{
			int x, y, r, b;
			checked {
				x = (int)(outwards && value.Width > 0 ? Math.Floor (value.Left) : Math.Ceiling (value.Left));
				y = (int)(outwards && value.Height > 0 ? Math.Floor (value.Top) : Math.Ceiling (value.Top));
				r = (int)(outwards && value.Width < 0 ? Math.Floor (value.Right) : Math.Ceiling (value.Right));
				b = (int)(outwards && value.Height < 0 ? Math.Floor (value.Bottom) : Math.Ceiling (value.Bottom));
			}

			return new SKRectI (x, y, r, b);
		}

		public static SKRectI Inflate (SKRectI rect, int x, int y)
		{
			var r = new SKRectI (rect.left, rect.top, rect.right, rect.bottom);
			r.Inflate (x, y);
			return r;
		}

		public void Inflate (SKSizeI size) =>
			Inflate (size.Width, size.Height);

		public void Inflate (int width, int height)
		{
			left -= width;
			top -= height;
			right += width;
			bottom += height;
		}

		public static SKRectI Intersect (SKRectI a, SKRectI b)
		{
			if (!a.IntersectsWithInclusive (b))
				return Empty;

			return new SKRectI (
				Math.Max (a.left, b.left),
				Math.Max (a.top, b.top),
				Math.Min (a.right, b.right),
				Math.Min (a.bottom, b.bottom));
		}

		public void Intersect (SKRectI rect) =>
			this = Intersect (this, rect);

		public static SKRectI Round (SKRect value)
		{
			int x, y, r, b;
			checked {
				x = (int)Math.Round (value.Left);
				y = (int)Math.Round (value.Top);
				r = (int)Math.Round (value.Right);
				b = (int)Math.Round (value.Bottom);
			}

			return new SKRectI (x, y, r, b);
		}

		public static SKRectI Floor (SKRect value) => Floor (value, false);

		public static SKRectI Floor (SKRect value, bool inwards)
		{
			int x, y, r, b;
			checked {
				x = (int)(inwards && value.Width > 0 ? Math.Ceiling (value.Left) : Math.Floor (value.Left));
				y = (int)(inwards && value.Height > 0 ? Math.Ceiling (value.Top) : Math.Floor (value.Top));
				r = (int)(inwards && value.Width < 0 ? Math.Ceiling (value.Right) : Math.Floor (value.Right));
				b = (int)(inwards && value.Height < 0 ? Math.Ceiling (value.Bottom) : Math.Floor (value.Bottom));
			}

			return new SKRectI (x, y, r, b);
		}

		public static SKRectI Truncate (SKRect value)
		{
			int x, y, r, b;
			checked {
				x = (int)value.Left;
				y = (int)value.Top;
				r = (int)value.Right;
				b = (int)value.Bottom;
			}

			return new SKRectI (x, y, r, b);
		}

		public static SKRectI Union (SKRectI a, SKRectI b) =>
			new SKRectI (
				Math.Min (a.Left, b.Left),
				Math.Min (a.Top, b.Top),
				Math.Max (a.Right, b.Right),
				Math.Max (a.Bottom, b.Bottom));

		public void Union (SKRectI rect) =>
			this = Union (this, rect);

		public readonly bool Contains (int x, int y) =>
			(x >= left) && (x < right) && (y >= top) && (y < bottom);

		public readonly bool Contains (SKPointI pt) =>
			Contains (pt.X, pt.Y);

		public readonly bool Contains (SKRectI rect) =>
			(left <= rect.left) && (right >= rect.right) &&
			(top <= rect.top) && (bottom >= rect.bottom);

		public readonly bool IntersectsWith (SKRectI rect) =>
			(left < rect.right) && (right > rect.left) && (top < rect.bottom) && (bottom > rect.top);

		public readonly bool IntersectsWithInclusive (SKRectI rect) =>
			(left <= rect.right) && (right >= rect.left) && (top <= rect.bottom) && (bottom >= rect.top);

		public void Offset (int x, int y)
		{
			left += x;
			top += y;
			right += x;
			bottom += y;
		}

		public void Offset (SKPointI pos) => Offset (pos.X, pos.Y);

		public readonly override string ToString () =>
			$"{{Left={Left},Top={Top},Width={Width},Height={Height}}}";

		public static SKRectI Create (SKSizeI size) =>
			Create (SKPointI.Empty.X, SKPointI.Empty.Y, size.Width, size.Height);

		public static SKRectI Create (SKPointI location, SKSizeI size) =>
			Create (location.X, location.Y, size.Width, size.Height);

		public static SKRectI Create (int width, int height) =>
			new SKRectI (SKPointI.Empty.X, SKPointI.Empty.X, width, height);

		public static SKRectI Create (int x, int y, int width, int height) =>
			new SKRectI (x, y, x + width, y + height);
	}
}
