namespace SkiaSharp.PackageValidation.Tests;

/// <summary>
/// Assembles a correct Apple package pair for a spec, so each test only has to describe the one
/// thing it breaks.
/// </summary>
public sealed class AppleFixture
{
	private readonly Dictionary<(string Rid, string Architecture), string> uuids = new ();

	public AppleFixture (ApplePackageSpec spec, string version = "3.999.0")
	{
		Spec = spec;
		Normal = new PackageBuilder (spec.PackageId, version);
		Symbols = new PackageBuilder (spec.PackageId, version);

		foreach (var rid in spec.RuntimeIdentifiers) {
			var slices = new List<(uint CpuType, byte[] Slice)> ();

			foreach (var architecture in PackageMatrix.AppleArchitectures) {
				var uuid = BinaryFixtures.NewUuid ();
				uuids[(rid, architecture)] = uuid;
				slices.Add ((CpuTypeFor (architecture), BinaryFixtures.MachO (CpuTypeFor (architecture), uuid)));

				var root = spec.GetDsymRoot (rid, architecture);
				Symbols.Add ($"{root}/Contents/Info.plist", BinaryFixtures.InfoPlist (spec.NativeLibrary));
				Symbols.Add (
					$"{root}/Contents/Resources/DWARF/{spec.DwarfFileName}",
					BinaryFixtures.MachO (CpuTypeFor (architecture), uuid, BinaryFixtures.MachOFileTypeDsym, totalSize: 4096));
			}

			var module = BinaryFixtures.FatMachO (slices.ToArray ());

			if (spec.Kind == ApplePayloadKind.CatalystFramework) {
				var zip = spec.GetFrameworkZipPath (rid)!;
				Normal.Add (zip, "framework-zip-payload");
				Symbols.Add (zip, "framework-zip-payload");
			} else {
				Normal.Add (spec.GetModulePathInNormalPackage (rid)!, module);
			}

			Symbols.Add (spec.GetModulePathInSymbolPackage (rid), module);
		}
	}

	public ApplePackageSpec Spec { get; }

	public PackageBuilder Normal { get; }

	public PackageBuilder Symbols { get; }

	public string Uuid (string rid, string architecture) => uuids[(rid, architecture)];

	public string FirstRid => Spec.RuntimeIdentifiers[0];

	public IReadOnlyList<string> Validate (TestWorkspace workspace)
	{
		var normalPath = Normal.Save (workspace.Packages);
		var symbolsPath = Symbols.Save (workspace.SymbolPackages, $"{Symbols.Id}.{Symbols.Version}.symbols.nupkg");

		using var normal = NuGetPackage.Open (normalPath);
		using var symbols = NuGetPackage.Open (symbolsPath);

		var log = new ValidationLog (TextWriter.Null);
		new AppleSymbolValidator (workspace.Extract).Validate (Spec, normal, symbols, log);
		return log.Errors;
	}

	public static uint CpuTypeFor (string architecture) => architecture switch {
		"arm64" => BinaryFixtures.CpuTypeArm64,
		"x86_64" => BinaryFixtures.CpuTypeX86_64,
		_ => throw new ArgumentOutOfRangeException (nameof (architecture)),
	};

	public static ApplePackageSpec SpecFor (string product, string platform) =>
		PackageMatrix.AppleSpecs.Single (s => s.Product == product && s.Platform == platform);
}
