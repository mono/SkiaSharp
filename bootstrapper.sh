#!/bin/bash
###############################################################
# This is the Cake bootstrapper script that is responsible for
# downloading Cake and all specified tools from NuGet.
###############################################################

# Define directories.
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
NUGET_EXE=$TOOLS_DIR/nuget.exe
XC_EXE=$TOOLS_DIR/xamarin-component.exe
CAKE_EXE=$TOOLS_DIR/Cake/Cake.exe

# BEGIN TEMP WORKAROUND
SYSIOCOMP=$TOOLS_DIR/System.IO.Compression.dll
# END TEMP WORKAROUND

# Define default arguments.
SCRIPT="build.cake"
TARGET="Default"
CONFIGURATION="Release"
VERBOSITY="diagnostic"
DRYRUN=false
SHOW_VERSION=false
EXTRA_ARGS=""

# Parse arguments.
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        -t|--target) TARGET="$2"; shift ;;
        -c|--configuration) CONFIGURATION="$2"; shift ;;
        -v|--verbosity) VERBOSITY="$2"; shift ;;
        -d|--dryrun) DRYRUN=true ;;
        --version) SHOW_VERSION=true ;;
        *) if [[ "$1" && "$2" ]];then EXTRA_ARGS+="$1=$2 "; fi; shift ;;
    esac
    shift
done

# Make sure the tools folder exist.
if [ ! -d $TOOLS_DIR ]; then
  mkdir $TOOLS_DIR
fi

# Download NuGet if it does not exist.
if [ ! -f $NUGET_EXE ]; then
    echo "Downloading NuGet..."
    curl -Lsfo $NUGET_EXE https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
    if [ $? -ne 0 ]; then
        echo "An error occured while downloading nuget.exe."
        exit 1
    fi
fi

# Download xamarin-component.exe if it does not exist.
if [ ! -f $XC_EXE ]; then
    echo "Downloading Xamarin-Component.exe..."
    curl -Lsfo "$TOOLS_DIR/xpkg.zip" https://components.xamarin.com/submit/xpkg
    if [ $? -ne 0 ]; then
        echo "An error occured while downloading nuget.exe."
        exit 1
    fi
    unzip -oq "$TOOLS_DIR/xpkg.zip" -d $TOOLS_DIR
fi


# BEGIN TEMP WORKAROUND
# There is a bug in Mono's System.IO.Compression
# This binary fixes the bug for now
# Download System.IO.Compression if it does not exist.
if [ ! -f "$SYSIOCOMP" ]; then
    echo "Downloading System.IO.Compression.dll (THIS SHOULD BE REMOVED) ..."
    curl -Lsfo "$SYSIOCOMP" http://xamarin-components-binaries.s3.amazonaws.com/System.IO.Compression.dll
    if [ $? -ne 0 ]; then
        echo "An error occured while downloading System.IO.Compression.dll."
        exit 1
    fi
fi
# END TEMP WORKAROUND


# Restore tools from NuGet.
pushd $TOOLS_DIR >/dev/null
mono $NUGET_EXE install -ExcludeVersion
if [ $? -ne 0 ]; then
    echo "Could not restore NuGet packages."
    exit 1
fi
popd >/dev/null

# Make sure that Cake has been installed.
if [ ! -f $CAKE_EXE ]; then
    echo "Could not find Cake.exe at '$CAKE_EXE'."
    exit 1
fi

echo "EXTRA ARGS: $EXTRA_ARGS"
# Start Cake
if $SHOW_VERSION; then
    mono $CAKE_EXE -version
elif $DRYRUN; then
    mono $CAKE_EXE $SCRIPT -verbosity=$VERBOSITY -configuration=$CONFIGURATION -target=$TARGET -dryrun $EXTRA_ARGS
else
    mono $CAKE_EXE $SCRIPT -verbosity=$VERBOSITY -configuration=$CONFIGURATION -target=$TARGET $EXTRA_ARGS
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
