# THIS IS JUST TEMPORARY
# We are waiting on the release of VS 2020 P5

Param (
    [string] $MSBuild = "$env:MSBUILD_EXE"
)

$ErrorActionPreference = "Stop"

$msbuildRoot = Split-Path -Parent $MSBuild

$files = @{
    'System.Text.Json' = '5.0.0.0';
    'System.Text.Encodings.Web' = '5.0.0.0';
    'System.Threading.Tasks.Extensions' = '4.2.0.1';
}

$urls = @{
    'https://globalcdn.nuget.org/packages/system.text.json.5.0.0.nupkg' = 'lib\net461\System.Text.Json.dll';
    'https://globalcdn.nuget.org/packages/system.text.encodings.web.5.0.0.nupkg' = 'lib\netstandard2.0\System.Text.Encodings.Web.dll';
    'https://globalcdn.nuget.org/packages/system.threading.tasks.extensions.4.5.4.nupkg' = 'lib\netstandard2.0\System.Threading.Tasks.Extensions.dll';
}

# backup
Write-Host "Backing up files..."
foreach ($file in $files.GetEnumerator()) {
    $p = Join-Path $msbuildRoot "$($file.Key).dll"
    if (!(Test-Path $p-old)) {
        Move-Item $p $p-old
    }
}

# replace
foreach ($url in $urls.GetEnumerator()) {
    $nupkg = Split-Path -Leaf $url.Key
    $dst = Join-Path $env:TEMP $nupkg
    $extract = "$dst-out"

    $dll = Split-Path -Leaf $url.Value
    $src = Join-Path $extract $url.Value
    $dst = Join-Path $msbuildRoot $dll

    Write-Host "Replacing file '$dll'..."

    # clean
    Remove-Item -Force -Recurse -ErrorAction Ignore $dst
    Remove-Item -Force -Recurse -ErrorAction Ignore $extract

    # download and extract
    Invoke-WebRequest -Uri $url.Key -OutFile $dst
    Expand-Archive -Path $dst -DestinationPath $extract

    # copy
    Copy-Item -Path $src -Destination $dst
}

# update config
$config = [xml](Get-Content "$MSBuild.config")
foreach ($file in $files.GetEnumerator()) {
    Write-Host "Updating config for file '$($file.Key).dll'..."

    $v = $file.Value
    $rootXpath = "/configuration/runtime/*[local-name()='assemblyBinding']/*[local-name()='dependentAssembly' and *[local-name()='assemblyIdentity' and @name='$($file.Key)']]/*[local-name()='bindingRedirect']"

    $oldVersionXpath = "$rootXpath/@oldVersion"
    $node = $config.SelectNodes($oldVersionXpath)[0]
    $node.Value = "0.0.0.0-$v"

    $newVersionXpath = "$rootXpath/@newVersion"
    $node = $config.SelectNodes($newVersionXpath)[0]
    $node.Value = $v
}
$config.Save("$MSBuild.config")