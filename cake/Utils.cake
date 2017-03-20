
var SetEnvironmentVariable = new Action<string, string> ((name, value) => {
    Information ("Setting Environment Variable {0} to {1}", name, value);
    Environment.SetEnvironmentVariable (name, value, EnvironmentVariableTarget.Process);
});

var AppendEnvironmentVariable = new Action<string, string> ((name, value) => {
    var old = EnvironmentVariable (name);
    var sep = IsRunningOnWindows () ? ';' : ':';
    
    if (!old.ToUpper ().Split (sep).Contains (value.ToUpper ())) {
        Information ("Adding {0} to Environment Variable {1}", value, name);
        value += sep + old;
        SetEnvironmentVariable (name, value);
    }
});

void ListEnvironmentVariables ()
{
    Information ("Environment Variables:");
    foreach (var envVar in EnvironmentVariables ()) {
        Information ("\tKey: {0}\tValue: \"{1}\"", envVar.Key, envVar.Value);
    }
}

FilePath GetToolPath (FilePath toolPath)
{
    var appRoot = Context.Environment.GetApplicationRoot ();
     var appRootExe = appRoot.Combine ("..").CombineWithFilePath (toolPath);
     if (FileExists (appRootExe))
         return appRootExe;
    throw new FileNotFoundException ("Unable to find tool: " + appRootExe); 
}

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

FilePath GetSNToolPath (string possible)
{
    if (string.IsNullOrEmpty (possible)) {
        if (IsRunningOnLinux ()) {
            possible = "/usr/lib/mono/4.5/sn.exe";
        } else if (IsRunningOnMac ()) {
            possible = "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5/sn.exe";
        } else if (IsRunningOnWindows ()) {
            // search through all the SDKs to find the latest
            var snExes = new List<string> ();
            var arch = Environment.Is64BitOperatingSystem ? "x64" : "";
            var progFiles = (DirectoryPath)Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
            var dirPath = progFiles.Combine ("Microsoft SDKs/Windows").FullPath + "/v*A";
            var dirs = GetDirectories (dirPath).OrderBy (d => {
                var version = d.GetDirectoryName ();
                return double.Parse (version.Substring (1, version.Length - 2));
            });
            foreach (var dir in dirs) {
                var path = dir.FullPath + "/bin/*/" + arch + "/sn.exe";
                var files = GetFiles (path).Select (p => p.FullPath).ToList ();
                files.Sort ();
                snExes.AddRange (files);
            }

            possible = snExes.LastOrDefault ();
        }
    }
    return possible;
}