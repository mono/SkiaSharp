using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.UI.Xaml;

namespace SkiaSharp.Tests.WinUI;

internal static class GpuCrashDiagnostics
{
	public static void Attach(Application application)
	{
		var directory = Environment.GetEnvironmentVariable("SKIASHARP_WINDOWS_GPU_DIAGNOSTICS");
		if (string.IsNullOrWhiteSpace(directory))
			return;

		var path = Path.Combine(directory, $"host-{Environment.ProcessId}.log");
		var sync = new object();
		void Write(string message)
		{
			var text = $"{DateTimeOffset.UtcNow:O} pid={Environment.ProcessId} thread={Environment.CurrentManagedThreadId} {message}";
			Console.Error.WriteLine(text);
			try
			{
				lock (sync)
				{
					Directory.CreateDirectory(directory);
					File.AppendAllText(path, text + Environment.NewLine);
				}
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				Console.Error.WriteLine($"Windows GPU host diagnostic write failed: {ex.Message}");
			}
		}

		Write($"Started {Environment.ProcessPath}");
		application.UnhandledException += (_, e) =>
			Write($"WinUI unhandled exception (Handled={e.Handled}): {e.Message}{Environment.NewLine}{e.Exception}");
		AppDomain.CurrentDomain.UnhandledException += (_, e) =>
			Write($"CLR unhandled exception (IsTerminating={e.IsTerminating}): {e.ExceptionObject}");
		AppDomain.CurrentDomain.ProcessExit += (_, _) => Write($"ProcessExit code={Environment.ExitCode}");
		if (Environment.GetEnvironmentVariable("SKIASHARP_WINDOWS_GPU_NATIVE_DUMP") == "1")
		{
			var ready = Path.Combine(directory, $"native-ready-{Environment.ProcessId}");
			var deadline = DateTime.UtcNow.AddSeconds(20);
			while (DateTime.UtcNow < deadline)
			{
				if (File.Exists(ready) &&
					File.ReadAllText(ready) == Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture) &&
					IsDebuggerPresent())
				{
					Write("PID-scoped native dump collector attached before test execution.");
					return;
				}
				Thread.Sleep(25);
			}
			throw new InvalidOperationException("The required PID-scoped native dump collector did not attach. See native-monitor-error.txt and ProcDump logs; no tests were started.");
		}
	}

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IsDebuggerPresent();
}
