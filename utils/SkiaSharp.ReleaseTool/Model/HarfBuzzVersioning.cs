using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	/// <summary>
	/// HarfBuzzSharp version increment and next-version calculation.
	/// Ported from Python's <c>release_model.increment_harfbuzz</c> and
	/// <c>calculate_next_versions</c>.
	/// </summary>
	public static class HarfBuzzVersioning
	{
		/// <summary>Bumps a HarfBuzzSharp version the same way create-release-branches.py did.</summary>
		public static string IncrementHarfBuzz(string value)
		{
			var parts = value.Split('.');
			if (parts.Length == 3 && Array.TrueForAll(parts, IsAllDigits))
				return $"{value}.1";
			if (parts.Length == 4 && Array.TrueForAll(parts, IsAllDigits))
				return string.Join('.', parts[0], parts[1], parts[2], (int.Parse(parts[3]) + 1).ToString());
			throw new PlanException($"cannot increment HarfBuzzSharp version '{value}'");
		}

		/// <summary>Computes the next preview.0 SkiaSharp/HarfBuzzSharp versions after a stable cut.</summary>
		public static (string NextSkia, string NextHarfBuzz) CalculateNextVersions(
			string releasedNumeric, string currentHarfBuzz)
		{
			var parts = releasedNumeric.Split('.');
			if (parts.Length != 3)
				throw new PlanException(
					$"cannot calculate next version from hotfix release '{releasedNumeric}'");
			var major = int.Parse(parts[0]);
			var minor = int.Parse(parts[1]);
			var patch = int.Parse(parts[2]);
			var nextSkia = $"{major}.{minor}.{patch + 1}";
			return (nextSkia, IncrementHarfBuzz(currentHarfBuzz));
		}

		private static bool IsAllDigits(string part) => part.Length > 0 && Array.TrueForAll(part.ToCharArray(), char.IsAsciiDigit);
	}
}
