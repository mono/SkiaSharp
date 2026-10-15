using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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
    private Process? ownedProcess;
    private OwnedProcessIdentity? ownedProcessIdentity;
    private DateTime? launchAttemptedAtUtc;
    private bool prelaunchGuardWasEmpty;

    private sealed record OwnedProcessIdentity(int ProcessId, DateTime StartTimeUtc, string WindowHandle, string Executable);

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

    internal override void Scroll(Rectangle container, int amount) =>
        Driver.ExecuteScript("windows: scroll", new Dictionary<string, object>
        {
            ["x"] = container.Left + container.Width / 2,
            ["y"] = container.Top + container.Height / 2,
            // Windows driver deltas are wheel notches, not pixels. Recheck geometry after each notch.
            ["deltaY"] = -Math.Sign(amount)
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
        prelaunchGuardWasEmpty = true;
        Own(ReleaseOwnedProcess);
        var options = Options("Windows", "Windows");
        options.App = Artifact;
        options.AddAdditionalAppiumOption("appWorkingDir", Path.GetDirectoryName(Artifact)!);
        options.AddAdditionalAppiumOption("systemPort", AppiumFixture.GetFreePort());
        options.AddAdditionalAppiumOption("ms:forcequit", true);
        launchAttemptedAtUtc = DateTime.UtcNow;
        SetDriver(new WindowsDriver(address, options, TimeSpan.FromMinutes(2)));
        AdoptSessionProcess(Driver.CurrentWindowHandle);
        await SaveOwnedProcess("owned-process-started.json", "verified-session-process");
        Wait(() => Driver.FindElement(By.XPath("/*")) is { Displayed: true } element ? element : null, $"{Artifact} desktop window");
    }

    private void AdoptSessionProcess(string windowHandle)
    {
        if (!prelaunchGuardWasEmpty)
            throw new InvalidOperationException("Windows process ownership requires an empty prelaunch artifact guard.");
        var digits = windowHandle.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? windowHandle[2..] : windowHandle;
        if (!ulong.TryParse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var value) || value == 0)
            throw new InvalidOperationException($"The owned Windows session returned an invalid native window handle: {windowHandle}.");
        var window = checked((nint)value);
        if (GetWindowThreadProcessId(window, out var processId) == 0 || processId == 0)
            throw new InvalidOperationException($"Could not resolve session window {windowHandle} to a process (Win32 error {Marshal.GetLastWin32Error()}).");

        var candidate = Process.GetProcessById(checked((int)processId));
        try
        {
            // Retain the native process handle so cleanup cannot target a reused PID.
            _ = candidate.Handle;
            var executable = candidate.MainModule?.FileName;
            if (executable is null || !Path.GetFullPath(executable).Equals(Artifact, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Session window {windowHandle} belongs to PID {processId} at '{executable}', not the unique artifact '{Artifact}'.");
            var started = candidate.StartTime.ToUniversalTime();
            if (launchAttemptedAtUtc is not { } attempted || started < attempted)
                throw new InvalidOperationException($"Session PID {processId} was not started after our launch attempt: start={started:O}, attempt={launchAttemptedAtUtc:O}.");
            if (GetWindowThreadProcessId(window, out var currentProcessId) == 0 || currentProcessId != processId)
                throw new InvalidOperationException($"Session window {windowHandle} changed process identity before ownership could be established.");
            ownedProcessIdentity = new(candidate.Id, started, windowHandle, executable);
            ownedProcess = candidate;
        }
        finally
        {
            if (ownedProcess != candidate)
                candidate.Dispose();
        }
    }

    protected override Task CapturePlatformDiagnostics() =>
        SaveOwnedProcess("owned-process-before-cleanup.json", "before-driver-cleanup");

    private Task SaveOwnedProcess(string name, string phase, bool terminationAttempted = false) =>
        SaveText(name, JsonSerializer.Serialize(new
        {
            Phase = phase,
            Artifact,
            PrelaunchGuardWasEmpty = prelaunchGuardWasEmpty,
            LaunchAttemptedAtUtc = launchAttemptedAtUtc,
            Identity = ownedProcessIdentity,
            HasExited = ownedProcess?.HasExited,
            TerminationAttempted = terminationAttempted,
            OwnershipUnverified = launchAttemptedAtUtc is not null && ownedProcessIdentity is null
        }, new JsonSerializerOptions { WriteIndented = true }), "application/json");

    private async Task ReleaseOwnedProcess()
    {
        if (ownedProcess is not { } process)
        {
            await SaveOwnedProcess("owned-process-after-cleanup.json", "no-verified-process-no-termination");
            return;
        }
        List<Exception> failures = [];
        var terminationAttempted = false;
        try
        {
            await Attempt(failures, async () =>
            {
                if (!process.HasExited)
                {
                    terminationAttempted = true;
                    try
                    {
                        process.Kill();
                    }
                    catch (InvalidOperationException) when (process.HasExited) { }
                    catch (Win32Exception) when (process.HasExited) { }
                }
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                try
                {
                    await process.WaitForExitAsync(timeout.Token);
                }
                catch (OperationCanceledException error) when (timeout.IsCancellationRequested)
                {
                    throw new TimeoutException($"Owned Windows PID {ownedProcessIdentity!.ProcessId} did not exit within 30 seconds.", error);
                }
            });
            await Attempt(failures, () => SaveOwnedProcess("owned-process-after-cleanup.json",
                "after-driver-and-server-cleanup", terminationAttempted));
            ThrowFailures(failures);
        }
        finally
        {
            process.Dispose();
            ownedProcess = null;
        }
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
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr OpenInputDesktop(uint flags, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint access);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseDesktop(IntPtr desktop);
    [DllImport("user32.dll", EntryPoint = "GetUserObjectInformationW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetUserObjectInformation(IntPtr handle, int index, StringBuilder value, int length, out int needed);
}
