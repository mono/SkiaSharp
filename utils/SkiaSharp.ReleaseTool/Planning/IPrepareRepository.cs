namespace SkiaSharp.ReleaseTool.Planning
{
	public interface IPrepareRepository
	{
		Task<bool> RefExistsAsync(string reference, CancellationToken cancellationToken = default);
		Task<string> ResolveAsync(string reference, CancellationToken cancellationToken = default);
		Task<string> ReadRefFileAsync(
			string reference,
			string path,
			CancellationToken cancellationToken = default);
		Task<string> ReadGitlinkAsync(
			string reference,
			string submodulePath,
			CancellationToken cancellationToken = default);
		Task<string?> RemoteShaAsync(
			string branch,
			string remote = "origin",
			CancellationToken cancellationToken = default);
		Task<IReadOnlyList<string>> ReleaseBranchesAsync(
			string remote = "origin",
			CancellationToken cancellationToken = default);
		Task<bool> IsAncestorAsync(
			string ancestor,
			string descendant,
			CancellationToken cancellationToken = default);
		Task<IReadOnlyList<string>> CommitSubjectsFirstParentAsync(
			string rangeSpec,
			CancellationToken cancellationToken = default);
		Task<string> CommitMessageAsync(string commit, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<string>> ChangedPathsAsync(
			string from,
			string to,
			CancellationToken cancellationToken = default);
	}
}
