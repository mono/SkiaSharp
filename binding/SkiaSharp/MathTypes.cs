#nullable disable

using System;
using System.Numerics;

namespace SkiaSharp
{
	/// <summary>
	/// Represents an ordered pair of floating-point x- and y-coordinates that defines a point in a two-dimensional plane.
	/// </summary>
	/// <remarks>To convert a <see cref="SKPoint" /> to a <see cref="SKPointI" />, use <see cref="M:SkiaSharp.SKPointI.Round(SkiaSharp.SKPoint)" /> or <see cref="M:SkiaSharp.SKPointI.Truncate(SkiaSharp.SKPoint)" />.</remarks>
	public partial struct SKPoint
	{
		/// <summary>
		/// Represents a new instance of the <see cref="SKPoint" /> class with member data left uninitialized.
		/// </summary>
		public static readonly SKPoint Empty;

		/// <summary>
		/// Creates a new instance of a point with the specified coordinates.
		/// </summary>
		/// <param name="x">The horizontal position of the point.</param>
		/// <param name="y">The vertical position of the point.</param>
		public SKPoint (float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		/// <summary>
		/// Gets a value indicating whether this point is empty.
		/// </summary>
		public readonly bool IsEmpty => this == Empty;

		/// <summary>
		/// Gets the Euclidean distance from the origin (0, 0).
		/// </summary>
		public readonly float Length => (float)Math.Sqrt (x * x + y * y);

		/// <summary>
		/// Gets the Euclidean distance squared from the origin (0, 0).
		/// </summary>
		public readonly float LengthSquared => x * x + y * y;

		/// <summary>
		/// Translates a given point by a specified offset.
		/// </summary>
		/// <param name="p">The offset value.</param>
		public void Offset (SKPoint p)
		{
			x += p.x;
			y += p.y;
		}

		/// <summary>
		/// Translates a given point by a specified offset.
		/// </summary>
		/// <param name="dx">The offset in the x-direction.</param>
		/// <param name="dy">The offset in the y-direction.</param>
		public void Offset (float dx, float dy)
		{
			x += dx;
			y += dy;
		}

		/// <summary>
		/// Converts this <see cref="SKPoint" /> to a human readable string.
		/// </summary>
		/// <returns>A string that represents this <see cref="SKPoint" />.</returns>
		public readonly override string ToString () => $"{{X={x}, Y={y}}}";

		/// <summary>
		/// Returns a point with the same direction as the specified point, but with a length of one.
		/// </summary>
		/// <param name="point">The point to normalize.</param>
		/// <returns>Returns a point with a length of one.</returns>
		public static SKPoint Normalize (SKPoint point)
		{
			var ls = point.x * point.x + point.y * point.y;
			var invNorm = 1.0 / Math.Sqrt (ls);
			return new SKPoint ((float)(point.x * invNorm), (float)(point.y * invNorm));
		}

		/// <summary>
		/// Calculate the Euclidean distance between two points.
		/// </summary>
		/// <param name="point">The first point.</param>
		/// <param name="other">The second point.</param>
		/// <returns>Returns the Euclidean distance between two points.</returns>
		public static float Distance (SKPoint point, SKPoint other)
		{
			var dx = point.x - other.x;
			var dy = point.y - other.y;
			var ls = dx * dx + dy * dy;
			return (float)Math.Sqrt (ls);
		}

		/// <summary>
		/// Calculate the Euclidean distance squared between two points.
		/// </summary>
		/// <param name="point">The first point.</param>
		/// <param name="other">The second point.</param>
		/// <returns>Returns the Euclidean distance squared between two points.</returns>
		public static float DistanceSquared (SKPoint point, SKPoint other)
		{
			var dx = point.x - other.x;
			var dy = point.y - other.y;
			return dx * dx + dy * dy;
		}

		/// <summary>
		/// Returns the reflection of a point off a surface that has the specified normal.
		/// </summary>
		/// <param name="point">The point to reflect.</param>
		/// <param name="normal">The normal.</param>
		/// <returns>Returns the reflection of a point.</returns>
		public static SKPoint Reflect (SKPoint point, SKPoint normal)
		{
			var dot = point.x * point.x + point.y * point.y;
			return new SKPoint (
				point.x - 2.0f * dot * normal.x,
				point.y - 2.0f * dot * normal.y);
		}

		/// <summary>
		/// Translates a given point by a specified size.
		/// </summary>
		/// <param name="pt">The point to translate</param>
		/// <param name="sz">The offset size.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint Add (SKPoint pt, SKSizeI sz) => pt + sz;
		/// <summary>
		/// Translates a given point by a specified size.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The offset size.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint Add (SKPoint pt, SKSize sz) => pt + sz;
		/// <summary>
		/// Translates a given point by a specified offset.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The offset value.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint Add (SKPoint pt, SKPointI sz) => pt + sz;
		/// <summary>
		/// Translates a given point by a specified offset.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The offset value.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint Add (SKPoint pt, SKPoint sz) => pt + sz;

		/// <summary>
		/// Translates a <see cref="SKPoint" /> by the negative of a specified size.
		/// </summary>
		/// <param name="pt">The <see cref="SKPoint" /> to translate.</param>
		/// <param name="sz">The <see cref="SKSize" /> that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated <see cref="SKPoint" />.</returns>
		public static SKPoint Subtract (SKPoint pt, SKSizeI sz) => pt - sz;
		/// <summary>
		/// Translates a <see cref="SKPoint" /> by the negative of a specified size.
		/// </summary>
		/// <param name="pt">The <see cref="SKPoint" /> to translate.</param>
		/// <param name="sz">The <see cref="SKSize" /> that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated <see cref="SKPoint" />.</returns>
		public static SKPoint Subtract (SKPoint pt, SKSize sz) => pt - sz;
		/// <summary>
		/// Translates a given point by the negative of a specified offset.
		/// </summary>
		/// <param name="pt">The <see cref="SKPoint" /> to translate.</param>
		/// <param name="sz">The offset that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated <see cref="SKPoint" />.</returns>
		public static SKPoint Subtract (SKPoint pt, SKPointI sz) => pt - sz;
		/// <summary>
		/// Translates a given point by the negative of a specified offset.
		/// </summary>
		/// <param name="pt">The <see cref="SKPoint" /> to translate.</param>
		/// <param name="sz">The offset that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated <see cref="SKPoint" />.</returns>
		public static SKPoint Subtract (SKPoint pt, SKPoint sz) => pt - sz;

		/// <summary>
		/// Translates a given point by a specified size.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The offset size.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint operator + (SKPoint pt, SKSizeI sz) =>
			new SKPoint (pt.x + sz.Width, pt.y + sz.Height);
		/// <summary>
		/// Translates a given point by a specified size.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The offset size.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint operator + (SKPoint pt, SKSize sz) =>
			new SKPoint (pt.x + sz.Width, pt.y + sz.Height);
		/// <summary>
		/// Translates a given point by a specified offset.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The offset value.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint operator + (SKPoint pt, SKPointI sz) =>
			new SKPoint (pt.x + sz.X, pt.y + sz.Y);
		/// <summary>
		/// Translates a given point by a specified offset.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The offset value.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint operator + (SKPoint pt, SKPoint sz) =>
			new SKPoint (pt.x + sz.X, pt.y + sz.Y);

		/// <summary>
		/// Translates a <see cref="SKPoint" /> by the negative of a given <see cref="SKSizeI" />.
		/// </summary>
		/// <param name="pt">The <see cref="SKPoint" /> to translate.</param>
		/// <param name="sz">The <see cref="SKSizeI" /> that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated <see cref="SKPoint" />.</returns>
		public static SKPoint operator - (SKPoint pt, SKSizeI sz) =>
			new SKPoint (pt.X - sz.Width, pt.Y - sz.Height);
		/// <summary>
		/// Translates a <see cref="SKPoint" /> by the negative of a given <see cref="SKSize" />.
		/// </summary>
		/// <param name="pt">The <see cref="SKPoint" /> to translate.</param>
		/// <param name="sz">The <see cref="SKSize" /> that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated <see cref="SKPoint" />.</returns>
		public static SKPoint operator - (SKPoint pt, SKSize sz) =>
			new SKPoint (pt.X - sz.Width, pt.Y - sz.Height);
		/// <summary>
		/// Translates a given point by the negative of a specified offset.
		/// </summary>
		/// <param name="pt">The <see cref="SKPoint" /> to translate.</param>
		/// <param name="sz">The point that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated <see cref="SKPoint" />.</returns>
		public static SKPoint operator - (SKPoint pt, SKPointI sz) =>
			new SKPoint (pt.X - sz.X, pt.Y - sz.Y);
		/// <summary>
		/// Translates a given point by the negative of a specified offset.
		/// </summary>
		/// <param name="pt">The <see cref="SKPoint" /> to translate.</param>
		/// <param name="sz">The point that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated <see cref="SKPoint" />.</returns>
		public static SKPoint operator - (SKPoint pt, SKPoint sz) =>
			new SKPoint (pt.X - sz.X, pt.Y - sz.Y);

		public static implicit operator Vector2 (SKPoint point) =>
			new Vector2 (point.x, point.y);

		public static implicit operator SKPoint (Vector2 vector) =>
			new SKPoint (vector.X, vector.Y);
	}

