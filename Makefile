SKIA_PATH ?= $(shell readlink -f externals/skia)
DEPOT_PATH ?= $(shell readlink -f externals/depot_tools)
HARFBUZZ_PATH ?= $(shell readlink -f externals/harfbuzz)
NATIVEBUILDS_PATH ?= $(shell readlink -f native-builds)

ARCH ?= x64
SKIA_VERSION ?= 1.58.0.0
HARFBUZZ_VERSION ?= 1.4.6.0
HARFBUZZ_VERSION_SOURCE ?= 1.4.6
SUPPORT_GPU ?= 1
ifeq (${SUPPORT_GPU},1)
skia_enable_gpu=true
endif
ifeq (${SUPPORT_GPU},0)
skia_enable_gpu=false
endif

noop = 
space = ${noop} ${noop}

skia_version_parts = $(subst ., ,${SKIA_VERSION})
skia_soname_version = $(word 2, ${skia_version_parts})
skia_file_version = $(subst ${space},.,$(wordlist 2, 4, ${skia_version_parts}))

harfbuzz_source_url = https://github.com/behdad/harfbuzz/releases/download/${HARFBUZZ_VERSION_SOURCE}/harfbuzz-${HARFBUZZ_VERSION_SOURCE}.tar.bz2
harfbuzz_source_archive = ${HARFBUZZ_PATH}/harfbuzz-${HARFBUZZ_VERSION_SOURCE}.tar.bz2
harfbuzz_version_parts = $(subst ., ,${HARFBUZZ_VERSION})
harfbuzz_soname_version = $(word 2, ${harfbuzz_version_parts})
harfbuzz_file_version = $(subst ${space},.,$(wordlist 1, 3, ${harfbuzz_version_parts}))

all:
	echo You can use "make update-docs" to update the docs, "or make assemble-docs" to assemble for deployment

# docs

update-docs:
	echo Not supported anymore, just "make assemble-docs"

assemble-docs:
	mdoc assemble --out=docs/SkiaSharp docs/en

# externals

externals-init:
	(cd "${SKIA_PATH}" && python "tools/git-sync-deps")


externals-harfbuzz:
	mkdir -p "${HARFBUZZ_PATH}"
	if ! [ -f "${harfbuzz_source_archive}" ]; then \
	  curl -Lo "${harfbuzz_source_archive}" "${harfbuzz_source_url}"; \
	fi
	if ! [ -f "${HARFBUZZ_PATH}/harfbuzz/config.h" ]; then \
	  tar jxf "${harfbuzz_source_archive}" -C "${HARFBUZZ_PATH}"; \
	  rm -rf "${HARFBUZZ_PATH}/harfbuzz"; \
	  mv -f "${HARFBUZZ_PATH}/harfbuzz-${HARFBUZZ_VERSION_SOURCE}" "${HARFBUZZ_PATH}/harfbuzz"; \
	  (cd "${HARFBUZZ_PATH}/harfbuzz" && bash ./configure); \
	fi

externals-linux-harfbuzz: externals-harfbuzz
# build libHarfBuzzSharp
	(cd "${NATIVEBUILDS_PATH}/libHarfBuzzSharp_linux" && \
	make ARCH=${ARCH} VERSION=${HARFBUZZ_VERSION})
# copy libHarfBuzzSharp to output
	mkdir -p "${NATIVEBUILDS_PATH}/lib/linux/${ARCH}"
	cp \
	  "${NATIVEBUILDS_PATH}/libHarfBuzzSharp_linux/bin/${ARCH}/libHarfBuzzSharp.so.${harfbuzz_file_version}" \
	  "${NATIVEBUILDS_PATH}/lib/linux/${ARCH}"


externals-linux-skiasharp: externals-init
# generate native skia build files
	(cd "${SKIA_PATH}" && "${SKIA_PATH}/bin/gn" gen "out/linux/${ARCH}" --args='is_official_build=true skia_enable_tools=false target_os="linux" target_cpu="${ARCH}" skia_use_icu=false skia_use_sfntly=false skia_use_system_freetype2=false skia_enable_gpu=${skia_enable_gpu} extra_cflags=[ "-DSKIA_C_DLL" ] extra_ldflags=[ ]')
# build native skia
	(cd "${SKIA_PATH}" && \
	"${DEPOT_PATH}/ninja" -C "out/linux/${ARCH}")
# build libSkiaSharp
	(cd "${NATIVEBUILDS_PATH}/libSkiaSharp_linux" && \
	make ARCH=${ARCH} VERSION=${SKIA_VERSION} SUPPORT_GPU=${SUPPORT_GPU})
# copy libSkiaSharp to output
	mkdir -p "${NATIVEBUILDS_PATH}/lib/linux/${ARCH}"
	cp \
	  "${NATIVEBUILDS_PATH}/libSkiaSharp_linux/bin/${ARCH}/libSkiaSharp.so.${skia_file_version}" \
	  "${NATIVEBUILDS_PATH}/lib/linux/${ARCH}"


externals-linux: externals-linux-skiasharp externals-linux-harfbuzz
