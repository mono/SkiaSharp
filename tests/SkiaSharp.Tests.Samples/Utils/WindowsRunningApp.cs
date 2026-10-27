using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using OpenQA.Selenium.Appium;
using SkiaSharp.Tests.Shared;
using Xunit;
using static SkiaSharp.Tests.Samples.Utils.FailureHandling;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class WindowsRunningApp(SampleWorkspace workspace, string artifact, string title)
	: AppiumRunningApp(workspace, artifact, new WindowsAppiumOperations())
{
	internal static string FindApp(string output, string project)
	{
		var executable = Path.Combine(output, Path.GetFileNameWithoutExtension(project) + ".exe");
		if (!File.Exists(executable))
			throw new FileNotFoundException($"Expected the built Windows executable in {output}. Run build mode first.", executable);
		return executable;
	}

	private Process? application;
	private Process? service;
	private Task<string>? serviceOutput;
	private Task<string>? serviceErrors;
	private IntPtr window;

	internal override string DeviceProfile => "windows-desktop";
	internal override GoldenCase Reference(string caseName) =>
		new(DeviceProfile, caseName, Environment.OSVersion.Version,
			RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant());

	internal override Task<string> ReadApplicationId()
	{
		Assert.True(OperatingSystem.IsWindows(), "Windows sample tests require a Windows desktop.");
		Assert.True(File.Exists(Artifact) && Artifact.EndsWith(".exe", StringComparison.OrdinalIgnoreCase),
			"Provide the exact built Windows executable.");
		return Task.FromResult(Path.GetFileName(Artifact));
	}

	protected override async Task Start(Uri address)
	{
		var servicePath = Environment.GetEnvironmentVariable("TEST_WINAPPDRIVER_PATH") ??
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
				"Windows Application Driver", "WinAppDriver.exe");
		Assert.True(File.Exists(servicePath), $"Install WinAppDriver before desktop tests: {servicePath}");

		var port = AppiumFixture.GetFreePort();
		service = ProcessRunner.Start(servicePath, ["127.0.0.1", $"{port}/wd/hub"], Workspace.Root);
		serviceOutput = service.StandardOutput.ReadToEndAsync();
		serviceErrors = service.StandardError.ReadToEndAsync();
		var endpoint = new Uri($"http://127.0.0.1:{port}/wd/hub");
		await WaitForService(service, endpoint);

		application = ProcessRunner.Start(Artifact, [], Path.GetDirectoryName(Artifact)!);
		_ = application.StandardOutput.ReadToEndAsync();
		_ = application.StandardError.ReadToEndAsync();
		window = WindowsWindow.WaitForOwnedWindow(application, title);

		var options = new AppiumOptions { PlatformName = "Windows", AutomationName = "Windows", DeviceName = "WindowsPC" };
		options.AddAdditionalAppiumOption("appTopLevelWindow", $"0x{window.ToInt64():x}");
		options.AddAdditionalAppiumOption("wadUrl", endpoint.AbsoluteUri);
		options.AddAdditionalAppiumOption("newCommandTimeout", 120);
		SetDriver(new DesktopAppiumDriver(address, options));
		Assert.Equal((uint)application.Id, WindowsWindow.ProcessId(window));
		var current = Driver.CurrentWindowHandle;
		Assert.True(current.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
			long.TryParse(current.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var handle) &&
			handle == window.ToInt64(), $"Appium attached to '{current}' instead of owned window '{window}'.");
	}

	internal Rectangle PointerWindow
	{
		get
		{
			Assert.True(window != IntPtr.Zero && application is not null &&
				WindowsWindow.ProcessId(window) == (uint)application.Id, "The Appium window must belong to the launched app.");
			return WindowsWindow.PhysicalBounds(window);
		}
	}

	internal Rectangle ReportedWindow
	{
		get
		{
			var position = Driver.Manage().Window.Position;
			var size = Driver.Manage().Window.Size;
			return new Rectangle(position, size);
		}
	}

	protected override async Task StopPlatform()
	{
		List<Exception> failures = [];
		foreach (var process in new[] { application, service })
		{
			if (process is null)
				continue;
			await Attempt(failures, async () =>
			{
				if (!process.HasExited)
					process.Kill(entireProcessTree: true);
				await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
			});
		}
		if (serviceOutput is not null)
			await Attempt(failures, async () => await SaveText("winappdriver.stdout.log", await serviceOutput.WaitAsync(TimeSpan.FromSeconds(30))));
		if (serviceErrors is not null)
			await Attempt(failures, async () => await SaveText("winappdriver.stderr.log", await serviceErrors.WaitAsync(TimeSpan.FromSeconds(30))));
		foreach (var process in new[] { application, service })
			await Attempt(failures, () =>
			{
				process?.Dispose();
				return Task.CompletedTask;
			});
		ThrowFailures(failures);
	}

	private static async Task WaitForService(Process service, Uri endpoint)
	{
		using var client = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(2) };
		var deadline = DateTime.UtcNow.AddSeconds(30);
		while (DateTime.UtcNow < deadline)
		{
			if (service.HasExited)
				throw new InvalidOperationException($"Owned WinAppDriver exited before readiness: {service.ExitCode}.");
			try
			{
				using var response = await client.GetAsync(new Uri(endpoint.AbsoluteUri.TrimEnd('/') + "/status"));
				if (response.StatusCode == HttpStatusCode.OK && !service.HasExited)
					return;
			}
			catch (Exception error) when (error is HttpRequestException or TaskCanceledException)
			{
				TestContext.Current.TestOutputHelper?.WriteLine($"Owned WinAppDriver readiness: {error.Message}");
			}
			await Task.Delay(250);
		}
		throw new TimeoutException("Owned WinAppDriver did not become ready; inspect its attached logs.");
	}
}