	/// <summary>
	/// Represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane.
	/// </summary>
	public partial struct SKPointI
	{
		/// <summary>
		/// Represents a new instance of the <see cref="SKPointI" /> class with member data left uninitialized.
		/// </summary>
		public static readonly SKPointI Empty;

		/// <summary>
		/// Initializes a new instance of the <see cref="SKPointI" /> class from a <see cref="SKSizeI" />.
		/// </summary>
		/// <param name="sz">A <see cref="SKSizeI" /> that specifies the coordinates for the new <see cref="SKPointI" />.</param>
		public SKPointI (SKSizeI sz)
		{
			x = sz.Width;
			y = sz.Height;
		}

		/// <summary>
		/// Initializes a point from two floating point values.
		/// </summary>
		/// <param name="x">The horizontal position of the point.</param>
		/// <param name="y">The vertical position of the point.</param>
		public SKPointI (int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="SKPointI" /> is empty.
		/// </summary>
		public readonly bool IsEmpty => this == Empty;

		/// <summary>
		/// Gets the Euclidean distance from the origin (0, 0).
		/// </summary>
		public readonly int Length => (int)Math.Sqrt (x * x + y * y);

		/// <summary>
		/// Gets the Euclidean distance squared from the origin (0, 0).
		/// </summary>
		public readonly int LengthSquared => x * x + y * y;

		/// <summary>
		/// Translates this <see cref="SKPointI" /> by the specified <see cref="SKPointI" />.
		/// </summary>
		/// <param name="p">The <see cref="SKPointI" /> used to offset this <see cref="SKPointI" />.</param>
		/// <remarks>This method adjusts the <see cref="SKPointI.X" /> and <see cref="SKPointI.Y" /> values of this <see cref="T:System.Drawing.Point" /> to the sum of the <see cref="SKPointI.X" /> and <see cref="SKPointI.Y" /> values of this <see cref="SKPointI" /> and <paramref name="p" />.</remarks>
		public void Offset (SKPointI p)
		{
			x += p.X;
			y += p.Y;
		}

		/// <summary>
		/// Translates this <see cref="SKPointI" /> by the specified amount.
		/// </summary>
		/// <param name="dx">The amount to offset the x-coordinate.</param>
		/// <param name="dy">The amount to offset the y-coordinate.</param>
		public void Offset (int dx, int dy)
		{
			x += dx;
			y += dy;
		}

		/// <summary>
		/// Converts this <see cref="SKPointI" /> to a human readable string.
		/// </summary>
		/// <returns>A string that represents this <see cref="SKPointI" />.</returns>
		public readonly override string ToString () => $"{{X={x},Y={y}}}";

		/// <summary>
		/// Returns a point with the same direction as the specified point, but with a length of one.
		/// </summary>
		/// <param name="point">The point to normalize.</param>
		/// <returns>Returns a point with a length of one.</returns>
		public static SKPointI Normalize (SKPointI point)
		{
			var ls = point.x * point.x + point.y * point.y;
			var invNorm = 1.0 / Math.Sqrt (ls);
			return new SKPointI ((int)(point.x * invNorm), (int)(point.y * invNorm));
		}

		/// <summary>
		/// Calculate the Euclidean distance between two points.
		/// </summary>
		/// <param name="point">The first point.</param>
		/// <param name="other">The second point.</param>
		/// <returns>Returns the Euclidean distance between two points.</returns>
		public static float Distance (SKPointI point, SKPointI other)
		{
			var dx = point.x - other.x;
			var dy = point.y - other.y;
			var ls = dx * dx + dy * dy;
			return (float)Math.Sqrt (ls);
		}

		/// <summary>
		/// Calculate the Euclidean distance squared between two points.
		/// </summary>
		/// <param name="point">The first point.</param>
		/// <param name="other">The second point.</param>
		/// <returns>Returns the Euclidean distance squared between two points.</returns>
		public static float DistanceSquared (SKPointI point, SKPointI other)
		{
			var dx = point.x - other.x;
			var dy = point.y - other.y;
			return dx * dx + dy * dy;
		}

		/// <summary>
		/// Returns the reflection of a point off a surface that has the specified normal.
		/// </summary>
		/// <param name="point">The point to reflect.</param>
		/// <param name="normal">The normal.</param>
		/// <returns>Returns the reflection of a point.</returns>
		public static SKPointI Reflect (SKPointI point, SKPointI normal)
		{
			var dot = point.x * point.x + point.y * point.y;
			return new SKPointI (
				(int)(point.x - 2.0f * dot * normal.x),
				(int)(point.y - 2.0f * dot * normal.y));
		}

		/// <summary>
		/// Converts the specified <see cref="SKPoint" /> to a <see cref="SKPointI" /> by rounding the values of the <see cref="SKPoint" /> to the next higher integer values.
		/// </summary>
		/// <param name="value">The <see cref="SKPoint" /> to convert.</param>
		/// <returns>The <see cref="SKPointI" /> this method converts to.</returns>
		public static SKPointI Ceiling (SKPoint value)
		{
			int x, y;
			checked {
				x = (int)Math.Ceiling (value.X);
				y = (int)Math.Ceiling (value.Y);
			}

			return new SKPointI (x, y);
		}

		/// <summary>
		/// Converts the specified <see cref="SKPoint" /> to a <see cref="SKPointI" /> object by rounding the <see cref="SKPoint" /> values to the nearest integer.
		/// </summary>
		/// <param name="value">The <see cref="SKPoint" /> to convert.</param>
		/// <returns>The <see cref="SKPointI" /> this method converts to.</returns>
		public static SKPointI Round (SKPoint value)
		{
			int x, y;
			checked {
				x = (int)Math.Round (value.X);
				y = (int)Math.Round (value.Y);
			}

			return new SKPointI (x, y);
		}

		/// <summary>
		/// Converts the specified <see cref="SKPoint" /> to a <see cref="SKPointI" /> by truncating the values of the <see cref="SKPoint" />.
		/// </summary>
		/// <param name="value">The <see cref="SKPoint" /> to convert.</param>
		/// <returns>The <see cref="SKPoint" /> this method converts to.</returns>
		public static SKPointI Truncate (SKPoint value)
		{
			int x, y;
			checked {
				x = (int)value.X;
				y = (int)value.Y;
			}

			return new SKPointI (x, y);
		}

		/// <summary>
		/// Translates a given <see cref="SKPointI" /> by the specified <see cref="SKSizeI" />.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The size that specifies the number to add to the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated point.</returns>
		public static SKPointI Add (SKPointI pt, SKSizeI sz) => pt + sz;
		/// <summary>
		/// Translates a given <see cref="SKPointI" /> by the specified point.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The point that specifies the number to add to the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated point.</returns>
		public static SKPointI Add (SKPointI pt, SKPointI sz) => pt + sz;

		/// <summary>
		/// Returns the result of subtracting specified <see cref="SKSizeI" /> from the specified <see cref="SKPointI" />.
		/// </summary>
		/// <param name="pt">The <see cref="SKPointI" /> to be subtracted from.</param>
		/// <param name="sz">The <see cref="SKSizeI" /> to subtract from the <see cref="SKPointI" />.</param>
		/// <returns>The <see cref="SKPointI" /> that is the result of the subtraction operation.</returns>
		public static SKPointI Subtract (SKPointI pt, SKSizeI sz) => pt - sz;
		/// <summary>
		/// Returns the result of subtracting specified point from the specified <see cref="SKPointI" />.
		/// </summary>
		/// <param name="pt">The <see cref="SKPointI" /> to be subtracted from.</param>
		/// <param name="sz">The point to subtract from the <see cref="SKPointI" />.</param>
		/// <returns>The <see cref="SKPointI" /> that is the result of the subtraction operation.</returns>
		public static SKPointI Subtract (SKPointI pt, SKPointI sz) => pt - sz;

		/// <summary>
		/// Translates a <see cref="SKPointI" /> by a given <see cref="SKSizeI" />.
		/// </summary>
		/// <param name="pt">The <see cref="SKPointI" /> to translate.</param>
		/// <param name="sz">A <see cref="SKSizeI" /> that specifies the pair of numbers to add to the coordinates of <paramref name="pt" />.</param>
		/// <returns>Returns the translated <see cref="SKPointI" />.</returns>
		public static SKPointI operator + (SKPointI pt, SKSizeI sz) =>
			new SKPointI (pt.X + sz.Width, pt.Y + sz.Height);
		/// <summary>
		/// Translates a <see cref="SKPointI" /> by a given offset.
		/// </summary>
		/// <param name="pt">The <see cref="SKPointI" /> to translate.</param>
		/// <param name="sz">A point that specifies the pair of numbers to add to the coordinates of <paramref name="pt" />.</param>
		/// <returns>Returns the translated <see cref="SKPointI" />.</returns>
		public static SKPointI operator + (SKPointI pt, SKPointI sz) =>
			new SKPointI (pt.X + sz.X, pt.Y + sz.Y);

		/// <summary>
		/// Translates a <see cref="SKPointI" /> by the negative of a given <see cref="SKSizeI" />.
		/// </summary>
		/// <param name="pt">The <see cref="SKPointI" /> to translate.</param>
		/// <param name="sz">The <see cref="SKSizeI" /> that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated <see cref="SKPointI" />.</returns>
		public static SKPointI operator - (SKPointI pt, SKSizeI sz) =>
			new SKPointI (pt.X - sz.Width, pt.Y - sz.Height);
		/// <summary>
		/// Translates a <see cref="SKPointI" /> by the negative of a given point.
		/// </summary>
		/// <param name="pt">The <see cref="SKPointI" /> to translate.</param>
		/// <param name="sz">The point that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
		/// <returns>The translated <see cref="SKPointI" />.</returns>
		public static SKPointI operator - (SKPointI pt, SKPointI sz) =>
			new SKPointI (pt.X - sz.X, pt.Y - sz.Y);

		/// <summary>
		/// Converts an <see cref="SKPointI" /> into an <see cref="SKSizeI" />.
		/// </summary>
		/// <param name="p">The <see cref="SKPointI" /> to convert.</param>
		/// <returns>The new <see cref="SKSizeI" />.</returns>
		public static explicit operator SKSizeI (SKPointI p) =>
			new SKSizeI (p.X, p.Y);
		/// <summary>
		/// Converts an <see cref="SKPointI" /> into an <see cref="SKPoint" />.
		/// </summary>
		/// <param name="p">The <see cref="SKPointI" /> to convert.</param>
		/// <returns>The new <see cref="SKPoint" />.</returns>
		public static implicit operator SKPoint (SKPointI p) =>
			new SKPoint (p.X, p.Y);

		public static implicit operator Vector2 (SKPointI point) =>
			new Vector2 (point.x, point.y);
	}

