using NuGet.Packaging;
using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Model;

namespace SkiaSharp.ReleaseTool.NuGet
{
	internal sealed record PublicReleaseReceipt(
		NuGetVersion SkiaSharpVersion,
		NuGetVersion Base,
		string Label,
		string? BuildRevision,
		string SourceCommit,
		string SourceBranch,
		NuGetVersion HarfBuzzSharpVersion,
		IReadOnlyList<VerifiedPackage> Packages,
		IReadOnlyList<string> Warnings);

	internal sealed record PackageRequest(string Id, NuGetVersion Version)
	{
		public override string ToString() => $"{Id} {Version.ToNormalizedString()}";
	}

	internal interface IPublicReceiptVerifier
	{
		Task<PublicReleaseReceipt> VerifyAsync(
			IFinishRepository repository,
			PublicReleaseVersion requestedVersion,
			ReleasePolicies policies,
			CancellationToken cancellationToken);
	}

	internal sealed class PublicReceiptVerifier : IPublicReceiptVerifier
	{
		public static readonly TimeSpan DefaultDeadline = TimeSpan.FromMinutes(20);
		public static readonly TimeSpan DefaultPollInterval = TimeSpan.FromSeconds(30);

		private readonly IPublicPackageSource source;
		private readonly IPackageSignatureVerifier signatureVerifier;
		private readonly TimeProvider timeProvider;
		private readonly Func<TimeSpan, CancellationToken, Task> delay;
		private readonly TimeSpan deadline;
		private readonly TimeSpan pollInterval;

		public PublicReceiptVerifier(
			IPublicPackageSource source,
			IPackageSignatureVerifier signatureVerifier,
			TimeProvider? timeProvider = null,
			Func<TimeSpan, CancellationToken, Task>? delay = null,
			TimeSpan? deadline = null,
			TimeSpan? pollInterval = null)
		{
			this.source = source;
			this.signatureVerifier = signatureVerifier;
			this.timeProvider = timeProvider ?? TimeProvider.System;
			this.delay = delay ?? ((duration, cancellationToken) =>
				Task.Delay(duration, this.timeProvider, cancellationToken));
			this.deadline = deadline ?? DefaultDeadline;
			this.pollInterval = pollInterval ?? DefaultPollInterval;
			if (this.deadline < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(deadline));
			if (this.pollInterval <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(pollInterval));
		}

