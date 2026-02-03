# Phase Checklists

Use these checklists to verify each phase is complete before proceeding.

## Before Starting (MANDATORY)

- [ ] **NOT on `main` or `skiasharp` branch** — Check with `git branch`
- [ ] **Created feature branch:** `git checkout -b dev/issue-NNNN-description`
- [ ] **Submodule on feature branch:** `cd externals/skia && git checkout -b dev/issue-NNNN-description`

## After Phase 1 (Analyze C++)

- [ ] Found C++ API in Skia headers
- [ ] Identified pointer type: raw / owned / ref-counted (virtual or non-virtual)
- [ ] Noted error handling: returns null? returns bool? can throw?
- [ ] Documented parameter types and const-ness

## After Phase 2 (C API)

- [ ] Header in `externals/skia/include/c/sk_*.h` with `SK_C_API`
- [ ] Implementation in `externals/skia/src/c/sk_*.cpp`
- [ ] Uses `AsType()`/`ToType()` conversion macros
- [ ] Ref-counted parameters use `sk_ref_sp()`
- [ ] Ref-counted returns use `.release()`

## After Phase 3 (Commit Submodule)

- [ ] Git configured in submodule (`git config user.email/name`)
- [ ] Changes committed IN submodule (`cd externals/skia && git commit`)
- [ ] Submodule staged in parent (`git add externals/skia`)
- [ ] Verified: `git status` shows "modified: externals/skia (new commits)"

## After Phase 4 (Generate)

- [ ] Ran `pwsh ./utils/generate.ps1`
- [ ] Verified `SkiaApi.generated.cs` contains new function
- [ ] Did NOT manually edit any `*.generated.cs` file

## After Phase 5 (C# Wrapper)

- [ ] Added wrapper in `binding/SkiaSharp/SK*.cs`
- [ ] Validates null parameters with `ArgumentNullException`
- [ ] Factory methods return null on failure
- [ ] Constructors throw on failure
- [ ] Uses overload chains (not default parameters) for ABI stability

## After Phase 6 (Test)

- [ ] Added test in `tests/Tests/SkiaSharp/`
- [ ] Test uses `[SkippableFact]` attribute
- [ ] Test uses `using` statements for all disposables
- [ ] Build passes: `dotnet build binding/SkiaSharp/SkiaSharp.csproj`
- [ ] **Tests pass**: `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj`
