using System;
using System.Numerics;
using System.Threading.Tasks;

namespace SkiaSharp
{
	public static class ImageResampler
	{
		public static SKBitmap ResampledBy (this SKBitmap srcBitmap,
			float scaleFactor, SKImageSamplingOptions options = null)
		{
			using var srcPixmap = srcBitmap.PeekPixels ();
			return ResampledBy (srcPixmap, scaleFactor, options);
		}

		public static SKBitmap ResampledBy (this SKBitmap srcBitmap,
			float widthFactor, float heightFactor, SKImageSamplingOptions options = null)
		{
			using var srcPixmap = srcBitmap.PeekPixels ();
			return ResampledBy (srcPixmap, widthFactor, heightFactor, options);
		}

		public static SKBitmap ResampledTo (this SKBitmap srcBitmap,
			float outputWidth, float outputHeight, SKImageSamplingOptions options = null)
		{
			using var srcPixmap = srcBitmap.PeekPixels ();
			return ResampledTo(srcPixmap, outputWidth, outputHeight, options);
		}

		public static SKBitmap ResampledBy (this SKPixmap srcPixmap, float scaleFactor, SKImageSamplingOptions options = null)
			=> ResampledTo (srcPixmap, srcPixmap.Width * scaleFactor, srcPixmap.Height * scaleFactor, options);

		public static SKBitmap ResampledBy (this SKPixmap srcPixmap, float widthFactor, float heightFactor, SKImageSamplingOptions options = null)
			=> ResampledTo (srcPixmap, srcPixmap.Width * widthFactor, srcPixmap.Height * heightFactor, options);

		public static SKBitmap ResampledTo (this SKPixmap srcPixmap, float outputWidth, float outputHeight, SKImageSamplingOptions options = null)
		{
			var filterRadius = options?.FilterRadius ?? 3;
			var outputLeft = options?.OutputOffset.X ?? 0;
			var outputTop = options?.OutputOffset.Y ?? 0;
			var superSample = options?.SuperSample ?? true;
			var parallelOptions = options?.ParallelOptions;
			var wrapAtEdges = options?.WrapAtEdges ?? false;
			var filterFunc = options?.FilterFunc ?? SKSamplingFilterFunctions.Lanczos;
			var outputColorType = options?.OutputColorType ?? srcPixmap.ColorType;
			var outputColorSpace = options?.OutputColorSpace ?? srcPixmap.ColorSpace ?? SKColorSpace.CreateSrgb ();

			// Validate arguments
			if (float.IsInfinity (filterRadius) || float.IsNaN (filterRadius) || filterRadius < 0 || filterRadius > 9)
				throw new ArgumentOutOfRangeException (nameof (filterRadius));

			if (float.IsInfinity (outputWidth) || float.IsNaN (outputWidth) || outputWidth <= 0)
				throw new ArgumentOutOfRangeException (nameof (outputWidth));

			if (float.IsInfinity (outputHeight) || float.IsNaN (outputHeight) || outputHeight <= 0)
				throw new ArgumentOutOfRangeException (nameof (outputHeight));

			if (srcPixmap == null)
				throw new ArgumentNullException (nameof (srcPixmap));

			var srcWidth = srcPixmap.Width;
			var srcHeight = srcPixmap.Height;

			if (outputWidth >= srcWidth && outputHeight >= srcHeight) {
				// We need to up-sample in both dimensions
				return new Sampler (outputLeft, outputTop, outputWidth, outputHeight, outputColorType, outputColorSpace, filterRadius).Up (srcPixmap);
			}

			if (outputWidth >= srcWidth) {
				// Up-sample horizontally, down-sample vertically
				using var tmpBitmap = new Sampler (outputLeft, 0, outputWidth, srcHeight, outputColorType, outputColorSpace, filterRadius).Up (srcPixmap);
				using var tmpPixels = tmpBitmap.PeekPixels ();
				return new Sampler (0, outputTop, tmpBitmap.Width, outputHeight, outputColorType, outputColorSpace, filterRadius)
					.Down (tmpPixels, filterFunc, superSample, parallelOptions, wrapAtEdges);
			}

			if (outputWidth >= srcWidth) {
				// Up-sample vertically, down-sample horizontally
				using var tmpBitmap = new Sampler (0, outputTop, srcWidth, outputHeight, outputColorType, outputColorSpace, filterRadius).Up (srcPixmap);
				using var tmpPixels = tmpBitmap.PeekPixels ();
				return new Sampler (outputLeft, 0, outputWidth, tmpBitmap.Height, outputColorType, outputColorSpace, filterRadius)
					.Down (tmpPixels, filterFunc, superSample, parallelOptions, wrapAtEdges);
			}

			// Down-sample in both dimensions.
			return new Sampler (outputLeft, outputTop, outputWidth, outputHeight, outputColorType, outputColorSpace, filterRadius)
				.Down (srcPixmap, filterFunc, superSample, parallelOptions, wrapAtEdges);
		}

