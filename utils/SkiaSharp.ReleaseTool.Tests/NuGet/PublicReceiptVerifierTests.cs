using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.NuGet;
using SkiaSharp.ReleaseTool.Planning;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.NuGet
{
	public sealed class PublicReceiptVerifierTests
	{
		private const string Commit = "0123456789abcdef0123456789abcdef01234567";
		private const string Head = "89abcdef0123456789abcdef0123456789abcdef";

		[Fact]
		public async Task Verifies_historical_inventory_all_hashes_and_three_anchor_signatures()
		{
			var source = ReceiptPackageSource.CreateStable();
			var signatures = new RecordingSignatureVerifier();
			var repository = new ReceiptRepository(Commit, Head);
			var verifier = new PublicReceiptVerifier(source, signatures);

			var receipt = await verifier.VerifyAsync(
				repository,
				PublicReleaseVersion.Parse("4.152.0"),
				PackageTestData.Policies(),
				CancellationToken.None);

			Assert.Equal(Commit, receipt.SourceCommit);
			Assert.Equal("release/4.152.0", receipt.SourceBranch);
			Assert.Equal("14.2.1.200", receipt.HarfBuzzSharpVersion.ToNormalizedString());
			Assert.Equal(4, receipt.Packages.Count);
			Assert.Equal(
				["HarfBuzzSharp", "SkiaSharp", "SkiaSharp.HarfBuzz"],
				signatures.VerifiedIds.Order(StringComparer.Ordinal).ToArray());
			Assert.Single(receipt.Warnings);
			Assert.Contains("advanced", receipt.Warnings[0]);
		}

		[Theory]
		[InlineData("SkiaSharp")]
		[InlineData("SkiaSharp.HarfBuzz")]
		[InlineData("HarfBuzzSharp")]
		public async Task Current_semantic_anchors_must_be_configured(string missing)
		{
			var policies = PackageTestData.Policies();
			policies = policies with
			{
				AnchorPackages = policies.AnchorPackages
					.Where(value => value != missing)
					.ToHashSet(StringComparer.Ordinal),
			};
			var verifier = new PublicReceiptVerifier(
				ReceiptPackageSource.CreateStable(),
				new RecordingSignatureVerifier());

			var error = await Assert.ThrowsAsync<NuGetReceiptException>(() =>
				verifier.VerifyAsync(
					new ReceiptRepository(Commit, Commit),
					PublicReleaseVersion.Parse("4.152.0"),
					policies,
					CancellationToken.None));

			Assert.Contains(missing, error.Message);
		}

		[Fact]
		public async Task Uses_one_monotonic_deadline_and_reports_all_pending_packages()
		{
			var source = new ReceiptPackageSource();
			var clock = new ManualTimeProvider();
			var verifier = new PublicReceiptVerifier(
				source,
				new RecordingSignatureVerifier(),
				clock,
				(duration, _) =>
				{
					clock.Advance(duration);
					return Task.CompletedTask;
				},
				TimeSpan.FromMinutes(1),
				TimeSpan.FromSeconds(30));

			var error = await Assert.ThrowsAsync<PackagesPendingException>(() =>
				verifier.VerifyAsync(
					new ReceiptRepository(Commit, Commit),
					PublicReleaseVersion.Parse("4.152.0"),
					PackageTestData.Policies(),
					CancellationToken.None));

			Assert.Equal(2, error.MissingPackages.Count);
			Assert.Equal(TimeSpan.FromMinutes(1), error.Deadline);
			Assert.Equal(TimeSpan.FromMinutes(1), error.Elapsed);
			Assert.Equal(6, source.MetadataRequests.Count);
		}

		[Fact]
		public async Task Invalid_visible_metadata_hard_fails_without_polling()
		{
			var source = ReceiptPackageSource.CreateStable();
			var request = new PackageRequest("SkiaSharp", NuGetVersion.Parse("4.152.0"));
			source.Catalog[request] = source.Catalog[request] with { PackageSize = 0 };
			var delays = 0;
			var verifier = new PublicReceiptVerifier(
				source,
				new RecordingSignatureVerifier(),
				delay: (_, _) =>
				{
					delays++;
					return Task.CompletedTask;
				});

			await Assert.ThrowsAsync<NuGetReceiptException>(() => verifier.VerifyAsync(
				new ReceiptRepository(Commit, Commit),
				PublicReleaseVersion.Parse("4.152.0"),
				PackageTestData.Policies(),
				CancellationToken.None));
			Assert.Equal(0, delays);
		}

		[Fact]
		public async Task Unlisted_visible_packages_are_pending_not_invalid()
		{
			var source = ReceiptPackageSource.CreateStable();
			var request = new PackageRequest("SkiaSharp", NuGetVersion.Parse("4.152.0"));
			source.Catalog[request] = source.Catalog[request] with { Listed = false };
			var verifier = new PublicReceiptVerifier(
				source,
				new RecordingSignatureVerifier(),
				deadline: TimeSpan.Zero);

			var error = await Assert.ThrowsAsync<PackagesPendingException>(() =>
				verifier.VerifyAsync(
					new ReceiptRepository(Commit, Commit),
					PublicReleaseVersion.Parse("4.152.0"),
					PackageTestData.Policies(),
					CancellationToken.None));

			Assert.Contains(error.MissingPackages, package => package.Id == "SkiaSharp");
		}

		[Fact]
		public async Task Transient_metadata_and_download_failures_retry_under_the_shared_deadline()
		{
			var source = ReceiptPackageSource.CreateStable();
			var skia = new PackageRequest("SkiaSharp", NuGetVersion.Parse("4.152.0"));
			source.TransientMetadataFailures[skia] = 1;
			source.TransientDownloadFailures[skia] = 1;
			var clock = new ManualTimeProvider();
			var verifier = new PublicReceiptVerifier(
				source,
				new RecordingSignatureVerifier(),
				clock,
				(duration, _) =>
				{
					clock.Advance(duration);
					return Task.CompletedTask;
				},
				TimeSpan.FromMinutes(2),
				TimeSpan.FromSeconds(30));

			var receipt = await verifier.VerifyAsync(
				new ReceiptRepository(Commit, Commit),
				PublicReleaseVersion.Parse("4.152.0"),
				PackageTestData.Policies(),
				CancellationToken.None);

			Assert.Equal(Commit, receipt.SourceCommit);
			Assert.Equal(2, source.MetadataRequests.Count(request => request == skia));
			Assert.Equal(2, source.DownloadRequests.Count(request => request == skia));
			Assert.Equal(TimeSpan.FromMinutes(1), clock.GetElapsedTime(0));
		}

		[Fact]
		public async Task Mixed_SkiaSharp_commits_are_rejected()
		{
			var source = ReceiptPackageSource.CreateStable(
				wrapperCommit: "1111111111111111111111111111111111111111");
			var verifier = new PublicReceiptVerifier(source, new RecordingSignatureVerifier());

			await Assert.ThrowsAsync<NuGetReceiptException>(() => verifier.VerifyAsync(
				new ReceiptRepository(Commit, Commit),
				PublicReleaseVersion.Parse("4.152.0"),
				PackageTestData.Policies(),
				CancellationToken.None));
		}

		[Fact]
		public async Task Mixed_HarfBuzzSharp_family_commits_are_rejected()
		{
			var source = ReceiptPackageSource.CreateStable(
				harfBuzzFamilyCommit: "1111111111111111111111111111111111111111");
			var verifier = new PublicReceiptVerifier(source, new RecordingSignatureVerifier());

			await Assert.ThrowsAsync<NuGetReceiptException>(() => verifier.VerifyAsync(
				new ReceiptRepository(Commit, Commit),
				PublicReleaseVersion.Parse("4.152.0"),
				PackageTestData.Policies(),
				CancellationToken.None));
		}

		[Fact]
		public async Task Missing_branch_containment_is_a_conflict()
		{
			var repository = new ReceiptRepository(Commit, Commit)
			{
				ContainsSource = false,
			};
			var verifier = new PublicReceiptVerifier(
				ReceiptPackageSource.CreateStable(),
				new RecordingSignatureVerifier());

			await Assert.ThrowsAsync<ConflictException>(() => verifier.VerifyAsync(
				repository,
				PublicReleaseVersion.Parse("4.152.0"),
				PackageTestData.Policies(),
				CancellationToken.None));
		}

		[Fact]
		public async Task Missing_package_source_commit_is_a_typed_receipt_failure()
		{
			var verifier = new PublicReceiptVerifier(
				ReceiptPackageSource.CreateStable(),
				new RecordingSignatureVerifier());

			var error = await Assert.ThrowsAsync<NuGetReceiptException>(() => verifier.VerifyAsync(
				new ReceiptRepository(new string('f', 40), Commit),
				PublicReleaseVersion.Parse("4.152.0"),
				PackageTestData.Policies(),
				CancellationToken.None));

			Assert.Contains("does not exist", error.Message);
		}

		[Fact]
		public void Historical_inventory_ignores_nonpublic_and_symbol_rows()
		{
			var text =
				"# nuget versions\n" +
				"# SkiaSharp\n" +
				"SkiaSharp nuget 4.152.0\n" +
				"SkiaSharp.Symbols nuget 4.152.0 symbols\n" +
				"SkiaSharp.Private nonpublic 4.152.0\n" +
				"# HarfBuzzSharp\n" +
				"HarfBuzzSharp nuget 14.2.1.200\n";

			var families = VersionsTxt.ParsePackageFamilies(text);

			Assert.Equal(["SkiaSharp"], families["SkiaSharp"]);
			Assert.Equal(["HarfBuzzSharp"], families["HarfBuzzSharp"]);
		}

		private sealed class ReceiptPackageSource : IPublicPackageSource
		{
			public Dictionary<PackageRequest, CatalogPackage> Catalog { get; } = [];
			public Dictionary<PackageRequest, byte[]> Packages { get; } = [];
			public List<PackageRequest> MetadataRequests { get; } = [];
			public List<PackageRequest> DownloadRequests { get; } = [];
			public Dictionary<PackageRequest, int> TransientMetadataFailures { get; } = [];
			public Dictionary<PackageRequest, int> TransientDownloadFailures { get; } = [];

			public static ReceiptPackageSource CreateStable(
				string? wrapperCommit = null,
				string? harfBuzzFamilyCommit = null)
			{
				var source = new ReceiptPackageSource();
				source.Add(
					"SkiaSharp",
					"4.152.0",
					PackageTestData.Create(
						"SkiaSharp",
						"4.152.0",
						Commit,
						"release/4.152.0"));
				source.Add(
					"SkiaSharp.HarfBuzz",
					"4.152.0",
					PackageTestData.Create(
						"SkiaSharp.HarfBuzz",
						"4.152.0",
						wrapperCommit ?? Commit,
						"release/4.152.0",
						("net8.0", "HarfBuzzSharp", "[14.2.1.200, )"),
						("net9.0", "HarfBuzzSharp", "14.2.1.200")));
				source.Add(
					"HarfBuzzSharp",
					"14.2.1.200",
					PackageTestData.Create(
						"HarfBuzzSharp",
						"14.2.1.200",
						"fedcba9876543210fedcba9876543210fedcba98",
						"release/4.151.1"));
				source.Add(
					"HarfBuzzSharp.NativeAssets.Test",
					"14.2.1.200",
					PackageTestData.Create(
						"HarfBuzzSharp.NativeAssets.Test",
						"14.2.1.200",
						harfBuzzFamilyCommit ?? "fedcba9876543210fedcba9876543210fedcba98",
						"release/4.151.1"));
				return source;
			}

			public Task<CatalogPackage?> GetCatalogPackageAsync(
				string id,
				NuGetVersion version,
				CancellationToken cancellationToken)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var request = new PackageRequest(id, version);
				MetadataRequests.Add(request);
				if (TransientMetadataFailures.GetValueOrDefault(request) > 0)
				{
					TransientMetadataFailures[request]--;
					throw new NuGetTransientException(
						"temporary metadata failure",
						new HttpRequestException("temporary metadata failure"));
				}
				return Task.FromResult(Catalog.GetValueOrDefault(request));
			}

			public Task<byte[]> DownloadPackageAsync(
				string id,
				NuGetVersion version,
				CancellationToken cancellationToken)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var request = new PackageRequest(id, version);
				DownloadRequests.Add(request);
				if (TransientDownloadFailures.GetValueOrDefault(request) > 0)
				{
					TransientDownloadFailures[request]--;
					throw new NuGetTransientException(
						"temporary download failure",
						new HttpRequestException("temporary download failure"));
				}
				return Task.FromResult(Packages[request]);
			}

			private void Add(string id, string version, byte[] bytes)
			{
				var request = new PackageRequest(id, NuGetVersion.Parse(version));
				Packages[request] = bytes;
				Catalog[request] = PackageTestData.Catalog(id, version, bytes);
			}
		}

		private sealed class ReceiptRepository(string commit, string head) : IFinishRepository
		{
			public bool ContainsSource { get; set; } = true;

			public Task<bool> CommitExistsAsync(
				string value,
				CancellationToken cancellationToken = default) =>
				Task.FromResult(value == commit);

			public Task<bool> RefExistsAsync(
				string reference,
				CancellationToken cancellationToken = default) =>
				Task.FromResult(reference == "refs/remotes/origin/release/4.152.0");

			public Task<bool> IsAncestorAsync(
				string ancestor,
				string descendant,
				CancellationToken cancellationToken = default) =>
				Task.FromResult(ContainsSource && ancestor == commit);

			public Task<string?> RemoteShaAsync(
				string branch,
				string remote = "origin",
				CancellationToken cancellationToken = default) =>
				Task.FromResult<string?>(head);

			public Task<string> ReadRefFileAsync(
				string reference,
				string path,
				CancellationToken cancellationToken = default) =>
				Task.FromResult(path switch
				{
					"scripts/azure-templates-variables.yml" =>
						"SKIASHARP_VERSION: 4.152.0\nPREVIEW_LABEL: 'stable'\n",
					"scripts/VERSIONS.txt" =>
						"SkiaSharp file 4.152.0.0\n" +
						"HarfBuzzSharp file 14.2.1.200\n" +
						"# nuget versions\n" +
						"# SkiaSharp\n" +
						"SkiaSharp nuget 4.152.0\n" +
						"SkiaSharp.HarfBuzz nuget 4.152.0\n" +
						"# HarfBuzzSharp\n" +
						"HarfBuzzSharp nuget 14.2.1.200\n" +
						"HarfBuzzSharp.NativeAssets.Test nuget 14.2.1.200\n",
					_ => throw new InvalidOperationException(path),
				});

			public Task<IReadOnlyDictionary<string, string>> RemoteTagsAsync(
				string remote = "origin",
				string pattern = "refs/tags/*",
				CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();

			public Task<string> ResolveAsync(string reference, CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();

			public Task<string> ReadGitlinkAsync(
				string reference,
				string submodulePath,
				CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();

			public Task<IReadOnlyList<string>> ReleaseBranchesAsync(
				string remote = "origin",
				CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();
		}

		private sealed class ManualTimeProvider : TimeProvider
		{
			private long timestamp;
			public override long TimestampFrequency => TimeSpan.TicksPerSecond;
			public override long GetTimestamp() => timestamp;
			public void Advance(TimeSpan duration) => timestamp += duration.Ticks;
		}
	}
}
