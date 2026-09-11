using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	/// <summary>Provides extension methods for converting between SkiaSharp and HarfBuzz color types.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// `ColorExtensions` provides convenient extension methods for converting between <xref:SkiaSharp.SKColor>, <xref:SkiaSharp.SKColorF>, and <xref:HarfBuzzSharp.HBColor>.
	///
	/// ## Examples
	///
	/// Converting an `SKColor` to an `HBColor` and back:
	///
	/// ```csharp
	/// SKColor skColor = SKColors.CornflowerBlue;
	/// HBColor hbColor = skColor.ToHBColor();
	/// SKColor restored = hbColor.ToSKColor();
	/// ```
	/// ]]></remarks>
	public static class ColorExtensions
	{
		/// <summary>Converts an <see cref="T:HarfBuzzSharp.HBColor" /> to an <see cref="T:SkiaSharp.SKColor" />.</summary>
		/// <param name="hbColor">The <see cref="T:HarfBuzzSharp.HBColor" /> to convert.</param>
		/// <returns>An <see cref="T:SkiaSharp.SKColor" /> representing the same color.</returns>
		/// <remarks></remarks>
		public static SKColor ToSKColor (this HBColor hbColor)
		{
			return new SKColor (hbColor.Red, hbColor.Green, hbColor.Blue, hbColor.Alpha);
		}

		/// <summary>Converts an <see cref="T:HarfBuzzSharp.HBColor" /> to an <see cref="T:SkiaSharp.SKColorF" />.</summary>
		/// <param name="hbColor">The <see cref="T:HarfBuzzSharp.HBColor" /> to convert.</param>
		/// <returns>An <see cref="T:SkiaSharp.SKColorF" /> representing the same color with each channel normalized to the range [0, 1].</returns>
		/// <remarks></remarks>
		public static SKColorF ToSKColorF (this HBColor hbColor)
		{
			return new SKColorF (
				hbColor.Red / 255f,
				hbColor.Green / 255f,
				hbColor.Blue / 255f,
				hbColor.Alpha / 255f);
		}

		/// <summary>Converts an array of <see cref="T:HarfBuzzSharp.HBColor" /> values to an array of <see cref="T:SkiaSharp.SKColor" /> values.</summary>
		/// <param name="hbColors">The array of <see cref="T:HarfBuzzSharp.HBColor" /> values to convert.</param>
		/// <returns>An array of <see cref="T:SkiaSharp.SKColor" /> values corresponding to each element of <paramref name="hbColors" />.</returns>
		/// <remarks></remarks>
		public static SKColor[] ToSKColors (this HBColor[] hbColors)
		{
			if (hbColors == null)
				return null;

			var result = new SKColor[hbColors.Length];
			for (int i = 0; i < hbColors.Length; i++)
				result[i] = hbColors[i].ToSKColor ();
			return result;
		}

		/// <summary>Converts an <see cref="T:SkiaSharp.SKColor" /> to an <see cref="T:HarfBuzzSharp.HBColor" />.</summary>
		/// <param name="color">The <see cref="T:SkiaSharp.SKColor" /> to convert.</param>
		/// <returns>An <see cref="T:HarfBuzzSharp.HBColor" /> representing the same color.</returns>
		/// <remarks></remarks>
		public static HBColor ToHBColor (this SKColor color)
		{
			return new HBColor (color.Red, color.Green, color.Blue, color.Alpha);
		}

		/// <summary>Converts an <see cref="T:SkiaSharp.SKColorF" /> to an <see cref="T:HarfBuzzSharp.HBColor" />.</summary>
		/// <param name="color">The <see cref="T:SkiaSharp.SKColorF" /> to convert.</param>
		/// <returns>An <see cref="T:HarfBuzzSharp.HBColor" /> representing the same color with each channel quantized to 8 bits.</returns>
		/// <remarks></remarks>
		public static HBColor ToHBColor (this SKColorF color)
		{
			return new HBColor (
				ClampToByte (color.Red * 255f),
				ClampToByte (color.Green * 255f),
				ClampToByte (color.Blue * 255f),
				ClampToByte (color.Alpha * 255f));
		}

		private static byte ClampToByte (float value)
		{
			if (value <= 0f) return 0;
			if (value >= 255f) return 255;
			return (byte)(value + 0.5f);
		}
	}
}
