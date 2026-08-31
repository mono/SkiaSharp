using ReleaseChecklist.Git;

namespace SkiaSharp.ReleaseChecklist.Tests;

public class ReleaseDiscoveryTests
{
	[Fact]
	public async Task MainInfersOnlyNextPreview()
	{
		var repository = FakeDiscoveryRepository.Create("main", "4.152.0", "preview.0");
		repository.AddRemote("release/4.152.0-preview.1", repository.SourceSha);

		var found = await DiscoverAsync(repository, "main", null);

		Assert.Equal("4.152.0-preview.2", found.Identity.Raw);
		Assert.Equal(ReleaseSourceKind.Main, found.SourceKind);
		Assert.False(found.MaintenanceExists);
	}

	[Fact]
	public async Task MaintenanceInfersNextPreview()
	{
		var repository = FakeDiscoveryRepository.Create(
			"release/4.152.x", "4.152.0", "preview.0");
		repository.AddRemote("release/4.152.x", repository.SourceSha);
		repository.AddRemote("release/4.152.0-preview.1", repository.SourceSha);

		var found = await DiscoverAsync(
			repository, "release/4.152.x", null);

		Assert.Equal("4.152.0-preview.2", found.Identity.Raw);
		Assert.Equal(ReleaseSourceKind.Maintenance, found.SourceKind);
	}

	[Theory]
	[InlineData("4.152.0-rc.1", false)]
	[InlineData("4.152.0", true)]
	public async Task ExplicitRcAndStableAreSelected(string release, bool stable)
	{
		var repository = FakeDiscoveryRepository.Create("main", "4.152.0", "preview.0");
		repository.AddRemote("release/4.152.x", repository.SourceSha);

		var found = await DiscoverAsync(repository, "main", release);

		Assert.Equal(release, found.Identity.Raw);
		Assert.Equal(stable, found.Identity.IsStable);
		Assert.Equal(stable, found.StableBump.Required);
	}

	[Theory]
	[InlineData("4.152.0-rc.1")]
	[InlineData("4.151.0.1-preview.1")]
	public async Task ExactBranchResumesExactIdentity(string release)
	{
		var numeric = ReleaseIdentity.Parse(release).Numeric;
		var label = ReleaseIdentity.Parse(release).Label;
		var branch = $"release/{release}";
		var repository = FakeDiscoveryRepository.Create(branch, numeric, label);
		repository.AddRemote(branch, repository.SourceSha);
		if (!ReleaseIdentity.Parse(release).IsHotfix)
		{
			repository.AddRemote(
				ReleaseIdentity.Parse(release).MaintenanceBranch,
				new string('2', 40),
				numeric,
				"preview.0");
		}

		var found = await DiscoverAsync(repository, branch, null);

		Assert.Equal(release, found.Identity.Raw);
		Assert.Equal(ReleaseSourceKind.ExactRelease, found.SourceKind);
	}

	[Theory]
	[InlineData("release/4.151.x")]
	[InlineData("release/4.151.0")]
	public async Task ExplicitHotfixStartsFromReleaseBranch(string branch)
	{
		var repository = FakeDiscoveryRepository.Create(branch, "4.151.0", "stable");
		repository.AddRemote(branch, repository.SourceSha);

		var found = await DiscoverAsync(
			repository, branch, "4.151.0.1-preview.1");

		Assert.True(found.Identity.IsHotfix);
		Assert.Equal(repository.SourceSha, found.ReleaseBaseSha);
		Assert.False(found.StableBump.Required);
	}

	[Theory]
	[InlineData("4.152.0-rc.1")]
	[InlineData("4.152.0")]
	public async Task ExactBranchSupportsExplicitChannelTransition(string release)
	{
		const string branch = "release/4.152.0-preview.1";
		var repository = FakeDiscoveryRepository.Create(
			branch, "4.152.0", "preview.1");
		repository.AddRemote(branch, repository.SourceSha);
		repository.AddRemote(
			"release/4.152.x",
			new string('2', 40),
			"4.152.0",
			"preview.0");

		var found = await DiscoverAsync(repository, branch, release);

		Assert.Equal(release, found.Identity.Raw);
		Assert.Equal(repository.SourceSha, found.ReleaseBaseSha);
	}

