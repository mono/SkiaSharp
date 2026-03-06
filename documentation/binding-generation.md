# Binding Generation Guide

This document describes how SkiaSharp's P/Invoke bindings are generated from C headers using the `SkiaSharpGenerator` tool.

## Overview

The binding generation system parses C headers (for Skia, HarfBuzz, etc.) and generates C# P/Invoke code. The process is controlled by JSON configuration files that specify type mappings, function customizations, and exclusions.

**Key Components:**
- `utils/SkiaSharpGenerator/` — The generator tool source code
- `binding/*.json` — Configuration files for each binding library
- `binding/*/*.generated.cs` — Output files (auto-generated, do not edit manually)

**Note:** The generated P/Invoke class is always `internal unsafe partial class`. P/Invoke function names are kept as the original C names and are not cleaned or renamed.

## Running the Generator

### Generate All Bindings

```bash
pwsh ./utils/generate.ps1
```

### Generate a Single Binding

```bash
pwsh ./utils/generate.ps1 -Config libHarfBuzzSharp.json
```

Valid config options:
- `libHarfBuzzSharp.json` — HarfBuzzSharp bindings
- `libSkiaSharp.json` — Main SkiaSharp bindings
- `libSkiaSharp.Skottie.json` — Skottie animation bindings
- `libSkiaSharp.SceneGraph.json` — Scene graph bindings
- `libSkiaSharp.Resources.json` — Resources bindings

### Direct Invocation

```bash
dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate \
    --config binding/libHarfBuzzSharp.json \
    --root externals/skia/third_party/externals/harfbuzz \
    --output binding/HarfBuzzSharp/HarfBuzzApi.generated.cs
```

## Configuration File Structure

The JSON configuration files control every aspect of binding generation. Here's the complete schema with all available options:

```json
{
  "dllName": "HARFBUZZ",           // DLL constant name used in [DllImport]
  "namespace": "HarfBuzzSharp",   // Root C# namespace
  "className": "HarfBuzzApi",     // Name of the class containing P/Invoke methods
  "includeDirs": ["src"],         // Additional include directories for C parser
  "headers": { ... },             // Header files to parse
  "source": { ... },              // Source file patterns (for reference)
  "namespaces": { ... },          // Prefix-to-namespace mappings
  "exclude": { ... },             // Files and types to exclude
  "mappings": {
    "types": { ... },             // Type customizations
    "functions": { ... }          // Function customizations
  }
}
```

## Configuration Options Reference

### Top-Level Options

| Option | Type | Description |
|--------|------|-------------|
| `dllName` | string | The constant name used in `[DllImport(DLLNAME)]`. Should match a constant defined in the hand-written code. |
| `namespace` | string | The root C# namespace for all generated code. |
| `className` | string | The name of the partial class that contains all P/Invoke method declarations. |
| `includeDirs` | string[] | Additional directories to add to the C parser's include path, relative to the source root. |

### headers

Specifies which header files to parse. The key is a directory path (relative to source root), and the value is an array of glob patterns.

```json
"headers": {
  "src": ["hb.h", "hb-ot.h"],
  "include/c": ["sk_*", "gr_*"]
}
```

The generator will:
1. Add each key directory to the include path
2. Find all files matching the patterns in that directory
3. Parse those files with CppAst

### source

Specifies source file patterns for the **verify** command. The verifier checks that header declarations have corresponding implementations in these source files.

```json
"source": {
  "src": ["hb-*"]
}
```

**Note:** Patterns use `Directory.EnumerateFiles` semantics (simple `*` and `?` wildcards within the directory), not recursive `**` globs.

### namespaces

Maps C name prefixes to C# namespace and naming customizations. This affects:
- **Type names:** The `prefix` is prepended after removing the C prefix
- **Type namespaces:** Types are placed in `{namespace}.{cs}` sub-namespace
- **Exclusion:** Types/functions with matching prefix can be excluded entirely

**Note:** Namespace mappings affect generated *types* (structs, enums, delegates), not P/Invoke functions. P/Invoke functions always remain in the root namespace class with their original C names.

