
using System;

namespace SkiaSharp.Skottie
{
	/// <summary>Contains statistics about the animation build process.</summary>
	/// <remarks>These statistics are available from <see cref="P:SkiaSharp.Skottie.AnimationBuilder.Stats" /> after calling one of the build methods.</remarks>
	public partial struct AnimationBuilderStats
	{
		/// <summary>Gets the total time spent loading the animation.</summary>
		/// <value>The total duration of the animation loading process, including JSON parsing and scene graph construction.</value>
		/// <remarks />
		public readonly TimeSpan TotalLoadTime =>
			TimeSpan.FromMilliseconds (fTotalLoadTimeMS);

		/// <summary>Gets the time spent parsing the JSON data.</summary>
		/// <value>The duration of the JSON parsing phase.</value>
		/// <remarks />
		public readonly TimeSpan JsonParseTime =>
			TimeSpan.FromMilliseconds (fJsonParseTimeMS);
		
		/// <summary>Gets the time spent parsing the scene graph.</summary>
		/// <value>The duration of the scene graph construction phase.</value>
		/// <remarks />
		public readonly TimeSpan SceneParseTime =>
			TimeSpan.FromMilliseconds (fSceneParseTimeMS);

		/// <summary>Gets the size of the JSON data in bytes.</summary>
		/// <value>The size of the Lottie JSON data in bytes.</value>
		/// <remarks />
		public readonly int JsonSize => (int)fJsonSize;

		/// <summary>Gets the total number of animators in the animation scene graph.</summary>
		/// <value>The number of animators created during the build process.</value>
		/// <remarks />
		public readonly int AnimatorCount => (int)fAnimatorCount;
	}
}
