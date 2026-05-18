using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	/// <summary>
	/// Extension methods for converting between <see cref="HBColor"/> and SkiaSharp color types.
	/// </summary>
	public static class ColorExtensions
	{
		/// <summary>
		/// Converts an <see cref="HBColor"/> to an <see cref="SKColor"/>.
		/// </summary>
		public static SKColor ToSKColor (this HBColor hbColor)
		{
			return new SKColor (hbColor.Red, hbColor.Green, hbColor.Blue, hbColor.Alpha);
		}

		/// <summary>
		/// Converts an <see cref="HBColor"/> to an <see cref="SKColorF"/> (normalized 0.0–1.0 channels).
		/// </summary>
		public static SKColorF ToSKColorF (this HBColor hbColor)
		{
			return new SKColorF (
				hbColor.Red / 255f,
				hbColor.Green / 255f,
				hbColor.Blue / 255f,
				hbColor.Alpha / 255f);
		}

		/// <summary>
		/// Converts an array of <see cref="HBColor"/> values to an array of <see cref="SKColor"/>.
		/// </summary>
		public static SKColor[] ToSKColors (this HBColor[] hbColors)
		{
			if (hbColors == null)
				return null;

			var result = new SKColor[hbColors.Length];
			for (int i = 0; i < hbColors.Length; i++)
				result[i] = hbColors[i].ToSKColor ();
			return result;
		}

		/// <summary>
		/// Converts an <see cref="SKColor"/> to an <see cref="HBColor"/>.
		/// </summary>
		public static HBColor ToHBColor (this SKColor color)
		{
			return new HBColor (color.Red, color.Green, color.Blue, color.Alpha);
		}

		/// <summary>
		/// Converts an <see cref="SKColorF"/> to an <see cref="HBColor"/> (clamped to 0–255 range).
		/// </summary>
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
