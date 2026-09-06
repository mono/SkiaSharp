#!/usr/bin/env python3

"""Audit paired Skia fork-patch integrity during Phases 05 and 10.

The upstream merge may silently drop or alter a fork patch without producing a
conflict. This helper compares the old and new upstream-relative fork deltas,
then requires an evidence-backed disposition for every changed patch.
"""

import argparse
import hashlib
import re
import subprocess
from pathlib import Path


ALLOWED_DISPOSITIONS = {
    "added": {"intentional-addition", "adapted", "renamed"},
    "removed": {"upstreamed", "obsolete", "renamed"},
    "changed": {"preserved", "adapted", "upstreamed", "obsolete", "renamed"},
}
ROW_PATTERN = re.compile(
    r"^\| `(?P<path>.+?)` \| (?P<change>[^|]+) \| `(?P<fingerprint>[0-9a-f]+)` \| "
    r"(?P<disposition>[^|]+) \| (?P<evidence>.*) \|$"
)
HUNK_PATTERN = re.compile(
    r"^@@ -\d+(?:,\d+)? \+\d+(?:,\d+)? @@(?P<context>.*)$"
)


def git(skia_root: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", *args],
        cwd=skia_root,
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        raise RuntimeError(
            f"git {' '.join(args)} failed in {skia_root}: {result.stderr.strip()}"
        )
    return result.stdout


def verify_ref(skia_root: Path, ref: str) -> None:
    git(skia_root, "rev-parse", "--verify", f"{ref}^{{commit}}")


def normalize_patch(raw: str) -> str:
    """Remove location metadata while retaining source context and binary payloads."""
    normalized = []
    in_header = True
    for line in raw.split("\n"):
        hunk = HUNK_PATTERN.match(line)
        if hunk:
            in_header = False
            normalized.append(f"@@ @@{hunk.group('context')}")
            continue
        if line == "GIT binary patch" or line.startswith("Binary files "):
            in_header = False
        if in_header and (
            line.startswith("diff --git ")
            or line.startswith("index ")
            or line.startswith("--- ")
            or line.startswith("+++ ")
        ):
            continue
        normalized.append(line)
    return "\n".join(normalized)


def patch(skia_root: Path, old: str, new: str, path: str) -> str:
    raw = git(
        skia_root,
        "diff",
        "--binary",
        "--full-index",
        "--no-renames",
        f"{old}..{new}",
        "--",
        path,
    )
    return normalize_patch(raw)


def changed_files(skia_root: Path, old: str, new: str) -> list[str]:
    return sorted(
        line
        for line in git(
            skia_root, "diff", "--name-only", "--no-renames", f"{old}..{new}", "--"
        ).splitlines()
        if line
    )


def compute_audit(
    skia_root: Path,
    old_upstream: str,
    new_upstream: str,
    fork_base: str,
    merged_head: str,
) -> dict[str, object]:
    """Classify fork patches by comparing old and new upstream-relative deltas."""
    for ref in (old_upstream, new_upstream, fork_base, merged_head):
        verify_ref(skia_root, ref)

    old_files = changed_files(skia_root, old_upstream, fork_base)
    new_files = changed_files(skia_root, new_upstream, merged_head)
    old_set = set(old_files)
    new_set = set(new_files)

    common = sorted(old_set & new_set)
    changed = []
    unchanged = []
    fingerprints = {}
    for path in common:
        old_patch = patch(skia_root, old_upstream, fork_base, path)
        new_patch = patch(skia_root, new_upstream, merged_head, path)
        (unchanged if old_patch == new_patch else changed).append(path)

    changes = {
        **{path: "added" for path in sorted(new_set - old_set)},
        **{path: "removed" for path in sorted(old_set - new_set)},
        **{path: "changed" for path in changed},
    }
    for path, change in changes.items():
        old_patch = patch(skia_root, old_upstream, fork_base, path)
        new_patch = patch(skia_root, new_upstream, merged_head, path)
        fingerprints[path] = hashlib.sha256(
            f"{change}\0{old_patch}\0{new_patch}".encode("utf-8")
        ).hexdigest()[:16]

    return {
        "added": sorted(new_set - old_set),
        "removed": sorted(old_set - new_set),
        "changed": changed,
        "unchanged": unchanged,
        "fingerprints": fingerprints,
        "old_count": len(old_set),
        "new_count": len(new_set),
    }


def read_decisions(path: Path) -> dict[str, tuple[str, str, str, str]]:
    """Load prior decisions so unchanged patch fingerprints retain their review."""
    if not path.exists():
        return {}
    decisions = {}
    for line in path.read_text(encoding="utf-8").splitlines():
        match = ROW_PATTERN.match(line)
        if match:
            decisions[match.group("path")] = (
                match.group("change").strip(),
                match.group("fingerprint").strip(),
                match.group("disposition").strip(),
                match.group("evidence").strip(),
            )
    return decisions


def render(
    audit: dict[str, object],
    decisions: dict[str, tuple[str, str, str, str]],
    old_upstream: str,
    new_upstream: str,
    fork_base: str,
    merged_head: str,
) -> str:
    """Render a concise review table, resetting stale decisions to TODO."""
    rows = []
    for change in ("removed", "changed", "added"):
        for path in audit[change]:
            fingerprint = audit["fingerprints"][path]
            previous = decisions.get(path)
            if previous and previous[:2] == (change, fingerprint):
                disposition, evidence = previous[2:]
            else:
                disposition, evidence = "TODO", "TODO"
            rows.append(
                f"| `{path}` | {change} | `{fingerprint}` | "
                f"{disposition} | {evidence} |"
            )

    if not rows:
        rows.append(
            "| _none_ | none | `0000000000000000` | preserved | "
            "No fork delta changed. |"
        )

    return "\n".join(
        [
            "# Fork patch integrity audit",
            "",
            f"- Old fork delta: `{old_upstream}..{fork_base}` ({audit['old_count']} files)",
            f"- New fork delta: `{new_upstream}..{merged_head}` ({audit['new_count']} files)",
            f"- Unchanged fork patches: {len(audit['unchanged'])}",
            "",
            "Every non-unchanged row must have one final disposition and concrete evidence.",
            "Do not leave provisional or contradictory decisions.",
            "",
            "| Path | Change | Patch ID | Disposition | Evidence |",
            "|---|---|---|---|---|",
            *rows,
            "",
            "Allowed dispositions:",
            "",
            "- removed: `upstreamed`, `obsolete`, `renamed`",
            "- changed: `preserved`, `adapted`, `upstreamed`, `obsolete`, `renamed`",
            "- added: `intentional-addition`, `adapted`, `renamed`",
            "",
            "Inspect a row with:",
            "",
            "```bash",
            f'git diff "{old_upstream}..{fork_base}" -- <path>',
            f'git diff "{new_upstream}..{merged_head}" -- <path>',
            "```",
            "",
        ]
    )


def validate(
    audit: dict[str, object],
    decisions: dict[str, tuple[str, str, str, str]],
) -> list[str]:
    """Reject missing, invalid, or stale decisions for every changed fork patch."""
    errors = []
    expected = {
        path: change
        for change in ("added", "removed", "changed")
        for path in audit[change]
    }
    for path, change in expected.items():
        if path not in decisions:
            errors.append(f"{path}: missing decision")
            continue
        recorded_change, recorded_fingerprint, disposition, evidence = decisions[path]
        if recorded_change != change:
            errors.append(
                f"{path}: recorded change {recorded_change!r} is stale; expected {change!r}"
            )
        if recorded_fingerprint != audit["fingerprints"][path]:
            errors.append(f"{path}: patch changed after it was reviewed")
        if disposition not in ALLOWED_DISPOSITIONS[change]:
            errors.append(
                f"{path}: disposition {disposition!r} is invalid for {change}"
            )
        if not evidence or evidence == "TODO":
            errors.append(f"{path}: concrete evidence is required")
    return errors


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Audit old and new paired Skia fork deltas and validate dispositions."
    )
    parser.add_argument("--skia-root", type=Path, default=Path("externals/skia"))
    parser.add_argument("--old-upstream", required=True)
    parser.add_argument("--new-upstream", required=True)
    parser.add_argument("--fork-base", required=True)
    parser.add_argument("--merged-head", default="HEAD")
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--validate", action="store_true")
    args = parser.parse_args()

    skia_root = args.skia_root.resolve()
    try:
        audit = compute_audit(
            skia_root,
            args.old_upstream,
            args.new_upstream,
            args.fork_base,
            args.merged_head,
        )
        decisions = read_decisions(args.output)
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(
            render(
                audit,
                decisions,
                args.old_upstream,
                args.new_upstream,
                args.fork_base,
                args.merged_head,
            ),
            encoding="utf-8",
        )
    except (OSError, RuntimeError) as error:
        print(f"ERROR: {error}")
        return 2

    if not args.validate:
        print(
            f"Fork audit written: {len(audit['removed'])} removed, "
            f"{len(audit['changed'])} changed, {len(audit['added'])} added."
        )
        return 0

    errors = validate(audit, read_decisions(args.output))
    if errors:
        for error in errors:
            print(f"ERROR: {error}")
        return 1
    print("GATE PASSED: every changed fork patch has a current final disposition.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
