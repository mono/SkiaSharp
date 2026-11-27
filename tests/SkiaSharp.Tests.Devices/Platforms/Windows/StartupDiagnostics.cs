using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace SkiaSharp.Tests.WinUI
{
	// CI diagnostics for the Windows device-test "no connection" hang: packaged apps have no
	// visible console, so startup timing and DeviceRunners result-channel logs are written
	// beside the registered (loose) app layout, where the test cake collects them.
	public static class StartupDiagnostics
	{
		public const string FileName = "device-runner-diagnostics.log";

		static readonly object sync = new object();

		public static string LogPath { get; } = Path.Combine(AppContext.BaseDirectory, FileName);

		public static void LogProcessStart()
		{
			using var process = Process.GetCurrentProcess();
			Write("startup", $"pid={process.Id} processStartUtc={process.StartTime.ToUniversalTime():O} args=[{string.Join(" ", Environment.GetCommandLineArgs())}]");
		}

		public static void Write(string category, string message)
		{
			var line = $"{DateTime.UtcNow:O} [{category}] {message}{Environment.NewLine}";
			try
			{
				lock (sync)
					File.AppendAllText(LogPath, line);
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				var error = $"Unable to write device runner diagnostics to {LogPath}: {ex}";
				Debug.WriteLine(error);
				Console.Error.WriteLine(error);
			}
		}
	}

	public sealed class StartupDiagnosticsLoggerProvider : ILoggerProvider
	{
		public ILogger CreateLogger(string categoryName) => new Logger(categoryName);

		public void Dispose()
		{
		}

		sealed class Logger : ILogger
		{
			readonly string category;

			public Logger(string category) => this.category = category;

			public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

			public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information && logLevel != LogLevel.None;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
				if (!IsEnabled(logLevel))
					return;

				var message = formatter(state, exception);
				if (exception != null)
					message += Environment.NewLine + exception;
				StartupDiagnostics.Write($"{logLevel}:{category}", message);
			}
		}
	}
}
