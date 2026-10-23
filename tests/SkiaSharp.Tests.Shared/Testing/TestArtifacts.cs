using Xunit;

namespace SkiaSharp.Tests.Shared;

public static class TestArtifacts
{
	public const long AttachmentLimit = 8 * 1024 * 1024;

	public static void AddFileAttachment(this ITestContext context, string file, string mediaType) =>
		AddFileAttachment(context, file, mediaType, Path.GetFileName(file));

	public static void AddFileAttachment(this ITestContext context, string file, string mediaType, string name)
	{
		context.TestOutputHelper?.WriteLine($"Artifact: {file}");
		if (new FileInfo(file).Length > AttachmentLimit)
		{
			context.TestOutputHelper?.WriteLine(
				"Retained as a pipeline artifact rather than embedding more than 8 MiB in the test result.");
			return;
		}
		AddAttachment(context, name, File.ReadAllBytes(file), mediaType);
	}

	public static void AddImageAttachment(this ITestContext context, GoldenImageSource source, string? name = null)
	{
		if (File.Exists(source.Location))
		{
			AddFileAttachment(context, source.Location, "image/png", name ?? Path.GetFileName(source.Location));
			return;
		}

		AddAttachment(context, name ?? Path.GetFileName(source.Location), source.ReadBytes(), "image/png");
	}

	private static void AddAttachment(ITestContext context, string name, byte[] bytes, string mediaType)
	{
		var attachmentName = name;
		for (var suffix = 2; context.Attachments?.ContainsKey(attachmentName) == true; suffix++)
			attachmentName = $"{name}-{suffix}";
		context.AddAttachment(attachmentName, bytes, mediaType);
	}
}
