#!/usr/bin/env bash
#
# Declares the GPU opt-out for the current macOS build agent.
#
# The x64 macOS agent pool exposes a *virtualized* Metal device. Merely creating
# a command queue on it leaves dispatch-queue state that never signals, hanging
# the test host's shutdown for hours — so Metal has to be opted out there.
# Apple Silicon agents run real hardware, keep Metal required, and a Metal
# failure on one of those is a red build, as it should be.
#
# This is deliberately a *declared* opt-out rather than a runtime sniff inside
# the test code (which is what it replaces). It describes this agent, not the
# macOS platform: what a platform can inherently do lives in GpuPolicy and is
# never configured. See documentation/dev/gpu-test-policy.md.
#
# Used by the macOS, iOS and Mac Catalyst test legs, which all run the shared
# Metal renderers on a macOS agent.

set -e

arch="$(uname -m)"
echo "macOS agent architecture: $arch"

if [ "$arch" = "x86_64" ]; then
    echo "Opting out of Metal: this x64 agent's virtualized Metal driver hangs the test host on shutdown."
    echo "##vso[task.setvariable variable=SKIASHARP_TEST_SKIP_GPU]ganesh-metal,graphite-metal"
else
    echo "Apple Silicon agent — Metal stays required."
fi
