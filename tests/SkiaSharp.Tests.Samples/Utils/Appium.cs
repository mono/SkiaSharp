using System.ComponentModel;
using System.Diagnostics;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class Appium
{
	internal static Process StartServer(int port, string workingDirectory)
	{
		string[] arguments = ["--address", "127.0.0.1", "--port", port.ToString(), "--log-timestamp"];

		try
		{
			return ProcessRunner.Start(
				"appium", arguments,
				workingDirectory);
		}
		catch (Win32Exception error)
		{
			throw new InvalidOperationException(
				"Install Appium 3 and the XCUITest driver before selecting iOS UI tests. " +
				"The test runner does not install prerequisites.", error);
		}
	}
}
