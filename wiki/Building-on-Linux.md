Building libSkiaSharp.so for Linux is straightforward. It is best to clone the [**mono/SkiaSharp**](https://github.com/mono/SkiaSharp) repository as it pulls down additional tools used to build skia, namely [**depot_tools**](https://chromium.googlesource.com/chromium/tools/depot_tools.git).

> **Note:** This guide applies to v1.60.1 and later. For much older versions (pre-v1.60.1), the build process was different.

## Downloading

The first step is to clone the [**mono/skia**](https://github.com/mono/skia) repository:

    git clone https://github.com/mono/skia.git -b v1.60.1

To build the current master, check out `xamarin-mobile-bindings` as this is the Xamarin "master" - the `master` branch is reserved for the Google "master".

In order to build `libSkiaSharp`, Google's [**depot_tools**](https://chromium.googlesource.com/chromium/tools/depot_tools.git) is also required:

    git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git

> In order to build on some platforms, it is necessary it skip **depot_tools** and rather use the native `gn` and `ninja` packages.

## Building

Once **skia** and **depot_tools** are cloned, building happens in 3 steps (before anything happens, make sure you are working in the directory that you cloned **skia** into - eg `cd skia`):

First, synchronize the **skia** dependencies:

    python tools/git-sync-deps

Next, create the build (ninja) files using various arguments:

    ./bin/gn gen 'out/linux/x64' --args='
        is_official_build=true skia_enable_tools=false
        target_os="linux" target_cpu="x64"
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true
        skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        skia_enable_gpu=true
        extra_cflags=[ "-DSKIA_C_DLL" ]
        linux_soname_version="60.1.0"'

> The latest arguments used per build can be found in the [`native/linux/build.cake` file](https://github.com/mono/SkiaSharp/blob/master/native/linux/build.cake)

Finally, build the native `libSkiaSharp.so` binary:

    ../depot_tools/ninja 'SkiaSharp' -C 'out/linux/x64'

Once the build completes, there will be a file located at `out/linux/x64/libSkiaSharp.so.60.1.0` which can be renamed to `libSkiaSharp.so` and used for SkiaSharp apps.

## Customizing

To build other types of linux-y binaries, you can tweak the `--args` value. Some example are:

 - disable GPU:  
   `--args='... skia_enable_gpu=false ...'`
 - static library (v2.80+):  
   `--args='... is_static_skiasharp=true ...'`
 - customize the compilers:  
   `--args='... cc="gcc" cxx="g++" ar="ar" ...'`
 - adding C/C++ flags:
    - flags for both C & C++:  
      `--args='... extra_cflags=[ "..." ] ...'`
    - flags for just C:  
      `--args='... extra_cflags_c=[ "..." ] ...'`
    - flags for just C++:  
      `--args='... extra_cflags_cc=[ "..." ] ...'`
 - adding linker flags:  
   `--args='... extra_ldflags=[ "..." ] ...'`
 - adding assembler flags:  
   `--args='... extra_asmflags=[ "..." ] ...'`

## Docker

Building for arbitrary versions and distros of Linux is often hard on a single machine. Using Docker containers to actually do the build means that you can build anything, without actually using your configuration. And then, that Dockerfile can be sent in as a PR so the SkiaSharp CI builds it for you.

All you really need is Docker installed on the development machine. You can easily use Linux or even [WSL 2 with Docker configured](https://docs.docker.com/docker-for-windows/wsl/).

Once everything is installed and the source is cloned, run `docker build` on your Dockerfile:

```sh
(cd <path-to-dockerfile-dir> && docker build --tag <custom-tag> .)
docker run --rm --name <custom-name> --volume $(pwd):/work <custom-tag> \
    /bin/bash ./bootstrapper.sh -t externals-linux --variant=<friendly-name> <additional-args>
```

> The `--variant` is just to set the folder name in the `./output/native` directory.

> By default, the `--gn` and `--ninja` points to the tools that skia downloads. For some platforms, this needs to be installed inside the Docker container and then the path to them needs to be set using the arguments.

> `-buildarch` is used to determine which architecture to build.

For example:

```sh
(cd scripts/Docker/alpine/amd64 && docker build --tag skiasharp-alpine .)
docker run --rm --name skiasharp-alpine --volume $(pwd):/work skiasharp-alpine \
    /bin/bash ./bootstrapper.sh -t externals-linux --variant=alpine --buildarch=x64 --gn=gn --ninja=ninja
```

> One current "limitation" of the process is that the container needs to have mono installed to run Cake. This most probably will change in the future, but the Cake file is needed to ensure that all the variables and checks are done.

> If you are building locally, the ownership of the files will probably wrong, you could run:
> ```
> chown -R $(id -u):$(id -g) .
> ```
> Or you can [configure your container](https://vsupalov.com/docker-shared-permissions/) to use your user