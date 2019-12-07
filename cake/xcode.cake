#addin nuget:?package=Cake.XCode&version=4.2.0

void RunXCodeBuild(FilePath project, string target, string sdk, string arch)
{
    var dir = project.GetDirectory();

    if (DirectoryExists(dir.Combine($"bin/{CONFIGURATION}/{arch}"))) {
        if (DirectoryExists(dir.Combine("build")))
            DeleteDirectory(dir.Combine("build"), true);
        MoveDirectory(dir.Combine($"bin/{CONFIGURATION}/{arch}"), dir.Combine("build"));
    }

    XCodeBuild(new XCodeBuildSettings {
        Project = project.FullPath,
        Target = target,
        Sdk = sdk,
        Arch = arch,
        Configuration = CONFIGURATION,
    });

    if (DirectoryExists(dir.Combine($"bin/{CONFIGURATION}/{arch}")))
        DeleteDirectory(dir.Combine($"bin/{CONFIGURATION}/{arch}"), true);
    EnsureDirectoryExists(dir.Combine($"bin/{CONFIGURATION}"));
    MoveDirectory(dir.Combine("build"), dir.Combine($"bin/{CONFIGURATION}/{arch}"));
}
