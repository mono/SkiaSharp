namespace DocsSamplesApp;

public class HomeBasePage : ContentPage
{
    protected async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is MenuItem item && !string.IsNullOrEmpty(item.Route))
        {
            // Clear selection
            if (sender is CollectionView cv)
                cv.SelectedItem = null;

            await Shell.Current.GoToAsync(item.Route);
        }
    }
}
