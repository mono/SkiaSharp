# Provision a software Vulkan ICD (SwiftShader) + the Khronos loader on the
# headless Windows CI agent so the ganesh-vulkan visual/GPU tests EXECUTE and
# golden-compare, mirroring what the Linux leg does with Mesa lavapipe.
#
# The binaries come from first-party, Apache-2.0, signed Silk.NET native NuGet
# packages, fetched by *immutable* api.nuget.org flat-container URL + pinned
# SHA-256. This is a plain HTTPS artifact download (the install-7zip.ps1 / LLVM
# convention), NOT a NuGet <PackageReference>, so the repo's curated
# dotnet-public / dotnet-eng feeds are untouched and there is no NU1101 risk.
#
#   Silk.NET.Vulkan.Loader.Native      -> vulkan-1.dll         (Khronos loader)
#   Silk.NET.Vulkan.SwiftShader.Native -> vk_swiftshader.dll   (software ICD)
#                                         vk_swiftshader_icd.json
#
# The loader is discovered by Silk.NET via the OS search path (LoadLibrary
# "vulkan-1"), so we prepend the extract dir to PATH. The ICD is discovered by
# the loader through the Windows REGISTRY -- the primary, officially-documented
# discovery path -- by writing the manifest's absolute path as a REG_DWORD=0
# value under HKLM\SOFTWARE\Khronos\Vulkan\Drivers. This is required because CI
# agents run ELEVATED, and the Khronos loader deliberately IGNORES the
# VK_ICD_FILENAMES / VK_DRIVER_FILES / VK_ADD_DRIVER_FILES environment variables
# for elevated processes (a security measure), so env-var selection alone is
# silently discarded and vkCreateInstance reports "Found no drivers!". We still
# also set those env vars as a harmless fallback for any non-elevated consumer.
# The manifest's library_path is ".\vk_swiftshader.dll" (relative to the json),
# so the json and dll are extracted into the same directory.
#
# SKIP-SAFE BY DESIGN: a missing/broken ICD makes the Vulkan cells skip (see
# tests/VulkanTests/VKTest.cs), never fail. Any error here is swallowed with a
# warning and `exit 0`, so this can only ADD coverage, never turn a leg red.

Param(
    [string] $LoaderVersion = '2025.9.12',
    [string] $SwiftShaderVersion = '2025.9.8',
    [ValidateSet('x64', 'x86')]
    [string] $Arch = 'x64'
)

# Pinned SHA-256 of each immutable .nupkg (nuget.org packages never change).
$loaderSha = '33811c05ab0bcba632ad38abba459b0d72d6e58859342e0782d011863dde07d1'
$swiftSha  = 'c65b7eaf5b4bfc3aa7ff8f0055f763a1f8898df8036bb45cc5793c6327876b17'

