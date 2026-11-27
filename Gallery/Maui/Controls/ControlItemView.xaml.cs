using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

public partial class ControlItemView : ContentView
{
    private bool applyingContext;

    public ControlItemView() => InitializeComponent();

    protected override void OnBindingContextChanged()
    {
        applyingContext = true;
        try
        {
            base.OnBindingContextChanged();
            // Range coercion during binding must not replace the sample's initial values.
            (BindingContext as ControlItem)?.Refresh();
        }
        finally
        {
            applyingContext = false;
        }
    }

    private ControlItem? EditableItem =>
        IsLoaded && !applyingContext ? BindingContext as ControlItem : null;

    private void SliderChanged(object? sender, ValueChangedEventArgs e) =>
        EditableItem?.ChangeSlider(e.NewValue);

    private void PickerChanged(object? sender, EventArgs e) =>
        EditableItem?.ChangePicker(OptionsPicker.SelectedIndex);

    private void ToggleChanged(object? sender, ToggledEventArgs e) =>
        EditableItem?.ChangeToggle(e.Value);

    private void GroupChanged(object? sender, ToggledEventArgs e) =>
        EditableItem?.ChangeGroup(e.Value);
}
