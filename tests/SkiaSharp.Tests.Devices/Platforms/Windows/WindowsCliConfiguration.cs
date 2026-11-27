using System;
using System.Linq;
using DeviceRunners.VisualRunners;

namespace SkiaSharp.Tests.WinUI
{
	// Replaces AddCliConfiguration on Windows. The DeviceRunners CLI starts its TCP
	// listener only after winapp.exe finishes launching the app, and a single host
	// name makes TcpResultChannel connect exactly once. If the app wins that race it
	// never reports results and the CLI waits forever. Adding a second host name
	// enables the channel's retry path, and the long retry window covers the launch.
	public static class WindowsCliConfiguration
	{
		const int RetryCount = 180;
		static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

		public static TBuilder AddWindowsCliConfiguration<TBuilder>(this TBuilder builder)
			where TBuilder : IVisualTestRunnerConfigurationBuilder
		{
			var autorun = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_AUTORUN");
			var portStr = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_PORT");
			var hostNamesRaw = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_HOST_NAMES");
			var filter = Environment.GetEnvironmentVariable("DEVICE_RUNNERS_FILTER");

			if (string.IsNullOrEmpty(autorun))
			{
				var args = Environment.GetCommandLineArgs();
				for (var i = 0; i < args.Length; i++)
				{
					if (args[i] == "--device-runners-autorun")
						autorun = "1";
					else if (args[i] == "--device-runners-port" && i + 1 < args.Length)
						portStr = args[++i];
					else if (args[i] == "--device-runners-host-names" && i + 1 < args.Length)
						hostNamesRaw = args[++i];
					else if (args[i] == "--device-runners-filter" && i + 1 < args.Length)
						filter = args[++i];
				}
			}

			if (string.IsNullOrEmpty(autorun))
				return builder;

			var port = int.TryParse(portStr, out var p) ? p : 16384;
			var hostNames = (hostNamesRaw ?? "localhost")
				.Split(';', StringSplitOptions.RemoveEmptyEntries)
				.Append("localhost")
				.Append("127.0.0.1")
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToArray();

			StartupDiagnostics.Write(nameof(WindowsCliConfiguration), $"TCP result channel: hosts={string.Join(";", hostNames)} port={port} retries={RetryCount} delay={RetryDelay}");

			builder.EnableAutoStart(autoTerminate: true);
			builder.SetTestCaseFilter(filter);
			builder.AddTcpResultChannel(new TcpResultChannelOptions
			{
				HostNames = hostNames,
				Port = port,
				Formatter = new EventStreamFormatter(),
				Required = false,
				Retries = RetryCount,
				RetryTimeout = RetryDelay,
				Timeout = TimeSpan.FromSeconds(30),
			});

			return builder;
		}
	}
}
