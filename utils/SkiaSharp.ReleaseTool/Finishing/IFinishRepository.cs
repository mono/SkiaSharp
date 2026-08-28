using SkiaSharp.ReleaseTool.Planning;

namespace SkiaSharp.ReleaseTool.Finishing
{
	public interface IFinishRepository : IPrepareRepository
	{
		Task<bool> CommitExistsAsync(string commit, CancellationToken cancellationToken = default);

		Task<IReadOnlyDictionary<string, string>> RemoteTagsAsync(
			string remote = "origin",
			string pattern = "refs/tags/*",
			CancellationToken cancellationToken = default);
	}
}