	/// <summary>
	/// Represents an ordered pair of floating-point x-, y- and z-coordinates that defines a point in a three-dimensional plane.
	/// </summary>
	public partial struct SKPoint3
	{
		/// <summary>
		/// Represents a new instance of the <see cref="SKPoint3" /> class with member data left uninitialized.
		/// </summary>
		public static readonly SKPoint3 Empty;

		/// <summary>
		/// Creates a new instance of a 3D point with the specified coordinates.
		/// </summary>
		/// <param name="x">The x-coordinate of the point.</param>
		/// <param name="y">The y-coordinate of the point.</param>
		/// <param name="z">The z-coordinate of the point.</param>
		public SKPoint3 (float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="SKPoint3" /> is empty.
		/// </summary>
		public readonly bool IsEmpty => this == Empty;

		/// <summary>
		/// Converts this <see cref="SKPoint3" /> to a human readable string.
		/// </summary>
		/// <returns>A string that represents this <see cref="SKPoint3" />.</returns>
		public readonly override string ToString () => $"{{X={x}, Y={y}, Z={z}}}";

		/// <summary>
		/// Translates a given point by a specified offset.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The offset value.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint3 Add (SKPoint3 pt, SKPoint3 sz) => pt + sz;

		/// <summary>
		/// Translates a given point by the negative of a specified offset.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The offset value.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint3 Subtract (SKPoint3 pt, SKPoint3 sz) => pt - sz;

		/// <summary>
		/// Translates a given point by a specified offset.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The offset value.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint3 operator + (SKPoint3 pt, SKPoint3 sz) =>
			new SKPoint3 (pt.X + sz.X, pt.Y + sz.Y, pt.Z + sz.Z);

		/// <summary>
		/// Translates a given point by the negative of a specified offset.
		/// </summary>
		/// <param name="pt">The point to translate.</param>
		/// <param name="sz">The offset value.</param>
		/// <returns>Returns the translated point.</returns>
		public static SKPoint3 operator - (SKPoint3 pt, SKPoint3 sz) =>
			new SKPoint3 (pt.X - sz.X, pt.Y - sz.Y, pt.Z - sz.Z);

		public static implicit operator Vector3 (SKPoint3 point) =>
			new Vector3 (point.x, point.y, point.z);

		public static implicit operator SKPoint3 (Vector3 vector) =>
			new SKPoint3 (vector.X, vector.Y, vector.Z);
	}

