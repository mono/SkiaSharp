
using System;

namespace SkiaSharp.Skottie
{
	public partial struct AnimationBuilderStats
	{
		public readonly TimeSpan TotalLoadTime =>
			TimeSpan.FromMilliseconds (fTotalLoadTimeMS);

		public readonly TimeSpan JsonParseTime =>
			TimeSpan.FromMilliseconds (fJsonParseTimeMS);
		
		public readonly TimeSpan SceneParseTime =>
			TimeSpan.FromMilliseconds (fSceneParseTimeMS);

		public readonly int JsonSize => (int)fJsonSize;

		public readonly int AnimatorCount => (int)fAnimatorCount;
	}
}
