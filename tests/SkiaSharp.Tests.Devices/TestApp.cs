using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Testing.Extensions;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Xunit.MicrosoftTestingPlatform;

namespace SkiaSharp.Tests;

public sealed class TestApp : Application
{
	internal static TaskCompletionSource Ready { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

	protected override Window CreateWindow(IActivationState activationState)
	{
		var page = new ContentPage { Content = new Label { Text = "Running SkiaSharp tests..." } };
		var shell = new Shell();
		shell.Items.Add(new ShellContent { Content = page });
		page.Loaded += OnLoaded;
		return new Window(shell);

		void OnLoaded(object sender, EventArgs e)
		{
			page.Loaded -= OnLoaded;
			Ready.TrySetResult();
#if !ANDROID
			_ = Task.Run(async () =>
			{
				try
				{
					var consumer = new ResultConsumer();
					var exitCode = await RunTestsAsync(consumer);
					Environment.Exit(exitCode);
				}
				catch (Exception error)
				{
					Console.Error.WriteLine(error);
					Environment.Exit(1);
				}
			});
#endif
		}
	}

	internal static Task<int> RunTestsAsync(ResultConsumer consumer)
	{
#if ANDROID
		// Android's native entry point does not set a managed entry assembly.
		System.Reflection.Assembly.SetEntryAssembly(typeof(TestApp).Assembly);
#endif
#if IOS || ANDROID
		// Mobile launchers consume the official template's output, not the desktop MTP pipe.
#if ANDROID
		var resultsPath = Path.Combine(Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath, "TestResults");
#else
		var resultsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TestResults");
#endif
		string[] arguments = ["--results-directory", resultsPath, "--report-trx"];
#else
		var arguments = Environment.GetCommandLineArgs().Skip(1).ToArray();
#endif
		return TestPlatformTestFramework.RunAsync(
			arguments,
			(builder, _) =>
			{
				builder.AddTrxReportProvider();
				builder.TestHost.AddDataConsumer(_ => consumer);
			});
	}

	// This is the workload template's result consumer and output contract.
	internal sealed class ResultConsumer : IDataConsumer
	{
		private int passed, failed, skipped;
		public int Passed => passed;
		public int Failed => failed;
		public int Skipped => skipped;
		public string TrxReportPath { get; private set; }
#if ANDROID
		internal Android.App.Instrumentation Instrumentation { get; init; }
#endif
		public string Uid => nameof(ResultConsumer);
		public string DisplayName => nameof(ResultConsumer);
		public string Description => "";
		public string Version => "1.0";
		public Task<bool> IsEnabledAsync() => Task.FromResult(true);
		public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage), typeof(SessionFileArtifact)];

		public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
		{
			if (value is SessionFileArtifact artifact)
			{
				TrxReportPath = artifact.FileInfo.FullName;
				Console.WriteLine($"Results: passed={Passed}, failed={Failed}, skipped={Skipped}");
				Console.WriteLine($"TRX report: {TrxReportPath}");
			}
			else if (value is TestNodeUpdateMessage { TestNode: var node })
			{
				var state = node.Properties.SingleOrDefault<TestNodeStateProperty>();
				string outcome = state switch
				{
					PassedTestNodeStateProperty => "passed",
					FailedTestNodeStateProperty or ErrorTestNodeStateProperty or TimeoutTestNodeStateProperty => "failed",
					SkippedTestNodeStateProperty => "skipped",
					_ => null,
				};
				if (outcome == null)
					return Task.CompletedTask;

				switch (outcome)
				{
					case "passed": Interlocked.Increment(ref passed); break;
					case "failed": Interlocked.Increment(ref failed); break;
					default: Interlocked.Increment(ref skipped); break;
				}
				var id = node.Properties.SingleOrDefault<TestMethodIdentifierProperty>();
				var name = id == null ? node.DisplayName : $"{id.Namespace}.{id.TypeName}.{id.MethodName}";
				Console.WriteLine($"[{outcome.ToUpperInvariant()}] {name}");
#if ANDROID
				var status = new Android.OS.Bundle();
				status.PutString("test", name);
				status.PutString("outcome", outcome);
				Instrumentation.SendStatus(0, status);
#endif
			}
			return Task.CompletedTask;
		}
	}
}
