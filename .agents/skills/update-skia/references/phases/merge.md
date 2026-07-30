# Phases 4–5: Branch and merge

Read only the dependency and merge-strategy sections (8, 13–15) of
[../known-gotchas.md](../known-gotchas.md).

## Phase 4 — branch from the resolved bases

Parent:

```bash
git fetch origin {BASE_BRANCH}
git checkout -b {HEAD_BRANCH} origin/{BASE_BRANCH}
```

Submodule:

```bash
BASE_SUB_SHA=$(git -C ../.. ls-tree "origin/{BASE_BRANCH}" -- externals/skia | awk '{print $3}')
git fetch origin {SKIA_BASE_BRANCH}
git checkout "$BASE_SUB_SHA"
git branch -r --contains "$BASE_SUB_SHA" | grep "origin/{SKIA_BASE_BRANCH}"
git checkout -b {HEAD_BRANCH}
```

Automation may already have aligned the submodule. Still verify both branches and bases.

## Phase 5 — genuine upstream merge

Before merging:

```bash
MB=$(git merge-base {SKIA_BASE_BRANCH} upstream/{UPSTREAM_REF})
git log --oneline "$MB..{SKIA_BASE_BRANCH}" > "$ARTIFACT_DIR/fork-patches-before.txt"
git merge --no-commit upstream/{UPSTREAM_REF}
```

If conflicts occur, batch the audit **before resolving any file**:

```bash
git diff --name-only --diff-filter=U > "$ARTIFACT_DIR/conflicted-files.txt"
while IFS= read -r file; do
  printf '\n## %s\n' "$file"
  git log --oneline "$MB..{SKIA_BASE_BRANCH}" -- "$file"
done < "$ARTIFACT_DIR/conflicted-files.txt" > "$ARTIFACT_DIR/conflict-fork-history.md"
```

Classify every fork patch:

- **Upstreamed** — take upstream's equivalent or improved form and record its SHA.
- **Not upstreamed** — reapply the fork patch on top of target changes.
- Never take an entire conflicted source/build file from one side before classifying all
  patches touching that file.

Typical strategy:

| File | Strategy |
|---|---|
| `BUILD.gn` | Combine target structure and fork build targets/flags |
| `DEPS` | Classify every revision and enabled/commented-state difference |
| Release notes/infra | Usually target, after checking fork history |
| `include/c`, `src/c` | Preserve and adapt the SkiaSharp C API |
| Other source | Verify-upstream-or-reapply |

## Dependency compatibility

Compare fork to target:

```bash
git show "origin/{SKIA_BASE_BRANCH}:DEPS" > "$ARTIFACT_DIR/deps-fork.txt"
git show "upstream/{UPSTREAM_REF}:DEPS" > "$ARTIFACT_DIR/deps-target.txt"
diff -u "$ARTIFACT_DIR/deps-fork.txt" "$ARTIFACT_DIR/deps-target.txt"
```

For each difference, choose and record:

| Decision | Evidence |
|---|---|
| Preserve fork | Fork patch/build file requires it |
| Accept target | No fork customization depends on the old state |
| Compatibility roll | Target source uses dependency API absent from the fork revision |

For compatibility-sensitive rolls, inspect the upstream roll commit and coupled source changes:

```bash
git log -S "<target revision>" --oneline "$DIFF_RANGE" -- DEPS
git show <roll-commit> -- DEPS src/ include/ third_party/
```

Write `$ARTIFACT_DIR/skia-dependency-decisions.md`, accounting for every changed entry.
HarfBuzz stays separate.

## Complete the merge

Verify source additions/deletions against GN lists, C API files remain present, and no marker
or whitespace error remains. Then create the required two-parent merge commit:

```bash
git diff --check
git diff --cached --diff-filter=U --name-only
git commit
```

Do not commit later dependency/C API fixes yet.

## Gate

- Parent and submodule branch from the resolved bases.
- Merge commit has two parents.
- Every conflicted fork patch is classified.
- Dependency report is complete.
- C API and build configuration survived intact.
