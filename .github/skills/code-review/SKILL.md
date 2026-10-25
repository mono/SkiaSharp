---
name: code-review
description: Review SkiaSharp pull requests for correctness, interop and ownership safety, ABI stability, generated binding consistency, and adequate validation. Use whenever Copilot code review analyzes a pull request in this repository.
allowed-tools: bash
---

# SkiaSharp code review

Read the repository `AGENTS.md` before reviewing. Report only actionable defects introduced by the
pull request, with the affected changed line and a concrete failure scenario.

## Classify the change before validating

Use the pull request diff to choose the bootstrap path:

- **Managed-only changes:** download the current prebuilt native artifacts with
  `bash scripts/infra/cloud/download-prebuilt-natives.sh`.
- **Native, C API, dependency, version, or `externals/skia` changes:** never download prebuilt
  natives. If runtime validation is needed, build the changed Linux native source with
  `dotnet cake --target=externals-linux --arch=x64`.
- **Agentic workflow changes:** compile the changed workflow source with
  `gh aw compile <workflow-id>` and inspect its generated `.lock.yml`.

The review environment already has repository dotnet tools, the Android workload, Clang/libc++,
fonts, Xvfb, and deterministic Mesa software OpenGL/Vulkan.

## Review priorities

1. Trace managed wrappers through generated P/Invoke and the C API when those layers changed.
2. Check native ownership, `owns:` flags, reference counting, same-instance returns, pinned-memory
   lifetimes, and disposal paths.
3. Reject public API removals or signature changes. New public APIs need accurate source `///`
   documentation, generated bindings where applicable, and tests.
4. Confirm C# validates inputs while the C API remains a thin trusted pass-through.
5. Check workflow and provisioning changes for host/container boundaries, persisted environment
   variables, strict failures, and stale-native hazards.
6. Require tests that exercise the changed behavior. Do not accept skipped failures or fallback
   artifact/tool discovery.

## Validation

Run the smallest command that can disprove the implementation, then expand only when needed:

- Managed build: `dotnet build binding/SkiaSharp/SkiaSharp.csproj`
- Final tests: `dotnet test tests/SkiaSharp.Tests.Console.slnx -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0`
- Shell changes: `shellcheck <changed-shell-scripts>`
- Repository dependency map: `python3 scripts/infra/caching/repo-deps.py validate`
- Workflow safety: `python3 -m unittest discover -s .github/scripts/tests -p "test_*.py" -v`

Do not claim a command passed unless you ran it. A failing or unavailable command is evidence to
interpret, not automatically a review finding; report the underlying defect only when it is caused
by the pull request.
