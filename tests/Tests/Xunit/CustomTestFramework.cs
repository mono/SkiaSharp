using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SkiaSharp.Tests
{
	public class CustomTestFramework : XunitTestFramework
	{
		public CustomTestFramework(IMessageSink messageSink)
			: base(messageSink)
		{
		}

		protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) =>
			new Executor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);

		public class Executor : XunitTestFrameworkExecutor
		{
			public Executor(
				AssemblyName assemblyName,
				ISourceInformationProvider sourceInformationProvider,
				IMessageSink diagnosticMessageSink)
				: base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
			{
			}

			protected override async void RunTestCases(
				IEnumerable<IXunitTestCase> testCases,
				IMessageSink executionMessageSink,
				ITestFrameworkExecutionOptions executionOptions)
			{
				using var assemblyRunner = new AssemblyRunner(
					TestAssembly,
					testCases,
					DiagnosticMessageSink,
					executionMessageSink,
					executionOptions);
				await assemblyRunner.RunAsync();
			}
		}

		public class AssemblyRunner : XunitTestAssemblyRunner
		{
			private readonly Dictionary<Type, object> assemblyFixtureMappings = new Dictionary<Type, object>();

			public AssemblyRunner(
				ITestAssembly testAssembly,
				IEnumerable<IXunitTestCase> testCases,
				IMessageSink diagnosticMessageSink,
				IMessageSink executionMessageSink,
				ITestFrameworkExecutionOptions executionOptions)
				: base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
			{
			}

			protected override async Task AfterTestAssemblyStartingAsync()
			{
				// Let everything initialize
				await base.AfterTestAssemblyStartingAsync();

				// Go find all the AssemblyFixtureAttributes adorned on the test assembly
				Aggregator.Run(() =>
				{
					var fixturesAttrs = ((IReflectionAssemblyInfo)TestAssembly.Assembly)
						.Assembly
						.GetCustomAttributes(typeof(AssemblyFixtureAttribute), false)
						.Cast<AssemblyFixtureAttribute>()
						.ToList();

					// Instantiate all the fixtures
					foreach (var fixtureAttr in fixturesAttrs)
						assemblyFixtureMappings[fixtureAttr.FixtureType] = Activator.CreateInstance(fixtureAttr.FixtureType);
				});
			}

			protected override Task BeforeTestAssemblyFinishedAsync()
			{
				// Make sure we clean up everybody who is disposable, and use Aggregator.Run to isolate Dispose failures
				foreach (var disposable in assemblyFixtureMappings.Values.OfType<IDisposable>())
					Aggregator.Run(disposable.Dispose);

				return base.BeforeTestAssemblyFinishedAsync();
			}

			protected override Task<RunSummary> RunTestCollectionAsync(
				IMessageBus messageBus,
				ITestCollection testCollection,
				IEnumerable<IXunitTestCase> testCases,
				CancellationTokenSource cancellationTokenSource)
			{
				var fixture = new CollectionRunner(
					assemblyFixtureMappings,
					testCollection,
					testCases,
					DiagnosticMessageSink,
					messageBus,
					TestCaseOrderer,
					new ExceptionAggregator(Aggregator),
					cancellationTokenSource);
				return fixture.RunAsync();
			}
		}

		public class CollectionRunner : XunitTestCollectionRunner
		{
			private readonly Dictionary<Type, object> assemblyFixtureMappings;
			private readonly IMessageSink diagnosticMessageSink;

			public CollectionRunner(
				Dictionary<Type, object> assemblyFixtureMappings,
				ITestCollection testCollection,
				IEnumerable<IXunitTestCase> testCases,
				IMessageSink diagnosticMessageSink,
				IMessageBus messageBus,
				ITestCaseOrderer testCaseOrderer,
				ExceptionAggregator aggregator,
				CancellationTokenSource cancellationTokenSource)
				: base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
			{
				this.assemblyFixtureMappings = assemblyFixtureMappings;
				this.diagnosticMessageSink = diagnosticMessageSink;
			}

			protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
			{
				// Don't want to use .Concat + .ToDictionary because of the possibility of overriding types,
				// so instead we'll just let collection fixtures override assembly fixtures.
				var combinedFixtures = new Dictionary<Type, object>(assemblyFixtureMappings);
				foreach (var kvp in CollectionFixtureMappings)
					combinedFixtures[kvp.Key] = kvp.Value;

				// We've done everything we need, so let the built-in types do the rest of the heavy lifting
				var runner = new XunitTestClassRunner(
					testClass,
					@class,
					testCases,
					diagnosticMessageSink,
					MessageBus,
					TestCaseOrderer,
					new ExceptionAggregator(Aggregator),
					CancellationTokenSource,
					combinedFixtures);
				return runner.RunAsync();
			}
		}
	}
}
