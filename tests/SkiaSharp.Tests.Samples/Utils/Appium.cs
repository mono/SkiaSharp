using System.ComponentModel;
using System.Diagnostics;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class Appium
{
	internal static Process StartServer(
		int port, string workingDirectory, Action<ProcessStartInfo>? configure = null)
	{
		string[] arguments = ["--address", "127.0.0.1", "--port", port.ToString(), "--log-timestamp"];

		try
		{
			return ProcessRunner.Start(
				OperatingSystem.IsWindows() ? "cmd.exe" : "appium",
				OperatingSystem.IsWindows() ? ["/d", "/c", "appium", .. arguments] : arguments,
				workingDirectory, configure);
		}
		catch (Win32Exception error)
		{
			throw new InvalidOperationException(
				"Install Appium and the selected platform driver before selecting UI tests. " +
				"The test runner does not install prerequisites.", error);
		}
	}
}
