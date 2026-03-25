namespace SkiaSharpSample.Controls;

public abstract record SampleControl(string Id, string Label);

public record SliderControl(string Id, string Label, float Min, float Max, float Value, float Step = 0) : SampleControl(Id, Label);

public record ToggleControl(string Id, string Label, bool Value) : SampleControl(Id, Label);

public record PickerControl(string Id, string Label, string[] Options, int SelectedIndex = 0) : SampleControl(Id, Label);
