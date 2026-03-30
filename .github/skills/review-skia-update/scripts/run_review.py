#!/usr/bin/env python3
"""Run all mechanical checks for a Skia update review.

Fetches PR metadata, checks out the working tree, runs all three check modules,
and outputs structured raw results for the model to process.
"""
import argparse
import json
import os
import re
import subprocess
import sys
from datetime import datetime, timezone

# Import sibling check modules
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import check_generated_files  # noqa: E402
import check_source  # noqa: E402
import check_deps  # noqa: E402
import check_companion  # noqa: E402


def eprint(*args, **kwargs):
    """Print to stderr (status messages)."""
    print(*args, file=sys.stderr, **kwargs)


def run_git(args: list, cwd: str, check: bool = True) -> subprocess.CompletedProcess:
    """Run a git command, streaming output to stderr."""
    result = subprocess.run(
        ["git"] + args,
        cwd=cwd,
        capture_output=True,
        text=True,
    )
    for line in result.stdout.strip().split("\n"):
        if line:
            eprint(f"   {line}")
    for line in result.stderr.strip().split("\n"):
        if line:
            eprint(f"   {line}")
    if check and result.returncode != 0:
        raise RuntimeError(
            f"git {' '.join(args)} failed (exit {result.returncode}): "
            f"{result.stderr.strip()}"
        )
    return result


