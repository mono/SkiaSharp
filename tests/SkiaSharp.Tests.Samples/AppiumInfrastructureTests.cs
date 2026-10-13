using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[RunGroup(SampleRunGroup.Build)]
public sealed class AppiumInfrastructureTests
{
	[Fact]
	public void AndroidReferenceProfileUsesTheConfiguredModel()
	{
		var root = Path.Combine(Path.GetTempPath(), $"appium-model-{Guid.NewGuid():N}");
		try
		{
			var inputs = Path.Combine(root, "inputs");
			Directory.CreateDirectory(Path.Combine(inputs, "Sample"));
			using var workspace = new SampleWorkspace(
				"Sample", inputs, Repo.PackagesDir,
				Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
			var app = new AndroidRunningApp(
				workspace, Path.Combine(workspace.Root, "sample.apk"), "com.skiasharp.sample");

			var model = Environment.GetEnvironmentVariable("SAMPLE_TEST_ANDROID_MODEL") ?? "pixel_9";
			var version = Environment.GetEnvironmentVariable("SAMPLE_TEST_ANDROID_VERSION") ?? "17";
			Assert.Equal($"{model}/{version}", app.DeviceProfile);
		}
		finally
		{
			SampleWorkspace.DeleteDirectory(root);
		}
	}

	[Fact]
	public async Task MissingInstalledAndroidImageFailsBeforeDeviceCreation()
	{
		var sdk = Path.Combine(Path.GetTempPath(), $"appium-sdk-{Guid.NewGuid():N}");
		try
		{
			Directory.CreateDirectory(sdk);
			var android = new Android(sdk);
			var error = await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
				android.CreateAvd("SkiaSharpSamples_missing", "system-images;android-37.1;google_apis_ps16k", "pixel_9"));
			Assert.Contains("Tests never download images", error.Message);
		}
		finally
		{
			if (Directory.Exists(sdk))
				SampleWorkspace.DeleteDirectory(sdk);
		}
	}

	[Fact]
	public void InteractionMustChangeTheSceneNotJustAnOverlay()
	{
		var root = Path.Combine(Path.GetTempPath(), $"appium-scene-{Guid.NewGuid():N}");
		Directory.CreateDirectory(root);
		try
		{
			var before = Path.Combine(root, "before.png");
			var after = Path.Combine(root, "after.png");
			using var bitmap = new SKBitmap(16, 16);
			bitmap.Erase(SKColors.White);
			Save(before);
			bitmap.SetPixel(15, 15, SKColors.Red);
			Save(after);
			var error = Assert.ThrowsAny<Exception>(() => AppiumRunningApp.AssertSceneChanged(before, after));
			Assert.Contains("rendered scene did not change", error.Message);
			bitmap.SetPixel(4, 4, SKColors.Blue);
			Save(after);
			AppiumRunningApp.AssertSceneChanged(before, after);

			void Save(string path)
			{
				using var image = SKImage.FromBitmap(bitmap);
				using var data = image.Encode(SKEncodedImageFormat.Png, 100);
				File.WriteAllBytes(path, data.ToArray());
			}
		}
		finally
		{
			SampleWorkspace.DeleteDirectory(root);
		}
	}

}
