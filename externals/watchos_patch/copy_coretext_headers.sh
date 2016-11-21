#!/bin/sh

# https://gist.github.com/dzenbot/918f8b65a51a2dd378c567579ca99077

# CoreText.framework is missing for the watchOS Simulator (but is available on the device platform SDK)
# https://openradar.appspot.com/27844864
#
# This script is a quick patch to solve this issue, so we are all capable of running CoreText APIs on the Watch simulator too,
# by copying the WatchOS SDK's CoreText header files into the WatchSimulator's. This works great, although since editing the SDK files
# is protected, it needs to be run with sudo.

# Returns the appropriate platform path containing the framework headers
function headersPath() {
    # Main Xcode path
    XCODE_PATH=$( xcode-select --print-path )

    echo "${XCODE_PATH}/Platforms/${1}.platform/Developer/SDKs/${1}.sdk/System/Library/Frameworks/${2}.framework/Headers/"
}

CoreText_WatchOS_Headers_Path=$( headersPath "WatchOS" "CoreText" )
CoreText_WatchSimulator_Headers_Path=$( headersPath "WatchSimulator" "CoreText" )

# Only perform if the CoreText framework headers are missing in the WatchSimulator
if [ ! -d ${CoreText_WatchSimulator_Headers_Path} ]; then
    echo "Patch:$(tput setaf 1) Missing CoreText headers for WatchSimulator at path '${CoreText_WatchSimulator_Headers_Path}'.$(tput sgr0)"

    # Copies the header files
    sudo cp -R ${CoreText_WatchOS_Headers_Path} ${CoreText_WatchSimulator_Headers_Path}

    echo "Patch:$(tput setaf 2) CoreText headers copied! You can now run your WatchOS apps using CoreText on the simulator.$(tput sgr0)"
else
    echo "Patch:$(tput setaf 2) CoreText headers are already available on the WatchOS simulator.$(tput sgr0)"
fi
