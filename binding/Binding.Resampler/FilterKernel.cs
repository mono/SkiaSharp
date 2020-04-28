using System;
using System.Numerics;

namespace SkiaSharp
{
	internal sealed class FilterKernel
	{
		private readonly int stride;
		private readonly float[] weights;
		private readonly int[] offsets;
		private readonly int[] counts;

		public FilterKernel (SKSamplingFilterFunction filterFunction, int filterRadius, int srcLength, int dstLength, double outputShift, double outputLength, bool superSampling, bool wrapAtEdges)
		{
			double srcScaleFactor = outputLength / srcLength;
			double dstScaleFactor = srcLength / outputLength;
			double srcKernelRadius = filterRadius * dstScaleFactor;

			// https://en.wikipedia.org/wiki/Lanczos_resampling
			stride = 2 + (int)Math.Ceiling (2 * srcKernelRadius);
			weights = new float[dstLength * stride];
			offsets = new int[dstLength];
			counts = new int[dstLength];

			// To detect indices out of range, and for better cache locality, we allocate this on the stack
			Span<float> localWeights = stackalloc float[stride];

			int dstMax = dstLength - 1;

			float startCoverage = (float)(1 - outputShift);
			float endCoverage = (float)(1 - (dstLength - outputShift - outputLength));

			for (int dstPixel = 0; dstPixel <= dstMax; dstPixel++) {
				var alpha = dstPixel == 0 ? startCoverage
					: dstPixel == dstMax ? endCoverage
					: 1;

				if (Math.Abs (dstScaleFactor - 1) < 1e-6) {
					// No scaling needed
					offsets[dstPixel] = dstPixel;
					counts[dstPixel] = 1;
					weights[dstPixel * stride] = alpha;
				} else {
					var srcMid = (dstPixel - outputShift) * dstScaleFactor;
					int srcMin = (int)Math.Floor (srcMid - srcKernelRadius);
					int srcMax = (int)Math.Ceiling (srcMid + srcKernelRadius);
					var srcClampedMin = Math.Max (0, srcMin);
					var srcClampedMax = Math.Min (srcLength - 1, srcMax);
					var count = srcClampedMax - srcClampedMin + 1;
					var last = count - 1;

					offsets[dstPixel] = srcClampedMin;
					counts[dstPixel] = count;

					var weightsOffset = dstPixel * stride;

					if (filterRadius == 0) {
						// Do a simple box filter.
						var weight = alpha / count;
						for (int i = 0; i < count; i++) {
							weights[weightsOffset + i] = weight;
						}
					} else {
						// Use a Lanczos filter.
						float sum = 0;

						// Compute the weight of each source pixel.
						// A source pixel can occur multiple times due to edge wrapping and super-sampling!
						for (int srcPixel = srcMin; srcPixel <= srcMax; ++srcPixel) {
							var weight = (float)filterFunction ((srcPixel - srcMid) * srcScaleFactor, filterRadius);
							int index = GetWeightIndex (srcPixel - srcClampedMin, last, wrapAtEdges);
							localWeights[index] += weight;
							sum += weight;
						}

						if (superSampling) {
							// Also include (s0 + s1)/2 source pixels (simple bi-linear interpolation)
							// TODO: Also use Lanczos for super sampling?
							for (int srcPixel = srcMin; srcPixel < srcMax; ++srcPixel) {
								var weight = (float)filterFunction ((srcPixel - srcMid + 0.5f) * srcScaleFactor, filterRadius);
								int index0 = GetWeightIndex (srcPixel - srcClampedMin, last, wrapAtEdges);
								int index1 = GetWeightIndex (srcPixel - srcClampedMin + 1, last, wrapAtEdges);
								sum += weight;
								weight *= 0.5f;
								localWeights[index0] += weight;
								localWeights[index1] += weight;
							}
						}

						//Debug.Assert(Math.Abs(localWeights.ToArray().Sum() - sum) < 0.001f);
						float normalize = alpha / sum;
						for (int i = 0; i < count; i++) {
							weights[weightsOffset + i] = localWeights[i] * normalize;
							localWeights[i] = 0;
						}
					}
				}
			}
		}

		private int GetWeightIndex (int offset, int last, bool wrap)
		{
			// Wrap around the edges, but always clamp if needed.
			if (offset < 0)
				return wrap ? Math.Min (-offset, last) : 0;

			if (offset > last)
				return wrap ? Math.Max (2 * last - offset, 0) : last;

			return offset;
		}

		public void Convolve (
			ReadOnlySpan<Vector4> srcColors,
			int srcOffset,
			Span<Vector4> dstColors,
			int dstOffset,
			int dstStride,
			int dstCount)
		{
			Span<float> weights = this.weights;

			for (int i = 0; i < dstCount; ++i) {
				var offset = offsets[i];
				var count = counts[i];
				var srcGroup = srcColors.Slice (srcOffset + offset, count);
				var srcWeights = weights.Slice (i * stride, count);
				var dots = DotProducts (srcGroup, srcWeights);
				dstColors[dstOffset + i * dstStride] = dots;
			}
		}

		private static Vector4 DotProducts (ReadOnlySpan<Vector4> values, ReadOnlySpan<float> weights)
		{
			// NOTE: Benchmarking showed that neither an unsafe context,
			// nor unrolling the loop matters, at least not in .NET Core 3.1
			var dots = Vector4.Zero;
			for (int i = 0; i < values.Length; ++i) {
				dots += values[i] * weights[i];
			}
			return dots;
		}
	}
}
