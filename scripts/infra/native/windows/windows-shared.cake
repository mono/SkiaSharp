var VERIFY_EXCLUDED = new[] { "VCRUNTIME", "MSVCP" };

void RunNinjaWithVcVars(
    DirectoryPath working,
    DirectoryPath outDir,
    string target,
    string architecture,
    string windowsSdkVersion,
    string vcVarsVersion)
{
    var vcVarsAll = ((DirectoryPath)VS_INSTALL)
        .CombineWithFilePath("VC/Auxiliary/Build/vcvarsall.bat");
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

    var spectrePaths = GetDirectories($"{VS_INSTALL}/VC/Tools/MSVC/*/lib/spectre/{spectreArch}");
    if (spectrePaths.Count == 0) {
        throw new Exception($"Could not find spectre library path for {spectreArch}, please ensure that --vsinstall is used or the envvar VS_INSTALL is set.");
    }
    return spectrePaths.First().FullPath;
}
