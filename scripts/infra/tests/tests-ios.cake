DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "tests-shared.cake"
#load "tests-shared-apple.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// IOS TESTS — DeviceRunners execution on one owned simulator
////////////////////////////////////////////////////////////////////////////////////////////////////

string OWNED_IOS_SIMULATOR = null;

Setup(context =>
{
    if (TARGET.Equals("Default", StringComparison.OrdinalIgnoreCase) ||
        TARGET.Equals("tests-ios", StringComparison.OrdinalIgnoreCase))
        SetupIosSimulator(ref OWNED_IOS_SIMULATOR);
});

Teardown(context =>
{
    TeardownIosSimulator(ref OWNED_IOS_SIMULATOR, context.ThrownException);
});

Task ("tests-ios")
    .Description ("Run all iOS tests.")
    .Does (() =>
{
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.ios/{DATE_TIME_STR}";

    var properties = new Dictionary<string, string> {
        { "DeviceRunnersDevice", OWNED_IOS_SIMULATOR },
    };

    RunDeviceRunnersTest(csproj, results, configuration: "Debug", framework: "net10.0-ios", noBuild: SKIP_BUILD, properties: properties);
});

Task ("Default")
    .IsDependentOn ("tests-ios");

RunTarget(TARGET);
