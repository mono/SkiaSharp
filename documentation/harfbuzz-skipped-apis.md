# HarfBuzz Skipped APIs

This document lists all HarfBuzz APIs that were excluded from the C# bindings and the rationale for each.

---

## Summary

| Category | Count | Reason |
|----------|-------|--------|
| Paint API | ~25+ functions | Complex ColorLine callback struct |
| Deprecated | ~10 functions | Excluded via file |
| Shape Plan | ~8 functions | Excluded via file |
| Variation (hb-ot-var) | ~15 functions | Excluded via file |

---

## Paint API (`hb_paint_*`, `hb_color_line_*`)

**Excluded via namespace:** `hb_paint_`, `hb_color_line_`

### Why Excluded

The Paint API introduced in HarfBuzz 7.0.0 provides a way to render COLRv1 color fonts. However, the API design uses a callback-based approach with the `hb_color_line_t` struct, which contains function pointer fields:

```c
typedef struct hb_color_line_t {
  void *data;
  hb_color_line_get_color_stops_func_t get_color_stops;
  void *get_color_stops_user_data;
  hb_color_line_get_extend_func_t get_extend;
  void *get_extend_user_data;
} hb_color_line_t;
```

This causes issues with the binding generator because:
1. In `USE_LIBRARY_IMPORT` mode, function pointers are represented as `delegate* unmanaged[Cdecl]<...>`
2. In standard mode, they're represented as managed delegate types
3. The struct property accessors reference the managed delegate type even in `USE_LIBRARY_IMPORT` mode, causing compilation errors

### Excluded Types

| Type | Description |
|------|-------------|
| `hb_color_line_t` | Color line with gradient stops |
| `hb_color_stop_t` | Individual color stop |
| `hb_paint_funcs_t` | Paint callbacks container |
| `hb_paint_extend_t` | Gradient extend mode enum |
| `hb_paint_composite_mode_t` | Compositing mode enum |
| `hb_color_line_get_color_stops_func_t` | Callback typedef |
| `hb_color_line_get_extend_func_t` | Callback typedef |
| All `hb_paint_*_func_t` | Callback typedefs |

### Excluded Functions

All functions in `hb-paint.h`:
- `hb_paint_funcs_create`
- `hb_paint_funcs_get_empty`
- `hb_paint_funcs_reference`
- `hb_paint_funcs_destroy`
- `hb_paint_funcs_set_user_data`
- `hb_paint_funcs_get_user_data`
- `hb_paint_funcs_make_immutable`
- `hb_paint_funcs_is_immutable`
- `hb_paint_funcs_set_push_transform_func`
- `hb_paint_funcs_set_pop_transform_func`
- `hb_paint_funcs_set_color_func`
- `hb_paint_funcs_set_image_func`
- `hb_paint_funcs_set_linear_gradient_func`
- `hb_paint_funcs_set_radial_gradient_func`
- `hb_paint_funcs_set_sweep_gradient_func`
- `hb_paint_funcs_set_push_clip_glyph_func`
- `hb_paint_funcs_set_push_clip_rectangle_func`
- `hb_paint_funcs_set_pop_clip_func`
- `hb_paint_funcs_set_push_group_func`
- `hb_paint_funcs_set_pop_group_func`
- `hb_paint_funcs_set_custom_palette_color_func`
- `hb_font_paint_glyph`

### How to Potentially Generate

**Option 1: Custom P/Invoke wrapper**
Write manual C# code that marshals the struct with function pointers using `Marshal.GetDelegateForFunctionPointer` and `Marshal.GetFunctionPointerForDelegate`.

**Option 2: Modify generator**
Add support for struct fields that are function pointers, generating appropriate marshaling code for both modes.

**Option 3: C shim layer**
Create a C shim that provides a simpler callback interface without embedded function pointers in structs.

---

## Draw API Callbacks

**Status:** Functions generated, but proxy implementations skipped

