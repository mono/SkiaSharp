namespace SkiaSharp.ReleaseChecklist.Tests;

public class ProgramTests
{
	[Fact]
	public async Task ExecuteRequiresExplicitReleaseIdentity()
	{
		var output = new StringWriter();
		var error = new StringWriter();

		var exitCode = await Program.InvokeAsync(
			["prepare", "--apply", "--repo", Directory.GetCurrentDirectory()],
			output,
			error);

		Assert.Equal(1, exitCode);
		Assert.Contains("Apply mode requires --release", error.ToString());
		Assert.Equal("", output.ToString());
	}
}
