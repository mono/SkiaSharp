using ReleaseChecklist.Git;

namespace SkiaSharp.ReleaseChecklist;

/// <summary>Adapts <see cref="GitRepository" /> to SkiaSharp release discovery.</summary>
/// <param name="repository">The Git repository.</param>
public sealed class GitReleaseDiscoveryRepository(GitRepository repository)
	: IReleaseDiscoveryRepository
{
	/// <inheritdoc />
	public string RemoteName => repository.Remote;

	/// <inheritdoc />
	public Task<string> CurrentBranchAsync(CancellationToken cancellationToken) =>
		repository.CurrentBranchAsync(cancellationToken);

	/// <inheritdoc />
	public Task<bool> RefExistsAsync(string reference, CancellationToken cancellationToken) =>
		repository.RefExistsAsync(reference, cancellationToken);

	/// <inheritdoc />
	public Task<string> ResolveAsync(string reference, CancellationToken cancellationToken) =>
		repository.ResolveAsync(reference, cancellationToken);

	/// <inheritdoc />
	public Task<string?> RemoteBranchShaAsync(string branch, CancellationToken cancellationToken) =>
		repository.RemoteBranchShaAsync(branch, cancellationToken);

	/// <inheritdoc />
	public Task<bool> IsContainedInRemoteBranchAsync(string sha, CancellationToken cancellationToken) =>
		repository.IsContainedInRemoteBranchAsync(sha, cancellationToken);

	/// <inheritdoc />
	public Task<IReadOnlyList<string>> ReleaseBranchesAsync(CancellationToken cancellationToken) =>
		repository.ReleaseBranchesAsync(cancellationToken);

	/// <inheritdoc />
	public Task<string> ReadRefFileAsync(
		string reference,
		string path,
		CancellationToken cancellationToken) =>
		repository.ReadRefFileAsync(reference, path, cancellationToken);

	/// <inheritdoc />
	public Task<string> ReadGitlinkAsync(
		string reference,
		string path,
		CancellationToken cancellationToken) =>
		repository.ReadGitlinkAsync(reference, path, cancellationToken);

	/// <inheritdoc />
	public Task<bool> IsAncestorAsync(
		string ancestor,
		string descendant,
		CancellationToken cancellationToken) =>
		repository.IsAncestorAsync(ancestor, descendant, cancellationToken);
}
