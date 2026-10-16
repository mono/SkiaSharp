////////////////////////////////////////////////////////////////////////////////////////////////////
// IOS SIMULATOR — explicit lifecycle for the UUID owned by this Cake run
////////////////////////////////////////////////////////////////////////////////////////////////////

void SetupIosSimulator(ref string ownedSimulator, string model, string version)
{
    // Cake 4 still invokes Setup/Teardown in a dry run and does not set IsDryRun().
    var dryRunOptions = new[] { "--dryrun", "--noop", "--whatif" };
    if (System.Environment.GetCommandLineArgs().Any(arg =>
        dryRunOptions.Contains(arg.Split('=')[0], StringComparer.OrdinalIgnoreCase)))
        return;

    if (!string.IsNullOrEmpty(ownedSimulator))
        throw new InvalidOperationException("This Cake run already owns an iOS simulator.");

    var name = $"SkiaSharp-Tests-{Guid.NewGuid():N}";
    var arguments = new ProcessArgumentBuilder()
        .Append("apple simulator create")
        .AppendQuoted(name)
        .Append("--device-type").AppendQuoted(model);

    if (!string.IsNullOrEmpty(version))
        arguments.Append("--runtime").AppendQuoted($"iOS {version}");

    arguments.Append("--format json");

    try {
        var settings = new ProcessSettings {
            Arguments = arguments,
            RedirectStandardOutput = true,
            EnvironmentVariables = new Dictionary<string, string> {
                ["NO_COLOR"] = "1",
                ["SPECTRE_ANSI"] = "0",
                ["DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION"] = "0",
            },
        };
        var exitCode = StartProcess("dotnet", settings, out var stdout);
        var output = string.Join(Environment.NewLine, stdout);
        Information("{0}", output);

        if (exitCode != 0)
            throw new Exception($"Apple simulator creation failed with exit code {exitCode}.");

        using var json = System.Text.Json.JsonDocument.Parse(output);
        var udid = json.RootElement.GetProperty("udid").GetString();
        if (!Guid.TryParse(udid, out _))
            throw new Exception($"Apple tool returned an invalid simulator UUID: {udid}");

        // Record ownership before booting so a failed boot still removes this device.
        ownedSimulator = udid;
        Information("Booting owned iOS simulator: {0}", ownedSimulator);
        DotNetTool($"apple simulator boot \"{ownedSimulator}\" --wait");
    } catch (Exception error) {
        TeardownIosSimulator(ref ownedSimulator, error);
        throw;
    }
}

void TeardownIosSimulator(ref string ownedSimulator, Exception originalError)
{
    if (string.IsNullOrEmpty(ownedSimulator))
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

    if (failures.Count == 0)
        return;

    var failure = new AggregateException("Owned iOS simulator cleanup failed.", failures);
    if (originalError != null)
        Error(failure);
    else
        throw failure;
}