		public async Task<PublicReleaseReceipt> VerifyAsync(
			IFinishRepository repository,
			PublicReleaseVersion requestedVersion,
			ReleasePolicies policies,
			CancellationToken cancellationToken)
		{
			var started = timeProvider.GetTimestamp();
			var warnings = new List<string>();
			var bootstrapRequests = new[]
			{
				new PackageRequest("SkiaSharp", requestedVersion.Version),
				new PackageRequest("SkiaSharp.HarfBuzz", requestedVersion.Version),
			};
			RequireConfiguredAnchors(
				policies,
				bootstrapRequests.Select(static request => request.Id));
			var bootstrapCatalog = await PollAsync(
				bootstrapRequests,
				started,
				cancellationToken).ConfigureAwait(false);
			var bootstrapPackages = await DownloadAllAsync(
				bootstrapRequests,
				bootstrapCatalog,
				started,
				policies,
				cancellationToken).ConfigureAwait(false);
			var skia = bootstrapPackages[bootstrapRequests[0]];
			var wrapper = bootstrapPackages[bootstrapRequests[1]];

			if (skia.SourceBranch != requestedVersion.Identity.ReleaseBranch)
				throw new NuGetReceiptException(
					$"SkiaSharp package source branch '{skia.SourceBranch}' does not match release '{requestedVersion.Identity.ReleaseBranch}'");
			if (wrapper.SourceCommit != skia.SourceCommit ||
				wrapper.SourceBranch != skia.SourceBranch)
			{
				throw new NuGetReceiptException(
					"SkiaSharp.HarfBuzz does not embed the SkiaSharp anchor source commit and branch");
			}

			if (!await repository.CommitExistsAsync(skia.SourceCommit, cancellationToken).ConfigureAwait(false))
				throw new NuGetReceiptException($"package source commit {skia.SourceCommit} does not exist");
			var sourceRef = $"refs/remotes/origin/{skia.SourceBranch}";
			if (!await repository.RefExistsAsync(sourceRef, cancellationToken).ConfigureAwait(false) ||
				!await repository.IsAncestorAsync(skia.SourceCommit, sourceRef, cancellationToken).ConfigureAwait(false))
			{
				throw new ConflictException(
					$"package source commit {skia.SourceCommit} is not contained by {skia.SourceBranch}");
			}
			var branchHead = await repository.RemoteShaAsync(
				skia.SourceBranch,
				cancellationToken: cancellationToken).ConfigureAwait(false);
			if (branchHead is null)
				throw new ConflictException($"package source branch {skia.SourceBranch} is absent on origin");
			if (branchHead != skia.SourceCommit)
			{
				warnings.Add(
					$"{skia.SourceBranch} has advanced to {branchHead} after package source commit {skia.SourceCommit}");
			}

			var variablesText = await repository.ReadRefFileAsync(
				skia.SourceCommit,
				"scripts/azure-templates-variables.yml",
				cancellationToken).ConfigureAwait(false);
			var versionsText = await repository.ReadRefFileAsync(
				skia.SourceCommit,
				"scripts/VERSIONS.txt",
				cancellationToken).ConfigureAwait(false);
			var historical = VersionStateReader.Parse(variablesText, versionsText);
			var requestedBase = new NuGetVersion(requestedVersion.Base);
			if (!VersionComparer.VersionRelease.Equals(historical.Skia, requestedBase))
			{
				throw new NuGetReceiptException(
					$"SkiaSharp base at {skia.SourceCommit} is {historical.Skia}, expected {requestedVersion.Base}");
			}
			if (requestedVersion.Identity.Stable)
			{
				if (historical.Label is not ("stable" or "preview.0"))
					throw new NuGetReceiptException($"stable release has incompatible PREVIEW_LABEL '{historical.Label}'");
				if (historical.Label == "preview.0")
				{
					warnings.Add(
						$"PREVIEW_LABEL at {skia.SourceCommit} is 'preview.0' for a stable release");
				}
			}
			else if (historical.Label != requestedVersion.Identity.Label)
			{
				throw new NuGetReceiptException(
					$"PREVIEW_LABEL at {skia.SourceCommit} is '{historical.Label}', expected '{requestedVersion.Identity.Label}'");
			}

			var harfBuzzVersion = requestedVersion.Identity.Stable
				? historical.HarfBuzz
				: NuGetVersion.Parse(
					$"{historical.HarfBuzzText}-{requestedVersion.Identity.Label}.{requestedVersion.BuildRevision}");
			var dependencyMinimum = CollapseDependencyMinimum(
				wrapper.DependencyGroups,
				"HarfBuzzSharp");
			if (!VersionComparer.VersionRelease.Equals(dependencyMinimum, harfBuzzVersion))
			{
				throw new NuGetReceiptException(
					$"SkiaSharp.HarfBuzz depends on HarfBuzzSharp {dependencyMinimum}, expected {harfBuzzVersion}");
			}

			var families = VersionsTxt.ParsePackageFamilies(versionsText);
			var requests = families["SkiaSharp"]
				.Select(id => new PackageRequest(id, requestedVersion.Version))
				.Concat(families["HarfBuzzSharp"].Select(id => new PackageRequest(id, harfBuzzVersion)))
				.Distinct()
				.ToArray();
			foreach (var anchor in policies.AnchorPackages)
			{
				if (!requests.Any(request => request.Id == anchor))
					throw new NuGetReceiptException($"historical package inventory does not contain required anchor '{anchor}'");
			}
			RequireConfiguredAnchors(policies, ["HarfBuzzSharp"]);

			var bootstrap = new Dictionary<PackageRequest, VerifiedPackage>
			{
				[bootstrapRequests[0]] = skia,
				[bootstrapRequests[1]] = wrapper,
			};
			var remainingRequests = requests.Where(request => !bootstrap.ContainsKey(request)).ToArray();
			var remainingCatalog = await PollAsync(
				remainingRequests,
				started,
				cancellationToken).ConfigureAwait(false);
			var remainingPackages = await DownloadAllAsync(
				remainingRequests,
				remainingCatalog,
				started,
				policies,
				cancellationToken).ConfigureAwait(false);
			var packages = requests
				.Select(request => bootstrap.TryGetValue(request, out var package)
					? package
					: remainingPackages[request])
				.ToArray();

			foreach (var package in packages.Where(
				package => families["SkiaSharp"].Contains(package.Id, StringComparer.Ordinal)))
			{
				if (package.SourceCommit != skia.SourceCommit || package.SourceBranch != skia.SourceBranch)
				{
					throw new NuGetReceiptException(
						$"{package.Id} embeds {package.SourceCommit} on {package.SourceBranch}, expected {skia.SourceCommit} on {skia.SourceBranch}");
				}
			}

			var harfBuzzPackages = packages.Where(
				package => families["HarfBuzzSharp"].Contains(package.Id, StringComparer.Ordinal)).ToArray();
			var harfBuzzAnchor = harfBuzzPackages.Single(package => package.Id == "HarfBuzzSharp");
			foreach (var package in harfBuzzPackages)
			{
				if (package.SourceCommit != harfBuzzAnchor.SourceCommit ||
					package.SourceBranch != harfBuzzAnchor.SourceBranch)
				{
					throw new NuGetReceiptException(
						$"HarfBuzzSharp family source is inconsistent: {package.Id} embeds {package.SourceCommit} on {package.SourceBranch}");
				}
			}

			return new PublicReleaseReceipt(
				requestedVersion.Version,
				new NuGetVersion(requestedVersion.Base),
				requestedVersion.Identity.Label,
				requestedVersion.BuildRevision,
				skia.SourceCommit,
				skia.SourceBranch,
				harfBuzzVersion,
				packages,
				warnings);
		}

