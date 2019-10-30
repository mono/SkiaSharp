ARCH ?= x64
SONAME_VERSION ?= 0.0.0
CC ?= clang
CXX ?= clang++
LDFLAGS += 

noop = 
space = ${noop} ${noop}

src_root = ../../externals/harfbuzz/src
obj_root = obj
bin_root = bin
objarch_root = ${obj_root}/${ARCH}
target_name = libHarfBuzzSharp.so
target = ${bin_root}/${ARCH}/${target_name}.${SONAME_VERSION}

library_dirs = 
include_dirs = \
	.                       \
	${src_root}
library_paths = 
defines = \
	-DHAVE_CONFIG_H -DNDEBUG
cflags = \
	-fno-rtti -fno-exceptions -fno-threadsafe-statics -fPIC                   \
	-g -Os -ffunction-sections -fdata-sections
ifeq "${ARCH}" "x86"
	  arch_cflags = -m32
else
	  arch_cflags = 
endif
cflags_c = ${cflags} ${CFLAGS}
cflags_cc = -std=c++11 ${CXXFLAGS}
ldflags = -s -Wl,--gc-sections $(library_dirs:%=-L%) ${LDFLAGS}
includes = $(include_dirs:%=-I%)
library_names = $(notdir ${library_paths})
libraries = $(library_names:lib%.a=-l%)

src = \
	${src_root}/hb-aat-layout.cc                          \
	${src_root}/hb-aat-map.cc                             \
	${src_root}/hb-blob.cc                                \
	${src_root}/hb-buffer-serialize.cc                    \
	${src_root}/hb-buffer.cc                              \
	${src_root}/hb-common.cc                              \
	${src_root}/hb-face.cc                                \
	${src_root}/hb-fallback-shape.cc                      \
	${src_root}/hb-font.cc                                \
	${src_root}/hb-map.cc                                 \
	${src_root}/hb-number.cc                              \
	${src_root}/hb-ot-cff1-table.cc                       \
	${src_root}/hb-ot-cff2-table.cc                       \
	${src_root}/hb-ot-color.cc                            \
	${src_root}/hb-ot-face.cc                             \
	${src_root}/hb-ot-font.cc                             \
	${src_root}/hb-ot-layout.cc                           \
	${src_root}/hb-ot-map.cc                              \
	${src_root}/hb-ot-math.cc                             \
	${src_root}/hb-ot-meta.cc                             \
	${src_root}/hb-ot-metrics.cc                          \
	${src_root}/hb-ot-name.cc                             \
	${src_root}/hb-ot-shape-complex-arabic.cc             \
	${src_root}/hb-ot-shape-complex-default.cc            \
	${src_root}/hb-ot-shape-complex-hangul.cc             \
	${src_root}/hb-ot-shape-complex-hebrew.cc             \
	${src_root}/hb-ot-shape-complex-indic-table.cc        \
	${src_root}/hb-ot-shape-complex-indic.cc              \
	${src_root}/hb-ot-shape-complex-khmer.cc              \
	${src_root}/hb-ot-shape-complex-myanmar.cc            \
	${src_root}/hb-ot-shape-complex-thai.cc               \
	${src_root}/hb-ot-shape-complex-use-table.cc          \
	${src_root}/hb-ot-shape-complex-use.cc                \
	${src_root}/hb-ot-shape-complex-vowel-constraints.cc  \
	${src_root}/hb-ot-shape-fallback.cc                   \
	${src_root}/hb-ot-shape-normalize.cc                  \
	${src_root}/hb-ot-shape.cc                            \
	${src_root}/hb-ot-tag.cc                              \
	${src_root}/hb-ot-var.cc                              \
	${src_root}/hb-set.cc                                 \
	${src_root}/hb-shape-plan.cc                          \
	${src_root}/hb-shape.cc                               \
	${src_root}/hb-shaper.cc                              \
	${src_root}/hb-static.cc                              \
	${src_root}/hb-subset-cff-common.cc                   \
	${src_root}/hb-subset-cff1.cc                         \
	${src_root}/hb-subset-cff2.cc                         \
	${src_root}/hb-subset-input.cc                        \
	${src_root}/hb-subset-plan.cc                         \
	${src_root}/hb-subset.cc                              \
	${src_root}/hb-ucd.cc                                 \
	${src_root}/hb-unicode.cc                             \
	${src_root}/hb-warning.cc

src_names = $(subst ${src_root}/,${noop},${src})
objs = $(src_names:%=${objarch_root}/%.o)
deps = $(objs:.o=.d)

${objarch_root}/%.o: ${src_root}/%
# build the local source
	@echo Building $<...
	@mkdir -p $(dir $@)
	@if [ "$(filter %.cc,$<)" = "" ]; then                                   \
		$(CC) -MMD -MF $@.d                                                  \
			${defines} ${includes} ${arch_cflags} ${cflags_c}                \
			-c $< -o $@                                                      \
	; else                                                                   \
		$(CXX) -MMD -MF $@.d                                                 \
			${defines} ${includes} ${arch_cflags} ${cflags_c} ${cflags_cc}   \
			-c $< -o $@                                                      \
	; fi

${target}: ${objs}
# link
	@echo Linking $@...
	@mkdir -p $(dir $@)
	$(CXX) -shared -rdynamic -s -o $@                                                                   \
		${defines} ${includes} ${arch_cflags} ${cflags_c} ${cflags_cc}                                  \
		-Wl,--start-group ${objarch_root}/*.o ${library_paths} -Wl,--end-group   \
		${ldflags} -Wl,--gc-sections -Wl,--no-undefined                                                 \
		-Wl,-soname,libHarfBuzzSharp.so.${SONAME_VERSION}

all: ${target}

clean:
	rm -rf ${obj_root} ${bin_root}
