$ErrorActionPreference = 'Stop'

New-Item -ItemType Directory "C:\tizen-data" -Force | Out-Null

# install OpenJDK

$jdkUri = "https://download.java.net/java/GA/jdk13.0.2/d4173c853231432d94f001e99d882ca7/8/GPL/openjdk-13.0.2_windows-x64_bin.zip"
$zip = "C:\tizen-data\openjdk.zip"
$jdk = "C:\openjdk"

if (!(Test-Path $zip)) {
    Invoke-WebRequest -Uri $jdkUri -OutFile $zip
}

if (!(Test-Path $jdk)) {
    Expand-Archive -Path $zip -DestinationPath $jdk
}

$env:JAVA_HOME = "$jdk\jdk-13.0.2"
$env:PATH = "$env:JAVA_HOME\bin;$env:PATH"

# install Tizen

$tizenUri = "http://download.tizen.org/sdk/Installer/tizen-studio_3.7/web-cli_Tizen_Studio_3.7_windows-64.exe"
$installer = "C:\tizen-data\installer.exe"
$ts = "${env:USERPROFILE}\tizen-studio"
$pm = "$ts\package-manager\package-manager-cli.exe"

if (!(Test-Path $installer)) {
    Invoke-WebRequest -Uri $tizenUri -OutFile $installer
}

if (!(Test-Path $ts)) {
    & $installer --accept-license --no-java-check $ts
}

# & $pm install --no-java-check --accept-license --file "C:\tizen-data\packages.zip" --remove-installed-sdk `
    #"MOBILE-6.0-NativeAppDevelopment,WEARABLE-6.0-NativeAppDevelopment,MOBILE-4.0-NativeAppDevelopment,WEARABLE-4.0-NativeAppDevelopment,NativeToolchain-Gcc-6.2,NativeToolchain-Gcc-9.2"

# modify internal files
$plugins = "C:\Users\AzureUser\tizen-studio\tools\smart-build-interface\plugins"
Copy-Item -Path "$plugins\llvm10.i586.core.app.xml" -Destination "$plugins\llvm10.i386.core.app.xml" -Force
[xml] $xml = (Get-Content -Path "$plugins\llvm10.i386.core.app.xml")
$xml.extension.toolchain.id = $xml.extension.toolchain.id.Replace("i586", "i386")
$xml.extension.toolchain.architecture = "i386"
$xml.Save("$plugins\llvm10.i386.core.app.xml")

exit $LASTEXITCODE
