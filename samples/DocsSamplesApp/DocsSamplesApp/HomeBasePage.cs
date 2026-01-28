namespace DocsSamplesApp;

public class HomeBasePage : ContentPage
{
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