The Draw API provides glyph outline drawing callbacks. The functions are generated but the managed proxy delegates (for easy C# consumption) are skipped.

### Proxy Generation Skipped

| Typedef | Why Skipped |
|---------|-------------|
| `hb_draw_close_path_func_t` | Complex callback with draw state |
| `hb_draw_cubic_to_func_t` | Complex callback with draw state |
| `hb_draw_line_to_func_t` | Complex callback with draw state |
| `hb_draw_move_to_func_t` | Complex callback with draw state |
| `hb_draw_quadratic_to_func_t` | Complex callback with draw state |

### How to Use

The raw P/Invoke functions are available. To use them:
1. Create a managed delegate matching the signature
2. Use `Marshal.GetFunctionPointerForDelegate` to get the function pointer
3. Pass to `hb_draw_funcs_set_*` functions

---

## Deprecated APIs

**Excluded via file:** `src/hb-deprecated.h`, `src/hb-ot-deprecated.h`

### Why Excluded

These contain deprecated functions that have replacements. Including them would:
1. Add unnecessary API surface
2. Confuse users about which APIs to use
3. Risk breaking changes when deprecation becomes removal

### Notable Deprecated Functions

| Function | Replacement |
|----------|-------------|
| `hb_font_get_glyph_shape` | `hb_font_draw_glyph` |
| `hb_font_funcs_set_glyph_shape_func` | `hb_font_funcs_set_draw_glyph_func` |
| Various font funcs | Newer callback-based APIs |

---

## Shape Plan API

**Excluded via file:** `src/hb-shape-plan.h`

### Why Excluded

The shape plan API is an advanced optimization API that caches shaping plans. Most users don't need it because:
1. HarfBuzz internally caches plans automatically
2. The API is complex and easy to misuse
3. Incorrect use can lead to wrong shaping results

### Excluded Functions

- `hb_shape_plan_create`
- `hb_shape_plan_create_cached`
- `hb_shape_plan_create2`
- `hb_shape_plan_get_empty`
- `hb_shape_plan_reference`
- `hb_shape_plan_destroy`
- `hb_shape_plan_set_user_data`
- `hb_shape_plan_get_user_data`
- `hb_shape_plan_execute`
- `hb_shape_plan_get_shaper`

### How to Potentially Generate

The API is straightforward P/Invoke. To add:
1. Remove `src/hb-shape-plan.h` from `exclude.files`
2. Regenerate
3. Create a C# wrapper class with proper disposal

---

## Variation API (hb-ot-var)

**Excluded via file:** `src/hb-ot-var.h`

### Why Excluded

Variable font (OpenType Font Variations) support is partially available through other APIs but the full variation API was excluded because:
1. Complex axis and instance management
2. Some functions require understanding of OpenType font tables
3. Limited testing capability without variable fonts

### Excluded Functions

- `hb_ot_var_has_data`
- `hb_ot_var_get_axis_count`
- `hb_ot_var_get_axis_infos`
- `hb_ot_var_find_axis_info`
- `hb_ot_var_get_named_instance_count`
- `hb_ot_var_named_instance_get_subfamily_name_id`
- `hb_ot_var_named_instance_get_postscript_name_id`
- `hb_ot_var_named_instance_get_design_coords`
- `hb_ot_var_normalize_variations`
- `hb_ot_var_normalize_coords`

### How to Potentially Generate

1. Remove `src/hb-ot-var.h` from `exclude.files`
2. Add type mappings for `hb_ot_var_axis_info_t`, `hb_ot_var_axis_flags_t`
3. Regenerate
4. Create C# wrapper classes for axis information

---

## Internal/Private Types

These types are excluded because they're internal implementation details:

| Type | Reason |
|------|--------|
| `hb_segment_properties_t` | Internal shaping state |
| `hb_user_data_key_t` | User data mechanism (advanced) |
| `_hb_var_int_t` | Internal union type |

---

## Generator Limitations Requiring Future Work

### 1. Function Pointer Fields in Structs

**Current limitation:** When a struct has function pointer fields, the generator creates incompatible code between `USE_LIBRARY_IMPORT` and standard modes.

**Fix needed:** Generate mode-specific struct definitions or use `IntPtr` for function pointer fields with manual marshaling.

### 2. Callback Proxy Generation

**Current limitation:** Complex callbacks with void pointer user data and nested callbacks are hard to wrap in managed code.

**Fix needed:** Add support for generating proxy implementations with GCHandle-based user data management.

### 3. Header Include Chain

**Current limitation:** Excluding a header file only works for directly enumerated files, not transitively included ones.

**Fix needed:** Track the source file of each declaration and filter based on that.

---

## Version Information

- **Generated from:** HarfBuzz 8.3.0
- **Target versions covered:** 2.8.2 through 8.3.0
- **Last updated:** During migration work
