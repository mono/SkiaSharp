using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace SkiaSharpFormsDemos
{
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

        public ICommand NavigateCommand { private set; get; }
    }
}
