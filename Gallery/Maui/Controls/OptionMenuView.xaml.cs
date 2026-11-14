using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

public partial class OptionMenuView : ContentView
{
    private protected OptionMenuView(string title, string id, double width, IReadOnlyList<MenuOptionItem> options)
    {
        TitleText = title;
        CloseId = id == "gallery-sort-menu" ? "gallery-sort-close" : "gallery-theme-close";
        Options = options;
        InitializeComponent();
        BindingContext = this;
        AutomationId = id;
        WidthRequest = width;
    }

    public string TitleText { get; }
    public string CloseId { get; }
    public IReadOnlyList<MenuOptionItem> Options { get; }

    private void OptionClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: MenuOptionItem option })
            option.Select();
    }

    private void CloseClicked(object? sender, EventArgs e) { }
}
