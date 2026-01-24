# Building on Linux

This guide covers building libSkiaSharp.so for Linux distributions.

## Prerequisites

- **.NET 8 SDK** - Pinned via `global.json`
- **Python 3**
- **Clang 14+**
- **Make**
- **Ninja** (build system)

## Downloading

The first step is to clone the [**mono/SkiaSharp**](https://github.com/mono/SkiaSharp) repository:

```bash
git clone https://github.com/mono/SkiaSharp.git
cd SkiaSharp
```

Google's [**depot_tools**](https://chromium.googlesource.com/chromium/tools/depot_tools.git) will be downloaded automatically during the build process.

## Building

The easiest way to build is using the Cake build script:

```bash
# Download dependencies and build native library
dotnet cake --target=externals-linux

# Or build everything including managed code
dotnet cake --target=everything
```

### Manual Building

If you need more control, you can build manually:

1. Clone the skia submodule and sync dependencies:
   ```bash
   git submodule update --init --recursive
   cd externals/skia
   python3 tools/git-sync-deps
   ```

2. Generate build files:
   ```bash
   ./bin/gn gen 'out/linux/x64' --args='
       is_official_build=true skia_enable_tools=false
       target_os="linux" target_cpu="x64"
       skia_use_icu=false skia_use_sfntly=false skia_use_piex=true
       skia_use_system_expat=false skia_use_system_freetype2=false 
       skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false 
       skia_use_system_libwebp=false skia_use_system_zlib=false
       skia_enable_gpu=true
       extra_cflags=[ "-DSKIA_C_DLL" ]'
   ```

   > The latest arguments can be found in [`native/linux/build.cake`](https://github.com/mono/SkiaSharp/blob/main/native/linux/build.cake)

3. Build:
   ```bash
   ninja -C 'out/linux/x64' SkiaSharp
   ```

## Customizing

To customize the build, modify the `--args` value:

| Option | Example |
|--------|---------|
| Disable GPU | `skia_enable_gpu=false` |
| Static library | `is_static_skiasharp=true` |
| Custom compilers | `cc="gcc" cxx="g++" ar="ar"` |
| Extra C/C++ flags | `extra_cflags=[ "-O3" ]` |
| Extra linker flags | `extra_ldflags=[ "-fuse-ld=lld" ]` |

## Docker

Building for different Linux distributions is easier with Docker. The repository includes Dockerfiles for various configurations.

```bash
# Build the Docker image
cd scripts/Docker/alpine/amd64
docker build --tag skiasharp-alpine .

# Run the build
docker run --rm --name skiasharp-alpine --volume $(pwd):/work skiasharp-alpine \
    /bin/bash ./bootstrapper.sh -t externals-linux --variant=alpine --buildarch=x64

# Fix file ownership if needed
chown -R $(id -u):$(id -g) .
```

> See the `scripts/Docker/` directory for available Dockerfiles.