using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SkiaSharp.Tests.Shared;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

public sealed class AppiumFixture : IAsyncDisposable
{
	private readonly Dictionary<string, AppiumAppSpec> built = new(StringComparer.Ordinal);
	private readonly HashSet<string> workspaces = new(StringComparer.Ordinal);
	private readonly AppiumMode? mode;
	private readonly string packagesDirectory;
	private readonly string appsDirectory;
	private readonly string? samplesDirectory;

	public AppiumFixture() : this(null, Repo.PackagesDir, Repo.SampleAppsDir) { }

	internal AppiumFixture(AppiumMode? mode, string packagesDirectory, string appsDirectory, string? samplesDirectory = null)
	{
		this.mode = mode;
		this.packagesDirectory = packagesDirectory;
		this.appsDirectory = appsDirectory;
		this.samplesDirectory = samplesDirectory;
	}

	internal AppiumMode Mode => mode ?? AppiumPreparedApp.ParseMode(Environment.GetEnvironmentVariable("TEST_APPIUM_MODE"));

	internal async Task<AppiumPreparedApp> Prepare(
		AppiumAppSpec spec, Func<SampleWorkspace, Task> build,
		Func<SampleWorkspace, string, AppiumRunningApp> sessionFactory)
	{
		ArgumentNullException.ThrowIfNull(build);
		ArgumentNullException.ThrowIfNull(sessionFactory);
		var directory = spec.DirectoryFor(appsDirectory);
		if (built.TryGetValue(spec.Key, out var previous) && previous != spec)
			throw new InvalidOperationException($"Conflicting Appium app key: {spec.Key}");
		workspaces.Add(directory);
		if (Mode != AppiumMode.Run && !built.ContainsKey(spec.Key))
		{
			if (Directory.Exists(directory))
				SampleWorkspace.DeleteDirectory(directory);
			var workspace = new SampleWorkspace(spec.Folder, samplesDirectory ?? Repo.SamplesDir,
				packagesDirectory, Repo.WorkspacesDir, Repo.ArtifactsDir, directory);
			await build(workspace);
		}
		var prepared = SampleWorkspace.ForAppiumRun(directory);
		var artifact = spec.FindApp(prepared);
		built[spec.Key] = spec;
		return new(prepared, artifact, sessionFactory);
	}

	internal async Task Run(AppiumPreparedApp app, Func<AppiumRunningApp, Task> scenario)
	{
		if (Mode == AppiumMode.Build)
		{
			TestContext.Current.TestOutputHelper?.WriteLine($"Built {app.Artifact}; build phase does not invoke the scenario.");
			return;
		}
		await Run(app.CreateRunningApp(), scenario);
	}

	public async ValueTask DisposeAsync()
	{
		if (Mode == AppiumMode.Build)
			return;
		List<Exception> failures = [];
		foreach (var directory in workspaces)
			if (Directory.Exists(directory))
				await FailureHandling.Attempt(failures, () =>
				{
					SampleWorkspace.DeleteDirectory(directory);
					return Task.CompletedTask;
				});
		FailureHandling.ThrowFailures(failures);
	}

	internal async Task Run(AppiumRunningApp app, Func<AppiumRunningApp, Task> scenario)
	{
		Process? server = null;
		Task<string>? stdout = null;
		Task<string>? stderr = null;
		var stopped = false;
		var diagnostics = app.Diagnostics;
		var port = GetFreePort();
		var listening = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var address = new Uri($"http://127.0.0.1:{port}");
		await app.Run(address, scenario, StartServer, StopServer);

		async Task StopServer()
		{
			if (stopped || server is null)
				return;
			stopped = true;
			List<Exception> failures = [];
			await FailureHandling.Attempt(failures, async () =>
			{
				if (!server.HasExited)
					server.Kill(entireProcessTree: true);
				await server.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
			});
			await FailureHandling.Attempt(failures, async () =>
				await SaveLog("server.stdout.log", await stdout!.WaitAsync(TimeSpan.FromSeconds(30))));
			await FailureHandling.Attempt(failures, async () =>
				await SaveLog("server.stderr.log", await stderr!.WaitAsync(TimeSpan.FromSeconds(30))));
			server.Dispose();
			FailureHandling.ThrowFailures(failures);
		}

		async Task StartServer()
		{
			Directory.CreateDirectory(diagnostics);
			server = Appium.StartServer(port, app.Workspace.Root);
			stdout = DrainOutput();
			stderr = server.StandardError.ReadToEndAsync();
			await WaitUntilReady(server, address, listening.Task);
		}

		async Task SaveLog(string name, string text)
		{
			var path = Path.Combine(diagnostics, name);
			await File.WriteAllTextAsync(path, text);
			TestContext.Current.AddFileAttachment(path, "text/plain");
		}

		async Task<string> DrainOutput()
		{
			var text = new StringBuilder();
			while (await server!.StandardOutput.ReadLineAsync().ConfigureAwait(false) is { } line)
			{
				text.AppendLine(line);
				if (line.Contains($"Appium REST http interface listener started on http://127.0.0.1:{port}", StringComparison.Ordinal))
					listening.TrySetResult();
			}
			return text.ToString();
		}
	}

	internal static int GetFreePort()
	{
		var listener = new TcpListener(IPAddress.Loopback, 0);
		try
		{
			listener.Start();
			return ((IPEndPoint)listener.LocalEndpoint).Port;
		}
		finally
		{
			listener.Stop();
		}
	}

	private static async Task WaitUntilReady(Process server, Uri address, Task listening)
	{
		using var client = new HttpClient(new HttpClientHandler { UseProxy = false })
		{
			Timeout = TimeSpan.FromSeconds(2)
		};
		var deadline = DateTime.UtcNow.AddSeconds(120);
		while (DateTime.UtcNow < deadline)
		{
			if (server.HasExited)
				throw new InvalidOperationException($"Owned Appium exited before readiness: {server.ExitCode}.");
			try
			{
				using var response = await client.GetAsync(new Uri(address, "/status"));
				if (response.IsSuccessStatusCode && listening.IsCompletedSuccessfully && !server.HasExited)
				{
					using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
					if (json.RootElement.GetProperty("value").GetProperty("ready").GetBoolean())
						return;
				}
			}
			catch (Exception error) when (error is HttpRequestException or TaskCanceledException)
			{
				TestContext.Current.TestOutputHelper?.WriteLine($"Owned Appium readiness: {error.Message}");
			}
			await Task.Delay(250);
		}
		throw new TimeoutException("Owned Appium did not become ready within 120 seconds; inspect attached server logs.");
	}
}
