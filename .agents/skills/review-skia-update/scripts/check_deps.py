#!/usr/bin/env python3
"""Audit DEPS changes between base, head, and upstream.

Parses DEPS files (handles comments, Var(), Python syntax).
Compares base vs head deps: added, removed, changed (url or revision differs).
ALL changes are REVIEW_REQUIRED — the fork is ahead of upstream with custom pins.
"""
import argparse
import ast
import json
import os
import subprocess
import sys


def eprint(*args, **kwargs):
    """Print to stderr (status messages)."""
    print(*args, file=sys.stderr, **kwargs)


# ---------------------------------------------------------------------------
# DEPS parser (merged from Parse-Deps.py)
# ---------------------------------------------------------------------------

def _resolve_var_calls(node: ast.AST, variables: dict) -> object:
    """Recursively evaluate an AST node using literal_eval + Var() resolution."""
    if isinstance(node, ast.Call):
        # Handle Var("name") calls
        if isinstance(node.func, ast.Name) and node.func.id == "Var" and len(node.args) == 1:
            key = ast.literal_eval(node.args[0])
            return variables.get(key, key)
        raise ValueError(f"Unsupported call: {ast.dump(node)}")
    if isinstance(node, ast.BinOp) and isinstance(node.op, ast.Add):
        # Handle string concatenation: "prefix" + Var("x") + "@" + Var("y")
        left = _resolve_var_calls(node.left, variables)
        right = _resolve_var_calls(node.right, variables)
        return left + right
    if isinstance(node, ast.Dict):
        # Recursively resolve dict nodes so Var() inside values are handled
        result = {}
        for k, v in zip(node.keys, node.values):
            result[_resolve_var_calls(k, variables)] = _resolve_var_calls(v, variables)
        return result
    # For plain literals (str, num, list, bool, None)
    return ast.literal_eval(node)


def _extract_dict(tree: ast.Module, name: str, variables: dict) -> dict:
    """Extract a top-level dict assignment from an AST, resolving Var() calls."""
    for node in ast.iter_child_nodes(tree):
        if isinstance(node, ast.Assign):
            for target in node.targets:
                if isinstance(target, ast.Name) and target.id == name:
                    if isinstance(node.value, ast.Dict):
                        result = {}
                        for k, v in zip(node.value.keys, node.value.values):
                            try:
                                key = _resolve_var_calls(k, variables)
                                val = _resolve_var_calls(v, variables)
                            except Exception as exc:
                                key_repr = ast.dump(k) if isinstance(k, ast.AST) else repr(k)
                                eprint(f"Warning: skipping DEPS entry {key_repr}: {exc}")
                                continue
                            result[key] = val
                        return result
    return {}


def parse_deps(content: str) -> dict:
    """Parse DEPS using AST analysis (no exec). Extracts vars and deps dicts."""
    try:
        tree = ast.parse(content)
    except SyntaxError as e:
        eprint(f"Error: DEPS parse failed: {e}")
        raise

    # First extract vars dict (all literal values)
    variables = _extract_dict(tree, "vars", {})

    # Then extract deps dict, resolving Var() calls with the variables
    raw_deps = _extract_dict(tree, "deps", variables)

    result = {}
    for path, value in raw_deps.items():
        # value can be a string "url@rev" or a dict with "url", "condition", etc.
        if isinstance(value, str):
            url_rev = value
        elif isinstance(value, dict):
            url_rev = value.get("url", "")
        else:
            continue

        if "@" in url_rev:
            url, revision = url_rev.rsplit("@", 1)
        else:
            url = url_rev
            revision = ""

        # Use the last path component as the short name
        name = path.rstrip("/").split("/")[-1]
        result[name] = {"url": url.strip(), "revision": revision.strip(), "path": path}

    return result


# ---------------------------------------------------------------------------
# DEPS audit
# ---------------------------------------------------------------------------

