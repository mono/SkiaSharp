using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	/// <summary>HarfBuzzSharp version increment and next-version calculation.</summary>
	public static class HarfBuzzVersioning
	{
		/// <summary>Bumps a HarfBuzzSharp version the same way create-release-branches.py did.</summary>
		public static string IncrementHarfBuzz(string value)
		{
			var version = ReleaseVersionPolicy.ParseStableVersion(
				value, "HarfBuzzSharp version", 3, 4);
			if (version.Version.Revision < 0)
				return $"{version.Version.ToString(3)}.1";

			if (version.Version.Revision == int.MaxValue)
				throw new PlanException($"cannot increment HarfBuzzSharp version '{value}': revision overflow");
			return new Version(
				version.Major,
				version.Minor,
				version.Patch,
				version.Version.Revision + 1).ToString(4);
		}

		/// <summary>Computes the next preview.0 SkiaSharp/HarfBuzzSharp versions after a stable cut.</summary>
		public static (string NextSkia, string NextHarfBuzz) CalculateNextVersions(
			string releasedNumeric, string currentHarfBuzz)
		{
			var released = ReleaseVersionPolicy.ParseStableVersion(
				releasedNumeric, "released SkiaSharp version", 3);
			if (released.Patch == int.MaxValue)
				throw new PlanException($"cannot calculate next version from '{releasedNumeric}': patch overflow");
			var nextSkia = new Version(released.Major, released.Minor, released.Patch + 1).ToString(3);
			return (nextSkia, IncrementHarfBuzz(currentHarfBuzz));
		}
	}
}
