#nullable disable

using System;
using System.Threading;

namespace SkiaSharp
{
	public unsafe class SKColorFilter : SKObject, ISKReferenceCounted
	{
		public const int ColorMatrixSize = 20;
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

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public static SKColorFilter CreateSrgbToLinearGamma () =>
			LazyInitializer.EnsureInitialized (
				ref srgbToLinear, ref srgbToLinearInitialized, ref srgbToLinearLock,
				() => GetDisposeProtectedObject (SkiaApi.sk_colorfilter_new_srgb_to_linear_gamma (), owns: false, unrefExisting: false));

		public static SKColorFilter CreateLinearToSrgbGamma () =>
			LazyInitializer.EnsureInitialized (
				ref linearToSrgb, ref linearToSrgbInitialized, ref linearToSrgbLock,
				() => GetDisposeProtectedObject (SkiaApi.sk_colorfilter_new_linear_to_srgb_gamma (), owns: false, unrefExisting: false));

		public static SKColorFilter CreateBlendMode(SKColor c, SKBlendMode mode)
		{
			return GetObject (SkiaApi.sk_colorfilter_new_mode((uint)c, mode));
		}

		public static SKColorFilter CreateLighting(SKColor mul, SKColor add)
		{
			return GetObject (SkiaApi.sk_colorfilter_new_lighting((uint)mul, (uint)add));
		}

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

		public static SKColorFilter CreateLerp(float weight, SKColorFilter filter0, SKColorFilter filter1)
		{
			_ = filter0 ?? throw new ArgumentNullException(nameof(filter0));
			_ = filter1 ?? throw new ArgumentNullException(nameof(filter1));

			var colorFilter = GetObject (SkiaApi.sk_colorfilter_new_lerp(weight, filter0.Handle, filter1.Handle));
			GC.KeepAlive (filter0);
			GC.KeepAlive (filter1);
			return colorFilter;
		}

		public static SKColorFilter CreateColorMatrix(float[] matrix)
		{
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));
			return CreateColorMatrix(matrix.AsSpan());
		}

		public static SKColorFilter CreateColorMatrix(ReadOnlySpan<float> matrix)
		{
			if (matrix.Length != 20)
				throw new ArgumentException("Matrix must have a length of 20.", nameof(matrix));
			fixed (float* m = matrix) {
				return GetObject (SkiaApi.sk_colorfilter_new_color_matrix (m));
			}
		}

		public static SKColorFilter CreateHslaColorMatrix(ReadOnlySpan<float> matrix)
		{
			if (matrix.Length != 20)
				throw new ArgumentException("Matrix must have a length of 20.", nameof(matrix));
			fixed (float* m = matrix) {
				return GetObject (SkiaApi.sk_colorfilter_new_hsla_matrix (m));
			}
		}

		public static SKColorFilter CreateLumaColor()
		{
			return GetObject (SkiaApi.sk_colorfilter_new_luma_color());
		}

		public static SKColorFilter CreateTable(byte[] table)
		{
			if (table == null)
				throw new ArgumentNullException(nameof(table));
			return CreateTable(table.AsSpan());
		}

		public static SKColorFilter CreateTable(ReadOnlySpan<byte> table)
		{
			if (table.Length != TableMaxLength)
				throw new ArgumentException($"Table must have a length of {TableMaxLength}.", nameof(table));
			fixed (byte* t = table) {
				return GetObject (SkiaApi.sk_colorfilter_new_table (t));
			}
		}

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

		public static SKColorFilter CreateHighContrast(SKHighContrastConfig config)
		{
			return GetObject (SkiaApi.sk_colorfilter_new_high_contrast(&config));
		}

		public static SKColorFilter CreateHighContrast(bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast)
		{
			return CreateHighContrast(new SKHighContrastConfig(grayscale, invertStyle, contrast));
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
