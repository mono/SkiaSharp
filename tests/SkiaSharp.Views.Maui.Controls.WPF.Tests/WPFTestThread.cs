using System;
using System.Runtime.ExceptionServices;
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
		ExceptionDispatchInfo? failure = null;
		var thread = new Thread(() =>
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
			dispatcher.BeginInvoke(new Action(async () =>
			{
				try
				{
					await action();
				}
				catch (Exception exception)
				{
					failure = ExceptionDispatchInfo.Capture(exception);
				}
				finally
				{
					dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
				}
			}));
			Dispatcher.Run();
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();
		failure?.Throw();
	}
}

internal sealed class WPFTestApplication : Application
{
	public WPFTestApplication() { }
}

[CollectionDefinition("WPF handlers", DisableParallelization = true)]
public sealed class WPFTestCollection { }
