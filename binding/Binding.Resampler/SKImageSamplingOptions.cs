#nullable enable
using System.Threading.Tasks;

namespace SkiaSharp
{
	public delegate double SKSamplingFilterFunction(double x, int r);

	public sealed class SKImageSamplingOptions
	{
		/// <summary>
		/// The color type of the output image. By default the color type of the input is used.
		/// </summary>
		/// <remarks>
		/// For high quality rendering, you can use <see cref="SKColorType.RgbaF16"/>
		/// </remarks>
		/// <seealso cref="OutputColorSpace"/>
		public SKColorType? OutputColorType;

		/// <summary>
		/// The color space of the output image. By default the color space of the input is used.
		/// </summary>
		/// <remarks>
		/// For high quality rendering, you should use <see cref="SKColorSpace.CreateSrgbLinear()"/>
		/// </remarks>
		/// <seealso cref="OutputColorType"/>
		public SKColorSpace? OutputColorSpace;

		/// <summary>
		/// The (fractional) offset of the resampled output image.
		/// Only the fractional part is used, the integer part is ignored.
		/// This allows shifting the output with sub-pixel precision.
		/// </summary>
		public SKPoint OutputOffset { get; set; }

		/// <summary>
		/// The filter function to use.
		/// </summary>
		public SKSamplingFilterFunction FilterFunc { get; set; } = SKSamplingFilterFunctions.Lanczos;

		/// <summary>
		/// The radius (half the width) of the sampling filter. 
		/// </summary>
		public int FilterRadius { get; set; } = 3;

		/// <summary>
		/// Sample interpolated source pixels too? Enabled by default since this is cheap and removes more aliasing.
		/// </summary>
		public bool SuperSample { get; set; } = true;

		/// <summary>
		/// When sampling outside of the source image,
		/// wrap around the nearest edge, or just clamp (the default)?
		/// </summary>
		public bool WrapAtEdges { get; set; }

		/// <summary>
		/// Re-sample in parallel?
		/// </summary>
		public ParallelOptions? ParallelOptions { get; set; }
	}
}
