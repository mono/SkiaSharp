using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class MacRunningApp(SampleWorkspace workspace, string artifact)
	: AppiumRunningApp(workspace, artifact)
{
	internal static string HostArchitecture => RuntimeInformation.OSArchitecture switch
	{
		Architecture.Arm64 => "arm64",
		Architecture.X64 => "x64",
		_ => throw new InvalidOperationException("Mac sample tests require an arm64 or x64 host.")
	};

	internal override string DeviceProfile => HostArchitecture;

	internal override PointerKind PointerKind => PointerKind.Mouse;

	internal override Task<string> ReadApplicationId() => ReadAppleApplicationId();

	protected override Task CapturePlatformDiagnostics()
	{
		if (!HasDriver)
			return Task.CompletedTask;
		var clear = FindOptional("basic-drawing-clear");
		return clear is null ? Task.CompletedTask : SaveText("drawing-clear-state.log",
			$"enabled={clear.Enabled}; hittable={clear.GetAttribute("hittable")}; " +
			$"focused={clear.GetAttribute("focused")}; location={clear.Location}; size={clear.Size}");
	}

	internal Task RecordOpenGlPixelFormats()
	{
		Assert.True(OperatingSystem.IsMacOS(), "CGL diagnostics require macOS.");
		TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
		Cgl.CGLGetVersion(out var major, out var minor);
		var lines = new List<string> { $"CGL version {major}.{minor}; format selection only, no context or renderer changes." };
		foreach (var (name, attributes) in OpenGlPixelFormatRequests())
		{
			var status = Cgl.CGLChoosePixelFormat(attributes, out var format, out var count);
			try
			{
				lines.Add($"{name}: status={status}; formats={count}; selected={format != IntPtr.Zero}");
				if (status == CGLError.kCGLNoError && format != IntPtr.Zero)
				{
					foreach (var attribute in new[]
					{
						CGLPixelFormatAttribute.kCGLPFAAccelerated,
						CGLPixelFormatAttribute.kCGLPFASampleBuffers,
						CGLPixelFormatAttribute.kCGLPFASamples,
						CGLPixelFormatAttribute.kCGLPFAOpenGLProfile
					})
					{
						var described = CGLDescribePixelFormat(format, 0, attribute, out var value);
						lines.Add($"  {attribute}: status={described}; " +
							$"value={(described == CGLError.kCGLNoError ? value.ToString() : "not returned")}");
					}
				}
			}
			finally
			{
				if (format != IntPtr.Zero)
					Cgl.CGLReleasePixelFormat(format);
			}
		}
		return SaveText("opengl-pixel-formats.log", string.Join(Environment.NewLine, lines));
	}

	internal static (string Name, CGLPixelFormatAttribute[] Attributes)[] OpenGlPixelFormatRequests()
	{
		CGLPixelFormatAttribute[] common =
		[
			CGLPixelFormatAttribute.kCGLPFADoubleBuffer,
			CGLPixelFormatAttribute.kCGLPFAColorSize, (CGLPixelFormatAttribute)32,
			CGLPixelFormatAttribute.kCGLPFAAlphaSize, (CGLPixelFormatAttribute)8,
			CGLPixelFormatAttribute.kCGLPFADepthSize, (CGLPixelFormatAttribute)24,
			CGLPixelFormatAttribute.kCGLPFAStencilSize, (CGLPixelFormatAttribute)8
		];
		CGLPixelFormatAttribute[] multisample =
		[
			CGLPixelFormatAttribute.kCGLPFAMultisample,
			CGLPixelFormatAttribute.kCGLPFASampleBuffers, (CGLPixelFormatAttribute)1,
			CGLPixelFormatAttribute.kCGLPFASamples, (CGLPixelFormatAttribute)4
		];
		// Mirror SKGLView's request and vary one requirement at a time; these are diagnostics, not fallbacks.
		return
		[
			("view-request", [CGLPixelFormatAttribute.kCGLPFAAccelerated, .. common, .. multisample, CGLPixelFormatAttribute.kCGLPFANone]),
			("no-msaa-requirement", [CGLPixelFormatAttribute.kCGLPFAAccelerated, .. common, CGLPixelFormatAttribute.kCGLPFANone]),
			("no-acceleration-requirement", [.. common, .. multisample, CGLPixelFormatAttribute.kCGLPFANone]),
			("no-acceleration-or-msaa-requirement", [.. common, CGLPixelFormatAttribute.kCGLPFANone]),
			("core-test", [CGLPixelFormatAttribute.kCGLPFAOpenGLProfile, (CGLPixelFormatAttribute)CGLOpenGLProfile.kCGLOGLPVersion_3_2_Core,
				CGLPixelFormatAttribute.kCGLPFADoubleBuffer, CGLPixelFormatAttribute.kCGLPFANone]),
			("core-profile-view-request", [CGLPixelFormatAttribute.kCGLPFAOpenGLProfile, (CGLPixelFormatAttribute)CGLOpenGLProfile.kCGLOGLPVersion_3_2_Core,
				CGLPixelFormatAttribute.kCGLPFAAccelerated, .. common, .. multisample, CGLPixelFormatAttribute.kCGLPFANone]),
			("core-profile-no-msaa", [CGLPixelFormatAttribute.kCGLPFAOpenGLProfile, (CGLPixelFormatAttribute)CGLOpenGLProfile.kCGLOGLPVersion_3_2_Core,
				CGLPixelFormatAttribute.kCGLPFAAccelerated, .. common, CGLPixelFormatAttribute.kCGLPFANone])
		];
	}

	[DllImport("/System/Library/Frameworks/OpenGL.framework/Versions/A/OpenGL", ExactSpelling = true)]
	private static extern CGLError CGLDescribePixelFormat(
		IntPtr format, int screen, CGLPixelFormatAttribute attribute, out int value);

	internal override void SubmitSearch() =>
		Driver.ExecuteScript("macos: keys", new Dictionary<string, object>
		{
			["keys"] = new[] { "XCUIKeyboardKeyReturn" }
		});

	internal override void Drag(Point start, Point end) =>
		Driver.ExecuteScript("macos: clickAndDrag", new Dictionary<string, object>
		{
			["startX"] = start.X,
			["startY"] = start.Y,
			["endX"] = end.X,
			["endY"] = end.Y,
			["duration"] = 0.5
		});

	internal override void Scroll(Rectangle container, int amount) =>
		Driver.ExecuteScript("macos: scroll", new Dictionary<string, object>
		{
			["x"] = container.Left + container.Width / 2,
			["y"] = container.Top + container.Height / 2,
			["deltaX"] = 0,
			["deltaY"] = -amount
		});

	protected override Task Start(Uri address)
	{
		Assert.True(OperatingSystem.IsMacOS(),
			"Mac2 tests require macOS, Xcode and XCTest Accessibility/automation permissions.");

		// Mac2 launches the exact appPath. Do not let its reset terminate an existing app/WDA.
		foreach (var process in Process.GetProcesses())
		{
			using (process)
				Assert.True(process.ProcessName != Path.GetFileNameWithoutExtension(Artifact) &&
					!process.ProcessName.Contains("WebDriverAgent", StringComparison.OrdinalIgnoreCase),
					"A target app or WDA is already running; close it before selecting Mac tests.");
		}

		var options = Options("mac", "mac2");
		options.AddAdditionalAppiumOption("bundleId", ApplicationId);
		options.AddAdditionalAppiumOption("appPath", Artifact);
		options.AddAdditionalAppiumOption("noReset", false);
		options.AddAdditionalAppiumOption("skipAppKill", false);
		options.AddAdditionalAppiumOption("systemPort", AppiumFixture.GetFreePort());
		options.AddAdditionalAppiumOption("systemHost", "127.0.0.1");
		options.AddAdditionalAppiumOption("showServerLogs", true);

		SetDriver(new MacDriver(address, options, TimeSpan.FromMinutes(3)));
		Wait(() => Driver.FindElements(By.XPath("//XCUIElementTypeWindow"))
			.FirstOrDefault(window => window.Displayed && window.Size.Width > 0 && window.Size.Height > 0),
			$"{ApplicationId} desktop window");

		return Task.CompletedTask;
	}
}
