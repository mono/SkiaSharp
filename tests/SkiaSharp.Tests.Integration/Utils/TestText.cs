namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Standard test text used for HarfBuzzSharp integration tests.
/// Provides consistent text shaping validation across platforms.
/// </summary>
public static class TestText
{
    public const string SampleText = "Hello SkiaSharp!";
    public const int ExpectedGlyphCount = 16; // One glyph per character

    /// <summary>
    /// Gets the C# code to embed in test apps that shapes text with HarfBuzz.
    /// Returns the glyph count and script name for validation.
    /// </summary>
    public static string GetShapeCode() => """
        using var buffer = new HarfBuzzSharp.Buffer();
        buffer.AddUtf8("Hello SkiaSharp!");
        buffer.GuessSegmentProperties();
        var glyphCount = buffer.Length;
        var script = buffer.Script.ToString();
        """;

    /// <summary>
    /// Gets the C# code that outputs the HarfBuzz test results.
    /// </summary>
    public static string GetOutputCode() => """
        Console.WriteLine($"HarfBuzzSharp OK: {glyphCount} glyphs, {script} script");
        """;

    /// <summary>
    /// Validates HarfBuzz output from a test run.
    /// </summary>
    public static bool ValidateOutput(string output)
    {
        return output.Contains("HarfBuzzSharp OK") &&
               output.Contains("glyphs") &&
               output.Contains("script");
    }
}
