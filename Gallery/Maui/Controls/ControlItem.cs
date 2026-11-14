using System.ComponentModel;
using System.Runtime.CompilerServices;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Controls;

public sealed class ControlItem : INotifyPropertyChanged
{
    private readonly Action<string, object> change;
    private SampleControl current;
    private string label = "";
    private string? description;
    private string displayValue = "";
    private bool enabled;
    private bool toggled;
    private bool adjustable;
    private double minimum;
    private double maximum = 1;
    private double sliderValue;
    private int selectedIndex = -1;
    private string[] options = [];
    private bool synchronizing;

    public ControlItem(string id, SampleControl control, Action<string, object> change)
    {
        Id = id;
        this.change = change;
        current = control;
        Children = [];
        Update(control);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; }
    public Type Kind => current.GetType();
    public string AutomationId => GalleryUi.StableId("control-", Id);
    public string ValueId => $"{AutomationId}-value";
    public string SliderId => IsSlider ? AutomationId : $"{AutomationId}-unused-slider";
    public string ToggleId => IsToggle ? AutomationId : $"{AutomationId}-unused-toggle";
    public string PickerId => IsPicker ? AutomationId : $"{AutomationId}-unused-picker";
    public string GroupId => IsGroup ? AutomationId : $"{AutomationId}-unused-group";
    public string Label { get => label; private set => Set(ref label, value); }
    public string? Description { get => description; private set { if (Set(ref description, value)) OnPropertyChanged(nameof(HasDescription)); } }
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
    public bool IsSlider => current is SliderControl;
    public bool IsToggle => current is ToggleControl;
    public bool IsPicker => current is PickerControl;
    public bool IsGroup => current is GroupControl;
    public string DisplayValue { get => displayValue; private set => Set(ref displayValue, value); }
    public bool Enabled
    {
        get => enabled;
        set { if (Set(ref enabled, value)) OnPropertyChanged(nameof(ShowChildren)); }
    }
    public bool ShowChildren => IsGroup && Enabled;
    public bool Toggled { get => toggled; set => Set(ref toggled, value); }
    public bool Adjustable { get => adjustable; private set => Set(ref adjustable, value); }
    public double Minimum { get => minimum; private set => Set(ref minimum, value); }
    public double Maximum { get => maximum; private set => Set(ref maximum, value); }
    public double SliderValue { get => sliderValue; set => Set(ref sliderValue, value); }
    public int SelectedIndex { get => selectedIndex; set => Set(ref selectedIndex, value); }
    public string[] Options { get => options; private set => Set(ref options, value); }
    public IList<ControlItem> Children { get; }

    public void Refresh() => Update(current);

    public void Update(SampleControl control)
    {
        synchronizing = true;
        try
        {
            current = control;
            Label = control.Label;
            Description = control.Description;
            switch (control)
            {
                case SliderControl slider:
                    var valid = float.IsFinite(slider.Min) && float.IsFinite(slider.Max) && slider.Max > slider.Min;
                    Adjustable = valid;
                    if (valid)
                    {
                        if (Maximum < slider.Max) Maximum = slider.Max;
                        Minimum = slider.Min;
                        Maximum = slider.Max;
                        SliderValue = Math.Clamp(slider.Value, slider.Min, slider.Max);
                    }
                    DisplayValue = valid ? $"{slider.Label}: {slider.Value:G4}" : $"{slider.Label}: {slider.Value:G4} (fixed)";
                    break;
                case PickerControl picker:
                    if (!Options.SequenceEqual(picker.Options)) Options = picker.Options.ToArray();
                    SelectedIndex = Math.Clamp(picker.SelectedIndex, -1, picker.Options.Length - 1);
                    break;
                case ToggleControl toggle:
                    Toggled = toggle.Value;
                    break;
                case GroupControl group:
                    Enabled = group.Enabled;
                    break;
            }
        }
        finally
        {
            synchronizing = false;
        }
    }

    public void ChangeSlider(double value)
    {
        if (synchronizing || !Adjustable || current is not SliderControl slider) return;
        var snapped = SampleDetailPage.Snap(slider, value);
        if (snapped != slider.Value) change(Id, snapped);
        SliderValue = ((SliderControl)current).Value;
    }

    public void ChangePicker(int value)
    {
        if (!synchronizing && value >= 0 && current is PickerControl picker && value != picker.SelectedIndex)
            change(Id, value);
    }

    public void ChangeToggle(bool value)
    {
        if (!synchronizing && current is ToggleControl toggle && value != toggle.Value)
            change(Id, value);
    }

    public void ChangeGroup(bool value)
    {
        if (!synchronizing && current is GroupControl group && value != group.Enabled)
            change(Id, value);
    }

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged(string? name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
