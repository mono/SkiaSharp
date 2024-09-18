#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace SkiaSharp.Views.Maui.Controls.Tests;

public static class MauiExtensions
{
	private static readonly Rect InitialFrame = new(0, 0, -1, -1);

	public static async Task WaitForLoaded(
		this VisualElement element,
		int timeout = 1000)
	{
		if (element.IsLoaded)
			return;

		var tcs = new TaskCompletionSource();

		element.Loaded += OnLoaded;

		await Task.WhenAny(tcs.Task, Task.Delay(timeout));

		element.Loaded -= OnLoaded;

		Assert.True(element.IsLoaded);

		void OnLoaded(object? sender, EventArgs e)
		{
			element.Loaded -= OnLoaded;
			tcs.SetResult();
		}
	}

	public static Task WaitForLayout(
		this View view,
		Rect? initialFrame = default,
		int timeout = 1000,
		int interval = 100)
	{
		initialFrame ??= InitialFrame;
		return AssertEx.Eventually(
			() => view.Frame != initialFrame,
			timeout,
			interval);
	}
}
