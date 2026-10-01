using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SkiaSharpSample;

internal static class GalleryUi
{
    public static readonly Color Navy = Color.FromArgb("#1A237E");
    public static readonly Color Accent = Color.FromArgb("#3F70DC");
    public const string IconFunnel = "\uf3e1";
    public const string IconSearch = "\uf52a";
    public const string IconSort = "\uf575";
    public const string IconChevronDown = "\uf282";
    public const string IconBack = "\uf284";
    public const string IconClose = "\uf659";

    public static Label Text(string text, int size = 14, bool bold = false, string color = "PrimaryText")
    {
        var label = new Label
        {
            Text = text,
            FontSize = size,
            FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None,
            LineBreakMode = LineBreakMode.WordWrap
        };
        label.Style = (Style)Application.Current!.Resources[color switch
        {
            "SecondaryText" => "SecondaryLabel",
            "AccentText" => "AccentLabel",
            _ => "PrimaryLabel"
        }];
        return label;
    }

    public static Border Card(View content, int radius = 16)
    {
        var border = new Border
        {
            Content = content,
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(radius) }
        };
        Background(border, "CardBackground");
        border.SetAppTheme<Brush>(Border.StrokeProperty,
            new SolidColorBrush(Token("CardBorder", false)),
            new SolidColorBrush(Token("CardBorder", true)));
        return border;
    }

    public static Button Action(string text, string id, EventHandler clicked)
    {
        var button = new Button
        {
            Text = text, AutomationId = id, CornerRadius = 10, FontSize = 13,
            BackgroundColor = Navy, TextColor = Colors.White,
            Padding = new Thickness(12, 8),
            MinimumHeightRequest = 44
        };
        button.Clicked += clicked;
        return button;
    }

    public static void Background(VisualElement element, string key) =>
        element.SetAppThemeColor(VisualElement.BackgroundColorProperty, Token(key, false), Token(key, true));

    public static void TextColor(VisualElement element, BindableProperty property, string key) =>
        element.SetAppThemeColor(property, Token(key, false), Token(key, true));

    public static Color Token(string key, bool dark) =>
        (Color)Application.Current!.Resources[$"{key}{(dark ? "Dark" : "Light")}"];

    public static void CategoryColor(Label label, string category)
    {
        var color = Color.FromArgb(SampleManager.GetCategoryFor(category).Color);
        label.SetAppThemeColor(Label.TextColorProperty, color,
            new Color((color.Red + 1) / 2, (color.Green + 1) / 2, (color.Blue + 1) / 2));
    }

    public static ImageButton IconButton(string glyph, string id, string description, EventHandler clicked)
    {
        var button = new ImageButton
        {
            AutomationId = id, Padding = new Thickness(10), CornerRadius = 11,
            MinimumWidthRequest = 44, MinimumHeightRequest = 44,
            Aspect = Aspect.Center
        };
        button.SetAppTheme<ImageSource>(ImageButton.SourceProperty,
            new FontImageSource { Glyph = glyph, FontFamily = "BootstrapIcons", Size = 17, Color = Navy },
            new FontImageSource { Glyph = glyph, FontFamily = "BootstrapIcons", Size = 17, Color = Token("PrimaryText", true) });
        Background(button, "SubtleBackground");
        SemanticProperties.SetDescription(button, description);
        button.Clicked += clicked;
        return button;
    }

    public static Button Pill(string text, string id, EventHandler clicked)
    {
        var button = new Button
        {
            Text = text, AutomationId = id,
            Style = (Style)Application.Current!.Resources["GalleryPill"]
        };
        button.Clicked += clicked;
        return button;
    }

    public static void SelectPill(Button button, bool selected, TagKind? kind = null)
    {
        Background(button, selected ? "SubtleBackground" :
            kind == TagKind.Type ? "TypeBackground" :
            kind == TagKind.Method ? "MethodBackground" : "CardBackground");
        TextColor(button, Button.TextColorProperty, selected ? "AccentText" : "PrimaryText");
        button.SetAppThemeColor(Button.BorderColorProperty,
            selected ? Accent : Token("CardBorder", false),
            selected ? Token("AccentText", true) : Token("CardBorder", true));
        button.BorderWidth = selected ? 1.5 : 1;
    }

    public static void SelectRow(Button button, bool selected)
    {
        Background(button, selected ? "SubtleBackground" : "CardBackground");
        TextColor(button, Button.TextColorProperty, selected ? "AccentText" : "PrimaryText");
        button.FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None;
    }

    public static void StylePicker(Picker picker)
    {
        Background(picker, "CardBackground");
        TextColor(picker, Picker.TextColorProperty, "PrimaryText");
        TextColor(picker, Picker.TitleColorProperty, "SecondaryText");
    }

    public static string StableId(string prefix, string text) =>
        prefix + string.Concat(text.ToLowerInvariant().Select(ch => char.IsAsciiLetterOrDigit(ch) ? ch : '-'));
}
