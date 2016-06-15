using System;
using System.Linq;

using Xamarin.Forms;
using SkiaSharp;

namespace Skia.Forms.Demo
{
    public class App : Application
    {
        public static readonly Color XamarinBlue = Color.FromHex("3498db");
        public static readonly Color XamarinGreen = Color.FromHex("77d065");
        public static readonly Color XamarinPurple = Color.FromHex("b455b6");
        public static readonly Color XamarinDark = Color.FromHex("2c3e50");

        public App()
        {
            var items =
                Device.OS == TargetPlatform.iOS ? Demos.SamplesForPlatform(Demos.Platform.iOS) :
                Device.OS == TargetPlatform.Android ? Demos.SamplesForPlatform(Demos.Platform.Android) :
                Device.OS == TargetPlatform.Windows ? Demos.SamplesForPlatform(Demos.Platform.UWP) :
                Demos.SamplesForPlatform(Demos.Platform.All);

            var masterDetail = new MasterDetailPage
            {
                IsGestureEnabled = true,
                MasterBehavior = MasterBehavior.Popover
            };
            var detailPage = new DetailsPage(items.First());
            var navPage = new NavigationPage(detailPage)
            {
                BarBackgroundColor = XamarinBlue,
                BarTextColor = Color.White,
            };
            var masterPage = new MasterPage(items, demo =>
            {
                masterDetail.IsPresented = false;
                detailPage.SetDemo(demo);
                navPage.Title = demo;
            });
            if (Device.OS == TargetPlatform.iOS)
            {
                masterPage.Icon = "hamburger.png";
            }

            masterDetail.Master = masterPage;
            masterDetail.Detail = navPage;

            MainPage = masterDetail;

            masterDetail.IsPresented = true;
        }

        public class DetailsPage : ContentPage
        {
            public DetailsPage(string demo)
            {
                SetDemo(demo);
            }

            public void SetDemo(string demo)
            {
                Title = demo;
                Content = new SkiaView(Demos.GetSample(demo));
            }
        }

        public class MasterPage : ContentPage
        {
            private readonly ListView listView;
            private readonly Action<string> showDemo;

            public MasterPage(string[] items, Action<string> showDemo)
            {
                this.showDemo = showDemo;

                Title = "SkiaSharp";
                BackgroundColor = Color.FromHex("EEEEEE");

                // a nice header
                var header = new ContentView
                {
                    BackgroundColor = XamarinDark,
                    Padding = new Thickness(12, 72, 12, 12),
                    Margin = 0,
                    Content = new Label
                    {
                        Text = Title,
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                        TextColor = Color.White
                    },
                };

                var spacer = new BoxView
                {
                    Color = XamarinBlue,
                    HorizontalOptions = LayoutOptions.Fill,
                    Margin = 0,
                    HeightRequest = 3
                };

                // the contents of the menu
                listView = new ListView()
                {
                    ItemsSource = items,
                };
                listView.ItemSelected += OnItemSelected;

                // UWP specific for more spacing there
                if (Device.OS == TargetPlatform.Windows)
                {
                    listView.RowHeight = 30;
                    listView.Margin = new Thickness(12, 6);
                    header.Padding = new Thickness(12, 36, 12, 12);
                }

                // put it all together
                Content = new StackLayout
                {
                    Children = { header, spacer, listView },
                    Padding = 0,
                    Spacing = 0
                };
            }

            private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
            {
                // deselect the menu item
                if (e.SelectedItem == null)
                {
                    return;
                }
                listView.SelectedItem = null;

                // display the selected demo
                showDemo(e.SelectedItem.ToString());
            }
        }
    }
}
