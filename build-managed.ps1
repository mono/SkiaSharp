Param (
    [bool] $BuildExternals = $true,
    [bool] $BuildManaged = $true,
    [bool] $AssembleDocs = $true
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

if (!$BuildManaged -and !$AssembleDocs) {
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

if ($BuildManaged) {
    # build the solution
    WriteLine "Building SkiaSharp and HarfBuzzSharp..."
    MSBuild "source/SkiaSharpSource.$operatingSystem.sln" -target "Restore"
    MSBuild "source/SkiaSharpSource.$operatingSystem.sln"
}

if ($AssembleDocs) {
    $mdoc = "externals/mdoc/tools/mdoc.exe"
    DownloadNuGet "mdoc" "5.5.0"
    
    # assemble the docs
    WriteLine "Assembling the docs..."
    New-Item "output/docs/mdoc" -itemtype "Directory" -force | Out-Null
    Exec $mdoc "assemble --out=""output/docs/mdoc/SkiaSharp"" ""docs/en"" --debug"
}

WriteLine "Build complete."
WriteLine "$hr"
WriteLine ""
