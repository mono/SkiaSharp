using ReleaseChecklist.Git;

namespace SkiaSharp.ReleaseChecklist;

/// <summary>Discovers and validates immutable inputs for the SkiaSharp Prepare definition.</summary>
public static class ReleaseDiscovery
{
	/// <summary>Asynchronously discovers release identity, source, branch, gitlink, and bump inputs.</summary>
	/// <param name="repository">The repository state reader.</param>
	/// <param name="options">The discovery inputs, or <see langword="null" /> to use the current branch and infer an eligible preview.</param>
	/// <param name="cancellationToken">A token that cancels discovery.</param>
	/// <returns>The validated immutable discovery result.</returns>
	/// <exception cref="ReleasePolicyException">The inputs are ambiguous, unsupported, unpublished, or inconsistent with repository state.</exception>
	public static async Task<ReleaseDiscoveryResult> DiscoverAsync(
		IReleaseDiscoveryRepository repository,
		ReleaseDiscoveryOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		options ??= new ReleaseDiscoveryOptions();
		var rawBranch = options.Branch ??
			await repository.CurrentBranchAsync(cancellationToken).ConfigureAwait(false);
		var branch = NormalizeBranch(rawBranch, repository.RemoteName);
		var sourceKind = Classify(branch);
		var sourceRef = await ResolveSourceRefAsync(
			repository, rawBranch, branch, cancellationToken).ConfigureAwait(false);
		var sourceSha = await repository.ResolveAsync(sourceRef, cancellationToken).ConfigureAwait(false);
		var sourceState = await ReadStateAsync(repository, sourceRef, cancellationToken).ConfigureAwait(false);
		var branches = await repository.ReleaseBranchesAsync(cancellationToken).ConfigureAwait(false);

		ReleaseIdentity identity;
		if (options.Release is not null)
		{
			identity = ReleaseIdentity.Parse(options.Release);
		}
		else if (sourceKind == ReleaseSourceKind.ExactRelease)
		{
			identity = ReleaseIdentity.Parse(branch["release/".Length..]);
		}
		else
		{
			identity = InferPreview(sourceState, branches);
		}

		ValidateSelection(identity, sourceKind, branch, sourceState, options.Release is not null);
		var remoteSourceSha = await repository.RemoteBranchShaAsync(branch, cancellationToken)
			.ConfigureAwait(false);
		if (remoteSourceSha != sourceSha)
		{
			throw new ReleasePolicyException(
				$"Selected source '{branch}' is not the exact remote SHA {sourceSha}.");
		}
		var maintenance = identity.MaintenanceBranch;
		var maintenanceSha = await repository.RemoteBranchShaAsync(maintenance, cancellationToken)
			.ConfigureAwait(false);
		var maintenanceExists = maintenanceSha is not null;
		var maintenanceExpectedSha = maintenanceSha ?? sourceSha;
		if (!maintenanceExists && !identity.IsHotfix)
		{
			if (sourceState.SkiaSharpVersion == identity.Numeric && sourceState.Label == "preview.0")
			{
				maintenanceExpectedSha = sourceSha;
			}
			else if (options.MaintenanceBase is not null)
			{
				var remotePrefix = $"refs/remotes/{repository.RemoteName}/";
				var isSha = options.MaintenanceBase.Length == 40 &&
					options.MaintenanceBase.All(static c =>
						c is >= '0' and <= '9' or >= 'a' and <= 'f');
				if (!isSha &&
					!options.MaintenanceBase.StartsWith(remotePrefix, StringComparison.Ordinal))
				{
					throw new ReleasePolicyException(
						$"--maintenance-base must be a full {remotePrefix} ref or commit SHA.");
				}
				var maintenanceBaseSha = await repository.ResolveAsync(
					options.MaintenanceBase, cancellationToken).ConfigureAwait(false);
				if (!await repository.IsContainedInRemoteBranchAsync(
					maintenanceBaseSha, cancellationToken).ConfigureAwait(false))
				{
					throw new ReleasePolicyException(
						$"Maintenance base {maintenanceBaseSha} is not contained in a remote branch.");
				}
				var maintenanceState = await ReadStateAsync(
					repository, options.MaintenanceBase, cancellationToken).ConfigureAwait(false);
				if (maintenanceState.SkiaSharpVersion != identity.Numeric ||
					maintenanceState.Label != "preview.0")
				{
					throw new ReleasePolicyException(
						$"Maintenance base '{options.MaintenanceBase}' is not " +
						$"{identity.Numeric} at preview.0.");
				}
				maintenanceExpectedSha = maintenanceBaseSha;
			}
			else
			{
				throw new ReleasePolicyException(
					$"Maintenance branch '{maintenance}' is missing; specify a reviewed " +
					$"--maintenance-base at {identity.Numeric} preview.0.");
			}
		}

		string releaseBaseRef;
		string releaseBaseSha;
		if (sourceKind == ReleaseSourceKind.ExactRelease || identity.IsHotfix)
		{
			releaseBaseRef = sourceRef;
			releaseBaseSha = sourceSha;
		}
		else
		{
			var latest = LatestPrerelease(branches, identity);
			if (identity.Channel == ReleaseChannel.Preview && identity.Iteration == 1)
				latest = null;
			if (latest is not null)
			{
				releaseBaseRef = RemoteRef(repository.RemoteName, latest);
				releaseBaseSha = await repository.ResolveAsync(
					releaseBaseRef, cancellationToken).ConfigureAwait(false);
			}
			else if (maintenanceExists)
			{
				releaseBaseRef = RemoteRef(repository.RemoteName, maintenance);
				releaseBaseSha = maintenanceSha!;
			}
			else
			{
				releaseBaseRef = sourceRef;
				releaseBaseSha = sourceSha;
			}
		}

		var skiaSha = await repository.ReadGitlinkAsync(
			releaseBaseRef, "externals/skia", cancellationToken).ConfigureAwait(false);
		var bump = StableBump.None;
		if (identity.IsStable && !identity.IsHotfix)
		{
			var releasedState = await ReadStateAsync(
				repository, releaseBaseRef, cancellationToken).ConfigureAwait(false);
			var (nextSkia, nextHarfBuzz) = VersionFiles.NextVersions(identity, releasedState);
			var maintenanceState = await ReadStateAsync(
				repository, maintenanceExpectedSha, cancellationToken).ConfigureAwait(false);
			var maintenanceVersion = NuGet.Versioning.NuGetVersion.Parse(
				maintenanceState.SkiaSharpVersion);
			var alreadyAdvanced =
				maintenanceState.Label == "preview.0" &&
				NuGet.Versioning.VersionComparer.VersionRelease.Compare(
					maintenanceVersion,
					NuGet.Versioning.NuGetVersion.Parse(nextSkia)) >= 0;
			if (!alreadyAdvanced)
			{
				bump = new StableBump(
					true,
					$"bump-version-{nextSkia}",
					nextSkia,
					nextHarfBuzz);
			}
		}

		return new ReleaseDiscoveryResult(
			identity,
			sourceKind,
			branch,
			sourceRef,
			sourceSha,
			sourceState,
			maintenance,
			maintenanceExists,
			maintenanceExpectedSha,
			releaseBaseRef,
			releaseBaseSha,
			identity.ReleaseBranch,
			skiaSha,
			bump);
	}

