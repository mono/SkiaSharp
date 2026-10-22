DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// ONEFUZZ - prepare validation drop
////////////////////////////////////////////////////////////////////////////////////////////////////

var ONEFUZZ_PLATFORM = Argument("platform", EnvironmentVariable("ONEFUZZ_PLATFORM"));
var ONEFUZZ_OUTPUT_DIRECTORY = Argument("outputDirectory", EnvironmentVariable("ONEFUZZ_OUTPUT_DIRECTORY"));
var ONEFUZZ_SHARPFUZZ_PATH = Argument("sharpFuzzPath", EnvironmentVariable("ONEFUZZ_SHARPFUZZ_PATH"));
var ONEFUZZ_CONFIGURATION = Argument("configuration", EnvironmentVariable("ONEFUZZ_CONFIGURATION") ?? "Release");
var ONEFUZZ_VALIDATION_BUILD_ID = Argument("validationBuildId", EnvironmentVariable("ONEFUZZ_VALIDATION_BUILD_ID"));

Task("onefuzz-prepare")
    .Description("Prepare the OneFuzz harness drop for Windows or Ubuntu validation runs.")
    .Does(() =>
{
    if (string.IsNullOrWhiteSpace(ONEFUZZ_PLATFORM))
        throw new Exception("The 'platform' argument is required (windows|ubuntu).");
    if (string.IsNullOrWhiteSpace(ONEFUZZ_OUTPUT_DIRECTORY))
        throw new Exception("The 'outputDirectory' argument is required.");
    if (string.IsNullOrWhiteSpace(ONEFUZZ_SHARPFUZZ_PATH))
        throw new Exception("The 'sharpFuzzPath' argument is required.");

    var scriptPath = ROOT_PATH.CombineWithFilePath("scripts/infra/security/prepare-onefuzz-drop.ps1");
    string Quoted(string value) => $"\"{value.Replace("\"", "\\\"")}\"";
    var args = new List<string>
    {
        "-NoLogo",
        "-NoProfile",
        "-File",
        Quoted(scriptPath.FullPath),
        "-Platform",
        Quoted(ONEFUZZ_PLATFORM),
        "-OutputDirectory",
        Quoted(ONEFUZZ_OUTPUT_DIRECTORY),
        "-SharpFuzzPath",
        Quoted(ONEFUZZ_SHARPFUZZ_PATH),
        "-Configuration",
        Quoted(ONEFUZZ_CONFIGURATION),
    };

    if (!string.IsNullOrWhiteSpace(ONEFUZZ_VALIDATION_BUILD_ID))
    {
        args.Add("-ValidationBuildId");
        args.Add(Quoted(ONEFUZZ_VALIDATION_BUILD_ID));
    }

    RunProcess("pwsh", string.Join(" ", args));
});

RunTarget(TARGET);
