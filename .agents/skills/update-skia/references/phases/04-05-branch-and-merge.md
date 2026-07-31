# 04–05 — Branch and merge

Require the Phase 03 analysis, dependency-decision, and review artifacts before using this file.
Read only `DEPS: Fork-Customized Dependencies`, `Genuine Merge Required`,
`Conflict Resolution by File Category`, and `Verify-Upstream-or-Reapply` in
[../known-gotchas.md](../known-gotchas.md).

## Phase 04 — branch from the resolved bases

Create or verify the parent feature branch:

```bash
git fetch origin "{BASE_BRANCH}"
git checkout -b "{HEAD_BRANCH}" "origin/{BASE_BRANCH}"
```

Create or verify the mono/skia feature branch at the exact submodule pointer recorded by the
parent base:

```bash
BASE_SUB_SHA=$(git ls-tree "origin/{BASE_BRANCH}" -- externals/skia | awk '{print $3}')
export SKIA_SYNC_SKIA_BASE_SHA="$BASE_SUB_SHA"
git -C externals/skia fetch origin "{SKIA_BASE_BRANCH}"
git -C externals/skia checkout "$BASE_SUB_SHA"
git -C externals/skia branch -r --contains "$BASE_SUB_SHA" |
  grep "origin/{SKIA_BASE_BRANCH}"
git -C externals/skia checkout -b "{HEAD_BRANCH}"
```

If a branch already exists, inspect its base and commits before continuing. Reuse only the
intended update branch; never reset or overwrite unrelated work. Automation may start with the
submodule already aligned, but must still verify the pointer and bases.

## Phase 05 — genuine upstream merge

### Snapshot the fork before merging

```bash
cd externals/skia
MB=$(git merge-base "origin/{SKIA_BASE_BRANCH}" "upstream/{UPSTREAM_REF}")
git log --oneline "$MB..origin/{SKIA_BASE_BRANCH}" \
  > "$ARTIFACT_DIR/fork-patches-before.txt"
git merge --no-commit --no-ff "upstream/{UPSTREAM_REF}"
```

If conflicts occur, batch the audit before resolving any file:

```bash
git diff --name-only --diff-filter=U > "$ARTIFACT_DIR/conflicted-files.txt"
while IFS= read -r file; do
  printf '\n## %s\n' "$file"
  git log --oneline "$MB..origin/{SKIA_BASE_BRANCH}" -- "$file"
done < "$ARTIFACT_DIR/conflicted-files.txt" \
  > "$ARTIFACT_DIR/conflict-fork-history.md"
```

Classify every fork patch touching a conflict:

- **Upstreamed** — retain upstream's equivalent or improved form and record its SHA.
- **Not upstreamed** — reapply the fork behavior on top of target changes.
- **Obsolete by design** — use only when replacement behavior is proven and record the evidence.

Call something a fork patch only when a commit in `MB..origin/{SKIA_BASE_BRANCH}` introduced or
changed it. Behavior inherited from the old upstream milestone is not a fork patch merely because
the fork relied on it; if target removes that behavior, handle it later as a compatibility
adaptation.

Never resolve an entire source, build, or dependency file with blanket `ours`/`theirs` before
classifying all fork changes in it.

### Audit dependencies against the fork base

Compare the complete fork and target `DEPS` files:

```bash
git show "origin/{SKIA_BASE_BRANCH}:DEPS" > "$ARTIFACT_DIR/deps-fork.txt"
git show "upstream/{UPSTREAM_REF}:DEPS" > "$ARTIFACT_DIR/deps-target.txt"
diff -u "$ARTIFACT_DIR/deps-fork.txt" "$ARTIFACT_DIR/deps-target.txt"
```

Account for every revision and enabled/commented-state difference:

| Decision | Required evidence |
|---|---|
| Preserve fork | A fork patch, build file, security update, or product choice depends on it |
| Accept target | No fork customization depends on the old state |
| Compatibility roll | Target source uses dependency API absent from the fork revision |

For a compatibility-sensitive revision, inspect the upstream roll commit and its coupled source
changes, then verify the selected revision contains every required field/function. Preserving every
pin unconditionally is as unsafe as accepting every target pin.

Update `$ARTIFACT_DIR/skia-dependency-decisions.md` with the final merged dispositions. Keep
HarfBuzz on the fork revision; its update is a separate dependency workflow. Replace provisional
rows rather than appending a second conclusion. A row that says accept-target while merged `DEPS`
retains the fork revision is a gate failure.

### Complete the merge

Before committing:

- Verify added/deleted sources against GN build lists.
- Verify C API and fork build targets survived.
- Verify every conflict and fork patch has a recorded disposition.
- Compare final `DEPS` against both the fork base and target, then confirm every dependency-decision
  row names the revision and enabled/commented state actually present in the merged file.
- Run `git diff --check` and confirm no unresolved paths remain.

Create the required two-parent merge commit. Verify its parents are the selected mono/skia base and
the target upstream commit. Build-driven dependency or C API adaptations belong in later, separate
commits after their need is proven.

### Audit the complete fork delta

Conflict resolution is not enough: a whole-file resolution can drop a separate non-conflicting
fork patch. Return to the parent repository root and generate the diff-of-diffs audit after the
merge:

```bash
python3 .agents/skills/update-skia/scripts/audit_fork_patches.py \
  --old-upstream "$SKIA_SYNC_BASE_UPSTREAM_SHA" \
  --new-upstream "$SKIA_SYNC_TARGET_UPSTREAM_SHA" \
  --fork-base "$SKIA_SYNC_SKIA_BASE_SHA" \
  --merged-head HEAD \
  --output "$ARTIFACT_DIR/skia-fork-patch-audit.md"
```

Inspect both old and new patches for every table row. Replace every `TODO` with a valid final
disposition and concrete source/commit evidence, then rerun with `--validate`. Reapply or adapt any
patch that was neither upstreamed nor intentionally obsolete. Never infer integrity from commit or
file counts alone.

## Gate

- Parent and mono/skia feature branches have the resolved bases.
- The merge commit has exactly two parents.
- Every fork patch and dependency difference is classified.
- `audit_fork_patches.py --validate` passes with no provisional decisions.
- C API and build configuration survive intact.
- Analysis artifacts still agree with the merged tree.