	/// <summary>
	/// Stores an ordered pair of floating-point numbers describing the width and height of a rectangle.
	/// </summary>
	public partial struct SKSize
	{
		/// <summary>
		/// Represents a new instance of the <see cref="SKSize" /> class with member data left uninitialized.
		/// </summary>
		public static readonly SKSize Empty;

		/// <summary>
		/// Creates a new size with a given width and height.
		/// </summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		public SKSize (float width, float height)
		{
			w = width;
			h = height;
		}

		/// <summary>
		/// Creates a new size from the offset of a point.
		/// </summary>
		/// <param name="pt">The offset to use as the dimensions of the new point.</param>
		public SKSize (SKPoint pt)
		{
			w = pt.X;
			h = pt.Y;
		}

		/// <summary>
		/// Gets a value that indicates whether this <see cref="SKSize" /> structure has zero width and height.
		/// </summary>
		public readonly bool IsEmpty => this == Empty;

		/// <summary>
		/// Converts a <see cref="SKSize" /> structure to a <see cref="SKPoint" /> structure.
		/// </summary>
		/// <returns>Returns a <see cref="SKPoint" /> structure.</returns>
		public readonly SKPoint ToPoint () =>
			new SKPoint (w, h);

		/// <summary>
		/// Converts a <see cref="SKSize" /> structure to a <see cref="SKSizeI" /> structure.
		/// </summary>
		/// <returns>Returns a <see cref="SKSizeI" /> structure.</returns>
		public readonly SKSizeI ToSizeI ()
		{
			int w, h;
			checked {
				w = (int)this.w;
				h = (int)this.h;
			}

			return new SKSizeI (w, h);
		}

		/// <summary>
		/// Converts this <see cref="SKSize" /> to a human readable string.
		/// </summary>
		/// <returns>A string that represents this <see cref="SKSize" />.</returns>
		public readonly override string ToString () =>
			$"{{Width={w}, Height={h}}}";

		/// <summary>
		/// Adds the width and height of one <see cref="SKSize" /> structure to the width and height of another <see cref="SKSize" /> structure.
		/// </summary>
		/// <param name="sz1">The first <see cref="SKSize" /> structure to add.</param>
		/// <param name="sz2">The second <see cref="SKSize" /> structure to add.</param>
		/// <returns>A <see cref="SKSize" /> structure that is the result of the addition operation.</returns>
		public static SKSize Add (SKSize sz1, SKSize sz2) => sz1 + sz2;

		/// <summary>
		/// Subtracts the width and height of one <see cref="SKSize" /> structure from the width and height of another <see cref="SKSize" /> structure.
		/// </summary>
		/// <param name="sz1">The <see cref="SKSize" /> structure on the left side of the subtraction operator.</param>
		/// <param name="sz2">The <see cref="SKSize" /> structure on the right side of the subtraction operator.</param>
		/// <returns>A <see cref="SKSize" /> that is the result of the subtraction operation.</returns>
		public static SKSize Subtract (SKSize sz1, SKSize sz2) => sz1 - sz2;

		/// <summary>
		/// Adds the width and height of one <see cref="SKSize" /> structure to the width and height of another <see cref="SKSize" /> structure.
		/// </summary>
		/// <param name="sz1">The first <see cref="SKSize" /> structure to add.</param>
		/// <param name="sz2">The second <see cref="SKSize" /> structure to add.</param>
		/// <returns>A <see cref="SKSize" /> structure that is the result of the addition operation.</returns>
		public static SKSize operator + (SKSize sz1, SKSize sz2) =>
			new SKSize (sz1.Width + sz2.Width, sz1.Height + sz2.Height);

		/// <summary>
		/// Subtracts the width and height of one <see cref="SKSize" /> structure from the width and height of another <see cref="SKSize" /> structure.
		/// </summary>
		/// <param name="sz1">The <see cref="SKSize" /> structure on the left side of the subtraction operator.</param>
		/// <param name="sz2">The <see cref="SKSize" /> structure on the right side of the subtraction operator.</param>
		/// <returns>A <see cref="SKSize" /> that is the result of the subtraction operation.</returns>
		public static SKSize operator - (SKSize sz1, SKSize sz2) =>
			new SKSize (sz1.Width - sz2.Width, sz1.Height - sz2.Height);

		/// <summary>
		/// Converts the specified <see cref="SKSize" /> structure to a <see cref="SKPoint" /> structure.
		/// </summary>
		/// <param name="size">The <see cref="SKSize" /> structure to be converted.</param>
		/// <returns>The <see cref="SKPoint" /> structure structure to which this operator converts.</returns>
		public static explicit operator SKPoint (SKSize size) =>
			new SKPoint (size.Width, size.Height);
		/// <summary>
		/// Converts the specified <see cref="SKSizeI" /> structure to a <see cref="SKSize" /> structure.
		/// </summary>
		/// <param name="size">The <see cref="SKSizeI" /> structure to be converted.</param>
		/// <returns>The <see cref="SKSize" /> structure structure to which this operator converts.</returns>
		public static implicit operator SKSize (SKSizeI size) =>
			new SKSize (size.Width, size.Height);
	}

