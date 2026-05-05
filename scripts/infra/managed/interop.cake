DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../native/shared/native-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// INTEROP — re-generate the P/Invoke binding files
////////////////////////////////////////////////////////////////////////////////////////////////////

Task("externals-interop")
    .IsDependentOn("git-sync-deps")
    .Does(() =>
{
    RunProcess("pwsh", $"{ROOT_PATH}/utils/generate.ps1");

    RunProcess("git", "diff --name-only binding/*/*.generated.cs", out var files);

    if (files.Any()) {
        Information("Generated files have changed:");
        foreach (var file in files) {
            Information($" - {file}");
        }

        if (Argument("validateInterop", false)) {
            throw new Exception("Generated interop files are out of date. Please run `pwsh ./utils/generate.ps1`.");
        } else {
            Warning("Generated interop files are out of date. Please run `pwsh ./utils/generate.ps1`.");
            Warning("##vso[task.logissue type=warning]Generated interop files are out of date. Please run `pwsh ./utils/generate.ps1`.");
        }
    }
});

Task ("Default")
    .IsDependentOn ("externals-interop");

RunTarget(TARGET);
