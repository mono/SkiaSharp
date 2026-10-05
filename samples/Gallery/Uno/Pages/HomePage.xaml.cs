using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace SkiaSharpSample.Pages;

public sealed partial class HomePage : Page
{
    private readonly SampleService sampleService;
    private readonly SampleBase[] allSamples;
    private List<SampleCardItem> currentItems = new();
    private string? selectedCategory;
    private string[] allCategories = Array.Empty<string>();
    private string searchText = string.Empty;
    private int currentColumns = 3;

    public HomePage()
    {
        this.InitializeComponent();
        sampleService = App.SampleService;
        allSamples = sampleService.GetAllSamples().ToArray();

        allCategories = SampleManager.GetCategories()
            .Select(c => c.Name)
            .Where(name => allSamples.Any(s => s.Category == name))
            .ToArray();
        RefreshCards();
        FooterText.Text = BuildFooter();
        SizeChanged += (_, args) =>
        {
            var columns = GetColumns(args.NewSize.Width);
            if (columns == currentColumns) return;
            currentColumns = columns;
            RenderCards();
        };
    }

    private int GetColumns(double width)
    {
        if (width < 640) return 1;
        if (width < 960) return 2;
        return 3;
    }

    private void BuildCategoryChips(Dictionary<string, int> liveCounts, int resultCount)
    {
        CategoryChipsHost.Children.Clear();

        var all = new Button
        {
            Content = new TextBlock
            {
                Text = resultCount == 0 ? "Reset filters" :
                    $"All categories  {resultCount}",
                FontSize = 11
            },
            Padding = new Thickness(8, 2, 8, 2),
            MinHeight = 0,
            MinWidth = 0,
            Background = new SolidColorBrush(Colors.Transparent),
            BorderThickness = new Thickness(0),
        };
        all.Click += (_, _) => OnAllClearClicked(resultCount == 0);
        CategoryChipsHost.Children.Add(all);

        foreach (var category in allCategories)
        {
            var count = liveCounts.GetValueOrDefault(category);
            if (count == 0) continue;
            var cat = SampleManager.GetCategoryFor(category);
            var color = ParseBrush(cat.Color);
            var button = new Button
            {
                Content = $"● {category}  {count}",
                Padding = new Thickness(10, 4, 12, 4),
                Tag = category,
                CornerRadius = new CornerRadius(16),
                BorderThickness = new Thickness(1),
            };
            button.Click += OnCategoryChipClicked;
            UpdateCategoryChipVisual(button, color, selected: selectedCategory == category);
            CategoryChipsHost.Children.Add(button);
        }
    }

    private void OnCategoryChipClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string category }) return;
        selectedCategory = category;
        RefreshCards();
    }

    private void OnAllClearClicked(bool noResults)
    {
        selectedCategory = null;
        if (noResults)
        {
            searchText = "";
            SearchBox.Text = "";
        }
        RefreshCards();
    }

    private static void UpdateCategoryChipVisual(Button btn, SolidColorBrush color, bool selected)
    {
        if (selected)
        {
            btn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0x1F, color.Color.R, color.Color.G, color.Color.B));
            btn.BorderBrush = color;
            btn.Foreground = color;
        }
        else
        {
            btn.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            btn.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0x50, 0x80, 0x80, 0x80));
            btn.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0x80, 0, 0, 0));
        }
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var text = SearchBox.Text ?? string.Empty;
        if (searchText == text) return;
        searchText = text;
        RefreshCards();
    }

    private void RefreshCards()
    {
        ISet<string>? categories = selectedCategory is { } category
            ? new HashSet<string>(StringComparer.Ordinal) { category }
            : null;
        var filtered = SampleManager.SearchSamples(allSamples, searchText, categories, sort: SampleSortOrder.NewestFirst)
            .ToArray();
        currentItems = filtered
            .Select(s => new SampleCardItem(s, allSamples))
            .ToList();
        BuildCategoryChips(SampleManager.GetCategoryCounts(filtered), filtered.Length);
        currentColumns = GetColumns(ActualWidth > 0 ? ActualWidth : 1200);
        RenderCards();
    }

    private void RenderCards()
    {
        CardGridHost.Children.Clear();
        Grid? currentRow = null;
        for (int i = 0; i < currentItems.Count; i++)
        {
            if (i % currentColumns == 0)
            {
                currentRow = new Grid { ColumnSpacing = 12 };
                for (int c = 0; c < currentColumns; c++)
                    currentRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                CardGridHost.Children.Add(currentRow);
            }
            var card = new Controls.SampleCard
            {
                Sample = currentItems[i].Sample,
                CategoryColor = currentItems[i].CategoryColorBrush,
                Icon = currentItems[i].Icon,
                Supported = currentItems[i].Supported,
            };
            // UserControl PointerReleased is unreliable under SkiaRenderer; HyperlinkButton.Click is not.
            var link = new HyperlinkButton
            {
                Content = card,
                Padding = new Thickness(0),
                Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                IsEnabled = currentItems[i].Supported,
                Tag = currentItems[i].Sample,
            };
            link.Click += OnCardClicked;
            Grid.SetColumn(link, i % currentColumns);
            currentRow!.Children.Add(link);
        }
    }

    private void OnCardClicked(object sender, RoutedEventArgs e)
    {
        if (sender is HyperlinkButton { Tag: SampleBase sample } && sample.IsSupported)
        {
            MainPage.Current?.NavigateToSample(sample);
        }
    }


    private string BuildFooter()
    {
        var parts = new List<string>
        {
            $"SkiaSharp {sampleService.SkiaSharpVersion}",
            $"HarfBuzzSharp {sampleService.HarfBuzzSharpVersion}",
        };
        if (sampleService.BuildTimestamp is { } ts)
            parts.Add($"built {ts.UtcDateTime:yyyy-MM-dd HH:mm} UTC");
        if (!string.IsNullOrWhiteSpace(sampleService.BuildFooter))
            parts.Add(sampleService.BuildFooter);
        return string.Join("  ·  ", parts);
    }

    private static SolidColorBrush ParseBrush(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            var c = Windows.UI.Color.FromArgb(0xFF,
                Convert.ToByte(hex[..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16));
            return new SolidColorBrush(c);
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x54, 0x6E, 0x7A));
    }
}

public sealed class SampleCardItem
{
    public SampleCardItem(SampleBase sample, IEnumerable<SampleBase> allSamples)
    {
        Sample = sample;
        Icon = SampleManager.GetUnicodeIcon(sample.Title);
        CategoryColorBrush = ParseBrush(SampleManager.GetCategoryFor(sample.Category).Color);
        Supported = sample.IsSupported;
        IsNew = SampleManager.IsNew(sample, allSamples);
    }

    public SampleBase Sample { get; }
    public string Icon { get; }
    public SolidColorBrush CategoryColorBrush { get; }
    public bool Supported { get; }
    public bool IsNew { get; }

    private static SolidColorBrush ParseBrush(string hex)
    {
        var c = ParseHexColor(hex);
        return new SolidColorBrush(c);
    }

    private static Windows.UI.Color ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
            return Windows.UI.Color.FromArgb(0xFF,
                Convert.ToByte(hex[..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16));
        return Windows.UI.Color.FromArgb(0xFF, 0x54, 0x6E, 0x7A);
    }
}
