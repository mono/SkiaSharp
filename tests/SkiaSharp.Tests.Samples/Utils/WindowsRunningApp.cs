using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class WindowsRunningApp(SampleWorkspace workspace, string artifact, string architecture)
    : AppiumRunningApp(workspace, artifact)
{
    internal override string DeviceProfile { get; } = architecture;

    internal override PointerKind PointerKind => PointerKind.Mouse;

    internal override Task<string> ReadApplicationId() => Task.FromResult(Artifact);

    internal override void SubmitSearch() =>
        Driver.ExecuteScript("windows: keys", new Dictionary<string, object>
        {
            ["actions"] = new[] { new Dictionary<string, object> { ["virtualKeyCode"] = 0x0D } }
        });

    internal override void Drag(Point start, Point end) =>
        Driver.ExecuteScript("windows: clickAndDrag", new Dictionary<string, object>
        {
            ["startX"] = start.X, ["startY"] = start.Y, ["endX"] = end.X, ["endY"] = end.Y,
            ["durationMs"] = 500
        });

    protected override async Task Start(Uri address)
    {
        Assert.True(OperatingSystem.IsWindows(), "Windows tests require a Windows host.");

        AssertInteractiveDesktop();

        Assert.True(Path.GetExtension(Artifact).Equals(".exe", StringComparison.OrdinalIgnoreCase),
            "Windows sample tests require the actual unpackaged executable.");
        Assert.True(!File.Exists(Path.Combine(Path.GetDirectoryName(Artifact)!, "AppxManifest.xml")),
            "Windows sample output must be unpackaged.");
        Assert.True(File.Exists(Artifact), $"Built application executable is missing: {Artifact}");
        using var processes = JsonDocument.Parse(await PowerShell(
            $"ConvertTo-Json -InputObject @(Get-Process | Where-Object {{ $_.Path -eq {Quote(Artifact)} }}) -Compress"));
        Assert.True(processes.RootElement.GetArrayLength() == 0,
            $"The target executable is already running; refusing to relaunch or terminate it: {Artifact}");
        var options = Options("Windows", "Windows");
        options.App = Artifact;
        options.AddAdditionalAppiumOption("appWorkingDir", Path.GetDirectoryName(Artifact)!);
        options.AddAdditionalAppiumOption("systemPort", AppiumFixture.GetFreePort());
        options.AddAdditionalAppiumOption("ms:forcequit", true);
        SetDriver(new WindowsDriver(address, options, TimeSpan.FromMinutes(2)));
        Wait(() => Driver.FindElement(By.XPath("/*")) is { Displayed: true } element ? element : null, $"{Artifact} desktop window");
    }

    private Task<string> PowerShell(string script, TimeSpan? timeout = null) =>
        Command("powershell.exe", ["-NoLogo", "-NoProfile", "-NonInteractive", "-Command",
            "$ErrorActionPreference = 'Stop'; [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false); " + script],
            timeout, configure: start =>
            {
                start.StandardOutputEncoding = Encoding.UTF8;
                start.StandardErrorEncoding = Encoding.UTF8;
            });
    private static string Quote(string value) => "'" + value.Replace("'", "''") + "'";

    private void AssertInteractiveDesktop()
    {
        Assert.True(Environment.UserInteractive && Process.GetCurrentProcess().SessionId != 0,
            "Windows UI tests require a logged-in interactive desktop, not service/session 0.");
        var desktop = OpenInputDesktop(0, false, 1);
        Assert.True(desktop != IntPtr.Zero, "No accessible input desktop. Unlock and dedicate the host.");
        try
        {
            var name = new StringBuilder(256);
            Assert.True(GetUserObjectInformation(desktop, 2, name, name.Capacity * 2, out _) &&
                name.ToString().Equals("Default", StringComparison.OrdinalIgnoreCase),
                "Windows UI tests require an unlocked Default input desktop.");
        }
        finally
        {
            if (!CloseDesktop(desktop))
                throw new InvalidOperationException("Could not release the input-desktop inspection handle.");
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr OpenInputDesktop(uint flags, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint access);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseDesktop(IntPtr desktop);
    [DllImport("user32.dll", EntryPoint = "GetUserObjectInformationW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetUserObjectInformation(IntPtr handle, int index, StringBuilder value, int length, out int needed);
}
