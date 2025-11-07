---
applyTo: "externals/skia/include/**/*.h,externals/skia/src/**/*.cpp,!externals/skia/include/c/**,!externals/skia/src/c/**"
---

# Native Skia C++ Instructions

You are viewing native Skia C++ code. This is **upstream code** and should generally **NOT be modified directly**.

## Understanding This Code

- This is the source C++ library that SkiaSharp wraps
- Pay attention to pointer types in function signatures
- Note: `sk_sp<T>` is a smart pointer with reference counting
- Note: Raw `T*` may be owning or non-owning (check docs/context)

## Pointer Type Identification

### Smart Pointers (Ownership)
- **`sk_sp<T>`** - Skia Smart Pointer (Reference Counted)
- **`std::unique_ptr<T>`** - Unique Ownership

### Reference Counting
- **`SkRefCnt`** base class → Reference counted
- Methods: `ref()` increment, `unref()` decrement

### Raw Pointers
- **`const T*` or `const T&`** → Usually non-owning, read-only
- **`T*`** → Could be owning or non-owning (requires context)

## If Creating Bindings

1. Identify pointer type from C++ signature
2. Create C API wrapper in `externals/skia/src/c/`
3. Handle ownership transfer appropriately
4. Ensure exceptions can't escape to C boundary

## What NOT to Do

❌ **Don't modify upstream Skia code** unless contributing upstream
❌ **Don't assume pointer ownership** without checking
❌ **Don't create C API here** - use `externals/skia/src/c/` instead
