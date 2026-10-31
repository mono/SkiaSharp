using Xunit;

namespace SkiaSharp.Tests.Visual
{
	[CollectionDefinition(Name, DisableParallelization = true)]
	public sealed class GpuRenderingCollection
	{
		public const string Name = nameof(GpuRenderingCollection);
	}
}
