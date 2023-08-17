using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SkiaSharp.Tests
{
	internal class ThreadlessXunitTestRunner
	{
		public bool Run(string assemblyFileName, IEnumerable<string> excludedTraits)
		{
			WebAssembly.Window.Eval($"if (document) document.body.innerHTML = ''");

			Log("Starting tests...");

			var filters = new XunitFilters();
			foreach (var trait in excludedTraits ?? Array.Empty<string>())
			{
				ParseEqualSeparatedArgument(filters.ExcludedTraits, trait);
			}

			var configuration = new TestAssemblyConfiguration
			{
				ShadowCopy = false,
				ParallelizeAssembly = false,
				ParallelizeTestCollections = false,
				MaxParallelThreads = 1,
				PreEnumerateTheories = false
			};
			var discoveryOptions = TestFrameworkOptions.ForDiscovery(configuration);
			var discoverySink = new TestDiscoverySink();
			var diagnosticSink = new ConsoleDiagnosticMessageSink();
			var testOptions = TestFrameworkOptions.ForExecution(configuration);
			var testSink = new TestMessageSink();
			var controller = new Xunit2(
				AppDomainSupport.Denied,
				new NullSourceInformationProvider(),
				assemblyFileName,
				configFileName: null,
				shadowCopy: false,
				shadowCopyFolder: null,
				diagnosticMessageSink: diagnosticSink,
				verifyTestAssemblyExists: false);

			discoveryOptions.SetSynchronousMessageReporting(true);
			testOptions.SetSynchronousMessageReporting(true);

			Log($"Discovering tests for {assemblyFileName}...");
			var assembly = Assembly.LoadFrom(assemblyFileName);
			var assemblyInfo = new Xunit.Sdk.ReflectionAssemblyInfo(assembly);
			var discoverer = new ThreadlessXunitDiscoverer(assemblyInfo, new NullSourceInformationProvider(), discoverySink);
			discoverer.FindWithoutThreads(includeSourceInformation: false, discoverySink, discoveryOptions);
			discoverySink.Finished.WaitOne();
			var testCasesToRun = discoverySink.TestCases.Where(filters.Filter).ToList();
			Log($"Discovery finished.");
			Log("");

			var summarySink = new DelegatingExecutionSummarySink(
				testSink,
				() => false,
				(completed, summary) => { Log($"Tests run: {summary.Total}, Errors: 0, Failures: {summary.Failed}, Skipped: {summary.Skipped}. Time: {TimeSpan.FromSeconds((double)summary.Time).TotalSeconds}s"); });

			var resultsXmlAssembly = new XElement("assembly");
			var resultsSink = new DelegatingXmlCreationSink(summarySink, resultsXmlAssembly);

			testSink.Execution.TestPassedEvent += args => { Log($"[PASS] {args.Message.Test.DisplayName}", color: "green"); };
			testSink.Execution.TestSkippedEvent += args => { Log($"[SKIP] {args.Message.Test.DisplayName}", color: "orange"); };
			testSink.Execution.TestFailedEvent += args => { Log($"[FAIL] {args.Message.Test.DisplayName}{Environment.NewLine}{ExceptionUtility.CombineMessages(args.Message)}{Environment.NewLine}{ExceptionUtility.CombineStackTraces(args.Message)}", color: "red"); };

			testSink.Execution.TestAssemblyStartingEvent += args => { Log($"Running tests for {args.Message.TestAssembly.Assembly}"); };
			testSink.Execution.TestAssemblyFinishedEvent += args => { Log($"Finished {args.Message.TestAssembly.Assembly}{Environment.NewLine}"); };

			controller.RunTests(testCasesToRun, resultsSink, testOptions);
			resultsSink.Finished.WaitOne();

			var resultsXml = new XElement("assemblies");
			resultsXml.Add(resultsXmlAssembly);

			Console.WriteLine(resultsXml.ToString());

			Log("");
			Log("Test results (Base64 encoded):");
			var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(resultsXml.ToString()));
			Log(base64, id: "results");

			return resultsSink.ExecutionSummary.Failed > 0 || resultsSink.ExecutionSummary.Errors > 0;
		}

		private void Log(string contents, string color = null, string id = null)
		{
			Console.WriteLine(contents);

			if (string.IsNullOrEmpty(contents))
				contents = "&nbsp;";

			var ele = "";
			if (!string.IsNullOrEmpty(id))
				ele += $"id=\"{id}\"";

			var style = "white-space: pre-wrap; word-break: break-all;";
			if (!string.IsNullOrEmpty(color))
				style += $"color: {color};";

			WebAssembly.Window.Eval($"if (document) document.body.innerHTML += '<pre {ele} style=\"{style}\">{contents.Replace("\n", "<br/>")}</pre>'");
		}

		private void ParseEqualSeparatedArgument(Dictionary<string, List<string>> targetDictionary, string argument)
		{
			var parts = argument.Split('=');
			if (parts.Length != 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
				throw new ArgumentException("Invalid argument value '{argument}'.", nameof(argument));

			var name = parts[0];
			var value = parts[1];

			List<string> excludedTraits;
			if (targetDictionary.TryGetValue(name, out excludedTraits!))
				excludedTraits.Add(value);
			else
				targetDictionary[name] = new List<string> { value };
		}
	}

	internal class ThreadlessXunitDiscoverer : Xunit.Sdk.XunitTestFrameworkDiscoverer
	{
		public ThreadlessXunitDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink)
			: base(assemblyInfo, sourceProvider, diagnosticMessageSink)
		{
		}

		public void FindWithoutThreads(bool includeSourceInformation, IMessageSink discoveryMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			using var messageBus = new Xunit.Sdk.SynchronousMessageBus(discoveryMessageSink);

			foreach (var type in AssemblyInfo.GetTypes(includePrivateTypes: false).Where(IsValidTestClass))
			{
				var testClass = CreateTestClass(type);
				if (!FindTestsForType(testClass, includeSourceInformation, messageBus, discoveryOptions))
					break;
			}

			messageBus.QueueMessage(new Xunit.Sdk.DiscoveryCompleteMessage());
		}
	}

	internal class ConsoleDiagnosticMessageSink : Xunit.Sdk.LongLivedMarshalByRefObject, IMessageSink
	{
		public bool OnMessage(IMessageSinkMessage message)
		{
			if (message is IDiagnosticMessage diagnosticMessage)
				Console.WriteLine(diagnosticMessage.Message);

			return true;
		}
	}
}
