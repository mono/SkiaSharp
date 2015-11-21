SKIA_ROOT=/cvs/skia

all:
	echo some targets are here

test-static-lib:
	(d=`pwd`; cd $(SKIA_ROOT)/out/Debug; 	for i in ../../include/c/*h; do j=`basename $$i`; echo "#include <$$j>"; done > all.h; sh $$d/link-statics-into-dynamic.dylib)
	(cd $(SKIA_ROOT)/out/Debug; nm libskia.dylib  | grep _sk_ | grep ' T ' | sed 's/_sk/sk/' | awk 'BEGIN {print "#include \"all.h\""} {print "void *p" n++ " = &" $$3 ";"} END { print "int main(){\n\tvoid *j;"; for (i = 0; i < n; i++){ print "\tj=p" i ";"  } print "\treturn 0;\n}"} ' > a.c)
	(cd $(SKIA_ROOT)/out/Debug; cc -I../../include/c a.c libskia_core.a libskia_opts_sse41.a libskia_opts_avx.a libjpeg-turbo.a libSkKTX.a libskia_core.a libskia_ports.a libskia_sfnt.a libskia_utils.a libskia_images.a libskia_skgpu.a libskia_effects.a libskia_opts.a libskia_opts_ssse3.a libwebp_dec.a libwebp_dsp.a libwebp_utils.a libwebp_enc.a libwebp_dsp_enc.a libetc1.a -framework Foundation -framework CoreGraphics -framework CoreText -framework OpenCL -framework OpenGL -framework ImageIO -framework CoreServices -lstdc++)