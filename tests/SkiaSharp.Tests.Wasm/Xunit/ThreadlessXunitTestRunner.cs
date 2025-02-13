using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Xunit.Sdk;

internal partial class ThreadlessXunitTestRunner
{
	[JSImport("xunit.sdk.runner.clearLog", "main.js")]
	internal static partial void ClearLog();

	[JSImport("xunit.sdk.runner.log", "main.js")]
	internal static partial void Log(string? message, string? type = null, string? id = null);

	[JSImport("xunit.sdk.runner.logResults", "main.js")]
	internal static partial void LogResults(string results);

	public bool Run(string assemblyFileName, IEnumerable<string> excludedTraits)
	{
		ClearLog();

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
		Log($"Discovered tests for {assemblyFileName}: Total: {discoverySink.TestCases.Count}, Skip: {discoverySink.TestCases.Count - testCasesToRun.Count}, Run: {testCasesToRun.Count}.");
		Log($"Test discovery finished.");
		Log("");

		var summarySink = new DelegatingExecutionSummarySink(
			testSink,
			() => false,
			(completed, summary) =>
			{
				Log($"""
					Tests run: {summary.Total}, Errors: 0, Failures: {summary.Failed}, Skipped: {summary.Skipped}.
					Total duration: {TimeSpan.FromSeconds((double)summary.Time).TotalSeconds}s
					""");
			});

		var resultsXmlAssembly = new XElement("assembly");
		var resultsSink = new DelegatingXmlCreationSink(summarySink, resultsXmlAssembly);

		testSink.Execution.TestPassedEvent += args => { Log($"[PASS] {args.Message.Test.DisplayName}", type: "pass"); };
		testSink.Execution.TestSkippedEvent += args => { Log($"[SKIP] {args.Message.Test.DisplayName}", type: "skip"); };
		testSink.Execution.TestFailedEvent += args =>
		{
			Log($"""
				[FAIL] {args.Message.Test.DisplayName}
				{ExceptionUtility.CombineMessages(args.Message)}
				{ExceptionUtility.CombineStackTraces(args.Message)}
				""", type: "fail");
		};

		testSink.Execution.TestAssemblyStartingEvent += args => { Log($"Running tests for {args.Message.TestAssembly.Assembly}..."); };
		testSink.Execution.TestAssemblyFinishedEvent += args => { Log($"Finished running tests for {args.Message.TestAssembly.Assembly}."); };

		controller.RunTests(testCasesToRun, resultsSink, testOptions);
		resultsSink.Finished.WaitOne();

		var resultsXml = new XElement("assemblies");
		resultsXml.Add(resultsXmlAssembly);

		Console.WriteLine(resultsXml.ToString());

		Log("");
		Log("Test results (Base64 encoded):");
		var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(resultsXml.ToString()));
		Console.WriteLine(base64);
		LogResults(base64);

		return resultsSink.ExecutionSummary.Failed > 0 || resultsSink.ExecutionSummary.Errors > 0;
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
