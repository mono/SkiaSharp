using Xunit.v3;

namespace SkiaSharp.Tests.Samples.Utils;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RunGroupAttribute : Attribute, ITraitAttribute
{
    public RunGroupAttribute(SampleRunGroup group)
    {
        if (!Enum.IsDefined(group))
            throw new ArgumentOutOfRangeException(nameof(group), group, "Unknown sample execution group.");
        Group = group;
    }

    public SampleRunGroup Group { get; }

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
        [new("RunGroup", Group.ToString())];
}
