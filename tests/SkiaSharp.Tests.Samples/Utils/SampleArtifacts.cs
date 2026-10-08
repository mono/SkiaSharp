using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Attaches small diagnostics to test results; larger files stay in pipeline artifacts.
internal static class SampleArtifacts
{
    internal const long AttachmentLimit = 8 * 1024 * 1024;

    public static void AddFileAttachment(this ITestContext context, string file, string mediaType)
    {
        context.TestOutputHelper?.WriteLine($"Artifact: {file}");
        if (new FileInfo(file).Length > AttachmentLimit)
        {
            context.TestOutputHelper?.WriteLine("Retained as a pipeline artifact rather than embedding more than 8 MiB in the test result.");
            return;
        }
        context.AddAttachment(Path.GetFileName(file), File.ReadAllBytes(file), mediaType);
    }
}