internal static partial class WindowsWindow
{
	private delegate bool EnumWindowsCallback(IntPtr handle, IntPtr state);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool EnumWindows(EnumWindowsCallback callback, IntPtr state);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool IsWindowVisible(IntPtr handle);

	[LibraryImport("user32.dll")]
	private static partial int GetWindowTextLengthW(IntPtr handle);

	[LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
	private static partial int GetWindowTextW(IntPtr handle, [Out] char[] text, int length);

	[LibraryImport("user32.dll")]
	internal static partial uint GetWindowThreadProcessId(IntPtr handle, out uint processId);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetWindowRect(IntPtr handle, out Rect bounds);

	[LibraryImport("user32.dll")]
	private static partial IntPtr SetThreadDpiAwarenessContext(IntPtr context);

	[LibraryImport("user32.dll")]
	internal static partial IntPtr GetThreadDpiAwarenessContext();

	[StructLayout(LayoutKind.Sequential)]
	private struct Rect { public int Left, Top, Right, Bottom; }

	internal static uint ProcessId(IntPtr handle)
	{
		GetWindowThreadProcessId(handle, out var id);
		return id;
	}

	internal static IntPtr WaitForOwnedWindow(Process process, string title)
	{
		var deadline = DateTime.UtcNow.AddSeconds(60);
		while (DateTime.UtcNow < deadline)
		{
			if (process.HasExited)
				throw new InvalidOperationException($"Owned sample application exited before its window appeared: {process.ExitCode}.");
			IntPtr found = IntPtr.Zero;
			if (!EnumWindows((handle, _) =>
			{
				if (!IsWindowVisible(handle) || ProcessId(handle) != (uint)process.Id)
					return true;
				var length = GetWindowTextLengthW(handle);
				if (length <= 0)
					return true;
				var text = new char[length + 1];
				GetWindowTextW(handle, text, text.Length);
				if (!string.Equals(new string(text, 0, Array.IndexOf(text, '\0')), title, StringComparison.Ordinal))
					return true;
				found = handle;
				return false;
			}, IntPtr.Zero) && found == IntPtr.Zero)
				throw new InvalidOperationException("Could not enumerate the owned application window.");
			if (found != IntPtr.Zero)
				return found;
			Thread.Sleep(250);
		}
		throw new TimeoutException($"Owned sample application did not expose window '{title}'.");
	}

	internal static Rectangle PhysicalBounds(IntPtr handle) => WithPhysicalDpi(() =>
	{
		if (!GetWindowRect(handle, out var rect))
			throw new InvalidOperationException("Could not read the owned window's physical screen bounds.");
		return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
	});

	internal static T WithPhysicalDpi<T>(Func<T> action)
	{
		var previous = SetThreadDpiAwarenessContext(new IntPtr(-4)); // Per-monitor aware v2, scoped to this thread.
		if (previous == IntPtr.Zero)
			throw new InvalidOperationException("Cannot enter per-monitor DPI awareness for Windows pointer coordinates.");
		List<Exception> failures = [];
		T result = default!;
		try
		{
			result = action();
		}
		catch (Exception error)
		{
			failures.Add(error);
		}
		finally
		{
			if (SetThreadDpiAwarenessContext(previous) == IntPtr.Zero)
				failures.Add(new InvalidOperationException("Cannot restore the calling thread's DPI awareness."));
		}
		ThrowFailures(failures);
		return result;
	}

	internal static Point ToPhysical(Point reportedPoint, Rectangle reportedWindow, Rectangle physicalWindow)
	{
		if (reportedWindow.Width <= 0 || reportedWindow.Height <= 0 ||
			physicalWindow.Width <= 0 || physicalWindow.Height <= 0)
			throw new ArgumentOutOfRangeException(nameof(reportedWindow), "Window bounds must have positive dimensions.");
		return new(
			physicalWindow.Left + (int)Math.Round((reportedPoint.X - reportedWindow.Left) *
				(double)physicalWindow.Width / reportedWindow.Width, MidpointRounding.AwayFromZero),
			physicalWindow.Top + (int)Math.Round((reportedPoint.Y - reportedWindow.Top) *
				(double)physicalWindow.Height / reportedWindow.Height, MidpointRounding.AwayFromZero));
	}
}
