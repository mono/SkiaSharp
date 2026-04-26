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


def ensure_remote(cwd: str, name: str, url: str):
    """Ensure a git remote exists and points at the expected URL."""
    result = subprocess.run(
        ["git", "remote", "get-url", name],
        cwd=cwd,
        capture_output=True,
        text=True,
    )

    if result.returncode == 0:
        current = result.stdout.strip()
        if current != url:
            eprint(f"▸ Updating remote '{name}' → {url}")
            run_git(["remote", "set-url", name, url], cwd=cwd)
        return

    eprint(f"▸ Adding remote '{name}' → {url}")
    run_git(["remote", "add", name, url], cwd=cwd)


def load_json_at_git_ref(cwd: str, git_ref: str, path: str) -> dict:
    """Load a JSON file from a specific git ref."""
    result = subprocess.run(
        ["git", "show", f"{git_ref}:{path}"],
        cwd=cwd,
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        raise RuntimeError(
            f"Failed to read {path} at {git_ref}: {result.stderr.strip()}"
        )
    try:
        return json.loads(result.stdout)
    except json.JSONDecodeError as ex:
        raise RuntimeError(
            f"Failed to parse {path} at {git_ref} as JSON: {ex}"
        ) from ex


def extract_skia_milestone_from_cgmanifest(cgmanifest: dict):
    """Extract the chrome/mNNN milestone from cgmanifest.json."""
    for reg in cgmanifest.get("registrations", []):
        comp = reg.get("component", {}).get("other", {})
        if comp.get("name") == "skia":
            version = comp.get("version", "")
            match = re.search(r"chrome/(m\d+)", version)
            if match:
                return f"chrome/{match.group(1)}"
    return None


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
        "--milestone", type=int, required=True,
        help="The target Chrome milestone number (e.g. 147). "
             "AI extracts this from the PR title/body; the orchestrator validates consistency.",
    )
    parser.add_argument(
        "--output-dir", default="",
        help="Override output directory. Default: /tmp/skiasharp/skia-review/{timestamp}",
    )
    args = parser.parse_args()

    skia_pr_number = args.skia_pr
    skiasharp_pr_number = args.skiasharp_pr
    caller_milestone = f"chrome/m{args.milestone}"
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
    # Pre-flight — Ensure submodule is initialised
    # =========================================================================
    # In worktree environments the skia submodule may not be initialised yet
    # (no .git file / dir inside externals/skia). Run submodule update so the
    # dirty-tree check below can inspect the submodule properly.
    skia_git_indicator = os.path.join(skia_root, ".git")
    if not os.path.exists(skia_git_indicator):
        eprint("▸ Initialising skia submodule (not yet present)...")
        run_git(["submodule", "update", "--init", "externals/skia"], cwd=repo_root)

    # Verify submodule origin points at mono/skia (worktrees sometimes inherit
    # the parent repo's remote instead).
    ensure_remote(skia_root, "origin", "https://github.com/mono/skia.git")

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

    # 1b. Fetch companion SkiaSharp PR metadata
    eprint(f"▸ Fetching companion SkiaSharp PR #{skiasharp_pr_number}...")
    companion_pr_result = subprocess.run(
        [
            "gh", "pr", "view", str(skiasharp_pr_number),
            "--repo", "mono/SkiaSharp",
            "--json", "title,headRefName,headRefOid,baseRefName,baseRefOid,body,state,author",
        ],
        capture_output=True, text=True,
    )
    if companion_pr_result.returncode != 0:
        raise RuntimeError(
            f"Failed to fetch SkiaSharp PR #{skiasharp_pr_number}: "
            f"{companion_pr_result.stderr.strip()}"
        )
    companion_pr = json.loads(companion_pr_result.stdout)

    eprint(f"   Title: {companion_pr['title']}")
    eprint(f"   Head:  {companion_pr['headRefOid']} ({companion_pr['headRefName']})")
    eprint(f"   Base:  {companion_pr['baseRefOid']} ({companion_pr['baseRefName']})")

    # 1c. Determine old/new upstream milestones
    # Read cgmanifest.json from the companion PR base and head commits, not from the
    # current working tree. This keeps the review correct even when the local repo is
    # already checked out to the bumped milestone branch.
    eprint("▸ Fetching SkiaSharp refs for companion PR base/head...")
    companion_base_ref = companion_pr.get("baseRefName", "main")
    run_git(["fetch", "origin", companion_base_ref, f"pull/{skiasharp_pr_number}/head"], cwd=repo_root)

    companion_base_sha = companion_pr.get("baseRefOid", "").strip()
    if not companion_base_sha or len(companion_base_sha) < 7:
        result = subprocess.run(
            ["git", "rev-parse", f"origin/{companion_base_ref}"],
            cwd=repo_root, capture_output=True, text=True,
        )
        if result.returncode == 0:
            companion_base_sha = result.stdout.strip()
    if not companion_base_sha or len(companion_base_sha) < 7:
        raise RuntimeError(
            f"Could not resolve companion PR base SHA. "
            f"baseRefOid={companion_pr.get('baseRefOid', '')!r}, "
            f"baseRefName={companion_pr.get('baseRefName', '')!r}"
        )

    old_cgmanifest = load_json_at_git_ref(repo_root, companion_base_sha, "cgmanifest.json")
    old_milestone = extract_skia_milestone_from_cgmanifest(old_cgmanifest)
    if not old_milestone:
        raise RuntimeError(
            f"Could not determine old upstream milestone from companion PR base "
            f"{companion_base_sha[:12]}:cgmanifest.json"
        )

    # 1c. Validate milestone consistency
    # The caller (AI) provides --milestone; we validate it against PR title,
    # PR body, branch name, and cgmanifest.json for consistency.
    eprint(f"▸ Validating milestone {caller_milestone}...")

    # Check PR title for milestone patterns
    title_milestone = None
    for pattern in [r"m(\d{3,})", r"milestone\s+(\d{2,})", r"skia[\s\-_]+(\d{2,})"]:
        m = re.findall(pattern, pr["title"], re.IGNORECASE)
        if m:
            title_milestone = f"chrome/m{m[-1]}"
            break
    if title_milestone and title_milestone != caller_milestone:
        eprint(f"   ⚠ PR title implies {title_milestone}, but --milestone says {caller_milestone}")
    elif not title_milestone:
        eprint(f"   ⚠ PR title '{pr['title']}' did not match any milestone pattern")

    # Check cgmanifest.json from companion PR head
    companion_head_sha = companion_pr.get("headRefOid", "").strip()
    cgmanifest_milestone = None
    if companion_head_sha and len(companion_head_sha) >= 7:
        new_cgmanifest = load_json_at_git_ref(repo_root, companion_head_sha, "cgmanifest.json")
        cgmanifest_milestone = extract_skia_milestone_from_cgmanifest(new_cgmanifest)
    if cgmanifest_milestone and cgmanifest_milestone != caller_milestone:
        eprint(
            f"   ⚠ cgmanifest.json records {cgmanifest_milestone}, "
            f"but --milestone says {caller_milestone}"
        )

    new_milestone = caller_milestone

    eprint(f"   New upstream: {new_milestone}")
    eprint(f"   Old upstream: {old_milestone}")

    # 1d. Fetch git refs
    eprint("▸ Fetching git refs...")
    ensure_remote(skia_root, "upstream", "https://github.com/google/skia.git")
    run_git(["fetch", "upstream", old_milestone, new_milestone], cwd=skia_root)
    # Fetch the base branch and the PR head via GitHub's PR ref. This works for
    # both same-repo and fork PRs — the branch name only exists on the fork's
    # remote, but refs/pull/{N}/head is always available on origin.
    skia_base_ref = pr.get("baseRefName", "skiasharp")
    run_git(["fetch", "origin", skia_base_ref, f"pull/{skia_pr_number}/head"], cwd=skia_root)

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

    # 1e. Check out SkiaSharp companion PR
    eprint(f"▸ Checking out SkiaSharp companion PR #{skiasharp_pr_number}...")
    run_git(["checkout", "--detach", companion_head_sha], cwd=repo_root)

    # 1f. Check out skia submodule at PR head
    eprint(f"▸ Checking out skia submodule at PR head: {pr_head_sha}")
    run_git(["checkout", pr_head_sha], cwd=skia_root)
    actual_sha = subprocess.run(
        ["git", "rev-parse", "HEAD"],
        cwd=skia_root, capture_output=True, text=True,
    ).stdout.strip()
    if actual_sha != pr_head_sha:
        raise RuntimeError(f"Submodule checkout mismatch: expected {pr_head_sha}, got {actual_sha}")
    eprint(f"   ✅ Submodule at {actual_sha}")

    # 1g. Sync third-party dependencies (harfbuzz, freetype, etc.)
    # These live under externals/skia/third_party/externals/ and are fetched
    # by Skia's DEPS mechanism, not git submodules. The cake task validates
    # milestone/increment versions and sets GIT_SYNC_DEPS_SKIP_EMSDK=1.
    eprint("▸ Syncing third-party dependencies (dotnet cake git-sync-deps)...")
    sync_result = subprocess.run(
        ["dotnet", "cake", "--target=git-sync-deps"],
        cwd=repo_root,
        capture_output=True,
        text=True,
    )
    for line in sync_result.stdout.strip().split("\n"):
        if line and ("@" in line or "Task" in line or "Total:" in line or "error" in line.lower()):
            eprint(f"   {line.strip()}")
    if sync_result.returncode != 0:
        eprint(f"   ❌ dotnet cake git-sync-deps failed (exit {sync_result.returncode})")
        for line in sync_result.stderr.strip().split("\n"):
            if line:
                eprint(f"   {line.strip()}")
        raise RuntimeError(
            f"Failed to sync third-party dependencies (dotnet cake exit {sync_result.returncode}). "
            f"Check that the skia submodule milestone matches VERSIONS.txt."
        )
    harfbuzz_path = os.path.join(skia_root, "third_party", "externals", "harfbuzz")
    if not os.path.isdir(harfbuzz_path):
        raise RuntimeError(
            f"third_party/externals/harfbuzz not found after sync. "
            f"Expected at: {harfbuzz_path}"
        )
    eprint("   ✅ Third-party dependencies synced")

    # 1h. Create output directory
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
