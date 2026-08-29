namespace SkiaSharp.ReleaseTool.Planning
{
	public interface IReleaseRepository : Finishing.IFinishRepository
	{
		string Root { get; }
		Task FetchAsync(string remote = "origin", CancellationToken cancellationToken = default);
		Task RequireCleanAsync(
			IReadOnlyList<string>? allowedUntrackedPaths = null,
			CancellationToken cancellationToken = default);
		Task<string> CurrentBranchAsync(CancellationToken cancellationToken = default);
		Task UpdateLocalBranchAsync(
			string branch,
			string sha,
			CancellationToken cancellationToken = default);
		Task SwitchAsync(string branch, CancellationToken cancellationToken = default);
		Task SwitchCreateAsync(
			string branch,
			string startPoint,
			CancellationToken cancellationToken = default);
		Task<string> CommitAsync(
			string message,
			IReadOnlyList<string>? paths = null,
			CancellationToken cancellationToken = default);
		Task PushBranchAsync(
			string branch,
			string remote = "origin",
			bool setUpstream = true,
			CancellationToken cancellationToken = default);
		Task PushTagAsync(
			string tag,
			string sha,
			string remote = "origin",
			CancellationToken cancellationToken = default);
		Task<string> ReadWorktreeFileAsync(
			string path,
			CancellationToken cancellationToken = default);
		Task WriteWorktreeFileAsync(
			string path,
			string content,
			CancellationToken cancellationToken = default);
		Task<IReadOnlyList<string>> CommitSubjectsFirstParentAsync(
			string? exclusiveLowerBound,
			string sourceCommit,
			CancellationToken cancellationToken = default);
	}
}
