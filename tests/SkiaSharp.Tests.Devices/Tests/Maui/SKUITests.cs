#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Views.Maui.Controls.Tests;

[Collection("SKUITests")]
public abstract class SKUITests : SKTest, IAsyncLifetime
{
	protected ContentPage CurrentPage { get; private set; } = null!;

	protected IMauiContext MauiContext { get; private set; } = null!;

	public async Task InitializeAsync()
	{
		Routing.RegisterRoute("uitests", typeof(ContentPage));

		await Shell.Current.GoToAsync("uitests");

		CurrentPage = (ContentPage)Shell.Current.CurrentPage;

		await CurrentPage.WaitForLoaded();

		MauiContext = CurrentPage.Handler!.MauiContext!;
	}

	public async Task DisposeAsync()
	{
		// pop all modals
		while (Shell.Current.CurrentPage.Navigation.ModalStack.Count > 0)
		{
			await Shell.Current.CurrentPage.Navigation.PopModalAsync();
		}

		// pop until we are back at our page
		while (Shell.Current.CurrentPage != CurrentPage)
		{
			await Shell.Current.CurrentPage.Navigation.PopAsync();
		}

		CurrentPage = null!;

		await Shell.Current.GoToAsync("..");

		Routing.UnRegisterRoute("uitests");
	}
}
