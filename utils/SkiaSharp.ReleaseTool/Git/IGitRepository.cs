namespace SkiaSharp.ReleaseTool.Git
{
	public interface IGitRepository
	{
		string Root { get; }

		Task FetchAsync(string remote = "origin", CancellationToken cancellationToken = default);
		Task<bool> RefExistsAsync(string reference, CancellationToken cancellationToken = default);
		Task<string> ResolveAsync(string reference, CancellationToken cancellationToken = default);
		Task<string> ReadRefFileAsync(string reference, string path, CancellationToken cancellationToken = default);
		Task<string> ReadGitlinkAsync(string reference, string submodulePath, CancellationToken cancellationToken = default);
		Task<string?> RemoteShaAsync(string branch, string remote = "origin", CancellationToken cancellationToken = default);
		Task<IReadOnlyDictionary<string, string>> RemoteTagsAsync(
			string remote = "origin",
			string pattern = "refs/tags/*",
			CancellationToken cancellationToken = default);
		Task<IReadOnlyList<string>> ReleaseBranchesAsync(string remote = "origin", CancellationToken cancellationToken = default);
		Task<string> MergeBaseAsync(string a, string b, CancellationToken cancellationToken = default);
		Task<bool> IsAncestorAsync(string ancestor, string descendant, CancellationToken cancellationToken = default);
		Task RequireCleanAsync(CancellationToken cancellationToken = default);
		Task<string> CurrentBranchAsync(CancellationToken cancellationToken = default);
		Task CreateBranchAsync(string branch, string startPoint, CancellationToken cancellationToken = default);
		Task SwitchAsync(string branch, CancellationToken cancellationToken = default);
		Task SwitchCreateAsync(string branch, string startPoint, CancellationToken cancellationToken = default);
		Task<string> CommitAsync(
			string message,
			IReadOnlyList<string>? paths = null,
			CancellationToken cancellationToken = default);
		Task PushBranchAsync(
			string branch,
			string remote = "origin",
			bool setUpstream = true,
			CancellationToken cancellationToken = default);
		Task PushTagAsync(string tag, string sha, string remote = "origin", CancellationToken cancellationToken = default);
		Task<bool> ContainsCommitAsync(string branchRef, string commit, CancellationToken cancellationToken = default);
	}
}
