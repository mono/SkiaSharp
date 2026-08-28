namespace SkiaSharp.ReleaseTool.Planning
{
	public interface IReleaseRepository : IPrepareRepository
	{
		string Root { get; }
		Task FetchAsync(string remote = "origin", CancellationToken cancellationToken = default);
	}
}
