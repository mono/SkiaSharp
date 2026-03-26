namespace SkiaSharpSample.Controls;

public abstract record SampleControl(string Id, string Label, string? Description = null);

public record SliderControl(string Id, string Label, float Min, float Max, float Value, float Step = 0, string? Description = null) : SampleControl(Id, Label, Description);

public record ToggleControl(string Id, string Label, bool Value, string? Description = null) : SampleControl(Id, Label, Description);

public record PickerControl(string Id, string Label, string[] Options, int SelectedIndex = 0, string? Description = null) : SampleControl(Id, Label, Description);

/// <summary>
/// A group of controls with an enable/disable toggle. Used for composable effect stacks.
/// Child control IDs are prefixed with "{GroupId}." when reported via OnControlChanged.
/// </summary>
public record GroupControl(string Id, string Label, bool Enabled, IReadOnlyList<SampleControl> Children, string? Description = null) : SampleControl(Id, Label, Description);
