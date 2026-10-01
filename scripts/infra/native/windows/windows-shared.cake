#addin nuget:?package=Cake.FileHelpers&version=4.0.1

var VERIFY_EXCLUDED = new[] { "VCRUNTIME", "MSVCP" };
// empty lets GN detect an installed SDK
string WINDOWS_SDK_VERSION = Argument("windowsSdkVersion", "");

string GetToolsetVersion()
{
    var build = ((DirectoryPath)VS_INSTALL).Combine("VC/Auxiliary/Build");
    var version = FileReadText(build.CombineWithFilePath("Microsoft.VCToolsVersion.default.txt")).Trim();
    var parsed = Version.Parse(version);
    if (parsed.Major != 14 || parsed.Minor < 30 || parsed.Minor >= 50)
        version = FileReadText(build.CombineWithFilePath("Microsoft.VCToolsVersion.v143.default.txt")).Trim();
    return version;
}

var TOOLSET_VERSION = new Lazy<string>(GetToolsetVersion);

void RunNinjaWithVcVars(
    DirectoryPath working,
    DirectoryPath outDir,
    string target,
    string architecture,
    string windowsSdkVersion,
    string vcVarsVersion)
{
    var vcVarsAll = ((DirectoryPath)VS_INSTALL).CombineWithFilePath("VC/Auxiliary/Build/vcvarsall.bat");

    // omitted rather than empty, as vcvarsall fails on an SDK that is not installed
    var windowsSdkVersionArg = string.IsNullOrEmpty(windowsSdkVersion)
        ? ""
        : $" {windowsSdkVersion}";
    var vcVarsVersionArg = string.IsNullOrEmpty(vcVarsVersion)
        ? ""
        : $" -vcvars_ver={vcVarsVersion}";
    var ninjaTarget = string.IsNullOrEmpty(target) ? "" : $" {target}";
    var command =
        $"call \"{vcVarsAll.FullPath}\" {architecture}{windowsSdkVersionArg}{vcVarsVersionArg}" +
        $" && \"{NINJA_EXE}\" -C \"{outDir.FullPath}\"{ninjaTarget}";

    Information($"Initializing the Visual C++ environment once for {architecture}.");
    RunProcess("cmd.exe", new ProcessSettings {
        Arguments = new ProcessArgumentBuilder()
            .Append("/d")
            .Append("/s")
            .Append("/c")
            .AppendQuoted(command),
        WorkingDirectory = working.FullPath,
    });
}

string GetSpectreLibPath(string arch)
{
    var spectreArch = arch.ToLower() switch {
        "win32" => "x86",
        _ => arch.ToLower()
    };

    var spectrePath = ((DirectoryPath)VS_INSTALL).Combine($"VC/Tools/MSVC/{TOOLSET_VERSION.Value}/lib/spectre/{spectreArch}");
    if (!DirectoryExists(spectrePath))
        throw new Exception($"Selected MSVC toolset {TOOLSET_VERSION.Value} has no Spectre libraries for {spectreArch} at {spectrePath}. Install the matching Spectre-mitigated libraries.");
    return spectrePath.FullPath;
}
