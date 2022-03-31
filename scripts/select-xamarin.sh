#!/bin/bash -e -o pipefail

$mono_version=$1

if [ -d "$AGENT_HOMEDIRECTORY/scripts/select-xamarin-sdk.sh" ]; then
  $AGENT_HOMEDIRECTORY/scripts/select-xamarin-sdk.sh $mono_version
else
  echo "Skipping the switch because the script did not exist."
fi