	/// <summary>
	/// Stores an ordered pair of integers describing the width and height of a rectangle.
	/// </summary>
	public partial struct SKSizeI
	{
		/// <summary>
		/// Represents a new instance of the <see cref="SKSizeI" /> class with member data left uninitialized.
		/// </summary>
		public static readonly SKSizeI Empty;

		/// <summary>
		/// Creates a new size with a given width and height.
		/// </summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		public SKSizeI (int width, int height)
		{
			w = width;
			h = height;
		}

		/// <summary>
		/// Creates a new size from the offset of a point.
		/// </summary>
		/// <param name="pt">The offset to use as the dimensions of the new point.</param>
		public SKSizeI (SKPointI pt)
		{
			w = pt.X;
			h = pt.Y;
		}

		/// <summary>
		/// Gets a value that indicates whether this <see cref="SKSizeI" /> structure has zero width and height.
		/// </summary>
		public readonly bool IsEmpty => this == Empty;

		/// <summary>
		/// Converts a <see cref="SKSizeI" /> structure to a <see cref="SKPointI" /> structure.
		/// </summary>
		/// <returns>Returns a <see cref="SKPointI" /> structure.</returns>
		public readonly SKPointI ToPointI () => new SKPointI (w, h);

		/// <summary>
		/// Converts this <see cref="SKSizeI" /> to a human readable string.
		/// </summary>
		/// <returns>A string that represents this <see cref="SKSizeI" />.</returns>
		public readonly override string ToString () =>
			$"{{Width={w}, Height={h}}}";

		/// <summary>
		/// Adds the width and height of one <see cref="SKSizeI" /> structure to the width and height of another <see cref="SKSizeI" /> structure.
		/// </summary>
		/// <param name="sz1">The first <see cref="SKSizeI" /> structure to add.</param>
		/// <param name="sz2">The second <see cref="SKSizeI" /> structure to add.</param>
		/// <returns>A <see cref="SKSizeI" /> structure that is the result of the addition operation.</returns>
		public static SKSizeI Add (SKSizeI sz1, SKSizeI sz2) => sz1 + sz2;

		/// <summary>
		/// Subtracts the width and height of one <see cref="SKSizeI" /> structure from the width and height of another <see cref="SKSizeI" /> structure.
		/// </summary>
		/// <param name="sz1">The <see cref="SKSizeI" /> structure on the left side of the subtraction operator.</param>
		/// <param name="sz2">The <see cref="SKSizeI" /> structure on the right side of the subtraction operator.</param>
		/// <returns>A <see cref="SKSizeI" /> that is the result of the subtraction operation.</returns>
		public static SKSizeI Subtract (SKSizeI sz1, SKSizeI sz2) => sz1 - sz2;

		/// <summary>
		/// Adds the width and height of one <see cref="SKSizeI" /> structure to the width and height of another <see cref="SKSizeI" /> structure.
		/// </summary>
		/// <param name="sz1">The first <see cref="SKSizeI" /> structure to add.</param>
		/// <param name="sz2">The second <see cref="SKSizeI" /> structure to add.</param>
		/// <returns>A <see cref="SKSizeI" /> structure that is the result of the addition operation.</returns>
		public static SKSizeI operator + (SKSizeI sz1, SKSizeI sz2) =>
			new SKSizeI (sz1.Width + sz2.Width, sz1.Height + sz2.Height);

		/// <summary>
		/// Subtracts the width and height of one <see cref="SKSizeI" /> structure from the width and height of another <see cref="SKSizeI" /> structure.
		/// </summary>
		/// <param name="sz1">The <see cref="SKSizeI" /> structure on the left side of the subtraction operator.</param>
		/// <param name="sz2">The <see cref="SKSizeI" /> structure on the right side of the subtraction operator.</param>
		/// <returns>A <see cref="SKSizeI" /> that is the result of the subtraction operation.</returns>
		public static SKSizeI operator - (SKSizeI sz1, SKSizeI sz2) =>
			new SKSizeI (sz1.Width - sz2.Width, sz1.Height - sz2.Height);

		/// <summary>
		/// Converts the specified <see cref="SKSizeI" /> structure to a <see cref="SKPointI" /> structure.
		/// </summary>
		/// <param name="size">The <see cref="SKSizeI" /> structure to be converted.</param>
		/// <returns>The <see cref="SKPointI" /> structure structure to which this operator converts.</returns>
		public static explicit operator SKPointI (SKSizeI size) =>
			new SKPointI (size.Width, size.Height);
	}

	/// <summary>
	/// Stores a set of four floating-point numbers that represent the upper-left corner and lower-right corner of a rectangle.
	/// </summary>
	public partial struct SKRect
	{
		/// <summary>
		/// Represents a new instance of the <see cref="SKRect" /> class with member data left uninitialized.
		/// </summary>
		public static readonly SKRect Empty;

		/// <summary>
		/// Initializes a new instance of the <see cref="SKRect" /> class with the specified upper-left corner and lower-right corner.
		/// </summary>
		/// <param name="left">The left coordinate.</param>
		/// <param name="top">The top coordinate.</param>
		/// <param name="right">The right coordinate.</param>
		/// <param name="bottom">The bottom coordinate.</param>
		public SKRect (float left, float top, float right, float bottom)
		{
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}

		/// <summary>
		/// Gets the x-coordinate of the middle of this rectangle.
		/// </summary>
		public readonly float MidX => left + (Width / 2f);

		/// <summary>
		/// Gets the y-coordinate of the middle of this rectangle.
		/// </summary>
		public readonly float MidY => top + (Height / 2f);

		/// <summary>
		/// Gets the width of the rectangle.
		/// </summary>
		public readonly float Width => right - left;

		/// <summary>
		/// Gets the height of the <see cref="SKRect" />.
		/// </summary>
		public readonly float Height => bottom - top;

		/// <summary>
		/// Gets a value indicating whether this rectangle has a zero size and location.
		/// </summary>
		public readonly bool IsEmpty => this == Empty;

		/// <summary>
		/// Gets or sets the size of the rectangle.
		/// </summary>
		public SKSize Size {
			readonly get => new SKSize (Width, Height);
			set {
				right = left + value.Width;
				bottom = top + value.Height;
			}
		}

		/// <summary>
		/// Gets or sets the offset of the rectangle.
		/// </summary>
		public SKPoint Location {
			readonly get => new SKPoint (left, top);
			set => this = SKRect.Create (value, Size);
		}

		/// <summary>
		/// Gets this rectangle and a new rectangle with a positive width and height.
		/// </summary>
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