def run_check(
    skia_root: str,
    base_sha: str,
    pr_head: str,
    upstream_branch: str,
    output_dir: str,
) -> dict:
    """Run DEPS audit. Returns result dict."""
    tmp_dir = output_dir if output_dir else None

    def git_show_deps(ref: str, filename: str) -> str:
        """Extract DEPS content at a given ref."""
        result = subprocess.run(
            ["git", "show", f"{ref}:DEPS"],
            cwd=skia_root,
            capture_output=True,
            text=True,
        )
        if result.returncode != 0:
            raise RuntimeError(f"git show {ref}:DEPS failed: {result.stderr.strip()}")
        if tmp_dir:
            os.makedirs(tmp_dir, exist_ok=True)
            filepath = os.path.join(tmp_dir, filename)
            with open(filepath, "w") as f:
                f.write(result.stdout)
        return result.stdout

    base_content = git_show_deps(base_sha, "DEPS-base")
    head_content = git_show_deps(pr_head, "DEPS-head")
    upstream_content = git_show_deps(upstream_branch, "DEPS-upstream")

    base_deps = parse_deps(base_content)
    head_deps = parse_deps(head_content)
    upstream_deps = parse_deps(upstream_content)

    eprint(f"  Base deps: {len(base_deps)} | Head deps: {len(head_deps)} | Upstream deps: {len(upstream_deps)}")

    # Compare base vs head
    all_names = sorted(set(list(base_deps.keys()) + list(head_deps.keys())))

    added = []
    removed = []
    changed = []
    unchanged_count = 0

    for name in all_names:
        in_base = name in base_deps
        in_head = name in head_deps

        if not in_base and in_head:
            h = head_deps[name]
            entry = {
                "name": name,
                "url": h["url"],
                "revision": h["revision"],
            }
            u = upstream_deps.get(name)
            if u:
                entry["upstreamRevision"] = u["revision"]
                entry["matchesUpstream"] = h["revision"] == u["revision"]
            added.append(entry)
            continue

        if in_base and not in_head:
            b = base_deps[name]
            removed.append({
                "name": name,
                "url": b["url"],
                "revision": b["revision"],
            })
            continue

        # Both exist — check if url or revision changed
        b = base_deps[name]
        h = head_deps[name]
        if b["url"] != h["url"] or b["revision"] != h["revision"]:
            entry = {
                "name": name,
                "oldUrl": b["url"],
                "oldRevision": b["revision"],
                "newUrl": h["url"],
                "newRevision": h["revision"],
            }
            u = upstream_deps.get(name)
            if u:
                entry["upstreamRevision"] = u["revision"]
            changed.append(entry)
        else:
            unchanged_count += 1

    status = "REVIEW_REQUIRED" if (added or removed or changed) else "PASS"

    eprint()
    if status == "PASS":
        eprint(f"✅ DEPS audit: PASS — {unchanged_count} deps unchanged")
    else:
        eprint("🔍 DEPS audit: REVIEW_REQUIRED")
        if added:
            eprint(f"   Added ({len(added)}):")
            for a in added:
                eprint(f"     + {a['name']}: {a['url']}@{a['revision']}")
        if removed:
            eprint(f"   Removed ({len(removed)}):")
            for r in removed:
                eprint(f"     - {r['name']}: {r['url']}@{r['revision']}")
        if changed:
            eprint(f"   Changed ({len(changed)}):")
            for c in changed:
                eprint(f"     ~ {c['name']}: {c['oldRevision']} → {c['newRevision']}")
        eprint(f"   Unchanged: {unchanged_count}")

    return {
        "status": status,
        "added": added,
        "removed": removed,
        "changed": changed,
        "unchanged": unchanged_count,
    }


def main():
    parser = argparse.ArgumentParser(description="Audit DEPS changes between base, head, and upstream.")
    parser.add_argument("--skia-root", required=True, help="Path to the skia submodule.")
    parser.add_argument("--base-sha", required=True, help="Base (skiasharp) branch SHA.")
    parser.add_argument("--pr-head", required=True, help="PR head SHA or ref.")
    parser.add_argument("--upstream-branch", required=True, help="Upstream branch ref.")
    parser.add_argument("--output-dir", default="", help="Directory for temp files.")
    args = parser.parse_args()

    result = run_check(
        skia_root=args.skia_root,
        base_sha=args.base_sha,
        pr_head=args.pr_head,
        upstream_branch=args.upstream_branch,
        output_dir=args.output_dir,
    )
    json.dump(result, sys.stdout, indent=2)
    print()


if __name__ == "__main__":
    main()
