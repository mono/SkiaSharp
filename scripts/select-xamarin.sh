#!/bin/bash -e -o pipefail

MONO_VERSION=$1

echo "Mono versions:"
ls -ld /Library/Frameworks/Mono.framework/Versions/*

echo "Xamarin.iOS versions:"
ls -ld /Library/Frameworks/Xamarin.iOS.framework/Versions/*

echo "Xamarin.Android versions:"
ls -ld /Library/Frameworks/Xamarin.Android.framework/Versions/*

echo "Xamarin.Mac versions:"
ls -ld /Library/Frameworks/Xamarin.Mac.framework/Versions/*

if [ -f "$AGENT_HOMEDIRECTORY/scripts/select-xamarin-sdk.sh" ]; then
  $AGENT_HOMEDIRECTORY/scripts/select-xamarin-sdk.sh $MONO_VERSION
else
  echo "Skipping the switch because the script did not exist."
fi