try {
    $ErrorActionPreference = 'Stop'

    $rid = "win-$Arch"

    $repoRoot = if ($env:BUILD_SOURCESDIRECTORY) {
        $env:BUILD_SOURCESDIRECTORY
    } else {
        (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')).Path
    }
    $dest = Join-Path $repoRoot "output\vulkan-icd\$rid"
    $tempDir = Join-Path $repoRoot "output\vulkan-icd\_download"
    New-Item -ItemType Directory -Force -Path $dest | Out-Null
    New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

    $downloadFile = Join-Path $PSScriptRoot '..\shared\download-file.ps1'

    # id (lower-case) => @{ version; sha256; entries = @{ zipEntryPath = destFileName } }
    $packages = @(
        @{
            Id      = 'silk.net.vulkan.loader.native'
            Version = $LoaderVersion
            Sha256  = $loaderSha
            Entries = @{ "runtimes/$rid/native/vulkan-1.dll" = 'vulkan-1.dll' }
        },
        @{
            Id      = 'silk.net.vulkan.swiftshader.native'
            Version = $SwiftShaderVersion
            Sha256  = $swiftSha
            Entries = @{
                "runtimes/$rid/native/vk_swiftshader.dll"      = 'vk_swiftshader.dll'
                "runtimes/$rid/native/vk_swiftshader_icd.json" = 'vk_swiftshader_icd.json'
            }
        }
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem

    foreach ($pkg in $packages) {
        $id = $pkg.Id
        $ver = $pkg.Version
        $uri = "https://api.nuget.org/v3-flatcontainer/$id/$ver/$id.$ver.nupkg"
        $nupkg = Join-Path $tempDir "$id.$ver.nupkg"

        Write-Host "Downloading $id $ver ..."
        Write-Host "  $uri"
        & $downloadFile -Uri $uri -OutFile $nupkg

        $actualSha = (Get-FileHash -Algorithm SHA256 -Path $nupkg).Hash.ToLowerInvariant()
        if ($actualSha -ne $pkg.Sha256.ToLowerInvariant()) {
            throw "SHA-256 mismatch for $id $ver`n  expected: $($pkg.Sha256)`n  actual:   $actualSha"
        }
        Write-Host "  SHA-256 OK ($actualSha)"

        $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg)
        try {
            foreach ($entryPath in $pkg.Entries.Keys) {
                $destName = $pkg.Entries[$entryPath]
                $entry = $zip.Entries | Where-Object { $_.FullName -eq $entryPath } | Select-Object -First 1
                if (-not $entry) {
                    throw "Package $id $ver is missing expected entry '$entryPath'."
                }
                $target = Join-Path $dest $destName
                [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $true)
                $len = (Get-Item $target).Length
                Write-Host "  extracted $destName ($len bytes)"
            }
        } finally {
            $zip.Dispose()
        }
    }

    $icdJson = Join-Path $dest 'vk_swiftshader_icd.json'
    $loaderDll = Join-Path $dest 'vulkan-1.dll'
    if (-not (Test-Path $icdJson))   { throw "ICD manifest not found after extraction: $icdJson" }
    if (-not (Test-Path $loaderDll)) { throw "Vulkan loader not found after extraction: $loaderDll" }

    Write-Host ""
    Write-Host "Provisioned software Vulkan ($rid) in: $dest"
    Get-ChildItem $dest | Select-Object Name, Length | Format-Table -AutoSize | Out-String | Write-Host
    Write-Host "ICD manifest contents:"
    Get-Content $icdJson | Write-Host

    # --- Register the ICD in the Windows registry (PRIMARY discovery path) --------
    # The Khronos loader IGNORES VK_ICD_FILENAMES / VK_DRIVER_FILES / VK_ADD_DRIVER_FILES
    # when the process is ELEVATED (documented security behaviour). CI agents run
    # elevated, so env-var selection is discarded and vkCreateInstance returns
    # "Found no drivers!". The registry works regardless of elevation and is the
    # officially-documented mechanism: a REG_DWORD value named with the manifest's
    # absolute path, data 0 (0 = enabled), under
    #   HKLM\SOFTWARE\Khronos\Vulkan\Drivers            (64-bit loader)
    #   HKLM\SOFTWARE\WOW6432Node\Khronos\Vulkan\Drivers (32-bit loader)
    # See KhronosGroup/Vulkan-Loader docs/LoaderDriverInterface.md ("Driver
    # Discovery on Windows" and "Exception for Elevated Privileges").
    $driversKey = if ($Arch -eq 'x86') {
        'HKLM:\SOFTWARE\WOW6432Node\Khronos\Vulkan\Drivers'
    } else {
        'HKLM:\SOFTWARE\Khronos\Vulkan\Drivers'
    }
    try {
        if (-not (Test-Path $driversKey)) {
            New-Item -Path $driversKey -Force | Out-Null
        }
        New-ItemProperty -Path $driversKey -Name $icdJson -PropertyType DWord -Value 0 -Force | Out-Null
        Write-Host "Registered ICD in registry: $driversKey"
        Write-Host "  `"$icdJson`" = dword:0"
    } catch {
        Write-Warning "Could not register the ICD in the registry (need elevation); the loader will ignore the VK_* env vars if elevated, so the Vulkan cells may skip: $($_.Exception.Message)"
    }

    # Loader discovery: Silk.NET loads "vulkan-1" by name off the OS search path.
    Write-Host "##vso[task.prependpath]$dest"
    # ICD discovery fallback: point the Vulkan loader at the SwiftShader manifest
    # for any NON-elevated consumer. Both the legacy (VK_ICD_FILENAMES) and current
    # (VK_DRIVER_FILES) variables are set so older/newer loaders both resolve it,
    # matching the Linux leg. NOTE: these are ignored by an elevated loader (see the
    # registry registration above, which is the mechanism that actually applies on CI).
    Write-Host "##vso[task.setvariable variable=VK_ICD_FILENAMES]$icdJson"
    Write-Host "##vso[task.setvariable variable=VK_DRIVER_FILES]$icdJson"
    # Make the Khronos loader explain (in the test leg's log) why it accepts or
    # rejects the ICD, so a skip is diagnosable. error,warn is concise; the loader
    # only logs around vkCreateInstance, which the Vulkan cells call and then skip.
    Write-Host "##vso[task.setvariable variable=VK_LOADER_DEBUG]error,warn"

    # --- In-step smoke test (diagnostic, never fatal) -----------------------------
    # Prove HERE (in this fast step, not buried in the 9 MB test TRX) whether the
    # loader can actually load and initialise the SwiftShader ICD. A direct
    # LoadLibrary distinguishes a missing dependency (err 126) from an architecture
    # mismatch (err 193); vkCreateInstance with VK_LOADER_DEBUG=all makes the loader
    # print the exact accept/reject reason. Wrapped so it can only inform, not fail.
    try {
        $swiftDll = Join-Path $dest 'vk_swiftshader.dll'

        Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class VkSmoke
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VkApplicationInfo {
        public int sType; public IntPtr pNext; public IntPtr pApplicationName;
        public uint applicationVersion; public IntPtr pEngineName; public uint engineVersion; public uint apiVersion;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct VkInstanceCreateInfo {
        public int sType; public IntPtr pNext; public uint flags; public IntPtr pApplicationInfo;
        public uint enabledLayerCount; public IntPtr ppEnabledLayerNames;
        public uint enabledExtensionCount; public IntPtr ppEnabledExtensionNames;
    }
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadLibraryW(string lpFileName);
    [DllImport("kernel32", SetLastError = true)]
    public static extern bool SetDllDirectory(string lpPathName);
    [DllImport("vulkan-1.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int vkCreateInstance(ref VkInstanceCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pInstance);
    [DllImport("vulkan-1.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern void vkDestroyInstance(IntPtr instance, IntPtr pAllocator);

    public static int DirectLoad(string dllPath, out int win32Error) {
        IntPtr h = LoadLibraryW(dllPath);
        win32Error = (h == IntPtr.Zero) ? Marshal.GetLastWin32Error() : 0;
        return (h == IntPtr.Zero) ? 0 : 1;
    }
    public static int CreateInstance() {
        var app = new VkApplicationInfo { sType = 0, apiVersion = (1u << 22) }; // API 1.0
        IntPtr appPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VkApplicationInfo)));
        Marshal.StructureToPtr(app, appPtr, false);
        var ci = new VkInstanceCreateInfo { sType = 1, pApplicationInfo = appPtr };
        IntPtr inst;
        int res = vkCreateInstance(ref ci, IntPtr.Zero, out inst);
        if (res == 0 && inst != IntPtr.Zero) vkDestroyInstance(inst, IntPtr.Zero);
        Marshal.FreeHGlobal(appPtr);
        return res;
    }
}
'@

        # Under elevation the registry registration above (not these env vars) is
        # what the loader uses; we still set them for a non-elevated loader and make
        # the loader maximally verbose so a skip stays diagnosable.
        $env:VK_ICD_FILENAMES = $icdJson
        $env:VK_DRIVER_FILES = $icdJson
        $env:VK_LOADER_DEBUG = 'all'
        $env:PATH = "$dest;$env:PATH"
        [VkSmoke]::SetDllDirectory($dest) | Out-Null

        Write-Host ""
        Write-Host "SMOKE: testing the SwiftShader ICD on this agent ..."

        $w32 = 0
        $loaded = [VkSmoke]::DirectLoad($swiftDll, [ref] $w32)
        if ($loaded -eq 1) {
            Write-Host "SMOKE: LoadLibrary(vk_swiftshader.dll) OK"
        } else {
            Write-Host "SMOKE: LoadLibrary(vk_swiftshader.dll) FAILED win32=$w32 (126=missing dependency DLL e.g. VC++ runtime, 193=architecture mismatch)"
        }

        try {
            $res = [VkSmoke]::CreateInstance()
            if ($res -eq 0) {
                Write-Host "SMOKE: vkCreateInstance -> 0 (VK_SUCCESS) -- the ICD works; ganesh-vulkan cells should execute."
            } else {
                Write-Host "SMOKE: vkCreateInstance -> $res (0=SUCCESS, -9=INCOMPATIBLE_DRIVER, -3=INITIALIZATION_FAILED). See the VK_LOADER_DEBUG=all lines above for the reason."
            }
        } catch {
            Write-Host "SMOKE: vkCreateInstance threw: $($_.Exception.Message)"
        }
        Write-Host "SMOKE: end of diagnostic."
        Write-Host ""
    } catch {
        Write-Warning "Vulkan smoke test could not run (diagnostic only, ignored): $($_.Exception.Message)"
    }

    Write-Host "Software Vulkan ICD provisioned; ganesh-vulkan cells should now execute."
    exit 0
} catch {
    # Never fail the leg: no ICD simply means the Vulkan cells skip.
    Write-Warning "Could not provision the software Vulkan ICD; the ganesh-vulkan tests will skip on this leg."
    Write-Warning $_.Exception.Message
    exit 0
}