	private static void ValidateSelection(
		ReleaseIdentity identity,
		ReleaseSourceKind sourceKind,
		string sourceBranch,
		VersionState sourceState,
		bool releaseWasExplicit)
	{
		if (sourceKind == ReleaseSourceKind.Maintenance &&
			sourceBranch != identity.MaintenanceBranch)
			throw new ReleasePolicyException(
				$"Source '{sourceBranch}' does not match release line '{identity.MaintenanceBranch}'.");
		if (sourceKind == ReleaseSourceKind.ExactRelease && !releaseWasExplicit &&
			sourceBranch != identity.ReleaseBranch)
			throw new ReleasePolicyException("Exact branch identity mismatch.");
		ReleaseIdentity? exactSource = null;
		if (sourceKind == ReleaseSourceKind.ExactRelease)
		{
			exactSource = ReleaseIdentity.Parse(sourceBranch["release/".Length..]);
			if (sourceState.SkiaSharpVersion != exactSource.Numeric ||
				sourceState.Label != exactSource.Label)
			{
				throw new ReleasePolicyException(
					$"Exact source branch '{sourceBranch}' does not match its version state.");
			}
			var compatibleNumeric = identity.IsHotfix
				? exactSource.Raw == identity.Raw ||
					exactSource.Numeric == identity.Version.Version.ToString(3)
				: exactSource.Numeric == identity.Numeric;
			if (!compatibleNumeric)
			{
				throw new ReleasePolicyException(
					$"Exact source '{exactSource.Raw}' is not in release lineage '{identity.Numeric}'.");
			}
		}
		if (identity.IsHotfix)
		{
			if (!releaseWasExplicit && sourceKind != ReleaseSourceKind.ExactRelease)
				throw new ReleasePolicyException("A new four-part hotfix is never inferred.");
			if (sourceKind == ReleaseSourceKind.Main)
				throw new ReleasePolicyException("A four-part hotfix must start from a release/* branch.");
			var parent = identity.Version.Version.ToString(3);
			if (sourceKind == ReleaseSourceKind.ExactRelease)
			{
				var resumesExact = exactSource!.Raw == identity.Raw;
				if (!resumesExact && exactSource.Numeric != parent)
					throw new ReleasePolicyException(
						$"Hotfix '{identity.Raw}' requires exact source version '{parent}'.");
			}
			else if (sourceState.SkiaSharpVersion != parent)
				throw new ReleasePolicyException(
					$"Hotfix '{identity.Raw}' requires exact source version '{parent}'.");
		}
		else if (sourceKind == ReleaseSourceKind.Maintenance)
		{
			if (!IsAtOrBeyondRelease(sourceState, identity))
			{
				throw new ReleasePolicyException(
					$"Source version '{sourceState.SkiaSharpVersion}/{sourceState.Label}' " +
					$"is not at or beyond '{identity.Numeric}' on its maintenance line.");
			}
		}
		else if (sourceKind != ReleaseSourceKind.ExactRelease &&
			sourceState.SkiaSharpVersion != identity.Numeric)
		{
			throw new ReleasePolicyException(
				$"Source version '{sourceState.SkiaSharpVersion}' does not match '{identity.Numeric}'.");
		}
	}

