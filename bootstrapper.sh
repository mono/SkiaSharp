#!/usr/bin/env bash

##########################################################################
# This is the Cake bootstrapper script for Linux and OS X.
# This file was downloaded from https://github.com/cake-build/resources
# Feel free to change this file to fit your needs.
##########################################################################

# Define directories.
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
ADDINS_DIR=$TOOLS_DIR/addins
MODULES_DIR=$TOOLS_DIR/modules
NUGET_EXE=$TOOLS_DIR/nuget.exe
CAKE_EXE=$TOOLS_DIR/Cake/Cake.exe
PACKAGES_CONFIG=$TOOLS_DIR/packages.config
PACKAGES_CONFIG_MD5=$TOOLS_DIR/packages.config.md5sum
ADDINS_PACKAGES_CONFIG=$ADDINS_DIR/packages.config
MODULES_PACKAGES_CONFIG=$MODULES_DIR/packages.config
CAKE_PACKAGES_CONFIG=$SCRIPT_DIR/cake.packages.config

# Define md5sum or md5 depending on Linux/OSX
MD5_EXE=
if [[ "$(uname -s)" == "Darwin" ]]; then
    MD5_EXE="md5 -r"
else
    MD5_EXE="md5sum"
fi

# Define default arguments.
SCRIPT="build.cake"
TARGET="Default"
CONFIGURATION="Release"
VERBOSITY="verbose"
DRYRUN=
SHOW_VERSION=false
SCRIPT_ARGUMENTS=()

# Parse arguments.
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        -t|--target) TARGET="$2"; shift ;;
        -c|--configuration) CONFIGURATION="$2"; shift ;;
        -v|--verbosity) VERBOSITY="$2"; shift ;;
        -d|--dryrun) DRYRUN="-dryrun" ;;
        --version) SHOW_VERSION=true ;;
        --) shift; SCRIPT_ARGUMENTS+=("$@"); break ;;
        *) SCRIPT_ARGUMENTS+=("$1") ;;
    esac
    shift
done

# Make sure the tools folder exist.
if [ ! -d "$TOOLS_DIR" ]; then
  mkdir "$TOOLS_DIR"
fi

# Make sure that packages.config exist.
if [[ (! -f "$PACKAGES_CONFIG") || (-f "$CAKE_PACKAGES_CONFIG") && ("$CAKE_PACKAGES_CONFIG" -nt "$PACKAGES_CONFIG") ]]; then
    if [ ! -f "$CAKE_PACKAGES_CONFIG" ]; then
        echo "Downloading packages.config..."
        curl -Lsfo "$PACKAGES_CONFIG" http://cakebuild.net/download/bootstrapper/packages
        if [ $? -ne 0 ]; then
            echo "An error occured while downloading packages.config."
            exit 1
        fi
    else
        echo "using local cake.packages.config..."
        cp "$CAKE_PACKAGES_CONFIG" "$PACKAGES_CONFIG"
    fi
fi

# Download NuGet if it does not exist.
if [ ! -f "$NUGET_EXE" ]; then
    echo "Downloading NuGet..."
    curl -Lsfo "$NUGET_EXE" https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
    if [ $? -ne 0 ]; then
        echo "An error occured while downloading nuget.exe."
        exit 1
    fi
fi

# Save nuget.exe path to environment to be available to child processed
export NUGET_EXE=$NUGET_EXE

# Restore tools from NuGet.
pushd "$TOOLS_DIR" >/dev/null
if [ ! -f "$PACKAGES_CONFIG_MD5" ] || [ "$( cat "$PACKAGES_CONFIG_MD5" | sed 's/\r$//' )" != "$( $MD5_EXE "$PACKAGES_CONFIG" | awk '{ print $1 }' )" ]; then
    find . -type d ! -name . | xargs rm -rf
fi

mono "$NUGET_EXE" install -ExcludeVersion
if [ $? -ne 0 ]; then
    echo "Could not restore NuGet tools."
    exit 1
fi

$MD5_EXE "$PACKAGES_CONFIG" | awk '{ print $1 }' >| "$PACKAGES_CONFIG_MD5"

popd >/dev/null

# Restore addins from NuGet.
if [ -f "$ADDINS_PACKAGES_CONFIG" ]; then
    pushd "$ADDINS_DIR" >/dev/null

    mono "$NUGET_EXE" install -ExcludeVersion
    if [ $? -ne 0 ]; then
        echo "Could not restore NuGet addins."
        exit 1
    fi

    popd >/dev/null
fi

# Restore modules from NuGet.
if [ -f "$MODULES_PACKAGES_CONFIG" ]; then
    pushd "$MODULES_DIR" >/dev/null

    mono "$NUGET_EXE" install -ExcludeVersion
    if [ $? -ne 0 ]; then
        echo "Could not restore NuGet modules."
        exit 1
    fi

    popd >/dev/null
fi

# Make sure that Cake has been installed.
if [ ! -f "$CAKE_EXE" ]; then
    echo "Could not find Cake.exe at '$CAKE_EXE'."
    exit 1
fi

# Start Cake
if $SHOW_VERSION; then
    exec mono "$CAKE_EXE" -version
else
    exec mono "$CAKE_EXE" $SCRIPT -verbosity=$VERBOSITY -configuration=$CONFIGURATION -target=$TARGET $DRYRUN "${SCRIPT_ARGUMENTS[@]}"
fi

code=$?

if [ $code -eq 0 ]; then
    TEMP_TARGET=`echo "${TARGET}" | tr '[A-Z]' '[a-z]'`
    if [ "$TEMP_TARGET" == "clean" ]; then
        echo "Removing Cake bits too..."
        rm -rf "$TOOLS_DIR/Addins/" "$TOOLS_DIR/Cake/"
        rm -rf "$TOOLS_DIR/Microsoft.DotNet.BuildTools.GenAPI/"
        rm -rf "$TOOLS_DIR/NUnit.ConsoleRunner/" "$TOOLS_DIR/xunit.runner.console/"
        rm -rf "$TOOLS_DIR/System.IO.Compression.dll"
        rm -rf "$TOOLS_DIR/nuget.exe"
        rm -rf "$TOOLS_DIR/__MACOSX/" "$TOOLS_DIR/xamarin-component.exe" "$TOOLS_DIR/xpkg.zip"
    fi
fi

exit $code