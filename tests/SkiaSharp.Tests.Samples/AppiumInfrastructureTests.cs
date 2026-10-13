using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[RunGroup(SampleRunGroup.Build)]
public sealed class AppiumInfrastructureTests
{
	[Theory]
	[InlineData("Basic/Maui/SkiaSharpSample/SkiaSharpSample.csproj", "com.companyname.SkiaSharpSample")]
	[InlineData("Gallery/Maui/SkiaSharpSample.Maui.csproj", "com.skiasharp.gallery.maui")]
	public void MauiApplicationIdComesFromProjectXml(string project, string expected)
	{
		var path = Path.Combine(Repo.RootDir, "samples", project);
		Assert.Equal(expected, MetadataProbe.ReadProjectApplicationId(path));
		Assert.Equal(expected, MetadataProbe.ReadAppleApplicationId(path));
		Assert.Equal(expected, MetadataProbe.ReadAndroidApplicationId(path));
	}

	[Fact]
	public void NativeAndroidApplicationIdComesFromSourceManifest()
	{
		var path = Path.Combine(Repo.RootDir, "samples", "Basic", "Android", "SkiaSharpSample", "SkiaSharpSample.csproj");
		Assert.Equal("com.companyname.skiasharpsample", MetadataProbe.ReadAndroidApplicationId(path));
	}

	[Theory]
	[InlineData("Basic/iOS", "com.companyname.skiasharpsample")]
	[InlineData("Basic/MacCatalyst", "com.companyname.SkiaSharpSample")]
	[InlineData("Basic/macOS", "com.companyname.skiasharpsample")]
	public void NativeAppleApplicationIdComesFromSourcePlist(string folder, string expected)
	{
		var path = Path.Combine(Repo.RootDir, "samples", folder, "SkiaSharpSample", "SkiaSharpSample.csproj");
		Assert.Null(MetadataProbe.ReadProjectApplicationId(path));
		Assert.Equal(expected, MetadataProbe.ReadAppleApplicationId(path));
	}

