# Issue Triage Report — #2435

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T23:40:00Z |
| Type | type/question (0.92 (92%)) |
| Area | area/Build (0.88 (88%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** Reporter asks how to compile a debug build of libSkiaSharp on Linux and encounters a 'z_verbose undefined symbol' runtime error after successfully building with debug flags.

**Analysis:** Reporter wants to build a debug version of libSkiaSharp.so on Linux with debug symbols for backtracing. The compile succeeds with -g/-gdwarf-2 flags, but the resulting .so has a runtime symbol conflict: z_verbose is undefined because the official build statically embeds zlib (skia_use_system_zlib=false), and debug mode in zlib exposes this external symbol that conflicts when another zlib is loaded at runtime.

**Recommendations:** **close-as-not-a-bug** — This is a usage/how-to question about the build system. The answer is known: use is_official_build=false and avoid -DDEBUG to prevent the zlib symbol conflict.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/Build |
| Platforms | os/Linux |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Attempt to compile libSkiaSharp on Linux with extra_cflags containing -DDEBUG, -g, -gdwarf-2
2. Build succeeds but binary has no debug symbols as expected
3. At runtime: 'libSkiaSharp.so: error: symbol lookup error: undefined symbol: z_verbose (fatal)'

**Environment:** Linux x64, SkiaSharp 2.88.3. Runtime error: /opt/nextpvr/system/runtimes/linux-x64/native/libSkiaSharp.so: error: symbol lookup error: undefined symbol: z_verbose (fatal)

**Code snippets:**

```csharp
./bin/gn gen 'out/linux/x64' --args='is_official_build=true skia_enable_tools=false target_os="linux" target_cpu="x64" skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false skia_enable_gpu=true extra_cflags=[ "-DSKIA_C_DLL", "-DDEBUG", "-g" "-gdwarf-2" ] linux_soname_version="60.1.0"'
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Linux build script structure and gn build args haven't fundamentally changed since 2.88.x; the z_verbose conflict and debug build approach remain relevant. |

## Analysis

### Technical Summary

Reporter wants to build a debug version of libSkiaSharp.so on Linux with debug symbols for backtracing. The compile succeeds with -g/-gdwarf-2 flags, but the resulting .so has a runtime symbol conflict: z_verbose is undefined because the official build statically embeds zlib (skia_use_system_zlib=false), and debug mode in zlib exposes this external symbol that conflicts when another zlib is loaded at runtime.

### Rationale

The title says [QUESTION] and the body asks 'Is there a way'. No broken API behavior is described. The runtime error about z_verbose is a zlib symbol-visibility issue when mixing debug zlib with the precompiled system zlib loaded by the application. The authoritative build script (scripts/build-linux.sh) uses is_official_build=true which optimizes and strips debug symbols — using is_official_build=false or adding is_debug=true would produce a debug build, but the z_verbose issue then appears because zlib's debug symbols leak into the shared object.

### Key Signals

- "I'd like to see some if there is information in the back trace" — **issue body** (Reporter wants debug symbols for crash analysis, not a production debug build. A normal debug build with -g should suffice.)
- "symbol lookup error: undefined symbol: z_verbose (fatal)" — **issue comment** (This is a zlib symbol leakage issue: bundled zlib's debug-mode z_verbose symbol leaks into libSkiaSharp.so's dynamic table, and then a different zlib loaded at runtime doesn't have it, causing the conflict.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `scripts/build-linux.sh` | 43-55 | direct | The official Linux build uses is_official_build=true and does not pass debug flags. Changing to is_official_build=false enables debug builds. The script uses skia_use_system_zlib=false to bundle zlib — debug zlib exposes z_verbose as a non-static symbol, which conflicts with the system zlib loaded separately at runtime. |
| `scripts/build-linux.sh` | 51 | direct | extra_ldflags uses -static-libstdc++ and -static-libgcc but not -Wl,--version-script, so internal symbols like z_verbose from bundled zlib can leak into the .so's dynamic symbol table in debug builds. |

### Workarounds

- Use is_official_build=false instead of is_official_build=true in gn args to enable debug mode with symbols
- Add extra_cflags=["-DSKIA_C_DLL", "-g", "-gdwarf-2"] without -DDEBUG (which triggers zlib debug mode)
- Add extra_ldflags=["-static-libstdc++", "-static-libgcc", "-Wl,--exclude-libs,ALL"] to prevent internal zlib symbols from leaking into the shared object's dynamic symbol table
- Use dotnet cake --target=externals-linux which wraps the gn build with correct flags via build-linux.sh

### Resolution Proposals

**Hypothesis:** Building with is_official_build=false and adding -Wl,--exclude-libs,ALL to extra_ldflags produces a debug-symbol build without the z_verbose conflict.

1. **Build with is_official_build=false and hide internal symbols** — fix, confidence 0.82 (82%), cost/xs, validated=untested
   - Replace is_official_build=true with is_official_build=false in the gn args, and add -Wl,--exclude-libs,ALL to extra_ldflags to prevent bundled zlib debug symbols from leaking.

```csharp
./bin/gn gen 'out/linux/x64' --args='
    is_official_build=false skia_enable_tools=false
    target_os="linux" target_cpu="x64"
    skia_use_icu=false skia_use_sfntly=false skia_use_piex=true
    skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
    skia_enable_gpu=true
    extra_cflags=[ "-DSKIA_C_DLL", "-g", "-gdwarf-2" ]
    extra_ldflags=[ "-static-libstdc++", "-static-libgcc", "-Wl,--exclude-libs,ALL" ]
    linux_soname_version="60.1.0"'
```

**Recommended proposal:** Build with is_official_build=false and hide internal symbols

**Why:** is_official_build=false enables debug symbols for backtraces. Removing -DDEBUG avoids zlib's debug-mode z_verbose symbol, and --exclude-libs,ALL prevents any bundled library symbols from leaking into the dynamic symbol table.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | This is a usage/how-to question about the build system. The answer is known: use is_official_build=false and avoid -DDEBUG to prevent the zlib symbol conflict. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question, build, and linux labels | labels=type/question, area/Build, os/Linux |
| add-comment | high | 0.82 (82%) | Explain how to produce a debug build and fix the z_verbose symbol conflict | — |
| close-issue | medium | 0.80 (80%) | Close as answered — this is a how-to question with a known answer | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question! To build a debug version of `libSkiaSharp.so` on Linux with debug symbols:

1. Replace `is_official_build=true` with `is_official_build=false` in your gn args — this enables debug symbol generation.
2. Remove `-DDEBUG` from `extra_cflags`. The `-DDEBUG` macro activates zlib's debug mode which exposes a `z_verbose` symbol that leaks into the `.so` and conflicts with the system zlib at runtime.
3. Add `-Wl,--exclude-libs,ALL` to `extra_ldflags` to prevent symbols from any bundled static library (zlib, freetype, etc.) from leaking into the shared object's dynamic symbol table.

Example gn args:
```
./bin/gn gen 'out/linux/x64' --args='
    is_official_build=false skia_enable_tools=false
    target_os="linux" target_cpu="x64"
    skia_use_icu=false skia_use_sfntly=false skia_use_piex=true
    skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
    skia_enable_gpu=true
    extra_cflags=[ "-DSKIA_C_DLL", "-g", "-gdwarf-2" ]
    extra_ldflags=[ "-static-libstdc++", "-static-libgcc", "-Wl,--exclude-libs,ALL" ]
    linux_soname_version="60.1.0"'
```

This should produce a `libSkiaSharp.so` with DWARF debug information suitable for backtraces without the `z_verbose` symbol conflict.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2435,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T23:40:00Z"
  },
  "summary": "Reporter asks how to compile a debug build of libSkiaSharp on Linux and encounters a 'z_verbose undefined symbol' runtime error after successfully building with debug flags.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.88
    },
    "platforms": [
      "os/Linux"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Attempt to compile libSkiaSharp on Linux with extra_cflags containing -DDEBUG, -g, -gdwarf-2",
        "Build succeeds but binary has no debug symbols as expected",
        "At runtime: 'libSkiaSharp.so: error: symbol lookup error: undefined symbol: z_verbose (fatal)'"
      ],
      "codeSnippets": [
        "./bin/gn gen 'out/linux/x64' --args='is_official_build=true skia_enable_tools=false target_os=\"linux\" target_cpu=\"x64\" skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false skia_enable_gpu=true extra_cflags=[ \"-DSKIA_C_DLL\", \"-DDEBUG\", \"-g\" \"-gdwarf-2\" ] linux_soname_version=\"60.1.0\"'"
      ],
      "environmentDetails": "Linux x64, SkiaSharp 2.88.3. Runtime error: /opt/nextpvr/system/runtimes/linux-x64/native/libSkiaSharp.so: error: symbol lookup error: undefined symbol: z_verbose (fatal)"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Linux build script structure and gn build args haven't fundamentally changed since 2.88.x; the z_verbose conflict and debug build approach remain relevant."
    }
  },
  "analysis": {
    "summary": "Reporter wants to build a debug version of libSkiaSharp.so on Linux with debug symbols for backtracing. The compile succeeds with -g/-gdwarf-2 flags, but the resulting .so has a runtime symbol conflict: z_verbose is undefined because the official build statically embeds zlib (skia_use_system_zlib=false), and debug mode in zlib exposes this external symbol that conflicts when another zlib is loaded at runtime.",
    "rationale": "The title says [QUESTION] and the body asks 'Is there a way'. No broken API behavior is described. The runtime error about z_verbose is a zlib symbol-visibility issue when mixing debug zlib with the precompiled system zlib loaded by the application. The authoritative build script (scripts/build-linux.sh) uses is_official_build=true which optimizes and strips debug symbols — using is_official_build=false or adding is_debug=true would produce a debug build, but the z_verbose issue then appears because zlib's debug symbols leak into the shared object.",
    "codeInvestigation": [
      {
        "file": "scripts/build-linux.sh",
        "lines": "43-55",
        "finding": "The official Linux build uses is_official_build=true and does not pass debug flags. Changing to is_official_build=false enables debug builds. The script uses skia_use_system_zlib=false to bundle zlib — debug zlib exposes z_verbose as a non-static symbol, which conflicts with the system zlib loaded separately at runtime.",
        "relevance": "direct"
      },
      {
        "file": "scripts/build-linux.sh",
        "lines": "51",
        "finding": "extra_ldflags uses -static-libstdc++ and -static-libgcc but not -Wl,--version-script, so internal symbols like z_verbose from bundled zlib can leak into the .so's dynamic symbol table in debug builds.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "I'd like to see some if there is information in the back trace",
        "source": "issue body",
        "interpretation": "Reporter wants debug symbols for crash analysis, not a production debug build. A normal debug build with -g should suffice."
      },
      {
        "text": "symbol lookup error: undefined symbol: z_verbose (fatal)",
        "source": "issue comment",
        "interpretation": "This is a zlib symbol leakage issue: bundled zlib's debug-mode z_verbose symbol leaks into libSkiaSharp.so's dynamic table, and then a different zlib loaded at runtime doesn't have it, causing the conflict."
      }
    ],
    "workarounds": [
      "Use is_official_build=false instead of is_official_build=true in gn args to enable debug mode with symbols",
      "Add extra_cflags=[\"-DSKIA_C_DLL\", \"-g\", \"-gdwarf-2\"] without -DDEBUG (which triggers zlib debug mode)",
      "Add extra_ldflags=[\"-static-libstdc++\", \"-static-libgcc\", \"-Wl,--exclude-libs,ALL\"] to prevent internal zlib symbols from leaking into the shared object's dynamic symbol table",
      "Use dotnet cake --target=externals-linux which wraps the gn build with correct flags via build-linux.sh"
    ],
    "resolution": {
      "hypothesis": "Building with is_official_build=false and adding -Wl,--exclude-libs,ALL to extra_ldflags produces a debug-symbol build without the z_verbose conflict.",
      "proposals": [
        {
          "title": "Build with is_official_build=false and hide internal symbols",
          "description": "Replace is_official_build=true with is_official_build=false in the gn args, and add -Wl,--exclude-libs,ALL to extra_ldflags to prevent bundled zlib debug symbols from leaking.",
          "codeSnippet": "./bin/gn gen 'out/linux/x64' --args='\n    is_official_build=false skia_enable_tools=false\n    target_os=\"linux\" target_cpu=\"x64\"\n    skia_use_icu=false skia_use_sfntly=false skia_use_piex=true\n    skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false\n    skia_enable_gpu=true\n    extra_cflags=[ \"-DSKIA_C_DLL\", \"-g\", \"-gdwarf-2\" ]\n    extra_ldflags=[ \"-static-libstdc++\", \"-static-libgcc\", \"-Wl,--exclude-libs,ALL\" ]\n    linux_soname_version=\"60.1.0\"'",
          "category": "fix",
          "confidence": 0.82,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Build with is_official_build=false and hide internal symbols",
      "recommendedReason": "is_official_build=false enables debug symbols for backtraces. Removing -DDEBUG avoids zlib's debug-mode z_verbose symbol, and --exclude-libs,ALL prevents any bundled library symbols from leaking into the dynamic symbol table."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "This is a usage/how-to question about the build system. The answer is known: use is_official_build=false and avoid -DDEBUG to prevent the zlib symbol conflict.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, build, and linux labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/Build",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain how to produce a debug build and fix the z_verbose symbol conflict",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thanks for the question! To build a debug version of `libSkiaSharp.so` on Linux with debug symbols:\n\n1. Replace `is_official_build=true` with `is_official_build=false` in your gn args — this enables debug symbol generation.\n2. Remove `-DDEBUG` from `extra_cflags`. The `-DDEBUG` macro activates zlib's debug mode which exposes a `z_verbose` symbol that leaks into the `.so` and conflicts with the system zlib at runtime.\n3. Add `-Wl,--exclude-libs,ALL` to `extra_ldflags` to prevent symbols from any bundled static library (zlib, freetype, etc.) from leaking into the shared object's dynamic symbol table.\n\nExample gn args:\n```\n./bin/gn gen 'out/linux/x64' --args='\n    is_official_build=false skia_enable_tools=false\n    target_os=\"linux\" target_cpu=\"x64\"\n    skia_use_icu=false skia_use_sfntly=false skia_use_piex=true\n    skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false\n    skia_enable_gpu=true\n    extra_cflags=[ \"-DSKIA_C_DLL\", \"-g\", \"-gdwarf-2\" ]\n    extra_ldflags=[ \"-static-libstdc++\", \"-static-libgcc\", \"-Wl,--exclude-libs,ALL\" ]\n    linux_soname_version=\"60.1.0\"'\n```\n\nThis should produce a `libSkiaSharp.so` with DWARF debug information suitable for backtraces without the `z_verbose` symbol conflict."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — this is a how-to question with a known answer",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
