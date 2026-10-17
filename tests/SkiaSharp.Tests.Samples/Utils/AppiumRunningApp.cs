using System.Diagnostics;
using OpenQA.Selenium.Appium;
using Xunit;
using static SkiaSharp.Tests.Samples.Utils.FailureHandling;

namespace SkiaSharp.Tests.Samples.Utils;

// Owns one app session and its diagnostics; platform setup belongs to the caller.
internal abstract class AppiumRunningApp
{
	private AppiumDriver? driver;
	private string? applicationId;
	private bool used;

	protected AppiumRunningApp(SampleWorkspace workspace, string artifact)
	{
		Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
		ArgumentException.ThrowIfNullOrWhiteSpace(artifact);
		Artifact = Path.GetFullPath(artifact);
		Diagnostics = Path.Combine(workspace.DiagnosticsRoot, "appium");
	}

	internal SampleWorkspace Workspace { get; }

	internal string Artifact { get; }

	internal string ApplicationId => applicationId ?? throw new InvalidOperationException("The built application's identity has not been read.");

	internal string Diagnostics { get; }

	internal abstract string DeviceProfile { get; }

	internal abstract AppiumOperations Operations { get; }

	internal AppiumDriver Driver => driver ?? throw new InvalidOperationException("The owned Appium session is not ready.");

	protected bool HasDriver => driver is not null;

	protected abstract Task Start(Uri address);

	internal abstract Task<string> ReadApplicationId();

	protected virtual Task CapturePlatformDiagnostics() => Task.CompletedTask;

	protected void SetDriver(AppiumDriver value)
	{
		driver = value;
		driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
	}

	internal async Task SaveDiagnostics()
	{
		List<Exception> failures = [];
		if (driver is not null)
		{
			await Attempt(failures, () => SaveText("page-source.xml", driver.PageSource, "application/xml"));
			await Attempt(failures, () =>
			{
				var path = Path.Combine(Diagnostics, "screen.png");
				driver.GetScreenshot().SaveAsFile(path);
				TestContext.Current.AddFileAttachment(path, "image/png");
				return Task.CompletedTask;
			});
		}

		await Attempt(failures, CapturePlatformDiagnostics);
		ThrowFailures(failures);
	}

	internal async Task Run(Uri address, Func<AppiumRunningApp, Task> scenario, Func<Task> startServer, Func<Task> stopServer)
	{
		if (used)
			throw new InvalidOperationException("A running app owns one test lifetime and cannot be reused.");
		used = true;

		List<Exception> failures = [];

		try
		{
			AssertArtifact();
			Directory.CreateDirectory(Diagnostics);
			applicationId = await ReadApplicationId();
			if (string.IsNullOrWhiteSpace(applicationId))
				throw new InvalidOperationException($"Built application identity is invalid: {Artifact}");

			await startServer();
			await Start(address);
			await scenario(this);
		}
		catch (Exception error)
		{
			failures.Add(error);
			await Attempt(failures, () => SaveText("failure.log", error.ToString()));
		}
		finally
		{
			await Attempt(failures, SaveDiagnostics);
			await Attempt(failures, () =>
			{
				driver?.Dispose();
				driver = null;
				return Task.CompletedTask;
			});

			// Failed session constructors can leave driver children alive.
			await Attempt(failures, stopServer);
		}

		ThrowFailures(failures);
	}

	protected async Task<string> Command(
		string executable, string[] arguments, TimeSpan? timeout = null,
		Action<ProcessStartInfo>? configure = null)
	{
		return await CaptureCommand(ProcessRunner.Run(
			executable, arguments, Workspace.Root, timeout ?? TimeSpan.FromSeconds(30), configure));
	}

	protected async Task<string> CaptureCommand(Task<(int ExitCode, string Output, string Error)> command)
	{
		var result = await command;
		List<Exception> failures = [];

		await Attempt(failures, () =>
		{
			Assert.True(result.ExitCode == 0,
				$"Application command failed ({result.ExitCode}); inspect {Diagnostics}.\n{result.Error}");
			return Task.CompletedTask;
		});
		await Attempt(failures, () => SaveText($"{SampleWorkspace.CreateIdentity("command")}.log",
			$"Exit: {result.ExitCode}\n{result.Output}\n{result.Error}"));

		ThrowFailures(failures);
		return result.Output;
	}

	protected async Task SaveText(string name, string text, string contentType = "text/plain")
	{
		Directory.CreateDirectory(Diagnostics);
		var path = Path.Combine(Diagnostics, name);
		await File.WriteAllTextAsync(path, text);
		TestContext.Current.AddFileAttachment(path, contentType);
	}

	private void AssertArtifact()
	{
		var relative = Path.GetRelativePath(Workspace.Root, Artifact);
		Assert.True(!Path.IsPathRooted(relative) && relative != ".." &&
			!relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal),
			$"The exact built artifact must belong to the selected workspace: {Artifact}");
		Assert.True(File.Exists(Artifact) || Directory.Exists(Artifact), $"Built artifact is missing: {Artifact}");
	}
}