	[Fact]
	public void AndroidReferenceProfileUsesTheConfiguredModel()
	{
		var root = Path.Combine(Path.GetTempPath(), SampleWorkspace.CreateIdentity("appium-model"));
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
	public void WindowsAppUsesItsExecutablePathAndBuiltArchitecture()
	{
		var root = Path.Combine(Path.GetTempPath(), SampleWorkspace.CreateIdentity("appium-windows"));
		try
		{
			var inputs = Path.Combine(root, "inputs");
			Directory.CreateDirectory(Path.Combine(inputs, "Sample"));
			using var workspace = new SampleWorkspace(
				"Sample", inputs, Repo.PackagesDir,
				Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
			var executable = Path.Combine(workspace.Root, "samples", "Sample", "bin", "Sample.exe");
			var app = new WindowsRunningApp(workspace, executable, "WinAppDriver.exe", "x64");
			Assert.Equal(Path.GetFullPath(executable), app.Artifact);
			Assert.Equal(app.Artifact, app.ApplicationId);
			Assert.Equal("x64", app.DeviceProfile);
		}
		finally
		{
			SampleWorkspace.DeleteDirectory(root);
		}
	}

	[Fact]
	public async Task MissingInstalledAndroidImageFailsBeforeDeviceCreation()
	{
		var sdk = Path.Combine(Path.GetTempPath(), SampleWorkspace.CreateIdentity("appium-sdk"));
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
	public Task GoldenRetryStopsAtTheExpectedFrame() =>
		WithImages(async (actual, expected) =>
		{
			var captures = 0;
			await SampleImage.Validate(() =>
			{
				captures++;
				if (captures == 2)
				{
					Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
					File.Copy(expected, actual, overwrite: true);
				}
				return actual;
			}, expected, TimeSpan.FromSeconds(2));
			Assert.Equal(2, captures);
			Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
		});

	[Fact]
	public Task MatchingGoldenNeedsOnlyOneCapture() =>
		WithImages(async (actual, expected) =>
		{
			File.Copy(expected, actual, overwrite: true);
			var captures = 0;
			await SampleImage.Validate(() =>
			{
				captures++;
				return actual;
			}, expected, TimeSpan.FromSeconds(2));
			Assert.Equal(1, captures);
		});

	[Fact]
	public Task AsyncCaptureUsesTheSameGoldenRetry() =>
		WithImages(async (actual, expected) =>
		{
			var captures = 0;
			await SampleImage.Validate(async () =>
			{
				await Task.Yield();
				if (++captures == 2)
					File.Copy(expected, actual, overwrite: true);
				return actual;
			}, expected);
			Assert.Equal(2, captures);
		});

	[Fact]
	public Task GoldenRetryReportsTheFinalMismatch() =>
		WithImages(async (actual, expected) =>
		{
			var captures = 0;
			var error = await Assert.ThrowsAsync<Xunit.Sdk.FailException>(() =>
				SampleImage.Validate(() =>
				{
					captures++;
					return actual;
				}, expected, TimeSpan.Zero));
			Assert.Contains("Sample pixels differ", error.Message);
			Assert.Equal(1, captures);
			Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
		});

	[Fact]
	public Task MissingGoldenIsNotRetriedOrAccepted() =>
		WithImages(async (actual, expected) =>
		{
			File.Delete(expected);
			var captures = 0;
			var error = await Assert.ThrowsAnyAsync<Exception>(() =>
				SampleImage.Validate(() =>
				{
					captures++;
					return actual;
				}, expected, TimeSpan.FromSeconds(2)));
			Assert.Contains("Missing reviewed sample golden", error.Message);
			Assert.Equal(1, captures);
			Assert.True(File.Exists(actual));
			Assert.False(File.Exists(expected));
		});

	[Fact]
	public Task GoldenRetryWaitsForTheExpectedDimensions() =>
		WithImages(async (actual, expected) =>
		{
			WriteImage(actual, SKColors.White, 8);
			var captures = 0;
			await SampleImage.Validate(() =>
			{
				captures++;
				if (captures == 2)
					File.Copy(expected, actual, overwrite: true);
				return actual;
			}, expected, TimeSpan.FromSeconds(2));
			Assert.Equal(2, captures);
		});

	[Fact]
	public Task CaptureFailureIsNotRetried() =>
		WithImages(async (_, expected) =>
		{
			var captures = 0;
			var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
				SampleImage.Validate((Func<string>)(() =>
				{
					captures++;
					throw new InvalidOperationException("Capture failed.");
				}), expected, TimeSpan.FromSeconds(2)));
			Assert.Equal("Capture failed.", error.Message);
			Assert.Equal(1, captures);
		});

	private static async Task WithImages(Func<string, string, Task> test)
	{
		var root = Path.Combine(Path.GetTempPath(), SampleWorkspace.CreateIdentity("appium-golden"));
		Directory.CreateDirectory(root);
		try
		{
			var actual = Path.Combine(root, "actual.png");
			var expected = Path.Combine(root, "expected.png");
			WriteImage(actual, SKColors.Red);
			WriteImage(expected, SKColors.White);
			await test(actual, expected);
		}
		finally
		{
			SampleWorkspace.DeleteDirectory(root);
		}
	}

	private static void WriteImage(string path, SKColor color, int size = 16)
	{
		using var bitmap = new SKBitmap(size, size);
		bitmap.Erase(color);
		using var image = SKImage.FromBitmap(bitmap);
		using var data = image.Encode(SKEncodedImageFormat.Png, 100);
		File.WriteAllBytes(path, data.ToArray());
	}

	private sealed class MetadataProbe : AppiumSampleRunTestBase
	{
		internal static string? ReadProjectApplicationId(string project) => ProjectApplicationId(project);

		internal static string ReadAppleApplicationId(string project) => AppleApplicationId(project);

		internal static string ReadAndroidApplicationId(string project) => AndroidApplicationId(project);

	}
}
