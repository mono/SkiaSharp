using System.Windows.Input;

namespace DocsSamplesApp;

public class HomeBasePage : ContentPage
{
    public HomeBasePage()
    {
        NavigateCommand = new Command<Type?>(async (Type? pageType) =>
        {
            if (pageType is null) return;
            var page = (Page?)Activator.CreateInstance(pageType);
            if (page is not null)
                await Navigation.PushAsync(page);
        });

        BindingContext = this;
    }

    public ICommand NavigateCommand { get; }

    protected async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is MenuItem item && item.PageType is not null)
        {
            // Clear selection
            if (sender is CollectionView cv)
                cv.SelectedItem = null;

            var page = (Page?)Activator.CreateInstance(item.PageType);
            if (page is not null)
                await Navigation.PushAsync(page);
        }
    }
}
