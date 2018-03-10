
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$powershellVersion = "$($PSVersionTable.PSVersion.ToString()) ($($PSVersionTable.PSEdition.ToString()))"
$operatingSystem = if ($IsMacOS) { 'Mac' } elseif ($IsLinux) { 'Linux' } else { 'Windows' }

$HOME_PATH = $(if ($IsMacOS -or $IsLinux) { $env:HOME } else { $env:USERPROFILE })

$nuget = "externals/nuget/nuget.exe"
$msbuild = ''
$msbuildVersion = ''

$hr = "=" * 80

Function WriteLine ([string] $message, [string] $color = "Cyan") {
    if ($host.UI.RawUI.ForegroundColor -eq $color) {
        Write-Output $message
    } else {
        $fc = $host.UI.RawUI.ForegroundColor
        $host.UI.RawUI.ForegroundColor = $color
        Write-Output $message
        $host.UI.RawUI.ForegroundColor = $fc
    }
}

Function CopyDirectoryContents ([string] $src, [string] $dest) {
    New-Item $dest -itemtype "Directory" -force | Out-Null
    Get-ChildItem $src -Directory | ForEach-Object {
        Copy-Item -literalpath "$src/$_" $dest -force -recurse | Out-Null
    }
}

Function Exec ([string] $file, [string[]] $a, [string] $wo) {
    if (!$wo) {
        $wo = "."
    }
    $process = Start-Process $file -args $a -wo $wo -nnw -wait -passthru
    if ($process.ExitCode -ne 0) {
        throw "Process '$file' exited with error code '$($process.ExitCode.ToString())'." 
    }
}

Function FindTool ([string] $tool) {
    if ($IsMacOS -or $IsLinux) {
        return & 'which' $tool
    } else {
        $where = "$env:SystemRoot\system32\where.exe"
        & $where /Q $tool
        if ($?) {
            return & $where $tool
        }
    }
    return $null
}

Function DownloadNuGet ([string] $id, [string] $version) {
    # download nuget.exe
    if (!(Test-Path $nuget)) {
        WriteLine "Downloading nuget.exe..."
        $dir = Split-Path $nuget
        New-Item $dir -itemtype "Directory" -force | Out-Null
        Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -out $nuget
    }

    # download the specific nuget
    if (!(Test-Path "externals/$id/$id.nupkg")) {
        WriteLine "Downloading $id..."
        Exec $nuget -a "install $id -Version ""$version"" -ExcludeVersion -OutputDirectory ""externals"" -Verbosity quiet"
    }
}

Function MSBuild ([string] $project, [string] $arch = "Any CPU", [string] $config = "Release", [string] $target = "Build") {
    # run MSBuild
    $v = if ($target -eq "Build") { "minimal" } else { "quiet" }
    Exec $msbuild -a """$project"" /p:Configuration=""$config"" /p:Platform=""$arch"" /t:""$target"" /v:$v /m /nologo"
}

Function GetVersion ([string]$lib, [string]$type = "nuget") {
    $matches = (Get-Content "versions.txt" | Select-String -pattern "^$lib\s*$type\s*(.*)$")
    return $matches.Matches[0].Groups[1].Value
}


# find vswhere
if (!$IsMacOS -and !$IsLinux) {
    $vswhereTemp = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if ($vswhereTemp -and (Test-Path $vswhereTemp)) {
        $vswhere = $vswhereTemp
    }
}

# find MSBuild
$msbuild = FindTool 'msbuild'
if (!$msbuild) {
    if ($IsMacOS -or $IsLinux) {
        # TODO: try find msbuild
    } else {
        if ($vswhere) {
            $msbuildRoot = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
            if ($msbuildRoot -and (Test-Path $msbuildRoot)) {
                $msbuild = Join-Path $msbuildRoot 'MSBuild\15.0\Bin\MSBuild.exe'
            }
        }
    }
}

# get the MSBuild version
$msbuildVersion = if ($msbuild) { & $msbuild -version -nologo }
