using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting.WPF;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Xunit;

namespace SkiaSharp.Views.Maui.Controls.WPF.Tests;

internal static class WPFTestThread
{
	// GLWpfControl shares a current OpenGL context, so every test uses the same STA thread.
	private static readonly Lazy<Dispatcher> dispatcher = new(CreateDispatcher);

	internal static MauiApp CreateApp() => MauiApp.CreateBuilder()
		.UseMauiAppWPF<WPFTestApplication>()
		.UseSkiaSharpWPF()
		.Build();

	internal static void Run(Action action) => RunAsync(() =>
	{
		action();
		return Task.CompletedTask;
	});

	internal static void RunAsync(Func<Task> action)
	{
		dispatcher.Value.InvokeAsync(action).Task.Unwrap().GetAwaiter().GetResult();
	}

	private static Dispatcher CreateDispatcher()
	{
		var ready = new TaskCompletionSource<Dispatcher>(TaskCreationOptions.RunContinuationsAsynchronously);
		var thread = new Thread(() =>
		{
			var current = Dispatcher.CurrentDispatcher;
			SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(current));
			ready.SetResult(current);
			Dispatcher.Run();
		})
		{
			IsBackground = true,
		};
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		return ready.Task.GetAwaiter().GetResult();
	}
}

internal sealed class WPFTestApplication : Application
{
	public WPFTestApplication() { }
}

[CollectionDefinition("WPF handlers", DisableParallelization = true)]
public sealed class WPFTestCollection { }
