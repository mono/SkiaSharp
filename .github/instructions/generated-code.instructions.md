---
applyTo: "binding/SkiaSharp/**/*.generated.cs,binding/SkiaSharp/**/SkiaApi.generated.cs"
---

# Generated Code Instructions

You are viewing or working near **GENERATED CODE**.

## Critical Rules

- ⛔ **DO NOT manually edit generated files**
- Look for generation markers/comments at the top of files
- To modify generated code, change the generation templates/configs instead
- Document generation source in commit messages

## If You Need to Change Generated Code

### Step 1: Find the Generator
Located in: `utils/SkiaSharpGenerator/`

### Step 2: Modify Template or Config
```bash
# Regenerate after changes
dotnet run --project utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate
```

### Step 3: Verify Output
- Check generated code matches expectations
- Ensure no unintended changes
- Test the modified bindings

## What You CAN Do

✅ **Add hand-written wrappers** in separate files
✅ **Add convenience overloads** in non-generated files
✅ **Reference generated code** from hand-written code

## What You CANNOT Do

❌ **Manually edit generated P/Invoke declarations**
❌ **Add custom logic to generated files**
❌ **Modify generated file directly** (changes will be lost)
