using System.Text.Json;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

public sealed class IosSimulatorFixture : IAsyncLifetime
{
	private readonly string diagnostics = Path.Combine(Repo.ArtifactsDir, SampleWorkspace.CreateIdentity("ios-simulator"));
	private string? ownedSimulator;
	private bool ready;
	private bool disposed;

	internal string Version { get; } =
		Environment.GetEnvironmentVariable("SAMPLE_TEST_IOS_VERSION") ?? "26.5";

	internal string Model { get; } =
		Environment.GetEnvironmentVariable("SAMPLE_TEST_IOS_MODEL") ?? "iPhone 16 Pro";

	internal string DeviceProfile => $"{Model.Replace(' ', '-')}/{Version}";

	internal string Udid => ready && !disposed
		? ownedSimulator!
		: throw new InvalidOperationException("The collection's iOS simulator is not ready.");

	public async ValueTask InitializeAsync()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(nameof(IosSimulatorFixture));
		}

		// Keep the existing per-test non-macOS skip rather than failing fixture initialization.
		if (!OperatingSystem.IsMacOS())
		{
			return;
		}

		var created = await Apple.CreateSimulator(
			SampleWorkspace.CreateIdentity("skiasharp-ios"), Model, $"iOS {Version}");

		List<Exception> failures = [];

		if (created.ExitCode == 0)
		{
			await AppiumRunningApp.Attempt(failures, () =>
			{
				using var json = JsonDocument.Parse(created.Output);
				var udid = json.RootElement.GetProperty("udid").GetString();
				Assert.True(Guid.TryParse(udid, out _), $"Apple tool returned an invalid simulator UUID: {udid}");

				// Register ownership before logging or booting: either can fail.
				ownedSimulator = udid;

				return Task.CompletedTask;
			});
		}

		await AppiumRunningApp.Attempt(failures, async () =>
		{
			await CaptureCommand("dotnet apple simulator create", created);
		});

		AppiumRunningApp.ThrowFailures(failures);

		await SaveText("simulator.json",
			JsonSerializer.Serialize(new { simulator = ownedSimulator, model = Model, version = Version }), "application/json");

		await CaptureCommand(
			$"dotnet apple simulator boot {ownedSimulator} --wait",
			await Apple.BootSimulator(ownedSimulator!));

		ready = true;
	}

	public async ValueTask DisposeAsync()
	{
		if (disposed)
		{
			return;
		}

		disposed = true;

		List<Exception> failures = [];

		if (ownedSimulator is not null)
		{
			// A failed boot or shutdown must not prevent deletion of our own device.
			await AppiumRunningApp.Attempt(failures, async () =>
			{
				await CaptureCommand(
					$"dotnet apple simulator shutdown {ownedSimulator}",
					await Apple.ShutdownSimulator(ownedSimulator));
			});

			await AppiumRunningApp.Attempt(failures, async () =>
			{
				await CaptureCommand(
					$"dotnet apple simulator delete {ownedSimulator}",
					await Apple.DeleteSimulator(ownedSimulator));
			});
		}

		AppiumRunningApp.ThrowFailures(failures);
	}

	internal void AttachDiagnostics()
	{
		foreach (var path in Directory.EnumerateFiles(diagnostics))
		{
			TestContext.Current.AddFileAttachment(path,
				Path.GetExtension(path) == ".json" ? "application/json" : "text/plain");
		}
	}

	private async Task<string> CaptureCommand(string command, (int ExitCode, string Output, string Error) result)
	{
		List<Exception> failures = [];

		await AppiumRunningApp.Attempt(failures, () =>
		{
			Assert.True(result.ExitCode == 0, $"Simulator command failed ({result.ExitCode}); inspect {diagnostics}.\n{result.Error}");

			return Task.CompletedTask;
		});

		await AppiumRunningApp.Attempt(failures, () =>
		{
			return SaveText($"{SampleWorkspace.CreateIdentity("command")}.log",
				$"{command}\nExit: {result.ExitCode}\n{result.Output}\n{result.Error}");
		});

		AppiumRunningApp.ThrowFailures(failures);

		return result.Output;
	}

	private async Task SaveText(string name, string text, string contentType = "text/plain")
	{
		Directory.CreateDirectory(diagnostics);

		var path = Path.Combine(diagnostics, name);
		await File.WriteAllTextAsync(path, text);

		if (TestContext.Current.Test is not null)
		{
			TestContext.Current.AddFileAttachment(path, contentType);
		}
		else
		{
			TestContext.Current.SendDiagnosticMessage($"iOS simulator diagnostics: {path}\n{text}");
		}
	}
}
