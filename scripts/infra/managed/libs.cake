DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../native/windows/msbuild.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// LIBS — build managed assemblies
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
    .Description ("Build all managed assemblies.")
    .WithCriteria (!SKIP_BUILD)
    .Does (() =>
{
    RunDotNetBuild ($"./source/SkiaSharpSource.{CURRENT_PLATFORM}.slnf", properties: MSBUILD_VERSION_PROPERTIES);
});

RunTarget(TARGET);
