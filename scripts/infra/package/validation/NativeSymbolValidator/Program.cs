using SkiaSharp.PackageValidation;

var options = ParseArguments (args);
if (options is null)
	return 2;

Console.WriteLine ("Validating native symbol packages...");
Console.WriteLine ($"  packages:        {options.PackagesDirectory}");
Console.WriteLine ($"  symbol packages: {options.SymbolPackagesDirectory}");
Console.WriteLine ($"  versions:        {options.VersionsFile}");
Console.WriteLine ($"  preview suffix:  {(string.IsNullOrEmpty (options.PreviewSuffix) ? "<none>" : options.PreviewSuffix)}");
Console.WriteLine ($"  require all:     {options.RequireAll}");
Console.WriteLine ();

var result = new Validator (options, Console.Out).Run ();

Console.WriteLine ();
Console.WriteLine ($"Validated {result.Validated} package pair(s), skipped {result.Skipped}, {result.Warnings.Count} warning(s), {result.Errors.Count} error(s).");

if (result.Succeeded) {
	Console.WriteLine ("Native symbol package validation succeeded.");
	return 0;
}

Console.Error.WriteLine ();
Console.Error.WriteLine ("Native symbol package validation FAILED:");
foreach (var error in result.Errors)
	Console.Error.WriteLine ($"  - {error}");

return 1;

static ValidatorOptions? ParseArguments (string[] args)
{
	string? packages = null;
	string? symbolPackages = null;
	string? versionsFile = null;
	string? previewSuffix = null;
	var requireAll = false;
	var verbose = false;

	for (var i = 0; i < args.Length; i++) {
		var argument = args[i];
		var separator = argument.IndexOf ('=');
		string? inlineValue = null;

		if (separator > 0) {
			inlineValue = argument[(separator + 1)..];
			argument = argument[..separator];
		}

		switch (argument) {
			case "--packages":
				packages = inlineValue ?? Next (args, ref i);
				break;
			case "--symbol-packages":
				symbolPackages = inlineValue ?? Next (args, ref i);
				break;
			case "--versions-file":
				versionsFile = inlineValue ?? Next (args, ref i);
				break;
			case "--preview-suffix":
				previewSuffix = inlineValue ?? Next (args, ref i);
				break;
			case "--require-all":
				requireAll = inlineValue is null || bool.Parse (inlineValue);
				break;
			case "--verbose":
				verbose = inlineValue is null || bool.Parse (inlineValue);
				break;
			case "--help":
			case "-h":
				WriteUsage ();
				return null;
			default:
				Console.Error.WriteLine ($"Unknown argument '{argument}'.");
				WriteUsage ();
				return null;
		}
	}

	if (packages is null || symbolPackages is null || versionsFile is null) {
		Console.Error.WriteLine ("--packages, --symbol-packages and --versions-file are all required.");
		WriteUsage ();
		return null;
	}

	if (!Directory.Exists (packages)) {
		Console.Error.WriteLine ($"The packages directory '{packages}' does not exist. Run the pack target first.");
		return null;
	}

	if (!Directory.Exists (symbolPackages)) {
		Console.Error.WriteLine ($"The symbol packages directory '{symbolPackages}' does not exist. Run the pack target first.");
		return null;
	}

	if (!File.Exists (versionsFile)) {
		Console.Error.WriteLine ($"The versions file '{versionsFile}' does not exist.");
		return null;
	}

	return new ValidatorOptions {
		PackagesDirectory = Path.GetFullPath (packages),
		SymbolPackagesDirectory = Path.GetFullPath (symbolPackages),
		VersionsFile = Path.GetFullPath (versionsFile),
		PreviewSuffix = string.IsNullOrWhiteSpace (previewSuffix) ? null : previewSuffix.Trim (),
		RequireAll = requireAll,
		Verbose = verbose,
	};

	static string Next (string[] args, ref int index)
	{
		index++;
		if (index >= args.Length)
			throw new ArgumentException ($"Missing value for '{args[index - 1]}'.");
		return args[index];
	}
}

static void WriteUsage ()
{
	Console.Error.WriteLine ();
	Console.Error.WriteLine ("Usage: NativeSymbolValidator --packages <dir> --symbol-packages <dir> --versions-file <path> [options]");
	Console.Error.WriteLine ();
	Console.Error.WriteLine ("  --packages <dir>          Directory containing the packed .nupkg files.");
	Console.Error.WriteLine ("  --symbol-packages <dir>   Directory containing the packed .symbols.nupkg files.");
	Console.Error.WriteLine ("  --versions-file <path>    Path to scripts/VERSIONS.txt.");
	Console.Error.WriteLine ("  --preview-suffix <value>  Also validate the preview packages built with this suffix.");
	Console.Error.WriteLine ("  --require-all             Fail when an expected package was not produced.");
	Console.Error.WriteLine ("  --verbose                 Print the symbol store keys for every binary.");
}
