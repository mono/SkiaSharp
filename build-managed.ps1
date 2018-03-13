Param (
    [bool] $BuildExternals = $true,
    [bool] $BuildManaged = $true,
    [bool] $AssembleDocs = $true,
    [ValidateSet('CurrentPlatform', 'AllPlatforms', 'None')]
    [string] $PackNuGets = "CurrentPlatform"
)

$ErrorActionPreference = 'Stop'

# Prepare the script itself
. "./build-common.ps1"

# build the native bits
if ($BuildExternals) {
    . "./build-externals.ps1"
} else {
    WriteLine "$hr"
    WriteLine "Skipping the native libraries."
    WriteLine "$hr"
    WriteLine ""
}

# jump out
if (!$BuildManaged -and !$AssembleDocs -and ($PackNuGets -eq 'None')) {
    WriteLine "$hr"
    WriteLine "Skipping the managed libraries and packaging."
    WriteLine "$hr"
    WriteLine ""

    if (!$BuildExternals) {
        Write-Warning "Nothing was actually built... Maybe something is not right?"
    }

    return
}

WriteLine "$hr"
WriteLine "Building the managed libraries and packaging..."
WriteLine ""

# we need mono and MSBuild
if ($IsMacOS -or $IsLinux) {
    if (!$mono -or !(Test-Path $mono)) {
        throw 'Unable to locate "mono". Make sure mono is installed.'
    }
}
if (!$msbuild -or !(Test-Path $msbuild)) {
    throw 'Unable to locate "MSBuild.exe". Make sure Visual Studio 2017 is installed.'
}

if ($BuildManaged) {

    # build the solution
    WriteLine "Building SkiaSharp and HarfBuzzSharp (and others too)..."
    MSBuild "source/SkiaSharpSource.$operatingSystem.sln" -target "Restore"
    MSBuild "source/SkiaSharpSource.$operatingSystem.sln"
}

if ($AssembleDocs) {
    $mdoc = Join-Path $EXTERNALS_PATH "mdoc/tools/mdoc.exe"
    DownloadNuGet "mdoc" (GetVersion "mdoc" "release")
    
    # assemble the docs
    WriteLine "Assembling the docs..."
    New-Item "$OUTPUT_PATH/docs/mdoc" -itemtype "Directory" -force | Out-Null
    Exec $mdoc "assemble --out=""$OUTPUT_PATH/docs/mdoc/SkiaSharp"" ""docs/en"" --debug"
}

if ($PackNuGets -ne 'None') {
    # use the .nuspec templates to generate the real .nuspec
    (Get-ChildItem "nuget/*.nuspec") | ForEach-Object {
        [xml] $xdoc = Get-Content $_.FullName
        $meta = $xdoc.package.metadata
        $id = $meta.id
        $out = "$OUTPUT_PATH/$id/nuget"

        # remove the platform attributes
        $xdoc.package.files.file | ForEach-Object {
            $file = $_
            $plat = $file.platform
            if ($plat) {
                if (($PackNuGets -eq 'CurrentPlatform') -and ($plat.ToLower() -ne $operatingSystem.ToLower())) {
                    $xdoc.package.files.RemoveChild($file)
                } else {
                    $file.RemoveAttribute("platform")
                }
            }
            $file.SetAttribute("target", $file.src)
        }

        #  generate andsave stable
        $meta.version = (GetVersion $id)
        $meta.dependencies.dependency | ForEach-Object {
            $nv = (GetVersion $_.id)
            if ($nv) { $_.version = $nv}
        }
        $meta.dependencies.group.dependency | ForEach-Object {
            $nv = (GetVersion $_.id)
            if ($nv) { $_.version = $nv}
        }
        $xdoc.Save("$out/$id.nuspec")

        # generate and save prerelease
        $suffix = "-build-$BUILD_NUMBER"
        $meta.version = (GetVersion $id) + $suffix
        $meta.dependencies.dependency | ForEach-Object {
            $nv = (GetVersion $_.id)
            if ($nv) { $_.version = $nv + $suffix}
        }
        $meta.dependencies.group.dependency | ForEach-Object {
            $nv = (GetVersion $_.id)
            if ($nv) { $_.version = $nv + $suffix}
        }
        $xdoc.Save("$out/$id.prerelease.nuspec")

        # the legal
        Copy-Item "LICENSE.txt" "$out/LICENSE.txt"
        Copy-Item "External-Dependency-Info.txt" "$out/THIRD-PARTY-NOTICES.txt"
    } | Out-Null

    (Get-ChildItem "$OUTPUT_PATH/*/nuget/*.nuspec") | ForEach-Object {
        Exec $nuget -a "pack $($_.FullName) -BasePath ""$($_.Directory)"" -OutputDirectory ""$OUTPUT_PATH"" -Verbosity normal"
    }
}

WriteLine "Build complete."
WriteLine "$hr"
WriteLine ""