		private static void RequireConfiguredAnchors(
			ReleasePolicies policies,
			IEnumerable<string> required)
		{
			foreach (var id in required)
			{
				if (!policies.AnchorPackages.Contains(id))
					throw new NuGetReceiptException($"public package policy is missing required anchor '{id}'");
			}
		}

		private async Task<IReadOnlyDictionary<PackageRequest, CatalogPackage>> PollAsync(
			IReadOnlyList<PackageRequest> requests,
			long started,
			CancellationToken cancellationToken)
		{
			var resolved = new Dictionary<PackageRequest, CatalogPackage>();
			var pending = requests.Distinct().ToList();
			while (pending.Count > 0)
			{
				var unresolved = new List<PackageRequest>();
				foreach (var batch in pending.Chunk(8))
				{
					var attempts = await Task.WhenAll(batch.Select(async request =>
					{
						try
						{
							var catalog = await source.GetCatalogPackageAsync(
								request.Id,
								request.Version,
								cancellationToken).ConfigureAwait(false);
							return new CatalogAttempt(request, catalog);
						}
						catch (NuGetTransientException)
						{
							return new CatalogAttempt(request, null);
						}
					})).ConfigureAwait(false);
					foreach (var attempt in attempts)
					{
						if (attempt.Catalog is null)
						{
							unresolved.Add(attempt.Request);
							continue;
						}
						PackageVerifier.ValidateCatalog(
							attempt.Request.Id,
							attempt.Request.Version,
							attempt.Catalog);
						if (!attempt.Catalog.Listed)
						{
							unresolved.Add(attempt.Request);
							continue;
						}
						resolved.Add(attempt.Request, attempt.Catalog);
					}
				}
				pending = unresolved;
				if (pending.Count == 0)
					break;

				var elapsed = timeProvider.GetElapsedTime(started);
				if (elapsed >= deadline)
					throw Pending(pending, elapsed);
				var wait = pollInterval < deadline - elapsed ? pollInterval : deadline - elapsed;
				await delay(wait, cancellationToken).ConfigureAwait(false);
			}
			return resolved;
		}

