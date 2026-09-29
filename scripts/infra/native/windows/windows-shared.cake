var VERIFY_EXCLUDED = new[] { "VCRUNTIME", "MSVCP" };

string GetV143ToolsetVersion(string requestedVersion)
{
    if (string.IsNullOrEmpty(VS_INSTALL))
        throw new Exception("VS_INSTALL must point to a Visual Studio 2022 installation.");

    var vc = ((DirectoryPath)VS_INSTALL).Combine("VC");
    var defaultVersionFile = vc.CombineWithFilePath("Auxiliary/Build/Microsoft.VCToolsVersion.default.txt");
    if (string.IsNullOrEmpty(requestedVersion)) {
        if (!FileExists(defaultVersionFile))
            throw new Exception($"Could not find the default MSVC toolset version at {defaultVersionFile}.");
        requestedVersion = System.IO.File.ReadAllText(defaultVersionFile.FullPath).Trim();
    }

    var versions = GetDirectories($"{vc}/Tools/MSVC/*")
        .Select(path => System.IO.Path.GetFileName(path.FullPath.TrimEnd('\\', '/')))
        .Where(version => System.Version.TryParse(version, out var parsed) &&
            parsed.Major == 14 && parsed.Minor >= 30 && parsed.Minor < 50)
        .Where(version => version == requestedVersion ||
            version.StartsWith(requestedVersion + ".", StringComparison.Ordinal) ||
            (requestedVersion == "14.3" && version.StartsWith("14.3", StringComparison.Ordinal)) ||
            (requestedVersion == "14.4" && version.StartsWith("14.4", StringComparison.Ordinal)))
        .OrderByDescending(version => System.Version.Parse(version))
        .ToArray();

    if (versions.Length == 0)
        throw new Exception($"No installed VS 2022 v143 toolset matches '{requestedVersion}' in {vc}/Tools/MSVC.");

    var selected = versions[0];
    var bin = vc.Combine($"Tools/MSVC/{selected}/bin/Hostx64/x64");
    foreach (var tool in new[] { "cl.exe", "link.exe", "dumpbin.exe" }) {
        if (!FileExists(bin.CombineWithFilePath(tool)))
            throw new Exception($"Selected MSVC toolset {selected} is missing {tool} in {bin}.");
    }
    Information($"Selected VS 2022 v143 MSVC toolset {selected}.");
    return selected;
}

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

string GetSpectreLibPath(string arch, string toolsetVersion = null)
{
    var spectreArch = arch.ToLower() switch {
        "win32" => "x86",
        _ => arch.ToLower()
    };

    toolsetVersion = toolsetVersion ?? GetV143ToolsetVersion("");
    var tools = ((DirectoryPath)VS_INSTALL)
        .Combine($"VC/Tools/MSVC/{toolsetVersion}/bin/Hostx64/{spectreArch}");
    foreach (var tool in new[] { "cl.exe", "link.exe" }) {
        if (!FileExists(tools.CombineWithFilePath(tool)))
            throw new Exception($"Selected MSVC toolset {toolsetVersion} is missing {tool} for {spectreArch} in {tools}.");
    }
    var spectrePath = ((DirectoryPath)VS_INSTALL)
        .Combine($"VC/Tools/MSVC/{toolsetVersion}/lib/spectre/{spectreArch}");
    if (!DirectoryExists(spectrePath) || GetFiles($"{spectrePath}/*.lib").Count == 0)
        throw new Exception($"Selected MSVC toolset {toolsetVersion} has no Spectre libraries for {spectreArch} at {spectrePath}. Install the matching Spectre-mitigated libraries.");
    return spectrePath.FullPath;
}
