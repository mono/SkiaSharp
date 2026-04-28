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
    private readonly HashSet<string> selectedCategories = new();
    private readonly List<Button> categoryButtons = new();
    private Button? allClearButton;
    private string[] allCategories = Array.Empty<string>();
    private string searchText = string.Empty;

    public HomePage()
    {
        this.InitializeComponent();
        sampleService = App.SampleService;

        allCategories = sampleService.GetAllSamples()
            .Select(s => s.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToArray();
        foreach (var c in allCategories) selectedCategories.Add(c);

        BuildCategoryChips();
        RefreshCards();
        FooterText.Text = BuildFooter();
    }

    private double lastKnownWidth;
    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        SizeChanged += (_, args) =>
        {
            var newCols = GetColumns(args.NewSize.Width);
            if (Math.Abs(args.NewSize.Width - lastKnownWidth) > 50)
            {
                lastKnownWidth = args.NewSize.Width;
                RefreshCards();
            }
        };
    }

    private int GetColumns(double width)
    {
        if (width < 640) return 1;
        if (width < 960) return 2;
        return 3;
    }

    private void BuildCategoryChips()
    {
        CategoryChipsHost.Children.Clear();
        categoryButtons.Clear();

        foreach (var category in allCategories)
        {
            var color = CategoryColors.Brush(category);
            var button = new Button
            {
                Content = "● " + category,
                Padding = new Thickness(10, 4, 12, 4),
                Tag = (category, true),
                CornerRadius = new CornerRadius(16),
                BorderThickness = new Thickness(1),
            };
            button.Click += OnCategoryChipClicked;
            UpdateCategoryChipVisual(button, color, selected: true);
            categoryButtons.Add(button);
            CategoryChipsHost.Children.Add(button);
        }

        allClearButton = new Button
        {
            Content = new TextBlock { Text = "Clear", FontSize = 11 },
            Padding = new Thickness(8, 2, 8, 2),
            MinHeight = 0,
            MinWidth = 0,
            Background = new SolidColorBrush(Colors.Transparent),
            BorderThickness = new Thickness(0),
        };
        allClearButton.Click += (_, _) => OnAllClearClicked();
        CategoryChipsHost.Children.Add(allClearButton);
    }

    private void OnCategoryChipClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not (string category, bool selected)) return;
        var newSelected = !selected;
        btn.Tag = (category, newSelected);
        UpdateCategoryChipVisual(btn, CategoryColors.Brush(category), newSelected);
        if (newSelected) selectedCategories.Add(category);
        else selectedCategories.Remove(category);
        UpdateAllClearLabel();
        RefreshCards();
    }

    private void OnAllClearClicked()
    {
        var allSelected = selectedCategories.Count == allCategories.Length;
        var target = !allSelected;
        foreach (var btn in categoryButtons)
        {
            if (btn.Tag is not (string category, _)) continue;
            btn.Tag = (category, target);
            UpdateCategoryChipVisual(btn, CategoryColors.Brush(category), target);
            if (target) selectedCategories.Add(category);
            else selectedCategories.Remove(category);
        }
        UpdateAllClearLabel();
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

    private void UpdateAllClearLabel()
    {
        if (allClearButton?.Content is TextBlock tb)
            tb.Text = selectedCategories.Count == allCategories.Length ? "Clear" : "All";
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        searchText = SearchBox.Text ?? string.Empty;
        RefreshCards();
    }

    private void RefreshCards()
    {
        var items = sampleService.GetAllSamples()
            .Where(s => selectedCategories.Contains(s.Category))
            .Where(s => s.MatchesFilter(searchText))
            .OrderBy(s => s.IsSupported ? 0 : 1)
            .ThenBy(s => s.Title)
            .Select(s => new SampleCardItem(s))
            .ToList();

        CardGridHost.Children.Clear();
        var columns = GetColumns(ActualWidth > 0 ? ActualWidth : 1200);
        Grid? currentRow = null;
        for (int i = 0; i < items.Count; i++)
        {
            if (i % columns == 0)
            {
                currentRow = new Grid { ColumnSpacing = 12 };
                for (int c = 0; c < columns; c++)
                    currentRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                CardGridHost.Children.Add(currentRow);
            }
            var card = new Controls.SampleCard
            {
                Sample = items[i].Sample,
                CategoryColor = items[i].CategoryColorBrush,
                Icon = items[i].Icon,
                Supported = items[i].Supported,
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
                IsEnabled = items[i].Supported,
                Tag = items[i].Sample,
            };
            link.Click += OnCardClicked;
            Grid.SetColumn(link, i % columns);
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
}

public sealed class SampleCardItem
{
    public SampleCardItem(SampleBase sample)
    {
        Sample = sample;
        Icon = CategoryColors.Icon(sample.Title);
        CategoryColorBrush = CategoryColors.Brush(sample.Category);
        Supported = sample.IsSupported;
    }

    public SampleBase Sample { get; }
    public string Icon { get; }
    public SolidColorBrush CategoryColorBrush { get; }
    public bool Supported { get; }
}
