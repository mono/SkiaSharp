#addin nuget:?package=Cake.FileHelpers&version=4.0.1

string WINDOWS_SDK_VERSION = Argument("windowsSdkVersion", EnvironmentVariable("WINDOWS_SDK_VERSION") ?? "10.0.28000.0");

if (string.IsNullOrEmpty(VS_INSTALL))
    throw new Exception("A Visual Studio installation is required. Pass --vsinstall or set VS_INSTALL.");
if (string.IsNullOrEmpty(WINDOWS_SDK_VERSION))
    throw new Exception("A Windows SDK version is required. Pass --windowsSdkVersion or set WINDOWS_SDK_VERSION.");

string GetToolsetVersion()
{
    var build = ((DirectoryPath)VS_INSTALL).Combine("VC/Auxiliary/Build");
    var version = FileReadText(build.CombineWithFilePath("Microsoft.VCToolsVersion.default.txt")).Trim();
    var parsed = Version.Parse(version);
    if (parsed.Major != 14 || parsed.Minor < 50)
        throw new Exception($"Visual Studio 2026 MSVC v145 or newer is required, found {version}.");
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

    var vcVarsVersionArg = string.IsNullOrEmpty(vcVarsVersion)
        ? ""
        : $" -vcvars_ver={vcVarsVersion}";
    var ninjaTarget = string.IsNullOrEmpty(target) ? "" : $" {target}";
    var command =
        $"call \"{vcVarsAll.FullPath}\" {architecture} {windowsSdkVersion}{vcVarsVersionArg}" +
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
