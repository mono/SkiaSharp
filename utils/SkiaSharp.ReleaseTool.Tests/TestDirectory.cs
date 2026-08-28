namespace SkiaSharp.ReleaseTool.Tests
{
	internal sealed class TestDirectory : IDisposable
	{
		public TestDirectory(string purpose)
		{
			Path = System.IO.Path.Combine(
				AppContext.BaseDirectory,
				"TestRuns",
				$"{purpose}-{Guid.NewGuid():N}");
			Directory.CreateDirectory(Path);
		}

		public string Path { get; }

		public void Dispose()
		{
			try
			{
				Directory.Delete(Path, recursive: true);
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
	}
}
