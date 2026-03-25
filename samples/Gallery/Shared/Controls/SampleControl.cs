namespace SkiaSharpSample.Controls;

public abstract record SampleControl(string Id, string Label);

public record SliderControl(string Id, string Label, float Min, float Max, float Value, float Step = 0) : SampleControl(Id, Label);

public record ToggleControl(string Id, string Label, bool Value) : SampleControl(Id, Label);

public record PickerControl(string Id, string Label, string[] Options, int SelectedIndex = 0) : SampleControl(Id, Label);

/// <summary>
/// A group of controls with an enable/disable toggle. Used for composable effect stacks.
/// Child control IDs are prefixed with "{GroupId}." when reported via OnControlChanged.
/// </summary>
public record GroupControl(string Id, string Label, bool Enabled, IReadOnlyList<SampleControl> Children) : SampleControl(Id, Label);
