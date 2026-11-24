namespace SkiaSharp.Tests.Samples.Utils;

// Fixed CI partition contract. Do not add groups; assign new tests to an existing group.
public enum SampleRunGroup
{
    Build,
    Host,
    Docker,
    Android,
    Ios,
    Mac,
    Windows,
}
