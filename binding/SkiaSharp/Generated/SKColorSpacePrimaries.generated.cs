using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_colorspace_primaries_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKColorSpacePrimaries : IEquatable<SKColorSpacePrimaries> {
		// public float fRX
		private Single fRX;
		/// <summary>Gets or sets the red X-coordinate.</summary>
		/// <value>The red primary X-coordinate.</value>
		/// <remarks />
		public Single RX {
			readonly get => fRX;
			set => fRX = value;
		}

		// public float fRY
		private Single fRY;
		/// <summary>Gets or sets the red Y-coordinate.</summary>
		/// <value>The red primary Y-coordinate.</value>
		/// <remarks />
		public Single RY {
			readonly get => fRY;
			set => fRY = value;
		}

		// public float fGX
		private Single fGX;
		/// <summary>Gets or sets the green X-coordinate.</summary>
		/// <value>The green primary X-coordinate.</value>
		/// <remarks />
		public Single GX {
			readonly get => fGX;
			set => fGX = value;
		}

		// public float fGY
		private Single fGY;
		/// <summary>Gets or sets the green Y-coordinate.</summary>
		/// <value>The green primary Y-coordinate.</value>
		/// <remarks />
		public Single GY {
			readonly get => fGY;
			set => fGY = value;
		}

		// public float fBX
		private Single fBX;
		/// <summary>Gets or sets the blue X-coordinate.</summary>
		/// <value>The blue primary X-coordinate.</value>
		/// <remarks />
		public Single BX {
			readonly get => fBX;
			set => fBX = value;
		}

		// public float fBY
		private Single fBY;
		/// <summary>Gets or sets the blue Y-coordinate.</summary>
		/// <value>The blue primary Y-coordinate.</value>
		/// <remarks />
		public Single BY {
			readonly get => fBY;
			set => fBY = value;
		}

		// public float fWX
		private Single fWX;
		/// <summary>Gets or sets the white X-coordinate.</summary>
		/// <value>The white point X-coordinate.</value>
		/// <remarks />
		public Single WX {
			readonly get => fWX;
			set => fWX = value;
		}

		// public float fWY
		private Single fWY;
		/// <summary>Gets or sets the white Y-coordinate.</summary>
		/// <value>The white point Y-coordinate.</value>
		/// <remarks />
		public Single WY {
			readonly get => fWY;
			set => fWY = value;
		}

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKColorSpacePrimaries obj) =>
#pragma warning disable CS8909
			fRX == obj.fRX && fRY == obj.fRY && fGX == obj.fGX && fGY == obj.fGY && fBX == obj.fBX && fBY == obj.fBY && fWX == obj.fWX && fWY == obj.fWY;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKColorSpacePrimaries f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> instances are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKColorSpacePrimaries left, SKColorSpacePrimaries right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> instances are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKColorSpacePrimaries left, SKColorSpacePrimaries right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fRX);
			hash.Add (fRY);
			hash.Add (fGX);
			hash.Add (fGY);
			hash.Add (fBX);
			hash.Add (fBY);
			hash.Add (fWX);
			hash.Add (fWY);
			return hash.ToHashCode ();
		}

	}
}
