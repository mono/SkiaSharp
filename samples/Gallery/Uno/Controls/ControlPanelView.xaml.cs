using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;

namespace SkiaSharpSample.Controls;

public sealed partial class ControlPanelView : UserControl
{
    public event EventHandler<(string Id, object Value)>? ControlChanged;

    public ControlPanelView()
    {
        this.InitializeComponent();
    }

    public void SetControls(IReadOnlyList<SampleControl> controls)
    {
        RootPanel.Children.Clear();
        foreach (var c in controls)
        {
            RootPanel.Children.Add(Build(c, string.Empty));
        }
    }

    private UIElement Build(SampleControl control, string idPrefix)
    {
        var fullId = string.IsNullOrEmpty(idPrefix) ? control.Id : $"{idPrefix}.{control.Id}";

        switch (control)
        {
            case SliderControl s:
                return BuildSlider(s, fullId);
            case ToggleControl t:
                return BuildToggle(t, fullId);
            case PickerControl p:
                return BuildPicker(p, fullId);
            case GroupControl g:
                return BuildGroup(g, fullId);
            default:
                return new TextBlock { Text = $"(unsupported control: {control.GetType().Name})" };
        }
    }

    private StackPanel BuildSlider(SliderControl s, string id)
    {
        var panel = new StackPanel { Spacing = 2 };
        var header = new TextBlock { FontSize = 12 };
        var labelRun = new Microsoft.UI.Xaml.Documents.Run { Text = s.Label + ": " };
        var valueRun = new Microsoft.UI.Xaml.Documents.Run
        {
            Text = FormatSliderValue(s.Value),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
        };
        header.Inlines.Add(labelRun);
        header.Inlines.Add(valueRun);
        panel.Children.Add(header);

        var slider = new Slider
        {
            Minimum = s.Min,
            Maximum = s.Max,
            Value = s.Value,
            StepFrequency = s.Step <= 0 ? (s.Max - s.Min) / 100.0 : s.Step,
            SmallChange = s.Step <= 0 ? (s.Max - s.Min) / 100.0 : s.Step,
            IsThumbToolTipEnabled = false,
        };
        slider.ValueChanged += (_, e) =>
        {
            valueRun.Text = FormatSliderValue(e.NewValue);
            ControlChanged?.Invoke(this, (id, (float)e.NewValue));
        };
        panel.Children.Add(slider);

        if (!string.IsNullOrWhiteSpace(s.Description))
        {
            panel.Children.Add(new TextBlock { Text = s.Description, FontSize = 10, Opacity = 0.5, TextWrapping = TextWrapping.Wrap });
        }
        return panel;
    }

    private static string FormatSliderValue(double v) =>
        v.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

    private StackPanel BuildToggle(ToggleControl t, string id)
    {
        var panel = new StackPanel { Spacing = 2 };
        var toggle = new ToggleSwitch
        {
            Header = t.Label,
            IsOn = t.Value,
            OffContent = string.Empty,
            OnContent = string.Empty,
        };
        toggle.Toggled += (_, _) => ControlChanged?.Invoke(this, (id, toggle.IsOn));
        panel.Children.Add(toggle);
        if (!string.IsNullOrWhiteSpace(t.Description))
        {
            panel.Children.Add(new TextBlock { Text = t.Description, FontSize = 10, Opacity = 0.5, TextWrapping = TextWrapping.Wrap });
        }
        return panel;
    }

    private StackPanel BuildPicker(PickerControl p, string id)
    {
        var panel = new StackPanel { Spacing = 2 };
        panel.Children.Add(new TextBlock { Text = p.Label, FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        var combo = new ComboBox { SelectedIndex = p.SelectedIndex };
        foreach (var option in p.Options)
        {
            combo.Items.Add(option);
        }
        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedIndex >= 0)
            {
                ControlChanged?.Invoke(this, (id, combo.SelectedIndex));
            }
        };
        panel.Children.Add(combo);
        if (!string.IsNullOrWhiteSpace(p.Description))
        {
            panel.Children.Add(new TextBlock { Text = p.Description, FontSize = 10, Opacity = 0.5, TextWrapping = TextWrapping.Wrap });
        }
        return panel;
    }

    private StackPanel BuildGroup(GroupControl g, string id)
    {
        var panel = new StackPanel { Spacing = 6 };
        var header = new ToggleSwitch
        {
            Header = g.Label,
            IsOn = g.Enabled,
            OffContent = string.Empty,
            OnContent = string.Empty,
        };
        var childrenPanel = new StackPanel { Spacing = 8, Padding = new Thickness(8, 0, 0, 0) };

        void UpdateChildrenVisibility()
        {
            childrenPanel.Visibility = header.IsOn ? Visibility.Visible : Visibility.Collapsed;
        }

        header.Toggled += (_, _) =>
        {
            UpdateChildrenVisibility();
            ControlChanged?.Invoke(this, (id, header.IsOn));
        };

        foreach (var child in g.Children)
        {
            childrenPanel.Children.Add(Build(child, id));
        }
        UpdateChildrenVisibility();

        panel.Children.Add(header);
        panel.Children.Add(childrenPanel);

        if (!string.IsNullOrWhiteSpace(g.Description))
        {
            panel.Children.Add(new TextBlock { Text = g.Description, FontSize = 10, Opacity = 0.5, TextWrapping = TextWrapping.Wrap });
        }
        return panel;
    }
}
