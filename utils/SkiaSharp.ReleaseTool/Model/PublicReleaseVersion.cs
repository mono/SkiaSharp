using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	/// <summary>An exact public package version and its build-revision-free release identity.</summary>
	public sealed record PublicReleaseVersion(
		NuGetVersion Version,
		SkiaSharpReleaseIdentity Identity,
		string Base,
		string? BuildRevision)
	{
		public string Text => Version.ToNormalizedString();

		public static PublicReleaseVersion Parse(string value)
		{
			if (!ReleaseVersionPolicy.TryParseNuGetVersion(value, out var version, out var componentCount) ||
				componentCount is not (3 or 4) ||
				!string.Equals(value, version.ToNormalizedString(), StringComparison.Ordinal))
			{
				throw new PlanException(
					$"invalid public version '{value}': expected X.Y.Z[.F] or X.Y.Z[.F]-(preview|rc).N.<build-revision>");
			}

			var numeric = ReleaseVersionPolicy.FormatNumeric(version, componentCount);
			var labels = version.ReleaseLabels.ToArray();
			if (labels.Length == 0)
			{
				var identity = SkiaSharpReleaseIdentity.Parse(numeric);
				return new PublicReleaseVersion(version, identity, numeric, null);
			}

			if (labels.Length < 3 ||
				labels[0] is not ("preview" or "rc") ||
				!int.TryParse(labels[1], out var iteration) ||
				iteration <= 0)
			{
				throw new PlanException(
					$"invalid public version '{value}': expected a preview.N or rc.N label followed by a build revision");
			}

			var buildRevision = string.Join('.', labels.Skip(2));
			if (!ReleaseVersionPolicy.IsBuildRevision(buildRevision))
				throw new PlanException($"public version '{value}' has an invalid build revision '{buildRevision}'");

			var release = SkiaSharpReleaseIdentity.Parse($"{numeric}-{labels[0]}.{labels[1]}");
			var (validatedBase, validatedRevision) = release.ValidatePublicVersion(value);
			return new PublicReleaseVersion(version, release, validatedBase, validatedRevision);
		}
	}
}
