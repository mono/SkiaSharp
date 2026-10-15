#!/usr/bin/env bash

set -e
nohup Xvfb :99 -screen 0 1280x1024x24 > /tmp/xvfb.log 2>&1 &
sleep 3
echo "Started Xvfb on :99"
# Expose the software-GPU environment to the test step. Forcing
# Mesa (GL) and the lavapipe ICD (Vulkan) keeps output deterministic.
#
# softpipe rather than llvmpipe for GL: llvmpipe's LLVM shader JIT
# segfaults compiling the fragment shader Skia generates for a
# runtime blender and takes the whole test host with it, which is
# what made this leg lose all its results (#4590). softpipe is the
# non-JIT gallium rasterizer and runs the same tests green. It
# caps at GL 3.3, which is well past the 3.0 Ganesh needs. See
# #4604 to move back to llvmpipe.
echo "##vso[task.setvariable variable=DISPLAY]:99"
echo "##vso[task.setvariable variable=LIBGL_ALWAYS_SOFTWARE]1"
echo "##vso[task.setvariable variable=GALLIUM_DRIVER]softpipe"
echo "##vso[task.setvariable variable=VK_ICD_FILENAMES]/usr/share/vulkan/icd.d/lvp_icd.x86_64.json"
echo "##vso[task.setvariable variable=VK_DRIVER_FILES]/usr/share/vulkan/icd.d/lvp_icd.x86_64.json"