		internal readonly struct Sampler
		{
			public readonly float OutputFracX;
			public readonly float OutputFracY;
			public readonly float OutputWidth;
			public readonly float OutputHeight;
			public readonly SKColorType OutputColorType;
			public readonly SKColorSpace OutputColorSpace;
			public readonly int FilterRadius;
			public readonly int DstWidth;
			public readonly int DstHeight;

			public Sampler (
				float outputLeft,
				float outputTop,
				float outputWidth,
				float outputHeight,
				SKColorType outputColorType,
				SKColorSpace outputColorSpace,
				int filterRadius)
			{
				var outputRight = outputLeft + outputWidth;
				var outputBottom = outputTop + outputHeight;

				var dstLeft = (int)Math.Floor (outputLeft);
				var dstTop = (int)Math.Floor (outputTop);

				OutputWidth = outputWidth;
				OutputHeight = outputHeight;

				OutputColorType = outputColorType;
				OutputColorSpace = outputColorSpace;

				FilterRadius = filterRadius;

				DstWidth = (int)(Math.Ceiling (outputRight) - dstLeft);
				DstHeight = (int)(Math.Ceiling (outputBottom) - dstTop);

				OutputFracX = outputLeft - dstLeft;
				OutputFracY = outputTop - dstTop;
			}

			public SKBitmap Down (SKPixmap srcPixmap, SKSamplingFilterFunction filterFunc,
				bool superSample, ParallelOptions? parallelOptions, bool wrapAtEdges)
			{
				var srcWidth = srcPixmap.Width;
				var srcHeight = srcPixmap.Height;

				// We need to down-sample the source pixmap.
				//
				// Outline of algorithm:
				// - scale the rows of the source image horizontally to the columns of temporary transposed bitmap of size (srcHeight, dstWidth)
				// - scale the rows of the transposed bitmap to the columns of the output bitmap of size (dstWidth, dstHeight)
				//
				// Temporary buffers are used to convert to and from a 32-bit float linear color space, needed to correctly average the colors
				var linearColorSpace = SKColorSpace.CreateSrgbLinear ();

				// Create temporary transposed bitmap in 32-bit linear sRGB space
				// We need pre-multiplied alpha to correctly average transparent pixels:
				// https://entropymine.com/imageworsener/resizealpha/
				var tmpWidth = srcHeight;
				var tmpHeight = DstWidth;
				var tmpImgInfo = new SKImageInfo (tmpWidth, tmpHeight, SKColorType.RgbaF32, SKAlphaType.Premul, linearColorSpace);
				using var tmpBitmap = new SKBitmap (tmpImgInfo);
				using var tmpPixmap = tmpBitmap.PeekPixels ();

				// Create a row bitmap, using Skia to convert the input to 32-bit linear sRGB space.
				var linRowInfo = new SKImageInfo (srcWidth, 1, SKColorType.RgbaF32, SKAlphaType.Premul, linearColorSpace);

				// Create a column bitmap in 32-bit linear sRGB space, using Skia to convert the output color type and space.
				var linColInfo = new SKImageInfo (1, DstHeight, SKColorType.RgbaF32, SKAlphaType.Premul, linearColorSpace);

				// Create the filter kernels
				var horScalingKernel = new FilterKernel (filterFunc, FilterRadius, srcWidth, DstWidth, OutputFracX, OutputWidth, superSample, wrapAtEdges);
				var verScalingKernel = new FilterKernel (filterFunc, FilterRadius, srcHeight, DstHeight, OutputFracY, OutputHeight, superSample, wrapAtEdges);

				// Create the output image bitmap
				var dstImgInfo = new SKImageInfo (DstWidth, DstHeight, OutputColorType, SKAlphaType.Premul, OutputColorSpace);

				var dstBitmap = new SKBitmap (dstImgInfo);

				try {
					SKBitmap ProcessRow (int srcRowTmpCol, SKBitmap rowBitmap)
					{
						// Convert to 32-bit linear colors
						using var rowPixmap = rowBitmap.PeekPixels ();
						srcPixmap.ReadPixels (rowPixmap, 0, srcRowTmpCol);

						// Convolve to the temporary transposed bitmap
						var tmpSpan = tmpPixmap.GetPixelSpan<Vector4> ();
						var rowSpan = rowPixmap.GetPixelSpan<Vector4> ();

						horScalingKernel.Convolve (rowSpan, 0,
							tmpSpan, srcRowTmpCol, tmpWidth, tmpHeight);

						return rowBitmap;
					};

					SKBitmap ProcessColumn (int tmpRowDstCol, SKBitmap colBitmap)
					{
						var tmpSpan = tmpPixmap.GetPixelSpan<Vector4> ();

						using var colPixmap = colBitmap.PeekPixels ();
						var colSpan = colPixmap.GetPixelSpan<Vector4> ();

						// Convolve to the destination column
						verScalingKernel.Convolve (
							tmpSpan, tmpRowDstCol * tmpWidth,
							colSpan, 0, 1, colSpan.Length);

						// Convert to output colors.
						colPixmap.ReadPixels (dstImgInfo, dstBitmap.GetAddress (tmpRowDstCol, 0), dstBitmap.RowBytes);

						return colBitmap;
					}

					var parForOptions = parallelOptions ?? new ParallelOptions { MaxDegreeOfParallelism = 1 };

					// Horizontal scaling pass, convolving each row in the source bitmap to a column in the temp transposed bitmap
					Parallel.For (0, srcHeight, parForOptions,
						() => new SKBitmap (linRowInfo),
						(srcRowTmpCol, state, linRowBitmap) => ProcessRow (srcRowTmpCol, linRowBitmap),
						linRowBitmap => linRowBitmap?.Dispose ());

					// Vertical scaling pass, convolving each row in the temp transposed bitmap to a column in the destination bitmap
					Parallel.For (0, DstWidth, parForOptions,
						() => new SKBitmap (linColInfo),
						(tmpRowDstCol, state, linColBitmap) => ProcessColumn (tmpRowDstCol, linColBitmap),
						linColBitmap => linColBitmap?.Dispose ());
				} catch {
					// If any exception occurs, dispose the dstBitmap
					dstBitmap.Dispose ();
					throw;
				}

				return dstBitmap;
			}

			public SKBitmap Up (SKPixmap srcPixmap)
			{
				// NOTE: We currently just use Skia for up-sampling. IMHO this always looks bad anyway, whatever alg is picked...
				var outBitmap = new SKBitmap (
					new SKImageInfo (DstWidth, DstHeight, OutputColorType, SKAlphaType.Premul, OutputColorSpace));

				// NOTE: We use DrawBitmap to deal with fractional output sizes.
				using var inImage = SKImage.FromPixels (srcPixmap);

				var filterQuality = FilterRadius <= 0 ? SKFilterQuality.Low
					: FilterRadius == 1 ? SKFilterQuality.Medium
					: SKFilterQuality.High;

				using var paint = new SKPaint { FilterQuality = filterQuality };
				using var canvas = new SKCanvas (outBitmap);
				canvas.DrawImage (inImage, SKRect.Create (OutputFracX, OutputFracY, OutputWidth, OutputHeight), paint);
				return outBitmap;
			}
		}
	}
}