	private static ReleaseIdentity InferPreview(
		VersionState state,
		IReadOnlyList<string> branches)
	{
		if (state.Label != "preview.0")
			throw new ReleasePolicyException("Only preview.0 state can infer a release.");
		if (state.SkiaSharpVersion.Split('.').Length != 3)
			throw new ReleasePolicyException("A four-part hotfix is never inferred.");
		var numeric = state.SkiaSharpVersion;
		if (branches.Contains($"release/{numeric}", StringComparer.Ordinal) ||
			branches.Any(branch => branch.StartsWith($"release/{numeric}-rc.", StringComparison.Ordinal)))
		{
			throw new ReleasePolicyException(
				$"Stable or RC state already exists for {numeric}; select --release explicitly.");
		}
		var prefix = $"release/{numeric}-preview.";
		var max = branches
			.Where(branch => branch.StartsWith(prefix, StringComparison.Ordinal))
			.Select(branch => int.TryParse(branch[prefix.Length..], out var value) ? value : 0)
			.DefaultIfEmpty()
			.Max();
		return ReleaseIdentity.Parse($"{numeric}-preview.{checked(max + 1)}");
	}

	private static string? LatestPrerelease(
		IReadOnlyList<string> branches,
		ReleaseIdentity requested)
	{
		var candidates = branches
			.Select(branch => (
				Branch: branch,
				Identity: branch.StartsWith("release/", StringComparison.Ordinal) &&
					ReleaseIdentity.TryParse(branch["release/".Length..], out var identity)
						? identity
						: null))
			.Where(item =>
				item.Identity is { IsStable: false } &&
				item.Identity.Numeric == requested.Numeric)
			.ToArray();
		if (candidates.Any(item => Compare(item.Identity!, requested) > 0))
		{
			throw new ReleasePolicyException(
				$"A later prerelease branch already exists for {requested.Numeric}.");
		}
		return candidates
			.Where(item => Compare(item.Identity!, requested) < 0)
			.OrderBy(item => item.Identity!, Comparer<ReleaseIdentity>.Create(Compare))
			.Select(static item => item.Branch)
			.LastOrDefault();
	}

