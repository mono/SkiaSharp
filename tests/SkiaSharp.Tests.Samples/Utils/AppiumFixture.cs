using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

public sealed class AppiumFixture
{
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
			await AppiumRunningApp.Attempt(failures, async () =>
			{
				if (!server.HasExited)
					server.Kill(entireProcessTree: true);
				await server.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
			});
			await AppiumRunningApp.Attempt(failures, async () =>
				await SaveLog("server.stdout.log", await stdout!.WaitAsync(TimeSpan.FromSeconds(30))));
			await AppiumRunningApp.Attempt(failures, async () =>
				await SaveLog("server.stderr.log", await stderr!.WaitAsync(TimeSpan.FromSeconds(30))));
			server.Dispose();
			AppiumRunningApp.ThrowFailures(failures);
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
		var deadline = DateTime.UtcNow.AddSeconds(30);
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
		throw new TimeoutException("Owned Appium did not become ready within 30 seconds; inspect attached server logs.");
	}
}
