#nullable disable

using System;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>Color filters for use with the <see cref="P:SkiaSharp.SKPaint.ColorFilter" /> property of a <see cref="T:SkiaSharp.SKPaint" />.</summary>
	/// <remarks />
	public unsafe class SKColorFilter : SKObject, ISKReferenceCounted
	{
		/// <summary>The size of the color matrix.</summary>
		/// <remarks />
		public const int ColorMatrixSize = 20;
		/// <summary>The size of a color table for a color component.</summary>
		/// <remarks />
		public const int TableMaxLength = 256;

		private static SKColorFilter srgbToLinear;
		private static bool srgbToLinearInitialized;
		private static object srgbToLinearLock = new object ();

		private static SKColorFilter linearToSrgb;
		private static bool linearToSrgbInitialized;
		private static object linearToSrgbLock = new object ();

		internal SKColorFilter(IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKColorFilter" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKColorFilter" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Creates a new color filter that converts sRGB gamma-encoded colors to linear RGB colors.</summary>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateSrgbToLinearGamma () =>
			LazyInitializer.EnsureInitialized (
				ref srgbToLinear, ref srgbToLinearInitialized, ref srgbToLinearLock,
				() => GetDisposeProtectedObject (SkiaApi.sk_colorfilter_new_srgb_to_linear_gamma (), owns: false, unrefExisting: false));

		/// <summary>Creates a new color filter that converts linear RGB colors to sRGB gamma-encoded colors.</summary>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateLinearToSrgbGamma () =>
			LazyInitializer.EnsureInitialized (
				ref linearToSrgb, ref linearToSrgbInitialized, ref linearToSrgbLock,
				() => GetDisposeProtectedObject (SkiaApi.sk_colorfilter_new_linear_to_srgb_gamma (), owns: false, unrefExisting: false));

		/// <summary>Creates a new color filter that uses the specified color and mode.</summary>
		/// <param name="c">The source color used with the specified mode.</param>
		/// <param name="mode">The blend mode that is applied to each color.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />, or <see langword="null" /> if the mode will have no effect.</returns>
		/// <remarks>If the <paramref name="mode" /> is <see cref="F:SkiaSharp.SKBlendMode.Dst" />, this function will return <see langword="null" /> (since that mode will have no effect on the result).</remarks>
		public static SKColorFilter CreateBlendMode(SKColor c, SKBlendMode mode)
		{
			return GetObject (SkiaApi.sk_colorfilter_new_mode((uint)c, mode));
		}

		/// <summary>Creates a new lighting color filter that multiplies the RGB channels by one color, and then adds a second color, pinning the result for each component to [0..255].</summary>
		/// <param name="mul">The color to multiply the source color by. The alpha component is ignored.</param>
		/// <param name="add">The color to add to the source color. The alpha component is ignored.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateLighting(SKColor mul, SKColor add)
		{
			return GetObject (SkiaApi.sk_colorfilter_new_lighting((uint)mul, (uint)add));
		}

		/// <summary>Creates a new composition color filter, whose effect is to first apply the inner filter and then apply the outer filter to the result of the inner.</summary>
		/// <param name="outer">The outer (second) filter to apply.</param>
		/// <param name="inner">The inner (first) filter to apply.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateCompose(SKColorFilter outer, SKColorFilter inner)
		{
			if (outer == null)
				throw new ArgumentNullException(nameof(outer));
			if (inner == null)
				throw new ArgumentNullException(nameof(inner));
			var colorFilter = GetObject (SkiaApi.sk_colorfilter_new_compose(outer.Handle, inner.Handle));
			GC.KeepAlive (outer);
			GC.KeepAlive (inner);
			return colorFilter;
		}

		/// <summary>Creates a new color filter that linearly interpolates between the results of two other color filters.</summary>
		/// <param name="weight">The interpolation weight between 0.0 and 1.0, where 0.0 returns the result of <paramref name="filter0" /> and 1.0 returns the result of <paramref name="filter1" />.</param>
		/// <param name="filter0">The first color filter to interpolate.</param>
		/// <param name="filter1">The second color filter to interpolate.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateLerp(float weight, SKColorFilter filter0, SKColorFilter filter1)
		{
			_ = filter0 ?? throw new ArgumentNullException(nameof(filter0));
			_ = filter1 ?? throw new ArgumentNullException(nameof(filter1));

			var colorFilter = GetObject (SkiaApi.sk_colorfilter_new_lerp(weight, filter0.Handle, filter1.Handle));
			GC.KeepAlive (filter0);
			GC.KeepAlive (filter1);
			return colorFilter;
		}

		/// <summary>Creates a new color filter that transforms a color by a 4x5 (row-major) matrix.</summary>
		/// <param name="matrix">An array of <see cref="F:SkiaSharp.SKColorFilter.ColorMatrixSize" /> elements.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks>The matrix is in row-major order and the translation column is specified in unnormalized, 0...255, space.</remarks>
		public static SKColorFilter CreateColorMatrix(float[] matrix)
		{
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));
			return CreateColorMatrix(matrix.AsSpan());
		}

		/// <summary>Creates a new color filter that transforms a color by a 4x5 (row-major) matrix.</summary>
		/// <param name="matrix">A span of <see cref="F:SkiaSharp.SKColorFilter.ColorMatrixSize" /> elements representing the 4x5 color matrix.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateColorMatrix(ReadOnlySpan<float> matrix)
		{
			if (matrix.Length != 20)
				throw new ArgumentException("Matrix must have a length of 20.", nameof(matrix));
			fixed (float* m = matrix) {
				return GetObject (SkiaApi.sk_colorfilter_new_color_matrix (m));
			}
		}

		/// <summary>Creates a new color filter that transforms a color by a 4x5 (row-major) matrix in HSLA color space.</summary>
		/// <param name="matrix">A span of <see cref="F:SkiaSharp.SKColorFilter.ColorMatrixSize" /> elements representing the 4x5 HSLA color matrix.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateHslaColorMatrix(ReadOnlySpan<float> matrix)
		{
			if (matrix.Length != 20)
				throw new ArgumentException("Matrix must have a length of 20.", nameof(matrix));
			fixed (float* m = matrix) {
				return GetObject (SkiaApi.sk_colorfilter_new_hsla_matrix (m));
			}
		}

		/// <summary>Creates a new luminance-to-alpha color filter.</summary>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateLumaColor()
		{
			return GetObject (SkiaApi.sk_colorfilter_new_luma_color());
		}

		/// <summary>Creates a new table color filter.</summary>
		/// <param name="table">The table of values for each color component, with a length of <see cref="F:SkiaSharp.SKColorFilter.TableMaxLength" />.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateTable(byte[] table)
		{
			if (table == null)
				throw new ArgumentNullException(nameof(table));
			return CreateTable(table.AsSpan());
		}

		/// <summary>Creates a new table color filter.</summary>
		/// <param name="table">The table of values for each color component, with a length of <see cref="F:SkiaSharp.SKColorFilter.TableMaxLength" />.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateTable(ReadOnlySpan<byte> table)
		{
			if (table.Length != TableMaxLength)
				throw new ArgumentException($"Table must have a length of {TableMaxLength}.", nameof(table));
			fixed (byte* t = table) {
				return GetObject (SkiaApi.sk_colorfilter_new_table (t));
			}
		}

		/// <summary>Creates a new table color filter.</summary>
		/// <param name="tableA">The table of values for the alpha component, with a length of <see cref="F:SkiaSharp.SKColorFilter.TableMaxLength" />.</param>
		/// <param name="tableR">The table of values for the red component, with a length of <see cref="F:SkiaSharp.SKColorFilter.TableMaxLength" />.</param>
		/// <param name="tableG">The table of values for the green component, with a length of <see cref="F:SkiaSharp.SKColorFilter.TableMaxLength" />.</param>
		/// <param name="tableB">The table of values for the blue component, with a length of <see cref="F:SkiaSharp.SKColorFilter.TableMaxLength" />.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateTable(byte[] tableA, byte[] tableR, byte[] tableG, byte[] tableB)
		{
			if (tableA == null)
				throw new ArgumentNullException(nameof(tableA));
			if (tableR == null)
				throw new ArgumentNullException(nameof(tableR));
			if (tableG == null)
				throw new ArgumentNullException(nameof(tableG));
			if (tableB == null)
				throw new ArgumentNullException(nameof(tableB));
			return CreateTable(tableA.AsSpan(), tableR.AsSpan(), tableG.AsSpan(), tableB.AsSpan());
		}

		/// <summary>Creates a new table color filter with separate tables for each color component.</summary>
		/// <param name="tableA">The table of values for the alpha component, with a length of <see cref="F:SkiaSharp.SKColorFilter.TableMaxLength" />.</param>
		/// <param name="tableR">The table of values for the red component, with a length of <see cref="F:SkiaSharp.SKColorFilter.TableMaxLength" />.</param>
		/// <param name="tableG">The table of values for the green component, with a length of <see cref="F:SkiaSharp.SKColorFilter.TableMaxLength" />.</param>
		/// <param name="tableB">The table of values for the blue component, with a length of <see cref="F:SkiaSharp.SKColorFilter.TableMaxLength" />.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks />
		public static SKColorFilter CreateTable(ReadOnlySpan<byte> tableA, ReadOnlySpan<byte> tableR, ReadOnlySpan<byte> tableG, ReadOnlySpan<byte> tableB)
		{
			if (tableA.Length != TableMaxLength)
				throw new ArgumentException($"Table A must have a length of {TableMaxLength}.", nameof(tableA));
			if (tableR.Length != TableMaxLength)
				throw new ArgumentException($"Table R must have a length of {TableMaxLength}.", nameof(tableR));
			if (tableG.Length != TableMaxLength)
				throw new ArgumentException($"Table G must have a length of {TableMaxLength}.", nameof(tableG));
			if (tableB.Length != TableMaxLength)
				throw new ArgumentException($"Table B must have a length of {TableMaxLength}.", nameof(tableB));

			fixed (byte* a = tableA)
			fixed (byte* r = tableR)
			fixed (byte* g = tableG)
			fixed (byte* b = tableB) {
				return GetObject (SkiaApi.sk_colorfilter_new_table_argb (a, r, g, b));
			}
		}

		/// <summary>Creates a new high contrast color filter which provides transformations to improve contrast for users with low vision.</summary>
		/// <param name="config">The high contrast configuration settings.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks>Applies the following transformations in this order: conversion to grayscale, color inversion, increasing the resulting contrast.</remarks>
		public static SKColorFilter CreateHighContrast(SKHighContrastConfig config)
		{
			return GetObject (SkiaApi.sk_colorfilter_new_high_contrast(&config));
		}

		/// <summary>Creates a new high contrast color filter which provides transformations to improve contrast for users with low vision.</summary>
		/// <param name="grayscale"><see langword="true" /> to convert the color to grayscale; otherwise, <see langword="false" />.</param>
		/// <param name="invertStyle">The style of brightness or lightness inversion to apply, or none.</param>
		/// <param name="contrast">The amount to adjust the contrast by, in the range -1.0 through 1.0.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <remarks>Applies the following transformations in this order: conversion to grayscale, color inversion, increasing the resulting contrast.</remarks>
		public static SKColorFilter CreateHighContrast(bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast)
		{
			return CreateHighContrast(new SKHighContrastConfig(grayscale, invertStyle, contrast));
		}

		/// <summary>Creates a color filter that visualizes overdraw by mapping the number of times each pixel has been drawn to one of the specified colors.</summary>
		/// <param name="colors">A span of exactly six colors that map to overdraw counts: index 0 is used where no draw occurs, indexes 1 through 4 correspond to one through four overlapping draws, and index 5 is used for five or more overlapping draws.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <exception cref="T:System.ArgumentException"><paramref name="colors" /> does not contain exactly six entries.</exception>
		/// <remarks><para>This is the same operation used by Skia's overdraw debug visualizer to highlight regions that are painted multiple times.</para></remarks>
		public static SKColorFilter CreateOverdraw(ReadOnlySpan<SKColor> colors)
		{
			if (colors.Length != 6)
				throw new ArgumentException("Exactly 6 colors are required.", nameof(colors));
			fixed (SKColor* c = colors)
				return GetObject(SkiaApi.sk_colorfilter_new_overdraw((uint*)c));
		}

		/// <summary>Creates a color filter that visualizes overdraw by mapping the number of times each pixel has been drawn to one of the specified colors.</summary>
		/// <param name="colors">An array of exactly six colors that map to overdraw counts: index 0 is used where no draw occurs, indexes 1 through 4 correspond to one through four overlapping draws, and index 5 is used for five or more overlapping draws.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKColorFilter" />.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="colors" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException"><paramref name="colors" /> does not contain exactly six entries.</exception>
		/// <remarks><para>This is the same operation used by Skia's overdraw debug visualizer to highlight regions that are painted multiple times.</para></remarks>
		public static SKColorFilter CreateOverdraw(SKColor[] colors)
		{
			if (colors == null)
				throw new ArgumentNullException(nameof(colors));
			return CreateOverdraw(colors.AsSpan());
		}

		internal static SKColorFilter GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKColorFilter (h, o));

		// owns/unrefExisting default to true for the general dispose-protected (but mortal) case.
		// The srgb<->linear gamma singletons pass owns:false because Skia returns an *immortal*
		// SkNoDestructor singleton (gSingleton) living in static storage. Unreffing it at
		// finalization drives the native refcount to 0 and runs ~SkColorSpaceXformColorFilter,
		// which calls free()/delete on non-heap memory => STATUS_HEAP_CORRUPTION at teardown.
		// owns:false makes Dispose(bool) skip DisposeNative (it gates on OwnsHandle), so the
		// binding never releases the immortal static. Leaking our single ref is correct: the
		// object is never destroyed by Skia anyway.
		internal static SKColorFilter GetDisposeProtectedObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddDisposeProtectedObject (handle, owns, unrefExisting, (h, o) => new SKColorFilter (h, o));
	}
}