def main():
    parser = argparse.ArgumentParser(
        description="Run all mechanical checks for a Skia update review."
    )
    parser.add_argument(
        "--skia-pr", type=int, required=True,
        help="The mono/skia PR number to review.",
    )
    parser.add_argument(
        "--skiasharp-pr", type=int, required=True,
        help="The SkiaSharp companion PR number.",
    )
    parser.add_argument(
        "--output-dir", default="",
        help="Override output directory. Default: /tmp/skiasharp/skia-review/{timestamp}",
    )
    args = parser.parse_args()

    skia_pr_number = args.skia_pr
    skiasharp_pr_number = args.skiasharp_pr
    output_dir = args.output_dir

    result = subprocess.run(
        ["git", "rev-parse", "--show-toplevel"],
        capture_output=True, text=True,
    )
    if result.returncode != 0 or not result.stdout.strip():
        raise RuntimeError(
            "Failed to determine git repository root. "
            "Ensure this script is run from within a git worktree."
        )
    repo_root = result.stdout.strip()
    skia_root = os.path.join(repo_root, "externals", "skia")

    # =========================================================================
    # Pre-flight — Ensure clean working trees
    # =========================================================================
    for label, cwd in [("SkiaSharp", repo_root), ("skia submodule", skia_root)]:
        status = subprocess.run(
            ["git", "status", "--porcelain"],
            cwd=cwd, capture_output=True, text=True,
        )
        dirty = status.stdout.strip()
        if dirty:
            raise RuntimeError(
                f"{label} working tree is dirty. Stash or clean before running.\n"
                f"  Directory: {cwd}\n"
                f"  Dirty files:\n" +
                "\n".join(f"    {line}" for line in dirty.splitlines()[:10])
            )

    # =========================================================================
    # Step 1 — Parse & Setup
    # =========================================================================
    eprint("═══ Step 1 — Parse & Setup ═══")

    # 1a. Fetch skia PR metadata
    eprint(f"▸ Fetching mono/skia PR #{skia_pr_number}...")
    pr_result = subprocess.run(
        [
            "gh", "pr", "view", str(skia_pr_number),
            "--repo", "mono/skia",
            "--json", "title,headRefName,headRefOid,baseRefName,baseRefOid,body,state,author",
        ],
        capture_output=True, text=True,
    )
    if pr_result.returncode != 0:
        raise RuntimeError(f"Failed to fetch skia PR #{skia_pr_number}: {pr_result.stderr.strip()}")
    pr = json.loads(pr_result.stdout)

    eprint(f"   Title: {pr['title']}")
    eprint(f"   Head:  {pr['headRefOid']} ({pr['headRefName']})")
    eprint(f"   Base:  {pr['baseRefOid']} ({pr['baseRefName']})")

    # 1b. Extract upstream branches from PR title
    # Match both "m122" and "milestone 122" patterns
    matches = re.findall(r"m(\d{3,})", pr["title"])
    if not matches:
        matches = re.findall(r"milestone\s+(\d{2,})", pr["title"], re.IGNORECASE)
    if not matches:
        raise RuntimeError(
            f"Could not extract milestone from PR title: {pr['title']}. Expected 'mNNN' or 'milestone NNN' pattern."
        )
    new_milestone = f"chrome/m{matches[-1]}"
    eprint(f"   New upstream: {new_milestone}")

    # Determine old upstream from cgmanifest.json
    cgmanifest_path = os.path.join(repo_root, "cgmanifest.json")
    with open(cgmanifest_path) as f:
        cgmanifest = json.load(f)

    old_milestone = None
    for reg in cgmanifest.get("registrations", []):
        comp = reg.get("component", {}).get("other", {})
        if comp.get("name") == "skia":
            version = comp.get("version", "")
            m = re.search(r"chrome/(m\d+)", version)
            if m:
                old_milestone = f"chrome/{m.group(1)}"
                break

    if not old_milestone:
        raise RuntimeError("Could not determine old upstream milestone from cgmanifest.json")
    eprint(f"   Old upstream: {old_milestone}")

    # 1c. Fetch git refs
    eprint("▸ Fetching git refs...")
    run_git(["fetch", "upstream", old_milestone, new_milestone], cwd=skia_root)
    # Fetch the base branch and the PR head via GitHub's PR ref. This works for
    # both same-repo and fork PRs — the branch name only exists on the fork's
    # remote, but refs/pull/{N}/head is always available on origin.
    run_git(["fetch", "origin", "skiasharp", f"pull/{skia_pr_number}/head"], cwd=skia_root)

    # Verify upstream branches exist
    old_result = subprocess.run(
        ["git", "rev-parse", f"upstream/{old_milestone}"],
        cwd=skia_root, capture_output=True, text=True,
    )
    if old_result.returncode != 0:
        raise RuntimeError(f"Old upstream branch upstream/{old_milestone} not found: {old_result.stderr.strip()}")
    old_upstream_sha = old_result.stdout.strip()
    if not old_upstream_sha or len(old_upstream_sha) < 7:
        raise RuntimeError(f"Old upstream branch upstream/{old_milestone} resolved to invalid SHA: {old_upstream_sha!r}")

    new_result = subprocess.run(
        ["git", "rev-parse", f"upstream/{new_milestone}"],
        cwd=skia_root, capture_output=True, text=True,
    )
    if new_result.returncode != 0:
        raise RuntimeError(f"New upstream branch upstream/{new_milestone} not found: {new_result.stderr.strip()}")
    new_upstream_sha = new_result.stdout.strip()
    if not new_upstream_sha or len(new_upstream_sha) < 7:
        raise RuntimeError(f"New upstream branch upstream/{new_milestone} resolved to invalid SHA: {new_upstream_sha!r}")

    # Use PR metadata for base SHA; fall back to resolving the ref
    base_sha = pr.get("baseRefOid", "").strip()
    if not base_sha or len(base_sha) < 7:
        base_ref_name = pr.get("baseRefName", "skiasharp")
        result = subprocess.run(
            ["git", "rev-parse", f"origin/{base_ref_name}"],
            cwd=skia_root, capture_output=True, text=True,
        )
        if result.returncode == 0:
            base_sha = result.stdout.strip()
        if not base_sha or len(base_sha) < 7:
            result = subprocess.run(
                ["git", "rev-parse", "origin/skiasharp"],
                cwd=skia_root, capture_output=True, text=True,
            )
            if result.returncode == 0:
                base_sha = result.stdout.strip()
    if not base_sha or len(base_sha) < 7:
        raise RuntimeError(
            f"Could not resolve base SHA from PR metadata or branch refs. "
            f"baseRefOid={pr.get('baseRefOid', '')!r}, baseRefName={pr.get('baseRefName', '')!r}"
        )
    pr_head_sha = pr["headRefOid"]

    eprint(f"   Base ({pr.get('baseRefName', 'skiasharp')}): {base_sha}")
    eprint(f"   PR head: {pr_head_sha}")
    eprint(f"   Old upstream: {old_upstream_sha}")
    eprint(f"   New upstream: {new_upstream_sha}")

    # 1d. Check out SkiaSharp companion PR
    eprint(f"▸ Checking out SkiaSharp companion PR #{skiasharp_pr_number}...")
    run_git(["fetch", "origin", f"pull/{skiasharp_pr_number}/head"], cwd=repo_root)
    run_git(["checkout", "--detach", "FETCH_HEAD"], cwd=repo_root)

    # 1e. Check out skia submodule at PR head
    eprint(f"▸ Checking out skia submodule at PR head: {pr_head_sha}")
    run_git(["checkout", pr_head_sha], cwd=skia_root)
    actual_sha = subprocess.run(
        ["git", "rev-parse", "HEAD"],
        cwd=skia_root, capture_output=True, text=True,
    ).stdout.strip()
    if actual_sha != pr_head_sha:
        raise RuntimeError(f"Submodule checkout mismatch: expected {pr_head_sha}, got {actual_sha}")
    eprint(f"   ✅ Submodule at {actual_sha}")

    # 1f. Create output directory
    if not output_dir:
        ts = datetime.now(timezone.utc).strftime("%Y%m%d-%H%M%S")
        output_dir = f"/tmp/skiasharp/skia-review/{ts}"
    os.makedirs(output_dir, exist_ok=True)
    eprint(f"   Output: {output_dir}")
    eprint()

    # =========================================================================
    # Steps 2–4 — Run checks (with try/finally to save partial results on failure)
    # =========================================================================
    gen_result = None
    source_result = None
    deps_result = None
    companion_result = None
    check_error = None

    try:
        # Step 2 — Generated File Verification
        eprint("═══ Step 2 — Generated Files ═══")
        gen_result = check_generated_files.run_check(
            repo_root=repo_root,
            output_dir=output_dir,
        )
        eprint()

        # Step 3 — Source Integrity
        eprint("═══ Step 3 — Source Integrity ═══")
        source_result = check_source.run_check(
            skia_root=skia_root,
            old_upstream_branch=f"upstream/{old_milestone}",
            new_upstream_branch=f"upstream/{new_milestone}",
            base_sha=base_sha,
            pr_head=pr_head_sha,
            output_dir=output_dir,
        )
        eprint()

        # Step 4 — DEPS Audit
        eprint("═══ Step 4 — DEPS Audit ═══")
        deps_result = check_deps.run_check(
            skia_root=skia_root,
            base_sha=base_sha,
            pr_head=pr_head_sha,
            upstream_branch=f"upstream/{new_milestone}",
            output_dir=output_dir,
        )
        eprint()

        # Step 5 — Companion PR Files
        eprint("═══ Step 5 — Companion PR ═══")
        # Fetch the main branch for comparison
        run_git(["fetch", "origin", "main"], cwd=repo_root)
        companion_result = check_companion.run_check(
            repo_root=repo_root,
            base_ref="origin/main",
            pr_ref="HEAD",
            output_dir=output_dir,
        )
        eprint()
    except Exception as exc:
        check_error = str(exc)
        eprint(f"\n❌ Check phase failed: {exc}")
        eprint("   Saving partial results...\n")

    # =========================================================================
    # Assemble raw results (includes partial results if a check failed)
    # =========================================================================
    eprint("═══ Assembling raw results ═══")

    raw_results = {
        "meta": {
            "skiaPrNumber": skia_pr_number,
            "skiasharpPrNumber": skiasharp_pr_number,
            "prTitle": pr["title"],
            "prState": pr["state"],
            "prAuthor": pr.get("author", {}).get("login", ""),
            "oldUpstreamBranch": old_milestone,
            "upstreamBranch": new_milestone,
            "shas": {
                "prHead": pr_head_sha,
                "base": base_sha,
                "upstream": new_upstream_sha,
            },
            "analyzedAt": datetime.now(timezone.utc).isoformat(),
            "outputDir": output_dir,
        },
        "generatedFiles": gen_result or {"status": "ERROR", "error": check_error},
        "upstreamIntegrity": (source_result or {}).get("upstreamIntegrity", {"status": "ERROR", "error": check_error}),
        "interopIntegrity": (source_result or {}).get("interopIntegrity", {"status": "ERROR", "error": check_error}),
        "depsAudit": deps_result or {"status": "ERROR", "error": check_error},
        "companionPr": {
            "prNumber": skiasharp_pr_number,
            **(companion_result or {"status": "ERROR", "error": check_error}),
        },
    }

    raw_results_path = os.path.join(output_dir, "raw-results.json")
    with open(raw_results_path, "w") as f:
        json.dump(raw_results, f, indent=2)

    file_size = os.path.getsize(raw_results_path)
    eprint(f"{'✅' if not check_error else '⚠️'} Raw results saved to: {raw_results_path} ({file_size} bytes)")
    eprint()

    if check_error:
        eprint(f"❌ Partial results saved — check phase failed: {check_error}")
        eprint(f"Raw results: {raw_results_path}")
        raise RuntimeError(f"Check phase failed: {check_error}")

    # Print summary
    eprint("═══ Summary ═══")
    eprint(f"  Generated Files:    {gen_result['status']}")
    eprint(f"  Upstream Integrity: {source_result['upstreamIntegrity']['status']}")
    eprint(f"  Interop Integrity:  {source_result['interopIntegrity']['status']}")
    eprint(f"  DEPS Audit:         {deps_result['status']}")
    eprint(f"  Companion PR:       {companion_result['status']}")
    eprint()
    eprint(f"Raw results: {raw_results_path}")
    eprint(f"Generator log: {os.path.join(output_dir, 'generator-output.log')}")
    eprint()
    eprint("Next: Read raw-results.json and write summaries + build report.")


if __name__ == "__main__":
    main()
