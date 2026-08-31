using ReleaseChecklist.Core;
using ReleaseChecklist.FileSystem;

namespace ReleaseChecklist.Tests.FileSystem;

public class FileSystemTests
{
	[Fact]
	public async Task PreviewExecuteAndIdempotentRerun()
	{
		using var directory = TestDirectory.Create();
		var target = Path.Combine(directory.Path, "nested", "state.txt");
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
		{
			root.Step(new StepOptions(
				"directory",
				"Directory")
			{
				Check = FileSystemPrimitives.DirectoryExists(Path.GetDirectoryName(target)!),
				Action = FileSystemPrimitives.EnsureDirectory(Path.GetDirectoryName(target)!),
			});
			root.Step(new StepOptions(
				"file",
				"File")
			{
				Check = FileSystemPrimitives.FileContentCheck(target, "expected\n"),
				Action = FileSystemPrimitives.WriteFile(target, "expected\n"),
			});
		});

		var preview = await ChecklistRunner.RunAsync(definition);
		Assert.False(File.Exists(target));
		Assert.Equal(ChecklistStatus.NotDone, preview.Root.Status);
		var applied = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });
		Assert.True(applied.Successful);
		Assert.Equal("expected\n", await File.ReadAllTextAsync(target));

		var rerun = await ChecklistRunner.RunAsync(definition);
		Assert.True(rerun.Successful);
		Assert.DoesNotContain(
			rerun.Root.Children,
			static child => child.ActionAvailable);
	}

}

internal sealed class TestDirectory : IDisposable
{
	private TestDirectory(string path)
	{
		Path = path;
		Directory.CreateDirectory(path);
	}

	public string Path { get; }

	public static TestDirectory Create([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
		new(System.IO.Path.Combine(
			Directory.GetCurrentDirectory(),
			".test-artifacts",
			name ?? "test",
			Guid.NewGuid().ToString("N")));

	public void Dispose()
	{
		if (Directory.Exists(Path))
			Directory.Delete(Path, recursive: true);
	}
}
