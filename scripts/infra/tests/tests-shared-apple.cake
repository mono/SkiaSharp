////////////////////////////////////////////////////////////////////////////////////////////////////
// IOS SIMULATOR — explicit lifecycle for the UUID owned by this Cake run
////////////////////////////////////////////////////////////////////////////////////////////////////

var TEST_IOS_SIMULATOR_MODEL = Argument("iosModel", EnvironmentVariable("TEST_IOS_SIMULATOR_MODEL") ?? "iPhone 16");
var TEST_IOS_SIMULATOR_VERSION = Argument("iosVersion", EnvironmentVariable("TEST_IOS_SIMULATOR_VERSION") ?? "");

void SetupIosSimulator(ref string ownedSimulator)
{
    if (!IsRunningOnMacOs())
        return;

    if (IsCakeListingCommand())
        return;

    if (!string.IsNullOrEmpty(ownedSimulator))
        throw new InvalidOperationException("This Cake run already owns an iOS simulator.");

    var name = $"SkiaSharp-Tests-{Guid.NewGuid():N}";
    var arguments = new ProcessArgumentBuilder()
        .Append("apple simulator create")
        .AppendQuoted(name)
        .Append("--device-type").AppendQuoted(TEST_IOS_SIMULATOR_MODEL);

    if (!string.IsNullOrEmpty(TEST_IOS_SIMULATOR_VERSION) &&
        !TEST_IOS_SIMULATOR_VERSION.Equals("latest", StringComparison.OrdinalIgnoreCase)) {
        if (!Version.TryParse(TEST_IOS_SIMULATOR_VERSION, out var requestedVersion))
            throw new ArgumentException($"Invalid iOS simulator version: {TEST_IOS_SIMULATOR_VERSION}");

        // CoreSimulator runtime identifiers use major/minor even when the
        // runtime reports an exact patch version such as 26.3.1.
        arguments.Append("--runtime").AppendQuoted(
            $"com.apple.CoreSimulator.SimRuntime.iOS-{requestedVersion.Major}-{requestedVersion.Minor}");
    }

    arguments.Append("--format json");

    try {
        var settings = new ProcessSettings {
            Arguments = arguments,
            EnvironmentVariables = new Dictionary<string, string> {
                ["NO_COLOR"] = "1",
                ["SPECTRE_ANSI"] = "0",
                ["DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION"] = "0",
            },
        };
        RunProcess("dotnet", settings, out var stdout);
        var output = string.Join(Environment.NewLine, stdout);
        Information("{0}", output);

        // The pinned Apple tool can print a failure message while returning exit code zero.
        if (!output.TrimStart().StartsWith("{", StringComparison.Ordinal))
            throw new Exception($"Apple simulator creation did not return JSON: {output}");

        using var json = System.Text.Json.JsonDocument.Parse(output);
        var udid = json.RootElement.GetProperty("udid").GetString();
        if (!Guid.TryParse(udid, out _))
            throw new Exception($"Apple tool returned an invalid simulator UUID: {udid}");

        // Record ownership before reading metadata or booting so either failure removes this device.
        ownedSimulator = udid;

        var version = json.RootElement.GetProperty("runtime").GetProperty("version").GetString();
        if (!Version.TryParse(version, out _))
            throw new Exception($"Apple tool returned an invalid simulator runtime version: {version}");

        TEST_IOS_SIMULATOR_VERSION = version;
        Information("Booting owned iOS simulator: {0} (iOS {1})", ownedSimulator, version);
        DotNetTool($"apple simulator boot \"{ownedSimulator}\" --wait");
    } catch (Exception error) {
        TeardownIosSimulator(ref ownedSimulator, error);
        throw;
    }
}

void TeardownIosSimulator(ref string ownedSimulator, Exception originalError)
{
    if (!IsRunningOnMacOs() || string.IsNullOrEmpty(ownedSimulator))
        return;

    var failures = new List<Exception>();

    try {
        DotNetTool($"apple simulator shutdown \"{ownedSimulator}\"");
    } catch (Exception error) {
        failures.Add(error);
    }

    // Deletion must still run after a failed boot or shutdown, and only targets our UUID.
    try {
        DotNetTool($"apple simulator delete \"{ownedSimulator}\"");
        ownedSimulator = null;
    } catch (Exception error) {
        failures.Add(error);
    }

    if (failures.Count != 0) {
        if (originalError != null)
            failures.Insert(0, originalError);
        throw new AggregateException("Owned iOS simulator cleanup failed.", failures);
    }
}
