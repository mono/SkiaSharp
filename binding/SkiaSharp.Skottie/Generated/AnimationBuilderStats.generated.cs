using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces

using SkiaSharp.Skottie;

#endregion

namespace SkiaSharp.Skottie
{

	// skottie_animation_builder_stats_t
	[StructLayout (LayoutKind.Sequential)]
	public readonly unsafe partial struct AnimationBuilderStats : IEquatable<AnimationBuilderStats> {
		// public float fTotalLoadTimeMS
		private readonly Single fTotalLoadTimeMS;

		// public float fJsonParseTimeMS
		private readonly Single fJsonParseTimeMS;

		// public float fSceneParseTimeMS
		private readonly Single fSceneParseTimeMS;

		// public size_t fJsonSize
		private readonly /* size_t */ IntPtr fJsonSize;

		// public size_t fAnimatorCount
		private readonly /* size_t */ IntPtr fAnimatorCount;

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.Skottie.AnimationBuilderStats" /> is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (AnimationBuilderStats obj) =>
#pragma warning disable CS8909
			fTotalLoadTimeMS == obj.fTotalLoadTimeMS && fJsonParseTimeMS == obj.fJsonParseTimeMS && fSceneParseTimeMS == obj.fSceneParseTimeMS && fJsonSize == obj.fJsonSize && fAnimatorCount == obj.fAnimatorCount;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is AnimationBuilderStats f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.Skottie.AnimationBuilderStats" /> values are equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (AnimationBuilderStats left, AnimationBuilderStats right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.Skottie.AnimationBuilderStats" /> values are not equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the values are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (AnimationBuilderStats left, AnimationBuilderStats right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fTotalLoadTimeMS);
			hash.Add (fJsonParseTimeMS);
			hash.Add (fSceneParseTimeMS);
			hash.Add (fJsonSize);
			hash.Add (fAnimatorCount);
			return hash.ToHashCode ();
		}

	}
}