		/// <summary>
		/// Calculates the largest rectangle that will fit inside the current rectangle using the specified size.
		/// </summary>
		/// <param name="size">The size of the existing rectangle.</param>
		/// <returns>Returns the largest rectangle that will fit inside the current rectangle.</returns>
		public readonly SKRect AspectFit (SKSize size) => AspectResize (size, true);

		/// <summary>
		/// Calculates the smallest rectangle that will fill the current rectangle using the specified size.
		/// </summary>
		/// <param name="size">The size of the existing rectangle.</param>
		/// <returns>Returns the smallest rectangle that will fill the current rectangle.</returns>
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

		/// <summary>
		/// Creates and returns an enlarged copy of the specified <see cref="SKRect" /> structure. The copy is enlarged by the specified amount and the original rectangle remains unmodified.
		/// </summary>
		/// <param name="rect">The <see cref="SKRect" /> to be copied. This rectangle is not modified.</param>
		/// <param name="x">The amount to enlarge the copy of the rectangle horizontally.</param>
		/// <param name="y">The amount to enlarge the copy of the rectangle vertically.</param>
		/// <returns>The enlarged <see cref="SKRect" />.</returns>
		public static SKRect Inflate (SKRect rect, float x, float y)
		{
			var r = new SKRect (rect.left, rect.top, rect.right, rect.bottom);
			r.Inflate (x, y);
			return r;
		}

		/// <summary>
		/// Enlarges this <see cref="SKRect" /> structure by the specified amount.
		/// </summary>
		/// <param name="size">The amount to inflate this <see cref="SKRect" />.</param>
		public void Inflate (SKSize size) =>
			Inflate (size.Width, size.Height);

		/// <summary>
		/// Enlarges this <see cref="SKRect" /> structure by the specified amount.
		/// </summary>
		/// <param name="x">The amount to inflate this <see cref="SKRect" /> structure horizontally.</param>
		/// <param name="y">The amount to inflate this <see cref="SKRect" /> structure vertically.</param>
		public void Inflate (float x, float y)
		{
			left -= x;
			top -= y;
			right += x;
			bottom += y;
		}

		/// <summary>
		/// Returns a <see cref="SKRect" /> structure that represents the intersection of two rectangles. If there is no intersection, and empty <see cref="SKRect" /> is returned.
		/// </summary>
		/// <param name="a">A rectangle to intersect.</param>
		/// <param name="b">A rectangle to intersect.</param>
		/// <returns>A third <see cref="SKRect" /> structure the size of which represents the overlapped area of the two specified rectangles.</returns>
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

		/// <summary>
		/// Replaces this <see cref="SKRect" /> structure with the intersection of itself and the specified <see cref="SKRect" /> structure.
		/// </summary>
		/// <param name="rect">The rectangle to intersect.</param>
		public void Intersect (SKRect rect) =>
			this = Intersect (this, rect);

		/// <summary>
		/// Creates the smallest possible third rectangle that can contain both of two rectangles that form a union.
		/// </summary>
		/// <param name="a">A rectangle to union.</param>
		/// <param name="b">A rectangle to union.</param>
		/// <returns>A third <see cref="SKRect" /> structure that contains both of the two rectangles that form the union.</returns>
		public static SKRect Union (SKRect a, SKRect b) =>
			new SKRect (
				Math.Min (a.left, b.left),
				Math.Min (a.top, b.top),
				Math.Max (a.right, b.right),
				Math.Max (a.bottom, b.bottom));

		/// <summary>
		/// Replaces this <see cref="SKRect" /> structure with the union of itself and the specified <see cref="SKRect" /> structure.
		/// </summary>
		/// <param name="rect">A rectangle to union.</param>
		public void Union (SKRect rect) =>
			this = Union (this, rect);

		/// <summary>
		/// Converts the specified <see cref="SKRectI" /> structure to a <see cref="SKRect" /> structure.
		/// </summary>
		/// <param name="r">The <see cref="SKRectI" /> structure to convert.</param>
		/// <returns>The <see cref="SKRect" /> structure that is converted from the specified <see cref="SKRectI" /> structure.</returns>
		public static implicit operator SKRect (SKRectI r) =>
			new SKRect (r.Left, r.Top, r.Right, r.Bottom);

		/// <summary>
		/// Determines whether the specified coordinates are inside this rectangle.
		/// </summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>Returns true if the coordinates are inside this rectangle, otherwise false.</returns>
		public readonly bool Contains (float x, float y) =>
			(x >= left) && (x < right) && (y >= top) && (y < bottom);

		/// <summary>
		/// Determines whether the specified point is inside this rectangle.
		/// </summary>
		/// <param name="pt">The point to test.</param>
		/// <returns>Returns true if the point is inside this rectangle, otherwise false.</returns>
		public readonly bool Contains (SKPoint pt) =>
			Contains (pt.X, pt.Y);

		/// <summary>
		/// Determines whether the specified rectangle is inside this rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to test.</param>
		/// <returns>Returns true if the rectangle is inside this rectangle, otherwise false.</returns>
		public readonly bool Contains (SKRect rect) =>
			(left <= rect.left) && (right >= rect.right) &&
			(top <= rect.top) && (bottom >= rect.bottom);

		/// <summary>
		/// Determines if this rectangle intersects with another rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to test.</param>
		/// <returns>This method returns true if there is any intersection.</returns>
		public readonly bool IntersectsWith (SKRect rect) =>
			(left < rect.right) && (right > rect.left) && (top < rect.bottom) && (bottom > rect.top);

		/// <summary>
		/// Determines if this rectangle intersects with another rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to test.</param>
		/// <returns>This method returns true if there is any intersection.</returns>
		public readonly bool IntersectsWithInclusive (SKRect rect) =>
			(left <= rect.right) && (right >= rect.left) && (top <= rect.bottom) && (bottom >= rect.top);

		/// <summary>
		/// Translates the this rectangle by the specified amount.
		/// </summary>
		/// <param name="x">The amount to offset the location horizontally.</param>
		/// <param name="y">The amount to offset the location vertically.</param>
		public void Offset (float x, float y)
		{
			left += x;
			top += y;
			right += x;
			bottom += y;
		}

		/// <summary>
		/// Translates the this rectangle by the specified amount.
		/// </summary>
		/// <param name="pos">The amount to offset the rectangle.</param>
		public void Offset (SKPoint pos) => Offset (pos.X, pos.Y);

		/// <summary>
		/// Converts this <see cref="SKRect" /> to a human readable string.
		/// </summary>
		/// <returns>A string that represents this <see cref="SKRect" />.</returns>
		public readonly override string ToString () =>
			$"{{Left={Left},Top={Top},Width={Width},Height={Height}}}";

		/// <summary>
		/// Creates a new rectangle with the specified location and size.
		/// </summary>
		/// <param name="location">The rectangle location.</param>
		/// <param name="size">The rectangle size.</param>
		/// <returns>Returns the new rectangle.</returns>
		public static SKRect Create (SKPoint location, SKSize size) =>
			Create (location.X, location.Y, size.Width, size.Height);

