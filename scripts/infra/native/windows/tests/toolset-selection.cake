var VS_INSTALL = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "skiasharp-v143-" + Guid.NewGuid().ToString("N"));
var NINJA_EXE = "ninja";

#load "../windows-shared.cake"

void RunProcess(FilePath process, ProcessSettings settings) =>
    throw new Exception("The selection tests must not launch Windows tools.");

Task("Default").Does(() =>
{
    var vc = System.IO.Path.Combine(VS_INSTALL, "VC");
    var versions = new[] { "14.29.30133", "14.39.33519", "14.40.33807", "14.44.35207" };

    try {
        foreach (var version in versions) {
            var bin = System.IO.Path.Combine(vc, "Tools", "MSVC", version, "bin", "Hostx64", "x64");
            System.IO.Directory.CreateDirectory(bin);
            foreach (var tool in new[] { "cl.exe", "link.exe", "dumpbin.exe" })
                System.IO.File.WriteAllText(System.IO.Path.Combine(bin, tool), "");
        }

        var defaultFile = System.IO.Path.Combine(vc, "Auxiliary", "Build", "Microsoft.VCToolsVersion.default.txt");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(defaultFile));
        System.IO.File.WriteAllText(defaultFile, "14.44.35207\r\n");

        if (GetV143ToolsetVersion("") != "14.44.35207" ||
            GetV143ToolsetVersion("14.3") != "14.39.33519" ||
            GetV143ToolsetVersion("14.4") != "14.44.35207" ||
            GetV143ToolsetVersion("14.44.35207") != "14.44.35207")
            throw new Exception("The selected v143 toolset did not match the requested/default installation.");

        foreach (var arch in new[] { "x86", "x64", "arm64" }) {
            var bin = System.IO.Path.Combine(vc, "Tools", "MSVC", "14.44.35207", "bin", "Hostx64", arch);
            System.IO.Directory.CreateDirectory(bin);
            foreach (var tool in new[] { "cl.exe", "link.exe" })
                System.IO.File.WriteAllText(System.IO.Path.Combine(bin, tool), "");
            var spectre = System.IO.Path.Combine(vc, "Tools", "MSVC", "14.44.35207", "lib", "spectre", arch);
            System.IO.Directory.CreateDirectory(spectre);
            System.IO.File.WriteAllText(System.IO.Path.Combine(spectre, "libcmt.lib"), "");
            var requestedArch = arch == "x86" ? "Win32" : arch;
            if (GetSpectreLibPath(requestedArch, "14.44.35207") != spectre)
                throw new Exception($"Spectre libraries did not match the selected version for {arch}.");
        }

        AssertFails(() => GetV143ToolsetVersion("14.2"));
        AssertFails(() => GetV143ToolsetVersion("14.50"));
        AssertFails(() => GetSpectreLibPath("ARM64", "14.39.33519"));
        System.IO.File.Delete(System.IO.Path.Combine(vc, "Tools", "MSVC", "14.44.35207", "bin", "Hostx64", "arm64", "link.exe"));
        AssertFails(() => GetSpectreLibPath("ARM64", "14.44.35207"));
        System.IO.File.Delete(System.IO.Path.Combine(vc, "Tools", "MSVC", "14.44.35207", "bin", "Hostx64", "x64", "link.exe"));
        AssertFails(() => GetV143ToolsetVersion(""));
    }
    finally {
        System.IO.Directory.Delete(VS_INSTALL, true);
    }
});

void AssertFails(Action action)
{
    try {
        action();
    }
    catch (Exception) {
        return;
    }
    throw new Exception("Expected toolset selection to reject an invalid or incomplete installation.");
}

RunTarget("Default");
