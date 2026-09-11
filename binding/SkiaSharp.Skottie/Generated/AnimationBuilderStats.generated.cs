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

		public readonly bool Equals (AnimationBuilderStats obj) =>
#pragma warning disable CS8909
			fTotalLoadTimeMS == obj.fTotalLoadTimeMS && fJsonParseTimeMS == obj.fJsonParseTimeMS && fSceneParseTimeMS == obj.fSceneParseTimeMS && fJsonSize == obj.fJsonSize && fAnimatorCount == obj.fAnimatorCount;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is AnimationBuilderStats f && Equals (f);

		public static bool operator == (AnimationBuilderStats left, AnimationBuilderStats right) =>
			left.Equals (right);

		public static bool operator != (AnimationBuilderStats left, AnimationBuilderStats right) =>
			!left.Equals (right);

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