```json
"namespaces": {
  "hb_ot_": {
    "prefix": "OpenType",
    "cs": "OpenType"
  },
  "hb_": {
    "prefix": ""
  },
  "sk_": {
    "prefix": "SK",
    "exclude": true
  }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `prefix` | string | String to prepend to cleaned type names after removing the C prefix. |
| `cs` | string | C# sub-namespace name. Generated types go into `{namespace}.{cs}`. |
| `exclude` | bool | If `true`, types/functions with this prefix are excluded from generation (except `using` aliases for opaque handles). |

**Important:** Namespace mappings are checked in order. More specific prefixes (like `hb_ot_`) should come before less specific ones (like `hb_`).

### exclude

Specifies files and types to exclude from generation.

```json
"exclude": {
  "files": [
    "src/hb-deprecated.h",
    "src/hb-shape-plan.h"
  ],
  "types": [
    "hb_segment_properties_t",
    "hb_user_data_key_t"
  ]
}
```

| Property | Type | Description |
|----------|------|-------------|
| `files` | string[] | Header files to skip entirely. Paths are relative to source root. |
| `types` | string[] | Type names to exclude. Pointer variants (`T*`, `T**`) are automatically excluded too. Functions using these types as parameters are also skipped. |

**When to use exclusions:**
- Deprecated APIs that shouldn't be exposed
- Types that require complex manual interop
- Internal implementation types not meant for public use

**Note:** Excluded types still have their `using Type = System.IntPtr;` aliases generated for opaque handles. The exclusion primarily affects struct/enum generation and functions using those types as parameters.

### mappings.types

Customizes how C types are mapped to C# types. This is the most commonly used section.

```json
"mappings": {
  "types": {
    "hb_bool_t": {
      "cs": "Boolean"
    },
    "hb_buffer_flags_t": {
      "flags": true,
      "members": {
        "HB_BUFFER_FLAG_BOT": "BeginningOfText",
        "HB_BUFFER_FLAG_EOT": "EndOfText"
      }
    }
  }
}
```

#### Type Mapping Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `cs` | string | (auto) | The C# type name to use. If not specified, the generator cleans the C name automatically. |
| `internal` | bool | `false` | If `true`, generates `internal` visibility instead of `public`. Internal structs have public fields but no public properties. |
| `flags` | bool | `false` | If `true`, adds `[Flags]` attribute to the generated enum. |
| `obsolete` | bool | `false` | If `true`, adds `[Obsolete]` attribute. **Only applies to enums.** |
| `properties` | bool | `true` | If `true`, generates public properties for struct fields (unless `internal` is also true). If `false`, only backing fields are generated with no public properties. |
| `generate` | bool | `true` | If `false`, skips generating this type. **Only applies to enums.** For structs, use `exclude.types` instead. |
| `readonly` | bool | `false` | If `true`, generates the struct as `readonly struct` and properties as `readonly`. |
| `equality` | bool | `true` | If `true`, generates `IEquatable<T>` implementation with `Equals()`, `GetHashCode()`, and `==`/`!=` operators. **Only applies to structs.** |
| `members` | object | `{}` | Maps C enum/struct member names to C# names. Empty string `""` hides the property (but the backing field is still generated for structs). |

#### Type Mapping Examples

**Simple type alias:**
```json
"hb_codepoint_t": {
  "cs": "UInt32"
}
```

**Enum with flags and renamed members:**
```json
"hb_buffer_flags_t": {
  "flags": true,
  "members": {
    "HB_BUFFER_FLAG_BOT": "BeginningOfText",
    "_HB_BUFFER_FLAG_MAX_VALUE": ""  // Exclude this member
  }
}
```

**Internal struct without properties:**
```json
"sk_imageinfo_t": {
  "cs": "SKImageInfoNative",
  "internal": true
}
```

**Readonly struct:**
```json
"sk_color4f_t": {
  "cs": "SKColorF",
  "readonly": true,
  "members": {
    "fR": "Red",
    "fG": "Green",
    "fB": "Blue",
    "fA": "Alpha"
  }
}
```

**Skip generation (manual definition):**
```json
"hb_script_t": {
  "cs": "UInt32",
  "generate": false
}
```

### mappings.functions

Customizes how C functions and function pointer types are mapped.

```json
"mappings": {
  "functions": {
    "hb_blob_create_from_file": {
      "parameters": {
        "0": "[MarshalAs (UnmanagedType.LPStr)] String"
      }
    },
    "hb_destroy_func_t": {
      "proxySuffixes": ["", "ForMulti"]
    }
  }
}
```

#### Function Mapping Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `cs` | string | (auto) | The C# name to use for **callback typedefs and delegate proxies only**. Does not rename P/Invoke functions. |
| `parameters` | object | `{}` | Maps parameter index to C# type override. Use `"-1"` for return type. |
| `generateProxy` | bool/null | null (treated as true) | If `false`, skips generating the delegate proxy for this callback type. |
| `proxySuffixes` | string[]/null | null (treated as `[""]`) | Generates multiple proxy implementations with different suffixes. Useful when the same callback needs different implementations. |

**Important:** The `cs` property in function mappings only affects **callback typedef names** and **delegate proxy names**. P/Invoke function names are never renamed — they always use the original C function name.

#### Function Mapping Examples

**Override parameter type for marshaling:**
```json
"hb_blob_create_from_file": {
  "parameters": {
    "0": "[MarshalAs (UnmanagedType.LPStr)] String"
  }
}
```

**Override return type:**
```json
"hb_unicode_combining_class_func_t": {
  "parameters": {
    "-1": "int"
  }
}
```

**Skip proxy generation:**
```json
"hb_buffer_message_func_t": {
  "generateProxy": false
}
```

**Multiple proxy implementations:**
```json
"hb_destroy_func_t": {
  "proxySuffixes": ["", "ForMulti"]
}
```

This generates both `DestroyProxy` and `DestroyProxyForMulti` implementations.

## Generator Internals

### Name Cleaning

The generator automatically cleans C names to produce idiomatic C# names. The algorithm handles both type names and enum field names differently.

#### Type Name Cleaning (`CleanName`)

1. **Trim leading underscores** (`_type` → `type`)
2. **Remove `_t` suffix** for types (`hb_buffer_t` → `hb_buffer`)
3. **Remove `_func`/`_proc` suffix** and add `ProxyDelegate` suffix
4. **Remove namespace prefix** and add configured prefix (`hb_buffer` → `Buffer` if `hb_` prefix maps to `""`)
5. **Convert to PascalCase**:
   - For mixed-case names: split on uppercase boundaries then underscores
   - For all-lowercase/uppercase: split on underscores
6. **Remove initial `f` segment** for struct fields (e.g., `fRed` → `Red`)

#### Enum Field Cleaning (`CleanEnumFieldName`)

1. **Remove enum type prefix/suffix** from the field name
2. **Handle `_flags` enum special case** (also checks for `_flag` variant)
3. **Strip `_sk_` or `_gr_` trailing segments** (Skia-specific workaround)
4. **Apply `CleanName`** to the result

Example: For enum `hb_buffer_flags_t` with member `HB_BUFFER_FLAGS_BOT`:
- Strips `hb_buffer_flags` prefix → `_BOT`
- CleanName → `Bot`

#### Keyword Escaping

C# keywords in parameter names are automatically prefixed with `@`:
- `out` → `@out`
- `in` → `@in`  
- `ref` → `@ref`
- `var` → `@var`

### Standard Type Mappings

The generator has built-in mappings for standard C types. Here are the key mappings (see `BaseTool.cs:124-169` for the complete list):

| C Type | C# Type |
|--------|---------|
| `uint8_t` | `Byte` |
| `uint16_t` | `UInt16` |
| `uint32_t` | `UInt32` |
| `uint64_t` | `UInt64` |
| `int8_t` | `SByte` |
| `int16_t` | `Int16` |
| `int32_t` | `Int32` |
| `int64_t` | `Int64` |
| `size_t` | `/* size_t */ IntPtr` |
| `usize_t` | `/* usize_t */ UIntPtr` |
| `intptr_t` | `IntPtr` |
| `uintptr_t` | `UIntPtr` |
| `bool` | `Byte` |
| `float` | `Single` |
| `double` | `Double` |
| `char` | `/* char */ void` |
| `void` | `void` |

**Note:** `bool` is mapped to `Byte` in the standard mappings. The marshaling attribute `[MarshalAs(UnmanagedType.I1)] bool` is added separately during P/Invoke function generation when the parameter or return type is boolean.

**Note:** `char`, `unsigned char`, and `signed char` map to `void` (with comments), so pointers like `char*` become `void*`. Use parameter overrides for string marshaling.

### Automatic Field Visibility

Struct fields have automatic visibility rules:
- Fields starting with `_private_` are made private (prefix is stripped from the name)
- Fields starting with `reserved` are made private

### Generated Code Structure

For each library, the generator produces:

1. **Class declarations** — `using` aliases for opaque handle types
2. **P/Invoke functions** — Static methods in the API class (names kept as C originals)
3. **Delegates** — Managed delegate types for callbacks
4. **Structs** — Value types for C structs
5. **Enums** — C# enums for C enums
6. **Delegate proxies** — Helper implementations for common callback patterns

### Build Configurations

The generated code supports three compilation modes via preprocessor defines:

1. **`USE_LIBRARY_IMPORT`** — Uses `[LibraryImport]` attribute (.NET 7+)
2. **Default (no define)** — Uses `[DllImport]` with `CallingConvention.Cdecl`
3. **`USE_DELEGATES`** — Uses runtime delegate loading via `UnmanagedFunctionPointer`

**Note:** For WASM compatibility, function pointer parameters in P/Invoke signatures are converted to `void*` even when `USE_LIBRARY_IMPORT` is enabled.

## Common Patterns

### Adding a New Type Alias

For simple typedefs that should map directly to a C# primitive:

```json
"hb_tag_t": {
  "cs": "UInt32"
}
```

### Making an Enum Flags-based

```json
"hb_buffer_serialize_flags_t": {
  "flags": true,
  "cs": "SerializeFlag"
}
```

### Excluding Deprecated APIs

Add to the exclude section:

```json
"exclude": {
  "files": ["src/hb-deprecated.h"]
}
```

### Handling String Parameters

C functions with `const char*` parameters need marshaling:

```json
"hb_language_from_string": {
  "parameters": {
    "0": "[MarshalAs (UnmanagedType.LPStr)] String"
  }
}
```

### Hiding Struct Member Properties

For structs where some member properties should not be exposed (the backing field is still generated):

```json
"hb_glyph_info_t": {
  "members": {
    "var1": "",
    "var2": ""
  }
}
```

Empty string means "hide the property" — the private backing field is still generated for correct struct layout.

### Creating Internal-Only Types

For types used only internally by the binding layer:

```json
"sk_codec_options_t": {
  "cs": "SKCodecOptionsInternal",
  "internal": true
}
```

**Note:** Internal structs have public fields but no public properties, regardless of the `properties` setting.

## Troubleshooting

### Parser Errors

If the generator fails with parse errors:

1. Ensure all header dependencies are available
2. Check `includeDirs` includes all necessary paths
3. On macOS, ensure Xcode Command Line Tools are installed

### Missing Types

If a type isn't generated:

1. Check if it's in an excluded file
2. Check if it's in an excluded namespace
3. Check if it uses an excluded type as a parameter

### Name Conflicts

If a generated name conflicts with a C# keyword or existing type:

1. Use the `cs` property to specify a different name
2. For parameters, the generator automatically prefixes with `@` for keywords like `out`, `in`, `ref`, `var`

## Version-Specific Notes

### HarfBuzz Binding

The HarfBuzz binding configuration excludes:
- `hb-deprecated.h` — Deprecated APIs
- `hb-shape-plan.h` — Complex internal type
- `hb-ot-deprecated.h` — Deprecated OpenType APIs
- `hb-ot-var.h` — Variable font APIs (complex)

Key type aliases:
- `hb_bool_t` → `Boolean`
- `hb_codepoint_t` → `UInt32`
- `hb_position_t` → `Int32`
- `hb_tag_t` → `UInt32`
- `hb_language_t` → `IntPtr` (opaque pointer)

## Workflow: Updating Bindings

1. **Update the native library** in `externals/skia/DEPS`
2. **Sync dependencies**: `dotnet cake --target=git-sync-deps`
3. **Run the generator**: `pwsh ./utils/generate.ps1 -Config <config>.json`
4. **Review the diff** in the generated file
5. **Adjust configuration** if new types/functions need customization
6. **Repeat steps 3-5** until the output is correct
7. **Build and test**: `dotnet build` and `dotnet test`
8. **Commit** both the config changes and generated output

## See Also

- `utils/SkiaSharpGenerator/ConfigJson/` — Configuration model source
- `utils/SkiaSharpGenerator/Generate/Generator.cs` — Generation logic
- `utils/SkiaSharpGenerator/BaseTool.cs` — Type mapping and cleaning logic

## CppAst Parser Configuration

The binding generator uses CppAst (version 0.24.0) to parse C headers. The generator automatically configures the parser with appropriate system include paths.

### Cross-Platform Include Paths

The generator automatically detects and configures system include paths:

**macOS:**
- Clang include path: `/Library/Developer/CommandLineTools/usr/lib/clang/<version>/include`
- SDK include path: `/Library/Developer/CommandLineTools/SDKs/MacOSX.sdk/usr/include` or Xcode SDK path

**Linux:**
- `/usr/include`
- `/usr/local/include`

**Windows:**
- Windows Kits ucrt path: `Program Files/Windows Kits/10/Include/<version>/ucrt`
