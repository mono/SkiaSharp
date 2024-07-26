using System.Windows.Input;

namespace DocsSamplesApp;

public class HomeBasePage : ContentPage
{
    public HomeBasePage()
    {
        NavigateCommand = new Command<Type>(async (Type pageType) =>
        {
            Page page = (Page)Activator.CreateInstance(pageType);
            await Navigation.PushAsync(page);
        });

        BindingContext = this;
    }

    public ICommand NavigateCommand { get; }
}
