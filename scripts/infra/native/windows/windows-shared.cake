var VERIFY_EXCLUDED = new[] { "VCRUNTIME", "MSVCP" };

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
