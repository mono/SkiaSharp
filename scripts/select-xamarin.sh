#!/bin/bash -e -o pipefail

MONO_VERSION=$1

if [ -d "$AGENT_HOMEDIRECTORY/scripts/select-xamarin-sdk.sh" ]; then
  $AGENT_HOMEDIRECTORY/scripts/select-xamarin-sdk.sh $MONO_VERSION
else
  echo "Skipping the switch because the script did not exist."
fi