	private static int Compare(ReleaseIdentity left, ReleaseIdentity right)
	{
		var channel = ChannelOrder(left.Channel).CompareTo(ChannelOrder(right.Channel));
		return channel != 0
			? channel
			: Nullable.Compare(left.Iteration, right.Iteration);
	}

	private static int ChannelOrder(ReleaseChannel channel) => channel switch
	{
		ReleaseChannel.Preview => 0,
		ReleaseChannel.ReleaseCandidate => 1,
		ReleaseChannel.Stable => 2,
		_ => throw new InvalidOperationException("Unknown release channel."),
	};

	private static async Task<string> ResolveSourceRefAsync(
		IReleaseDiscoveryRepository repository,
		string raw,
		string branch,
		CancellationToken cancellationToken)
	{
		if (raw.StartsWith("refs/", StringComparison.Ordinal) &&
			await repository.RefExistsAsync(raw, cancellationToken).ConfigureAwait(false))
			return raw;
		var remote = RemoteRef(repository.RemoteName, branch);
		if (await repository.RefExistsAsync(remote, cancellationToken).ConfigureAwait(false))
			return remote;
		throw new ReleasePolicyException($"Selected source branch '{branch}' does not exist on origin.");
	}

	private static async Task<VersionState> ReadStateAsync(
		IReleaseDiscoveryRepository repository,
		string reference,
		CancellationToken cancellationToken)
	{
		var variables = await repository.ReadRefFileAsync(
			reference, VersionFiles.VariablesPath, cancellationToken).ConfigureAwait(false);
		var versions = await repository.ReadRefFileAsync(
			reference, VersionFiles.VersionsPath, cancellationToken).ConfigureAwait(false);
		return VersionFiles.Parse(variables, versions);
	}

	private static string NormalizeBranch(string value, string remote)
	{
		foreach (var prefix in new[] { $"refs/remotes/{remote}/", "refs/heads/", $"{remote}/" })
			if (value.StartsWith(prefix, StringComparison.Ordinal))
				return value[prefix.Length..];
		return value;
	}

	private static string RemoteRef(string remote, string branch) =>
		$"refs/remotes/{remote}/{branch}";

	private static bool IsAtOrBeyondRelease(VersionState state, ReleaseIdentity identity)
	{
		if (state.Label != "preview.0" ||
			state.SkiaSharpVersion.Split('.').Length != 3 ||
			!NuGet.Versioning.NuGetVersion.TryParse(state.SkiaSharpVersion, out var version))
			return false;
		return version.Major == identity.Version.Major &&
			version.Minor == identity.Version.Minor &&
			NuGet.Versioning.VersionComparer.VersionRelease.Compare(version, identity.Version) >= 0;
	}

	private static ReleaseSourceKind Classify(string branch)
	{
		if (branch == "main")
			return ReleaseSourceKind.Main;
		if (!branch.StartsWith("release/", StringComparison.Ordinal))
			throw new ReleasePolicyException(
				$"Unsupported release source '{branch}'; expected main or release/*.");
		var suffix = branch["release/".Length..];
		if (suffix.EndsWith(".x", StringComparison.Ordinal) &&
			suffix[..^2].Split('.') is [var major, var minor] &&
			major.All(char.IsAsciiDigit) &&
			minor.All(char.IsAsciiDigit))
		{
			return ReleaseSourceKind.Maintenance;
		}
		_ = ReleaseIdentity.Parse(suffix);
		return ReleaseSourceKind.ExactRelease;
	}
}