	[Fact]
	public async Task HotfixIsNeverInferred()
	{
		var repository = FakeDiscoveryRepository.Create(
			"release/4.151.x", "4.151.0.1", "preview.0");
		repository.AddRemote("release/4.151.x", repository.SourceSha);

		await Assert.ThrowsAsync<ReleasePolicyException>(() =>
			DiscoverAsync(repository, "release/4.151.x", null));
	}

	[Fact]
	public async Task FeatureBranchIsNotTreatedAsMain()
	{
		var repository = FakeDiscoveryRepository.Create(
			"feature/release-test", "4.152.0", "preview.0");

		var exception = await Assert.ThrowsAsync<ReleasePolicyException>(() =>
			DiscoverAsync(repository, "feature/release-test", null));

		Assert.Contains("Unsupported release source", exception.Message);
	}

	[Fact]
	public async Task MissingRequestedReleaseBranchDoesNotFallBackToHead()
	{
		var repository = FakeDiscoveryRepository.Create("main", "4.152.0", "preview.0");

		var exception = await Assert.ThrowsAsync<ReleasePolicyException>(() =>
			DiscoverAsync(
				repository,
				"release/4.152.x",
				"4.152.0-preview.1"));

		Assert.Contains("does not exist on origin", exception.Message);
	}

	[Fact]
	public async Task ExactReleaseCanUseSeparateMaintenanceCreationPoint()
	{
		const string branch = "release/4.152.0-rc.1";
		var repository = FakeDiscoveryRepository.Create(branch, "4.152.0", "rc.1");
		repository.AddRemote(
			"maintenance-seed",
			new string('2', 40),
			"4.152.0",
			"preview.0");

		var found = await DiscoverAsync(
			repository,
			branch,
			"4.152.0",
			"refs/remotes/origin/maintenance-seed");

		Assert.False(found.MaintenanceExists);
		Assert.Equal(new string('2', 40), found.MaintenanceExpectedSha);
		Assert.Equal(repository.SourceSha, found.ReleaseBaseSha);
	}

	[Fact]
	public async Task RequestedPreviewCannotUseLaterReleaseCandidateAsBase()
	{
		var repository = FakeDiscoveryRepository.Create("main", "4.152.0", "preview.0");
		repository.AddRemote("release/4.152.0-rc.1", new string('2', 40));

		var exception = await Assert.ThrowsAsync<ReleasePolicyException>(() =>
			DiscoverAsync(
				repository,
				"main",
				"4.152.0-preview.2"));

		Assert.Contains("later prerelease", exception.Message);
	}

	[Fact]
	public async Task LocalOnlyMaintenanceBaseIsRejected()
	{
		const string branch = "release/4.152.0-rc.1";
		var repository = FakeDiscoveryRepository.Create(branch, "4.152.0", "rc.1");
		var localSha = new string('3', 40);
		repository.AddLocalRef(localSha, "4.152.0", "preview.0");

		var exception = await Assert.ThrowsAsync<ReleasePolicyException>(() =>
			DiscoverAsync(
				repository,
				branch,
				"4.152.0",
				localSha));

		Assert.Contains("not contained in a remote branch", exception.Message);
	}

	private static Task<ReleaseDiscoveryResult> DiscoverAsync(
		IReleaseDiscoveryRepository repository,
		string? branch,
		string? release,
		string? maintenanceBase = null) =>
		ReleaseDiscovery.DiscoverAsync(
			repository,
			new ReleaseDiscoveryOptions
			{
				Branch = branch,
				Release = release,
				MaintenanceBase = maintenanceBase,
			});
}

internal sealed class FakeDiscoveryRepository : IReleaseDiscoveryRepository
{
	private readonly Dictionary<string, (string Variables, string Versions)> files =
		new(StringComparer.Ordinal);
	private readonly Dictionary<string, string> refs = new(StringComparer.Ordinal);
	private readonly Dictionary<string, string> remotes = new(StringComparer.Ordinal);