		/// <summary>
		/// Creates a new rectangle with the specified size.
		/// </summary>
		/// <param name="size">The rectangle size.</param>
		/// <returns>Returns the new rectangle.</returns>
		public static SKRect Create (SKSize size) =>
			Create (SKPoint.Empty, size);

		/// <summary>
		/// Creates a new rectangle with the specified size.
		/// </summary>
		/// <param name="width">The rectangle width.</param>
		/// <param name="height">The rectangle height.</param>
		/// <returns>Returns the new rectangle.</returns>
		public static SKRect Create (float width, float height) =>
			new SKRect (SKPoint.Empty.X, SKPoint.Empty.Y, width, height);

		/// <summary>
		/// Creates a new rectangle with the specified location and size.
		/// </summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="width">The rectangle width.</param>
		/// <param name="height">The rectangle height.</param>
		/// <returns>Returns the new rectangle.</returns>
		public static SKRect Create (float x, float y, float width, float height) =>
			new SKRect (x, y, x + width, y + height);
	}

	/// <summary>
	/// Stores a set of four integers that represent the upper-left corner and lower-right corner of a rectangle.
	/// </summary>
	public partial struct SKRectI
	{
		/// <summary>
		/// Represents a new instance of the <see cref="SKRectI" /> class with member data left uninitialized.
		/// </summary>
		public static readonly SKRectI Empty;

		/// <summary>
		/// Initializes a new instance of the <see cref="SKRectI" /> class with the specified upper-left corner and lower-right corner.
		/// </summary>
		/// <param name="left">The left coordinate.</param>
		/// <param name="top">The top coordinate.</param>
		/// <param name="right">The right coordinate.</param>
		/// <param name="bottom">The bottom coordinate.</param>
		public SKRectI (int left, int top, int right, int bottom)
		{
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}

		/// <summary>
		/// Gets the x-coordinate of the middle of this rectangle.
		/// </summary>
		public readonly int MidX => left + (Width / 2);

		/// <summary>
		/// Gets the y-coordinate of the middle of this rectangle.
		/// </summary>
		public readonly int MidY => top + (Height / 2);

		/// <summary>
		/// Gets the width of the <see cref="SKRectI" />.
		/// </summary>
		public readonly int Width => right - left;

		/// <summary>
		/// Gets the height of the <see cref="SKRectI" />.
		/// </summary>
		public readonly int Height => bottom - top;

		/// <summary>
		/// Gets a value indicating whether this rectangle has a zero size and location.
		/// </summary>
		public readonly bool IsEmpty => this == Empty;

		/// <summary>
		/// Gets or sets the size of the <see cref="SKRectI" />.
		/// </summary>
		public SKSizeI Size {
			readonly get => new SKSizeI (Width, Height);
			set {
				right = left + value.Width;
				bottom = top + value.Height;
			}
		}

		/// <summary>
		/// Gets or sets the offset of the rectangle.
		/// </summary>
		public SKPointI Location {
			readonly get => new SKPointI (left, top);
			set => this = SKRectI.Create (value, Size);
		}

		/// <summary>
		/// Gets this rectangle and a new rectangle with a positive width and height.
		/// </summary>
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

		/// <summary>
		/// Calculates the largest rectangle that will fit inside the current rectangle using the specified size.
		/// </summary>
		/// <param name="size">The size of the existing rectangle.</param>
		/// <returns>Returns the largest rectangle that will fit inside the current rectangle.</returns>
		public readonly SKRectI AspectFit (SKSizeI size) =>
			Floor (((SKRect)this).AspectFit (size));

		/// <summary>
		/// Calculates the smallest rectangle that will fill the current rectangle using the specified size.
		/// </summary>
		/// <param name="size">The size of the existing rectangle.</param>
		/// <returns>Returns the smallest rectangle that will fill the current rectangle.</returns>
		public readonly SKRectI AspectFill (SKSizeI size) =>
			Floor (((SKRect)this).AspectFill (size));

		/// <summary>
		/// Converts the specified <see cref="SKRect" /> structure to a <see cref="SKRectI" /> structure by rounding the <see cref="SKRect" /> values to the next higher integer values.
		/// </summary>
		/// <param name="value">The <see cref="SKRect" /> structure to be converted.</param>
		/// <returns>Returns a <see cref="SKRectI" />.</returns>
		public static SKRectI Ceiling (SKRect value) =>
			Ceiling (value, false);

		/// <summary>
		/// Converts the specified <see cref="SKRect" /> structure to a <see cref="SKRectI" /> structure by rounding the <see cref="SKRect" /> values to the next higher integer values.
		/// </summary>
		/// <param name="value">The <see cref="SKRect" /> structure to be converted.</param>
		/// <param name="outwards">Whether or not to move in the direction of the side.</param>
		/// <returns>Returns a <see cref="SKRectI" />.</returns>
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

		/// <summary>
		/// Creates and returns an enlarged copy of the specified <see cref="SKRectI" /> structure. The copy is enlarged by the specified amount and the original rectangle remains unmodified.
		/// </summary>
		/// <param name="rect">The <see cref="SKRectI" /> to be copied. This rectangle is not modified.</param>
		/// <param name="x">The amount to enlarge the copy of the rectangle horizontally.</param>
		/// <param name="y">The amount to enlarge the copy of the rectangle vertically.</param>
		/// <returns>The enlarged <see cref="SKRectI" />.</returns>
		public static SKRectI Inflate (SKRectI rect, int x, int y)
		{
			var r = new SKRectI (rect.left, rect.top, rect.right, rect.bottom);
			r.Inflate (x, y);
			return r;
		}

		/// <summary>
		/// Enlarges this <see cref="SKRectI" /> structure by the specified amount.
		/// </summary>
		/// <param name="size">The amount to inflate this <see cref="SKRectI" />.</param>
		public void Inflate (SKSizeI size) =>
			Inflate (size.Width, size.Height);

		/// <summary>
		/// Enlarges this <see cref="SKRectI" /> structure by the specified amount.
		/// </summary>
		/// <param name="width">The amount to inflate this <see cref="SKRectI" /> structure horizontally.</param>
		/// <param name="height">The amount to inflate this <see cref="SKRectI" /> structure vertically.</param>
		public void Inflate (int width, int height)
		{
			left -= width;
			top -= height;
			right += width;
			bottom += height;
		}

		/// <summary>
		/// Returns a <see cref="SKRectI" /> structure that represents the intersection of two rectangles. If there is no intersection, and empty <see cref="SKRectI" /> is returned.
		/// </summary>
		/// <param name="a">A rectangle to intersect.</param>
		/// <param name="b">A rectangle to intersect.</param>
		/// <returns>A third <see cref="SKRectI" /> structure the size of which represents the overlapped area of the two specified rectangles.</returns>
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

