using System.Diagnostics;
using Xunit;

namespace SkiaSharp.Tests.Native;

public class NativeTests
{
	private static string CasesDirectory => Path.Combine(AppContext.BaseDirectory, "Cases");

	private static string NativeDirectory =>
		AppContext.GetData("NativeTest.RepositoryDirectory") as string is { Length: > 0 } root
			? Path.Combine(Path.GetFullPath(root), "output", "native")
			: throw new InvalidOperationException("NativeTest.RepositoryDirectory runtime configuration is missing");

	public static IEnumerable<object[]> TestCases()
	{
		var tests = Directory.EnumerateFiles(CasesDirectory, "*.ps1").Select(path => Path.GetRelativePath(CasesDirectory, path))
			.Concat(Directory.EnumerateDirectories(CasesDirectory).Select(path => Path.Combine(Path.GetFileName(path), "test.ps1")))
			.OrderBy(path => path, StringComparer.Ordinal)
			.ToArray();
		if (tests.Length == 0)
			throw new InvalidOperationException($"No native test cases found in {CasesDirectory}.");

		foreach (var test in tests)
			yield return [test];
	}

	[Theory]
	[MemberData(nameof(TestCases))]
	public async Task Run(string name)
	{
		var script = Path.Combine(CasesDirectory, name);
		var directory = Path.GetDirectoryName(script);
		Assert.NotNull(directory);
		Assert.True(File.Exists(script), $"Missing native test entry point: {script}");

		await RunPwshAsync(script, NativeDirectory, directory);
	}

	private static async Task RunPwshAsync(string script, string nativeDirectory, string workingDirectory)
	{
		var start = new ProcessStartInfo("pwsh")
		{
			WorkingDirectory = workingDirectory,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
		};
		foreach (var argument in new[] { "-NoLogo", "-NoProfile", "-File", script, nativeDirectory })
			start.ArgumentList.Add(argument);

		using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start pwsh.");
		var stdout = process.StandardOutput.ReadToEndAsync();
		var stderr = process.StandardError.ReadToEndAsync();
		using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
		var timedOut = false;
		try
		{
			await process.WaitForExitAsync(timeout.Token);
		}
		catch (OperationCanceledException) when (timeout.IsCancellationRequested)
		{
			timedOut = true;
			if (!process.HasExited)
				process.Kill(entireProcessTree: true);
			await process.WaitForExitAsync();
		}

		var output = $"Script: {script}\nWorking directory: {workingDirectory}\nNative directory: {nativeDirectory}\n" +
			$"stdout:\n{await stdout}\nstderr:\n{await stderr}";
		Assert.False(timedOut, $"Native test exceeded five minutes.\n{output}");
		Assert.True(process.ExitCode == 0, $"Native test exited {process.ExitCode}.\n{output}");
	}
}
