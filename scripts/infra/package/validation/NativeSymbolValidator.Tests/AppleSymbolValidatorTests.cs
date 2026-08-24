using Xunit;

namespace SkiaSharp.PackageValidation.Tests;

public class AppleSymbolValidatorTests
{
	public static TheoryData<string, string> AllApplePackages ()
	{
		var data = new TheoryData<string, string> ();
		foreach (var spec in PackageMatrix.AppleSpecs)
			data.Add (spec.Product, spec.Platform);
		return data;
	}

	[Theory]
	[MemberData (nameof (AllApplePackages))]
	public void CorrectPackagePairPassesForEveryProductAndPlatform (string product, string platform)
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor (product, platform));

		Assert.Empty (fixture.Validate (workspace));
	}

	[Fact]
	public void DsymUuidThatDoesNotMatchTheRuntimeSliceFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "iOS"));
		var rid = fixture.FirstRid;
		var root = fixture.Spec.GetDsymRoot (rid, "arm64");

		// A dSYM built from a different link of the same source: right shape, wrong binary.
		fixture.Symbols.Add (
			$"{root}/Contents/Resources/DWARF/{fixture.Spec.DwarfFileName}",
			BinaryFixtures.MachO (BinaryFixtures.CpuTypeArm64, BinaryFixtures.NewUuid (), BinaryFixtures.MachOFileTypeDsym, totalSize: 4096));

		var errors = fixture.Validate (workspace);

		Assert.Contains (errors, e => e.Contains ("does not match the runtime", StringComparison.Ordinal));
		Assert.Contains (errors, e => e.Contains ("have no matching dSYM", StringComparison.Ordinal));
	}

	[Fact]
	public void DsymBundleWithoutInfoPlistFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("HarfBuzzSharp", "iOS"));
		var root = fixture.Spec.GetDsymRoot (fixture.FirstRid, "x86_64");

		fixture.Symbols.Remove ($"{root}/Contents/Info.plist");

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("Info.plist", StringComparison.Ordinal));
	}

	[Fact]
	public void DwarfPayloadUnderAnUnofficialNameFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "iOS"));
		var rid = fixture.FirstRid;
		var root = fixture.Spec.GetDsymRoot (rid, "arm64");
		var official = $"{root}/Contents/Resources/DWARF/{fixture.Spec.DwarfFileName}";

		// dsymutil names framework DWARF payloads after the extensionless module; a ".dylib" here
		// would be silently ignored by every consumer.
		fixture.Symbols.Add ($"{root}/Contents/Resources/DWARF/{fixture.Spec.NativeLibrary}.dylib",
			BinaryFixtures.MachO (BinaryFixtures.CpuTypeArm64, fixture.Uuid (rid, "arm64"), BinaryFixtures.MachOFileTypeDsym));
		fixture.Symbols.Remove (official);

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("missing the DWARF payload", StringComparison.Ordinal));
	}

	[Fact]
	public void MissingArchitectureDsymFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "macOS"));
		var root = fixture.Spec.GetDsymRoot (fixture.FirstRid, "x86_64");

		fixture.Symbols.Remove ($"{root}/Contents/Info.plist");
		fixture.Symbols.Remove ($"{root}/Contents/Resources/DWARF/{fixture.Spec.DwarfFileName}");

		var errors = fixture.Validate (workspace);

		Assert.Contains (errors, e => e.Contains ("missing the dSYM bundle", StringComparison.Ordinal));
		Assert.Contains (errors, e => e.Contains ("have no matching dSYM", StringComparison.Ordinal));
	}

	[Fact]
	public void ModuleMissingAnExpectedArchitectureFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "tvOS"));
		var rid = fixture.FirstRid;
		var thin = BinaryFixtures.FatMachO (
			(BinaryFixtures.CpuTypeArm64, BinaryFixtures.MachO (BinaryFixtures.CpuTypeArm64, fixture.Uuid (rid, "arm64"))));

		fixture.Normal.Add (fixture.Spec.GetModulePathInNormalPackage (rid)!, thin);
		fixture.Symbols.Add (fixture.Spec.GetModulePathInSymbolPackage (rid), thin);

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("missing expected architecture", StringComparison.Ordinal));
	}

	[Fact]
	public void DebugPayloadLeakedIntoTheCustomerPackageFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "iOS"));
		var rid = fixture.FirstRid;
		var root = fixture.Spec.GetDsymRoot (rid, "arm64");

		fixture.Normal.Add ($"{root}/Contents/Info.plist", BinaryFixtures.InfoPlist (fixture.Spec.NativeLibrary));

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("must only ship in the symbol package", StringComparison.Ordinal));
	}

	[Fact]
	public void CatalystSymbolPackageMustCarryTheUnpackedMachO ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "MacCatalyst"));

		fixture.Symbols.Remove (fixture.Spec.GetModulePathInSymbolPackage (fixture.FirstRid));

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("missing the native module", StringComparison.Ordinal));
	}

	[Fact]
	public void CatalystCustomerPackageMustNotUnpackTheFramework ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("HarfBuzzSharp", "MacCatalyst"));
		var rid = fixture.FirstRid;

		fixture.Normal.Add (
			$"runtimes/{rid}/native/{fixture.Spec.NativeLibrary}.framework/Versions/A/{fixture.Spec.NativeLibrary}",
			BinaryFixtures.MachO (BinaryFixtures.CpuTypeArm64, fixture.Uuid (rid, "arm64")));

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("unexpectedly contains an unpacked framework", StringComparison.Ordinal));
	}

	[Fact]
	public void CatalystFrameworkZipCarryingADifferentBinaryFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "MacCatalyst"));
		var rid = fixture.FirstRid;
		var spec = fixture.Spec;

		// The zip is the only Mach-O a Catalyst customer consumes, so a stale one silently ships a
		// binary that no dSYM in the symbol package can resolve.
		var stale = BinaryFixtures.Zip ((
			$"{spec.NativeLibrary}.framework/Versions/A/{spec.NativeLibrary}",
			BinaryFixtures.FatMachO (
				(BinaryFixtures.CpuTypeArm64, BinaryFixtures.MachO (BinaryFixtures.CpuTypeArm64, BinaryFixtures.NewUuid ())),
				(BinaryFixtures.CpuTypeX86_64, BinaryFixtures.MachO (BinaryFixtures.CpuTypeX86_64, BinaryFixtures.NewUuid ())))));

		fixture.Normal.Add (spec.GetFrameworkZipPath (rid)!, stale);
		fixture.Symbols.Add (spec.GetFrameworkZipPath (rid)!, stale);

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("would not resolve against these symbols", StringComparison.Ordinal));
	}

	[Fact]
	public void CatalystFrameworkZipMissingTheBinaryFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("HarfBuzzSharp", "MacCatalyst"));
		var rid = fixture.FirstRid;
		var empty = BinaryFixtures.Zip (("readme.txt", "no binary here"u8.ToArray ()));

		fixture.Normal.Add (fixture.Spec.GetFrameworkZipPath (rid)!, empty);
		fixture.Symbols.Add (fixture.Spec.GetFrameworkZipPath (rid)!, empty);

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("does not contain 'Versions/A/", StringComparison.Ordinal));
	}

	[Fact]
	public void CatalystFrameworkZipThatIsNotAZipFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "MacCatalyst"));
		var rid = fixture.FirstRid;

		fixture.Normal.Add (fixture.Spec.GetFrameworkZipPath (rid)!, "definitely not a zip archive");
		fixture.Symbols.Add (fixture.Spec.GetFrameworkZipPath (rid)!, "definitely not a zip archive");

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("is not a readable zip archive", StringComparison.Ordinal));
	}

	[Fact]
	public void TruncatedModuleWithValidMagicFailsInsteadOfCrashing ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "macOS"));
		var rid = fixture.FirstRid;

		// Valid magic and a plausible load-command count, but the commands run past the end of the
		// file: the reader must report this rather than throw out of the validator.
		var truncated = BinaryFixtures.MachO (BinaryFixtures.CpuTypeArm64, BinaryFixtures.NewUuid ())[..40];

		fixture.Normal.Add (fixture.Spec.GetModulePathInNormalPackage (rid)!, truncated);
		fixture.Symbols.Add (fixture.Spec.GetModulePathInSymbolPackage (rid), truncated);

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("not a readable Mach-O image", StringComparison.Ordinal));
	}

	[Fact]
	public void TruncatedDwarfPayloadWithValidMagicFailsInsteadOfCrashing ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("HarfBuzzSharp", "iOS"));
		var rid = fixture.FirstRid;
		var root = fixture.Spec.GetDsymRoot (rid, "arm64");

		var truncated = BinaryFixtures
			.MachO (BinaryFixtures.CpuTypeArm64, fixture.Uuid (rid, "arm64"), BinaryFixtures.MachOFileTypeDsym, totalSize: 4096)[..40];

		fixture.Symbols.Add ($"{root}/Contents/Resources/DWARF/{fixture.Spec.DwarfFileName}", truncated);

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("not a readable Mach-O image", StringComparison.Ordinal));
	}

	[Fact]
	public void ModuleWithAnUnreadableFatSliceFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "tvOS"));
		var rid = fixture.FirstRid;

		// A slice that cannot be parsed must never be dropped silently: the module would then report
		// exactly the architectures the dSYMs cover and reconcile against a truncated set.
		var damaged = BinaryFixtures.FatMachO (
			(BinaryFixtures.CpuTypeArm64, BinaryFixtures.MachO (BinaryFixtures.CpuTypeArm64, fixture.Uuid (rid, "arm64"))),
			(BinaryFixtures.CpuTypeX86_64, BinaryFixtures.MachO (BinaryFixtures.CpuTypeX86_64, fixture.Uuid (rid, "x86_64"))[..40]));

		fixture.Normal.Add (fixture.Spec.GetModulePathInNormalPackage (rid)!, damaged);
		fixture.Symbols.Add (fixture.Spec.GetModulePathInSymbolPackage (rid), damaged);

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("unreadable", StringComparison.Ordinal));
	}

	[Fact]
	public void CorruptModuleFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "iOS"));

		fixture.Symbols.Add (fixture.Spec.GetModulePathInSymbolPackage (fixture.FirstRid), new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("not a readable Mach-O image", StringComparison.Ordinal));
	}

	[Fact]
	public void FatDsymPayloadFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "iOS"));
		var rid = fixture.FirstRid;
		var root = fixture.Spec.GetDsymRoot (rid, "arm64");

		// A per-architecture dSYM must be thin; a fat one means the archive layout collapsed.
		fixture.Symbols.Add (
			$"{root}/Contents/Resources/DWARF/{fixture.Spec.DwarfFileName}",
			BinaryFixtures.FatMachO (
				(BinaryFixtures.CpuTypeArm64, BinaryFixtures.MachO (BinaryFixtures.CpuTypeArm64, fixture.Uuid (rid, "arm64"), BinaryFixtures.MachOFileTypeDsym)),
				(BinaryFixtures.CpuTypeX86_64, BinaryFixtures.MachO (BinaryFixtures.CpuTypeX86_64, fixture.Uuid (rid, "x86_64"), BinaryFixtures.MachOFileTypeDsym))));

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("must be thin", StringComparison.Ordinal));
	}

	[Fact]
	public void DwarfPayloadThatIsNotADsymFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AppleFixture (AppleFixture.SpecFor ("SkiaSharp", "macOS"));
		var rid = fixture.FirstRid;
		var root = fixture.Spec.GetDsymRoot (rid, "arm64");

		// Copying the dylib into the bundle produces the right UUID but carries no debug info.
		fixture.Symbols.Add (
			$"{root}/Contents/Resources/DWARF/{fixture.Spec.DwarfFileName}",
			BinaryFixtures.MachO (BinaryFixtures.CpuTypeArm64, fixture.Uuid (rid, "arm64"), BinaryFixtures.MachOFileTypeDylib));

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("must be", StringComparison.Ordinal));
	}
}