		/// <summary>
		/// Replaces this <see cref="SKRectI" /> structure with the intersection of itself and the specified <see cref="SKRectI" /> structure.
		/// </summary>
		/// <param name="rect">The rectangle to intersect.</param>
		public void Intersect (SKRectI rect) =>
			this = Intersect (this, rect);

		/// <summary>
		/// Converts the specified <see cref="SKRect" /> structure to a <see cref="SKRectI" /> structure by rounding the <see cref="SKRect" /> values to the nearest integer values.
		/// </summary>
		/// <param name="value">The <see cref="SKRect" /> structure to be converted.</param>
		/// <returns>Returns a <see cref="SKRectI" />.</returns>
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

		/// <summary>
		/// Converts the specified <see cref="SKRect" /> structure to a <see cref="SKRectI" /> structure by rounding the <see cref="SKRect" /> values to the closest lower integer values.
		/// </summary>
		/// <param name="value">The <see cref="SKRect" /> structure to be converted.</param>
		/// <returns>Returns a <see cref="SKRectI" />.</returns>
		public static SKRectI Floor (SKRect value) => Floor (value, false);

		/// <summary>
		/// Converts the specified <see cref="SKRect" /> structure to a <see cref="SKRectI" /> structure by rounding the <see cref="SKRect" /> values to the closest lower integer values.
		/// </summary>
		/// <param name="value">The <see cref="SKRect" /> structure to be converted.</param>
		/// <param name="inwards">Whether or not to move in the direction of the side.</param>
		/// <returns>Returns a <see cref="SKRectI" />.</returns>
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

		/// <summary>
		/// Converts the specified <see cref="SKRect" /> structure to a <see cref="SKRectI" /> structure by truncating the <see cref="SKRect" /> values.
		/// </summary>
		/// <param name="value">The <see cref="SKRect" /> to be converted.</param>
		/// <returns>The truncated value of the <see cref="SKRectI" />.</returns>
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

		/// <summary>
		/// Creates the smallest possible third rectangle that can contain both of two rectangles that form a union.
		/// </summary>
		/// <param name="a">A rectangle to union.</param>
		/// <param name="b">A rectangle to union.</param>
		/// <returns>A third <see cref="SKRectI" /> structure that contains both of the two rectangles that form the union.</returns>
		public static SKRectI Union (SKRectI a, SKRectI b) =>
			new SKRectI (
				Math.Min (a.Left, b.Left),
				Math.Min (a.Top, b.Top),
				Math.Max (a.Right, b.Right),
				Math.Max (a.Bottom, b.Bottom));

		/// <summary>
		/// Replaces this <see cref="SKRectI" /> structure with the union of itself and the specified <see cref="SKRectI" /> structure.
		/// </summary>
		/// <param name="rect">A rectangle to union.</param>
		public void Union (SKRectI rect) =>
			this = Union (this, rect);

		/// <summary>
		/// Determines whether the specified coordinates are inside this rectangle.
		/// </summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>Returns true if the coordinates are inside this rectangle, otherwise false.</returns>
		public readonly bool Contains (int x, int y) =>
			(x >= left) && (x < right) && (y >= top) && (y < bottom);

		/// <summary>
		/// Determines whether the specified point is inside this rectangle.
		/// </summary>
		/// <param name="pt">The point to test.</param>
		/// <returns>Returns true if the point is inside this rectangle, otherwise false.</returns>
		public readonly bool Contains (SKPointI pt) =>
			Contains (pt.X, pt.Y);

		/// <summary>
		/// Determines whether the specified rectangle is inside this rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to test.</param>
		/// <returns>Returns true if the rectangle is inside this rectangle, otherwise false.</returns>
		public readonly bool Contains (SKRectI rect) =>
			(left <= rect.left) && (right >= rect.right) &&
			(top <= rect.top) && (bottom >= rect.bottom);

		/// <summary>
		/// Determines if this rectangle intersects with another rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to test.</param>
		/// <returns>This method returns true if there is any intersection.</returns>
		public readonly bool IntersectsWith (SKRectI rect) =>
			(left < rect.right) && (right > rect.left) && (top < rect.bottom) && (bottom > rect.top);

		/// <summary>
		/// Determines if this rectangle intersects with another rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to test.</param>
		/// <returns>This method returns true if there is any intersection.</returns>
		public readonly bool IntersectsWithInclusive (SKRectI rect) =>
			(left <= rect.right) && (right >= rect.left) && (top <= rect.bottom) && (bottom >= rect.top);

		/// <summary>
		/// Translates the this rectangle by the specified amount.
		/// </summary>
		/// <param name="x">The amount to offset the location horizontally.</param>
		/// <param name="y">The amount to offset the location vertically.</param>
		public void Offset (int x, int y)
		{
			left += x;
			top += y;
			right += x;
			bottom += y;
		}

		/// <summary>
		/// Translates the this rectangle by the specified amount.
		/// </summary>
		/// <param name="pos">The amount to offset the rectangle.</param>
		public void Offset (SKPointI pos) => Offset (pos.X, pos.Y);

		/// <summary>
		/// Converts this <see cref="SKRectI" /> to a human readable string.
		/// </summary>
		/// <returns>A string that represents this <see cref="SKRectI" />.</returns>
		public readonly override string ToString () =>
			$"{{Left={Left},Top={Top},Width={Width},Height={Height}}}";

		/// <summary>
		/// Creates a new rectangle with the specified size.
		/// </summary>
		/// <param name="size">The rectangle size.</param>
		/// <returns>Returns the new rectangle.</returns>
		public static SKRectI Create (SKSizeI size) =>
			Create (SKPointI.Empty.X, SKPointI.Empty.Y, size.Width, size.Height);

		/// <summary>
		/// Creates a new rectangle with the specified location and size.
		/// </summary>
		/// <param name="location">The rectangle location.</param>
		/// <param name="size">The rectangle size.</param>
		/// <returns>Returns the new rectangle.</returns>
		public static SKRectI Create (SKPointI location, SKSizeI size) =>
			Create (location.X, location.Y, size.Width, size.Height);

		/// <summary>
		/// Creates a new rectangle with the specified width and height.
		/// </summary>
		/// <param name="width">The rectangle width.</param>
		/// <param name="height">The rectangle height.</param>
		/// <returns>Returns the new rectangle.</returns>
		public static SKRectI Create (int width, int height) =>
			new SKRectI (SKPointI.Empty.X, SKPointI.Empty.X, width, height);

		/// <summary>
		/// Creates a new rectangle with the specified location and size.
		/// </summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="width">The rectangle width.</param>
		/// <param name="height">The rectangle height.</param>
		/// <returns>Returns the new rectangle.</returns>
		public static SKRectI Create (int x, int y, int width, int height) =>
			new SKRectI (x, y, x + width, y + height);
	}
}
