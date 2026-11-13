using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace SkiaSharp.Tests;

[Instrumentation(Name = "com.companyname.SkiaSharpTests.TestInstrumentation")]
public class TestInstrumentation : Instrumentation
{
	protected TestInstrumentation(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	public override void OnCreate(Bundle arguments)
	{
		base.OnCreate(arguments);
		Start();
	}

	public override async void OnStart()
	{
		base.OnStart();
		var result = new Bundle();
		try
		{
			var intent = new Intent(TargetContext, typeof(MainActivity));
			intent.AddFlags(ActivityFlags.NewTask);
			StartActivitySync(intent);
			await TestApp.Ready.Task.WaitAsync(TimeSpan.FromMinutes(1));
			var consumer = new TestApp.ResultConsumer { Instrumentation = this };
			var exitCode = await TestApp.RunTestsAsync(consumer);
			result.PutInt("passed", consumer.Passed);
			result.PutInt("failed", consumer.Failed);
			result.PutInt("skipped", consumer.Skipped);
			result.PutString("resultsPath", consumer.TrxReportPath);
			Finish(exitCode == 0 ? Result.Ok : Result.Canceled, result);
		}
		catch (Exception error)
		{
			result.PutString("error", error.ToString());
			Finish(Result.Canceled, result);
		}
	}
}