	private FakeDiscoveryRepository(string branch, string sourceSha)
	{
		Branch = branch;
		SourceSha = sourceSha;
	}

	public string Branch { get; }
	public string SourceSha { get; }
	public string RemoteName => "origin";
	public List<string> Branches { get; } = [];
	public string SkiaSha { get; } = new('a', 40);

	public static FakeDiscoveryRepository Create(string branch, string version, string label)
	{
		var repository = new FakeDiscoveryRepository(branch, new string('1', 40));
		var fullRef = $"refs/remotes/origin/{branch}";
		repository.refs[fullRef] = repository.SourceSha;
		repository.refs["HEAD"] = repository.SourceSha;
		repository.files[fullRef] = Files(version, label);
		repository.files["HEAD"] = Files(version, label);
		repository.files[repository.SourceSha] = Files(version, label);
		repository.remotes[branch] = repository.SourceSha;
		repository.Branches.Add(branch);
		return repository;
	}

	public void AddRemote(
		string branch,
		string sha,
		string? version = null,
		string? label = null)
	{
		remotes[branch] = sha;
		var reference = $"refs/remotes/origin/{branch}";
		refs[reference] = sha;
		if (version is not null && label is not null)
		{
			files[reference] = Files(version, label);
			files[sha] = Files(version, label);
		}
		else if (!files.ContainsKey(reference))
		{
			files[reference] = files[$"refs/remotes/origin/{Branch}"];
			files[sha] = files[reference];
		}
		if (!Branches.Contains(branch, StringComparer.Ordinal))
			Branches.Add(branch);
	}

	public void AddLocalRef(string reference, string version, string label)
	{
		refs[reference] = reference;
		files[reference] = Files(version, label);
	}

	public Task<string> CurrentBranchAsync(CancellationToken cancellationToken) =>
		Task.FromResult(Branch);

	public Task<bool> RefExistsAsync(string reference, CancellationToken cancellationToken) =>
		Task.FromResult(refs.ContainsKey(reference));

	public Task<string> ResolveAsync(string reference, CancellationToken cancellationToken) =>
		Task.FromResult(refs[reference]);

	public Task<string?> RemoteBranchShaAsync(
		string branch,
		CancellationToken cancellationToken) =>
		Task.FromResult(remotes.GetValueOrDefault(branch));

	public Task<bool> IsContainedInRemoteBranchAsync(
		string sha,
		CancellationToken cancellationToken) =>
		Task.FromResult(remotes.Values.Contains(sha, StringComparer.Ordinal));

	public Task<IReadOnlyList<string>> ReleaseBranchesAsync(CancellationToken cancellationToken) =>
		Task.FromResult<IReadOnlyList<string>>(Branches);

	public Task<string> ReadRefFileAsync(
		string reference,
		string path,
		CancellationToken cancellationToken)
	{
		var content = path == VersionFiles.VariablesPath
			? files[reference].Variables
			: files[reference].Versions;
		return Task.FromResult(content);
	}

	public Task<string> ReadGitlinkAsync(
		string reference,
		string path,
		CancellationToken cancellationToken) =>
		Task.FromResult(SkiaSha);

	public Task<bool> IsAncestorAsync(
		string ancestor,
		string descendant,
		CancellationToken cancellationToken) =>
		Task.FromResult(ancestor == descendant);

	private static (string Variables, string Versions) Files(string version, string label) =>
		(
			$"""
			variables:
			  SKIASHARP_VERSION: {version}
			  PREVIEW_LABEL: '{label}'

			""",
			$"""
			SkiaSharp file {NormalizeFile(version)}
			HarfBuzzSharp file 8.0.0.1
			# nuget versions
			# SkiaSharp
			SkiaSharp nuget {version}
			SkiaSharp.Views nuget {version}
			# HarfBuzzSharp
			HarfBuzzSharp nuget 8.0.0.1

			""");

	private static string NormalizeFile(string version) =>
		version.Split('.').Length == 3 ? $"{version}.0" : version;
}
