
internal static class MacPlatformDetector
{
    internal static readonly Lazy<bool> IsMac = new Lazy<bool> (IsRunningOnMac);

    [DllImport ("libc")]
    static extern int uname (IntPtr buf);

    static bool IsRunningOnMac ()
    {
        IntPtr buf = IntPtr.Zero;
        try {
            buf = Marshal.AllocHGlobal (8192);
            // This is a hacktastic way of getting sysname from uname ()
            if (uname (buf) == 0) {
                string os = Marshal.PtrToStringAnsi (buf);
                if (os == "Darwin")
                    return true;
            }
        } catch {
        } finally {
            if (buf != IntPtr.Zero)
                Marshal.FreeHGlobal (buf);
        }
        return false;
    }
}

bool IsRunningOnMac ()
{
    return System.Environment.OSVersion.Platform == PlatformID.MacOSX || MacPlatformDetector.IsMac.Value;
}

bool IsRunningOnLinux ()
{
    return IsRunningOnUnix () && !IsRunningOnMac ();
}

string GetMSBuildToolPath (string possible)
{
    if (string.IsNullOrEmpty (possible)) {
        if (IsRunningOnLinux ()) {
            possible = "/usr/bin/msbuild";
        } else if (IsRunningOnMac ()) {
            possible = "/Library/Frameworks/Mono.framework/Versions/Current/Commands/msbuild";
        } else if (IsRunningOnWindows ()) {
            possible = null; // use the default
        }
    }
    return possible;
}

string GetVersion (string lib, string type = "nuget")
{
    try {
        var contents = FileReadText ("./VERSIONS.txt");
        var match = Regex.Match(contents, $@"^{lib}\s*{type}\s*(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        return match.Groups[1].Value.Trim();
    } catch {
        return "";
    }
}