		private PackagesPendingException Pending(
			IReadOnlyList<PackageRequest> pending,
			TimeSpan elapsed)
		{
			var missing = pending
				.Select(request => new PendingPackage(request.Id, request.Version.ToNormalizedString()))
				.ToArray();
			var names = string.Join(", ", pending);
			return new PackagesPendingException(
				$"{pending.Count} package(s) not yet verifiable on NuGet.org after {elapsed.TotalSeconds:F0}s " +
				$"(deadline {deadline.TotalSeconds:F0}s): {names}; rerun once indexing completes",
				missing,
				elapsed,
				deadline);
		}

		private async Task<IReadOnlyDictionary<PackageRequest, VerifiedPackage>> DownloadAllAsync(
			IReadOnlyList<PackageRequest> requests,
			IReadOnlyDictionary<PackageRequest, CatalogPackage> catalogs,
			long started,
			ReleasePolicies policies,
			CancellationToken cancellationToken)
		{
			var resolved = new Dictionary<PackageRequest, VerifiedPackage>();
			var pending = requests.Distinct().ToList();
			while (pending.Count > 0)
			{
				var unresolved = new List<PackageRequest>();
				foreach (var batch in pending.Chunk(4))
				{
					var attempts = await Task.WhenAll(batch.Select(async request =>
					{
						try
						{
							var bytes = await source.DownloadPackageAsync(
								request.Id,
								request.Version,
								cancellationToken).ConfigureAwait(false);
							var package = await PackageVerifier.VerifyAsync(
								request.Id,
								request.Version,
								catalogs[request],
								bytes,
								policies.AnchorPackages.Contains(request.Id),
								signatureVerifier,
								policies,
								cancellationToken).ConfigureAwait(false);
							return new DownloadAttempt(request, package);
						}
						catch (NuGetTransientException)
						{
							return new DownloadAttempt(request, null);
						}
					})).ConfigureAwait(false);
					foreach (var attempt in attempts)
					{
						if (attempt.Package is null)
							unresolved.Add(attempt.Request);
						else
							resolved.Add(attempt.Request, attempt.Package);
					}
				}

				pending = unresolved;
				if (pending.Count == 0)
					break;
				var elapsed = timeProvider.GetElapsedTime(started);
				if (elapsed >= deadline)
					throw Pending(pending, elapsed);
				var wait = pollInterval < deadline - elapsed ? pollInterval : deadline - elapsed;
				await delay(wait, cancellationToken).ConfigureAwait(false);
			}
			return resolved;
		}

		internal static NuGetVersion CollapseDependencyMinimum(
			IReadOnlyList<PackageDependencyGroup> groups,
			string dependencyId)
		{
			var ranges = groups
				.SelectMany(static group => group.Packages)
				.Where(dependency => string.Equals(
					dependency.Id,
					dependencyId,
					StringComparison.OrdinalIgnoreCase))
				.Select(static dependency => dependency.VersionRange)
				.ToArray();
			var minimums = ranges
				.Select(range => range.MinVersion
					?? throw new NuGetReceiptException(
						$"{dependencyId} dependency range '{range}' has no minimum"))
				.Distinct(VersionComparer.VersionRelease)
				.ToArray();
			var minimum = minimums.Length switch
			{
				0 => throw new NuGetReceiptException($"no dependency group references {dependencyId}"),
				1 => NuGetVersion.Parse(minimums[0].ToNormalizedString()),
				_ => throw new NuGetReceiptException(
					$"{dependencyId} minimum version disagrees across target frameworks: {string.Join(", ", minimums.AsEnumerable())}"),
			};
			if (ranges.Any(range => !range.Satisfies(minimum)))
			{
				throw new NuGetReceiptException(
					$"{dependencyId} minimum version {minimum} is excluded by a dependency range");
			}
			return minimum;
		}

		private sealed record CatalogAttempt(
			PackageRequest Request,
			CatalogPackage? Catalog);

		private sealed record DownloadAttempt(
			PackageRequest Request,
			VerifiedPackage? Package);
	}
}
