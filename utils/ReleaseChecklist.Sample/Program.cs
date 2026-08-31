using ReleaseChecklist.Core;
using ReleaseChecklist.FileSystem;
using ReleaseChecklist.Git;

var known = new HashSet<string>(StringComparer.Ordinal) { "--apply", "--keep" };
if (args.Any(argument => !known.Contains(argument)))
{
	Console.Error.WriteLine("Usage: dotnet run --project utils/ReleaseChecklist.Sample -- [--apply] [--keep]");
	return 1;
}

var apply = args.Contains("--apply", StringComparer.Ordinal);
var keep = args.Contains("--keep", StringComparer.Ordinal);
var sampleRoot = Path.Combine(
	Path.GetTempPath(),
	$"release-checklist-sample-{Guid.NewGuid():N}");

try
{
	var bare = Path.Combine(sampleRoot, "origin.git");
	var worktree = Path.Combine(sampleRoot, "worktree");
	var receipt = Path.Combine(sampleRoot, "sample-complete.txt");
	var timestamp = Path.Combine(sampleRoot, "last-run.txt");
	Directory.CreateDirectory(sampleRoot);

	var process = new ProcessRunner();
	await process.RunAsync("git", ["init", "--bare", bare], sampleRoot);
	await process.RunAsync("git", ["clone", bare, worktree], sampleRoot);

	var repository = new GitRepository(worktree, repositoryIdentity: "sample/repository");
	await repository.GitAsync(["config", "user.name", "Release Checklist Sample"]);
	await repository.GitAsync(["config", "user.email", "sample@example.invalid"]);
	await repository.WriteWorktreeFileAsync("message.txt", "message=initial\n");
	await repository.GitAsync(["add", "message.txt"]);
	await repository.GitAsync(["commit", "-m", "Create sample repository"]);
	await repository.GitAsync(["branch", "-M", "main"]);
	await repository.GitAsync(["push", "-u", "origin", "main"]);

	var start = await repository.ResolveAsync("HEAD");
	const string branch = "release/sample";
	var definition = new ChecklistBuilder().Sequence(
		"sample",
		"Run the local release checklist sample",
		root =>
		{
			root.GitBranch(new GitBranchOptions
			{
				Id = "create-branch",
				Title = "Create and switch to the sample branch",
				Repository = repository,
				Branch = branch,
				StartPoint = start,
			});

			root.FileContents(new FileContentsOptions
			{
				Id = "update-file",
				Title = "Set message=updated in message.txt",
				Path = Path.Combine(worktree, "message.txt"),
				Transform = current =>
				{
					var lines = (current ?? "").Split(
						'\n',
						StringSplitOptions.RemoveEmptyEntries).ToList();
					var index = lines.FindIndex(line =>
						line.StartsWith("message=", StringComparison.Ordinal));
					if (index >= 0)
						lines[index] = "message=updated";
					else
						lines.Add("message=updated");
					return string.Join('\n', lines) + "\n";
				},
			});

			root.GitCommit(new GitCommitOptions
			{
				Id = "commit-file",
				Title = "Commit the message.txt change",
				Repository = repository,
				Paths = ["message.txt"],
				Message = "Update the sample message",
			});

			root.GitPush(new GitPushOptions
			{
				Id = "push-branch",
				Title = "Push the sample branch",
				Repository = repository,
				Branch = branch,
			});

			root.Step(new StepOptions(
				"record-time",
				"Record the current UTC time")
			{
				Action = ChecklistBuilder.Action(token =>
					new ValueTask(File.WriteAllTextAsync(
						timestamp,
						$"{DateTimeOffset.UtcNow:O}\n",
						token))),
			});

			root.FileContents(new FileContentsOptions
			{
				Id = "sample-receipt",
				Title = "Write a local completion receipt",
				Path = receipt,
				Transform = _ => "sample complete\n",
			});
		});

	var report = await ChecklistRunner.RunAsync(
		definition,
		new ChecklistRunOptions
		{
			Mode = apply ? ChecklistRunMode.Apply : ChecklistRunMode.DryRun,
		});

	Console.WriteLine($"{(apply ? "Apply" : "Dry run")} in {sampleRoot}");
	Console.Write(ConsoleTreeRenderer.Render(report));
	if (keep)
		Console.WriteLine($"Kept sample files at {sampleRoot}");

	return report.Root.HasErrors || report.Root.Status == ChecklistStatus.Blocked ? 2 : 0;
}
finally
{
	if (!keep && Directory.Exists(sampleRoot))
		Directory.Delete(sampleRoot, recursive: true);
}
