DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath ANGLE_PATH = ROOT_PATH.Combine("externals/angle");
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/uwp"));

string ANGLE_VERSION = GetVersion("ANGLE", "release");

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/shared/msbuild.cake"
#load "../../scripts/infra/native/windows/windows-shared.cake"
#load "../../scripts/infra/native/windows/angle-shared.cake"

// ---------------------------------------------------------------------------
// UWP-only patches
// ---------------------------------------------------------------------------

// ANGLE's IsWindowsNOrGreater() helpers call VerSetConditionMask /
// VerifyVersionInfoW. On ARM64 the Windows SDK declares those only inside the
// desktop partition, so building with target_os='winuwp' (WINAPI_FAMILY_PC_APP)
// hides them and platform_helpers.cpp fails with C3861. x86/x64 compile the
// exact same call - the declarations just survive the partition guards there.

void PatchAngleUwpPlatformHelpers(DirectoryPath anglePath)
{
    const string marker = "ANGLE_UWP_ARM64_VERSION_HELPERS";

    var helpers = anglePath.CombineWithFilePath("src/common/platform_helpers.cpp");
    if (!FileExists(helpers))
        throw new Exception($"Unable to patch {helpers}: file not found.");

    var contents = System.IO.File.ReadAllText(helpers.FullPath);

    if (contents.Contains(marker))
        return;

    // Upstream dropped the VerifyVersionInfo path - the patch is obsolete and
    // would now be a redefinition. Skip it and flag for removal.
    if (!contents.Contains("VerSetConditionMask")) {
        Warning($"{helpers} no longer references VerSetConditionMask; skipping the UWP ARM64 patch. " +
                 "Verify it is still needed and remove PatchAngleUwpPlatformHelpers if not.");
        return;
    }

    var anchor = contents.IndexOf("namespace angle");
    if (anchor < 0)
        throw new Exception($"Unable to patch {helpers}: anchor 'namespace angle' not found.");

    var fix = @"// ANGLE_UWP_ARM64_VERSION_HELPERS
//
// On ARM64 the Windows SDK declares VerSetConditionMask / VerifyVersionInfoW
// only for the desktop partition, so under WINAPI_FAMILY_PC_APP they are
// invisible here and this file fails to compile with C3861.
//
// This is compile-time visibility, not availability: VerifyVersionInfoW is
// reachable from the app partition via the api-ms-win-core-kernel32-legacy
// API set contract, which is what the UWP import library resolves it through.
// So re-declare it exactly as winbase.h would, and reimplement
// VerSetConditionMask locally - it is pure bit math over eight 3-bit slots
// (see the VER_* condition mask layout in the Win32 docs).
#if defined(_M_ARM64) && defined(WINAPI_FAMILY_PARTITION) && \
    !WINAPI_FAMILY_PARTITION(WINAPI_PARTITION_DESKTOP)

#    ifndef VER_CONDITION_MASK
#        define VER_CONDITION_MASK 0x07
#    endif
#    ifndef VER_NUM_BITS_PER_CONDITION_MASK
#        define VER_NUM_BITS_PER_CONDITION_MASK 3
#    endif

extern ""C"" WINBASEAPI BOOL WINAPI VerifyVersionInfoW(LPOSVERSIONINFOEXW lpVersionInformation,
                                                      DWORD dwTypeMask,
                                                      DWORDLONG dwlConditionMask);

static ULONGLONG VerSetConditionMask(ULONGLONG conditionMask, DWORD typeMask, BYTE condition)
{
    condition &= VER_CONDITION_MASK;
    if (typeMask == 0 || condition == 0)
        return conditionMask;

    unsigned int slot;
    if (typeMask & VER_PRODUCT_TYPE)          slot = 7;
    else if (typeMask & VER_SUITENAME)        slot = 6;
    else if (typeMask & VER_SERVICEPACKMAJOR) slot = 5;
    else if (typeMask & VER_SERVICEPACKMINOR) slot = 4;
    else if (typeMask & VER_PLATFORMID)       slot = 3;
    else if (typeMask & VER_BUILDNUMBER)      slot = 2;
    else if (typeMask & VER_MAJORVERSION)     slot = 1;
    else if (typeMask & VER_MINORVERSION)     slot = 0;
    else                                      return conditionMask;  // unknown field

    return conditionMask |
           (static_cast<ULONGLONG>(condition) << (slot * VER_NUM_BITS_PER_CONDITION_MASK));
}

#endif  // _M_ARM64 && !WINAPI_PARTITION_DESKTOP

";

    Information($"Patching {helpers} for UWP ARM64...");
    System.IO.File.WriteAllText(helpers.FullPath, contents.Insert(anchor, fix));
}

// ---------------------------------------------------------------------------
// tasks
// ---------------------------------------------------------------------------

Task("sync-ANGLE")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    // NOTE: unlike the WinUI (winui-angle) build, the UWP build does NOT use
    // the Windows App SDK. Modern .NET UWP apps use the in-box
    // Windows.UI.Xaml stack, so no WinAppSDK download or header generation is
    // needed here (angle_is_winappsdk stays false).
    SyncAngle(ANGLE_PATH, ANGLE_VERSION);

    PatchAngleUwpPlatformHelpers(ANGLE_PATH);
});

Task("ANGLE")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    foreach (var arch in new[] { "x64", "x86", "arm64" })
    {
        Build(arch, "libEGL");
        Build(arch, "libGLESv2");
    }

    void Build(string arch, string target)
    {
        if (Skip(arch)) return;

        BuildAngle(
            anglePath: ANGLE_PATH,
            outputPath: OUTPUT_PATH,
            outName: "winuwp",
            arch: arch,
            target: target,
            gnArgs: AngleGnArgs(arch,
                new [] {
                    "target_os='winuwp'",
                    "angle_is_winappsdk=false",
                }),
            verifyDependencies: false);
    }
});

Task("Default")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("ANGLE");

RunTarget(TARGET);