cmake_minimum_required(VERSION 3.20)
project(graphite_c_smoke C)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)

# Resolve the SkiaSharp source tree relative to this CMakeLists.
get_filename_component(SKIASHARP_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../../../.." ABSOLUTE)

find_package(Vulkan REQUIRED)

add_executable(graphite_c_smoke graphite_c_smoke.c)

target_include_directories(graphite_c_smoke PRIVATE
    "${SKIASHARP_ROOT}/externals/skia"
)

# Link against the freshly-built libSkiaSharp.so, plus the system Vulkan loader.
target_link_libraries(graphite_c_smoke PRIVATE
    "${SKIASHARP_ROOT}/output/native/linux/x64/libSkiaSharp.so"
    Vulkan::Vulkan
)

# rpath so the executable finds libSkiaSharp.so at runtime without LD_LIBRARY_PATH gymnastics.
set_target_properties(graphite_c_smoke PROPERTIES
    BUILD_RPATH "${SKIASHARP_ROOT}/output/native/linux/x64"
    INSTALL_RPATH "${SKIASHARP_ROOT}/output/native/linux/x64"
)
