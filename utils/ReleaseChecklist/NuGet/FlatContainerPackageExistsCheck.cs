using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ReleaseChecklist.Core;

namespace ReleaseChecklist.NuGet;

/// <summary>Checks whether an exact package version appears in a NuGet flat-container index.</summary>
/// <remarks>
/// This lightweight check proves only flat-container presence. Finish-phase package validation must
/// additionally verify listing, registration metadata, retrievability, package bytes, and signatures.
/// </remarks>
public sealed class FlatContainerPackageExistsCheck : IChecklistCheck
{
	private readonly Func<string, CancellationToken, Task<IReadOnlyList<NuGetVersion>>> getVersions;
	private readonly string packageId;
	private readonly NuGetVersion expected;

	/// <summary>Initializes a new instance of the <see cref="FlatContainerPackageExistsCheck" /> class.</summary>
	/// <param name="source">The NuGet V3 service index URL.</param>
	/// <param name="packageId">The package identifier.</param>
	/// <param name="expected">The exact expected version.</param>
	public FlatContainerPackageExistsCheck(
		string source,
		string packageId,
		NuGetVersion expected)
		: this(CreateVersionReader(source), packageId, expected)
	{
	}

	internal FlatContainerPackageExistsCheck(
		Func<string, CancellationToken, Task<IReadOnlyList<NuGetVersion>>> getVersions,
		string packageId,
		NuGetVersion expected)
	{
		this.getVersions = getVersions;
		this.packageId = packageId;
		this.expected = expected;
	}

	/// <inheritdoc />
	public async ValueTask<CheckResult> EvaluateAsync(CancellationToken cancellationToken)
	{
		var versions = await getVersions(packageId, cancellationToken).ConfigureAwait(false);
		var matches = versions
			.Where(version => VersionComparer.VersionRelease.Equals(version, expected))
			.ToArray();
		var observation = new ObservationBuilder()
			.Add("package", packageId)
			.Add("version", expected.ToNormalizedString())
			.Add("matches", matches.Length)
			.Build();
		return matches.Length switch
		{
			0 => CheckResult.NotDone(
				$"{packageId} {expected.ToNormalizedString()} is absent from the flat container.",
				observation),
			1 => CheckResult.Done(
				"Exactly one flat-container entry was discovered.",
				observation),
			_ => CheckResult.Blocked(
				$"{packageId} {expected.ToNormalizedString()} has ambiguous flat-container entries.",
				observation),
		};
	}

	private static Func<string, CancellationToken, Task<IReadOnlyList<NuGetVersion>>> CreateVersionReader(
		string source)
	{
		var repository = Repository.Factory.GetCoreV3(new PackageSource(source));
		return async (packageId, cancellationToken) =>
		{
			var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken)
				.ConfigureAwait(false);
			using var cache = new SourceCacheContext();
			var versions = await resource!.GetAllVersionsAsync(
				packageId,
				cache,
				NullLogger.Instance,
				cancellationToken).ConfigureAwait(false);
			return versions.ToArray();
		};
	}
}
