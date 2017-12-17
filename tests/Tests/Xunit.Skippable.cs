using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
	public class SkipException : Exception
	{
		public SkipException(string reason)
			: base(reason)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	[XunitTestCaseDiscoverer("Xunit.SkippableTheoryDiscoverer", "SkiaSharp.Tests")]
	public class SkippableTheoryAttribute : TheoryAttribute
	{
		public SkippableTheoryAttribute(params Type[] skippingExceptions)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	[XunitTestCaseDiscoverer("Xunit.SkippableFactDiscoverer", "SkiaSharp.Tests")]
	public class SkippableFactAttribute : FactAttribute
	{
		public SkippableFactAttribute(params Type[] skippingExceptions)
		{
		}
	}

	public class SkippableTheoryDiscoverer : IXunitTestCaseDiscoverer
	{
		private readonly IMessageSink diagnosticMessageSink;
		private readonly TheoryDiscoverer theoryDiscoverer;

		public SkippableTheoryDiscoverer(IMessageSink diagnosticMessageSink)
		{
			this.diagnosticMessageSink = diagnosticMessageSink;
			this.theoryDiscoverer = new TheoryDiscoverer(diagnosticMessageSink);
		}

		public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
		{
			string[] skippingExceptionNames = SkippableFactDiscoverer.GetSkippableExceptionNames(factAttribute);
			TestMethodDisplay defaultMethodDisplay = discoveryOptions.MethodDisplayOrDefault();
			TestMethodDisplayOptions defaultMethodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();

			var basis = this.theoryDiscoverer.Discover(discoveryOptions, testMethod, factAttribute);
			foreach (var testCase in basis)
			{
				if (testCase is XunitTheoryTestCase)
				{
					yield return new SkippableTheoryTestCase(skippingExceptionNames, this.diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testCase.TestMethod);
				}
				else
				{
					yield return new SkippableFactDiscoverer.SkippableFactTestCase(skippingExceptionNames, this.diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testCase.TestMethod, testCase.TestMethodArguments);
				}
			}
		}

		private class SkippableTheoryTestCase : XunitTheoryTestCase
		{
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("Called by the de-serializer", true)]
			public SkippableTheoryTestCase()
			{
			}

			public SkippableTheoryTestCase(string[] skippingExceptionNames, IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod)
				: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
			{
				this.SkippingExceptionNames = skippingExceptionNames ?? throw new ArgumentNullException(nameof(skippingExceptionNames));
			}

			internal string[] SkippingExceptionNames { get; private set; }

			public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
			{
				var messageBusInterceptor = new SkippableTestMessageBus(messageBus, this.SkippingExceptionNames);
				var result = await base.RunAsync(diagnosticMessageSink, messageBusInterceptor, constructorArguments, aggregator, cancellationTokenSource);
				result.Failed -= messageBusInterceptor.SkippedCount;
				result.Skipped += messageBusInterceptor.SkippedCount;
				return result;
			}

			public override void Serialize(IXunitSerializationInfo data)
			{
				base.Serialize(data);
				data.AddValue(nameof(this.SkippingExceptionNames), this.SkippingExceptionNames);
			}

			public override void Deserialize(IXunitSerializationInfo data)
			{
				base.Deserialize(data);
				this.SkippingExceptionNames = data.GetValue<string[]>(nameof(this.SkippingExceptionNames));
			}
		}
	}

	public class SkippableFactDiscoverer : IXunitTestCaseDiscoverer
	{
		private readonly IMessageSink diagnosticMessageSink;

		public SkippableFactDiscoverer(IMessageSink diagnosticMessageSink)
		{
			this.diagnosticMessageSink = diagnosticMessageSink;
		}

		public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
		{
			string[] skippingExceptionNames = GetSkippableExceptionNames(factAttribute);
			yield return new SkippableFactTestCase(skippingExceptionNames, this.diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod);
		}

		internal static string[] GetSkippableExceptionNames(IAttributeInfo factAttribute)
		{
			var firstArgument = (object[])factAttribute.GetConstructorArguments().FirstOrDefault();
			var skippingExceptions = firstArgument?.Cast<Type>().ToArray() ?? new Type[0];
			Array.Resize(ref skippingExceptions, skippingExceptions.Length + 1);
			skippingExceptions[skippingExceptions.Length - 1] = typeof(SkipException);

			var skippingExceptionNames = skippingExceptions.Select(ex => ex.FullName).ToArray();
			return skippingExceptionNames;
		}

		internal class SkippableFactTestCase : XunitTestCase
		{
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("Called by the de-serializer", true)]
			public SkippableFactTestCase()
			{
			}

			public SkippableFactTestCase(string[] skippingExceptionNames, IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod, object[] testMethodArguments = null)
				: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
			{
				this.SkippingExceptionNames = skippingExceptionNames ?? throw new ArgumentNullException(nameof(skippingExceptionNames));
			}

			internal string[] SkippingExceptionNames { get; private set; }

			public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
			{
				var messageBusInterceptor = new SkippableTestMessageBus(messageBus, this.SkippingExceptionNames);
				var result = await base.RunAsync(diagnosticMessageSink, messageBusInterceptor, constructorArguments, aggregator, cancellationTokenSource);
				result.Failed -= messageBusInterceptor.SkippedCount;
				result.Skipped += messageBusInterceptor.SkippedCount;
				return result;
			}

			public override void Serialize(IXunitSerializationInfo data)
			{
				base.Serialize(data);
				data.AddValue(nameof(this.SkippingExceptionNames), this.SkippingExceptionNames);
			}

			public override void Deserialize(IXunitSerializationInfo data)
			{
				base.Deserialize(data);
				this.SkippingExceptionNames = data.GetValue<string[]>(nameof(this.SkippingExceptionNames));
			}
		}
	}

	internal class SkippableTestMessageBus : IMessageBus
	{
		private readonly IMessageBus inner;

		internal SkippableTestMessageBus(IMessageBus inner, string[] skippingExceptionNames)
		{
			this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
			this.SkippingExceptionNames = skippingExceptionNames ?? throw new ArgumentNullException(nameof(skippingExceptionNames));
		}

		internal string[] SkippingExceptionNames { get; }

		internal int SkippedCount { get; private set; }

		public void Dispose()
		{
			this.inner.Dispose();
		}

		public bool QueueMessage(IMessageSinkMessage message)
		{
			var failed = message as TestFailed;
			if (failed != null)
			{
				var outerException = failed.ExceptionTypes.FirstOrDefault();
				if (outerException != null && Array.IndexOf(this.SkippingExceptionNames, outerException) >= 0)
				{
					this.SkippedCount++;
					return this.inner.QueueMessage(new TestSkipped(failed.Test, failed.Messages[0]));
				}
			}

			return this.inner.QueueMessage(message);
		}
	}
}
