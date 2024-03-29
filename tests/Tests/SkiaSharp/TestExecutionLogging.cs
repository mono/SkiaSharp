using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Xunit.Sdk;
using System.Reflection;

namespace SkiaSharp.Tests
{
	public class LogTestExecutionAttribute : BeforeAfterTestAttribute
	{
		public override void Before(MethodInfo methodUnderTest) =>
			TestExecutionLogging.LogBegin(methodUnderTest);

		public override void After(MethodInfo methodUnderTest) =>
			TestExecutionLogging.LogEnd(methodUnderTest);
	}

	internal static class TestExecutionLogging
	{
		private static readonly string LogFileName = Path.Combine(TestConfig.Current.PathRoot, $"TestExecutionOrder_{DateTime.UtcNow:yyyy_MM_dd_HH_mm}_D_{AppDomain.CurrentDomain.Id}.csv");

		private static readonly ConcurrentDictionary<string, TestExecutionData> testDataDict = new();
		private static readonly object writeLocker = new();
		private static int _startedOrder = 0;
		private static int _endedOrder = 0;

		public static void LogBegin(MethodInfo testInfo)
		{
			var name = $"{testInfo.DeclaringType.FullName}.{testInfo.Name}";
			var order = Interlocked.Add(ref _startedOrder, 1);
			var startedUtc = DateTime.UtcNow;
			
			var data = testDataDict.GetOrAdd(name, new TestExecutionData());
			data.StartedUtc = startedUtc;
			data.StartedOrder = order;
			data.TestName = name;
			data.Status = "Started";
			data.StartThreadId = Thread.CurrentThread.ManagedThreadId;

			WriteLog(data);
		}

		public static void LogEnd(MethodInfo testInfo)
		{
			var name = $"{testInfo.DeclaringType.FullName}.{testInfo.Name}";
			var dataEndedUtc = DateTime.UtcNow;
			var order = Interlocked.Add(ref _endedOrder, 1);

			var data = testDataDict[name];
			data.EndedUtc = dataEndedUtc;
			data.EndedOrder = order;
			data.Status = "Ended";
			data.EndThreadId = Thread.CurrentThread.ManagedThreadId;

			WriteLog(data);
		}

		private static void WriteLog(TestExecutionData data)
		{
			lock (writeLocker)
			{
				File.AppendAllLines(LogFileName, new [] { data.ToString() });
			}
		}

		private class TestExecutionData
		{
			public int StartedOrder { get; set; }

			public int EndedOrder { get; set; }

			public DateTime StartedUtc { get; set; }

			public DateTime EndedUtc { get; set; }

			public string TestName { get; set; }

			public string Status { get; set; }

			public int StartThreadId { get; set; }

			public int EndThreadId { get; set; }

			public override string ToString() =>
				$"{TestName};{Status};{StartedOrder};{EndedOrder};{StartedUtc:o};{EndedUtc:o};{Math.Max(0, ( EndedUtc - StartedUtc ).TotalMilliseconds)};{StartThreadId};{EndThreadId}";
		}
	}
}
