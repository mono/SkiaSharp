using NuGet.Versioning;
using ReleaseChecklist.Core;
using ReleaseChecklist.NuGet;

namespace ReleaseChecklist.Tests.NuGet;

public class NuGetPackageCheckTests
{
	[Theory]
	[InlineData(0, ChecklistStatus.NotDone)]
	[InlineData(1, ChecklistStatus.Done)]
	[InlineData(2, ChecklistStatus.Blocked)]
	public async Task ExactPackageDiscoveryDistinguishesStates(
		int matches,
		ChecklistStatus expected)
	{
		var version = NuGetVersion.Parse("1.2.3");
		var versions = Enumerable.Repeat(version, matches).ToArray();
		var check = new FlatContainerPackageExistsCheck(
			(_, _) => Task.FromResult<IReadOnlyList<NuGetVersion>>(versions),
			"Example",
			version);
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.Step(new StepOptions("package", "Package") { Check = check }));

		var report = await ChecklistRunner.RunAsync(definition);

		Assert.Equal(expected, report.Root.Children[0].Status);
	}
}
