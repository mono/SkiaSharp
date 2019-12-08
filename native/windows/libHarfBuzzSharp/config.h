/* config.h.in.  Generated from configure.ac by autoheader.  */

/* The normal alignment of `struct{char;}', in bytes. */
#define ALIGNOF_STRUCT_CHAR__ 1

/* Define to 1 if you have the `atexit' function. */
#define HAVE_ATEXIT 1

/* Have cairo graphics library */
/* #undef HAVE_CAIRO */

/* Have cairo-ft support in cairo graphics library */
/* #undef HAVE_CAIRO_FT */

/* Have Core Text backend */
/* #undef HAVE_CORETEXT */

/* Define to 1 if you have the <dlfcn.h> header file. */
/* #undef HAVE_DLFCN_H */

/* Have DirectWrite Library */
/* #undef HAVE_DIRECTWRITE */

/* Have simple TrueType Layout backend */
/* #undef HAVE_FALLBACK */

/* Have fontconfig library */
/* #undef HAVE_FONTCONFIG */

/* Have FreeType 2 library */
/* #undef HAVE_FREETYPE */

/* Define to 1 if you have the `getpagesize' function. */
/* #undef HAVE_GETPAGESIZE */

/* Have glib2 library */
/* #undef HAVE_GLIB */

/* Have gobject2 library */
/* #undef HAVE_GOBJECT */

/* Have Graphite2 library */
/* #undef HAVE_GRAPHITE2 */

/* Have ICU library */
/* #undef HAVE_ICU */

/* Have Intel __sync_* atomic primitives */
/* #undef HAVE_INTEL_ATOMIC_PRIMITIVES */

/* Define to 1 if you have the <inttypes.h> header file. */
#if !defined (_MSC_VER) || (_MSC_VER >= 1800)
#define HAVE_INTTYPES_H 1
#endif

/* Define to 1 if you have the `isatty' function. */
#define HAVE_ISATTY 1

/* Define to 1 if you have the <memory.h> header file. */
#define HAVE_MEMORY_H 1

/* Define to 1 if you have the `mmap' function. */
/* #undef HAVE_MMAP */

/* Define to 1 if you have the `mprotect' function. */
/* #undef HAVE_MPROTECT */

/* Have native OpenType Layout backend */
#define HAVE_OT 1

/* Have POSIX threads */
/* #undef HAVE_PTHREAD */

/* Have PTHREAD_PRIO_INHERIT. */
/* #undef HAVE_PTHREAD_PRIO_INHERIT */

/* Define to 1 if you have the <sched.h> header file. */
/* #undef HAVE_SCHED_H */

/* Have sched_yield */
/* #undef HAVE_SCHED_YIELD */

/* Have Solaris __machine_*_barrier and atomic_* operations */
/* #undef HAVE_SOLARIS_ATOMIC_OPS */

/* Define to 1 if you have the <stdint.h> header file. */
#if !defined (_MSC_VER) || (_MSC_VER >= 1600)
#define HAVE_STDINT_H 1
#endif

/* Define to 1 if you have the <stdlib.h> header file. */
#define HAVE_STDLIB_H 1

/* Define to 1 if you have the <strings.h> header file. */
#ifndef _MSC_VER
#define HAVE_STRINGS_H 1
#endif

/* Define to 1 if you have the <string.h> header file. */
#define HAVE_STRING_H 1

/* Define to 1 if you have the `sysconf' function. */
/* #undef HAVE_SYSCONF */

/* Define to 1 if you have the <sys/mman.h> header file. */
/* #undef HAVE_SYS_MMAN_H */

/* Define to 1 if you have the <sys/stat.h> header file. */
#define HAVE_SYS_STAT_H 1

/* Define to 1 if you have the <sys/types.h> header file. */
#define HAVE_SYS_TYPES_H 1

/* Have Uniscribe library */
/* #undef HAVE_UNISCRIBE */

/* Define to 1 if you have the <unistd.h> header file. */
#ifndef _MSC_VER
#define HAVE_UNISTD_H 1
#endif

/* Define to 1 if you have the <usp10.h> header file. */
#define HAVE_USP10_H 1

/* Define to 1 if you have the <windows.h> header file. */
#define HAVE_WINDOWS_H 1

/* Define to the sub-directory in which libtool stores uninstalled libraries.
   */
#define LT_OBJDIR ".libs/"

/* Define to the address where bug reports for this package should be sent. */
#define PACKAGE_BUGREPORT "https://github.com/behdad/harfbuzz/issues/new"

/* Define to the full name of this package. */
#define PACKAGE_NAME "HarfBuzz"

/* Define to the full name and version of this package. */
#define PACKAGE_STRING "HarfBuzz 2.6.1"

/* Define to the one symbol short name of this package. */
#define PACKAGE_TARNAME "harfbuzz"

/* Define to the home page for this package. */
#define PACKAGE_URL "http://harfbuzz.org/"

/* Define to the version of this package. */
#define PACKAGE_VERSION "2.6.1"

/* Define to necessary symbol if this constant uses a non-standard name on
   your system. */
/* #undef PTHREAD_CREATE_JOINABLE */

/* Define to 1 if you have the ANSI C header files. */
#define STDC_HEADERS 1
