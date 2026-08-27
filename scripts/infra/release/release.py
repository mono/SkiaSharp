#!/usr/bin/env python3
"""The stable release-automation CLI entry point.

    release.py prepare plan
    release.py prepare apply --plan <file>
    release.py finish plan --version <exact public version>
    release.py finish create-draft --plan <file>
    release.py finish plan-publication --plan <file>
    release.py finish publish --plan <file>
    release.py finish closeout
    release.py inspect --release-branch <release/X.Y.Z...>

Every "plan" subcommand is read-only. Every "apply"/"create-draft"/"publish"/
"closeout" subcommand takes an already schema-validated, digest-stamped plan
file and revalidates live state before writing; none of them interpret plan
fields as commands.
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import release_common as common
import release_finish as finish
import release_github as gh
import release_milestones as milestones
import release_nuget as nuget
import release_prepare as prepare
from release_git import GitRepository


def _repo(root: Path | None) -> GitRepository:
    start = root or Path.cwd()
    return GitRepository.discover(start)


def cmd_prepare_plan(args: argparse.Namespace) -> int:
    repo = _repo(args.repo)
    github = gh.GhCliGitHubClient()
    repo.fetch()
    plan = prepare.build_prepare_plan(
        repo,
        integration_target=args.integration_target,
        requested_version=args.version,
        tooling_sha=args.tooling_sha or repo.resolve("HEAD"),
        github=github,
        approved_base=args.approved_base,
    )
    stamped = common.write_plan(Path(args.output), plan, schema_name=prepare.PREPARE_SCHEMA)
    common.print_json(stamped)
    return 0


def cmd_prepare_apply(args: argparse.Namespace) -> int:
    plan = common.read_plan(Path(args.plan), schema_name=prepare.PREPARE_SCHEMA)
    repo = _repo(args.repo)
    skia_repo = GitRepository(root=(repo.root / prepare.SKIA_SUBMODULE_PATH))
    github = gh.GhCliGitHubClient()
    report = prepare.apply_prepare_plan(plan, repo=repo, skia_repo=skia_repo, github=github)
    common.print_json(report)
    return 0


def cmd_finish_plan(args: argparse.Namespace) -> int:
    repo = _repo(args.repo)
    package_dir = Path(__file__).resolve().parent
    manifest = nuget.load_manifest(package_dir / "public-packages.json")
    fingerprints = nuget.load_fingerprints(package_dir / "trusted-signing-certificates.json")
    nuget_client = nuget.HttpNuGetClient()
    github = gh.GhCliGitHubClient()
    signature_verifier = nuget.DotNetSignatureVerifier(runner=common.DEFAULT_RUNNER)
    plan = finish.build_finish_plan(
        requested_version=args.version,
        nuget_client=nuget_client,
        repo=repo,
        github=github,
        manifest=manifest,
        fingerprints=fingerprints,
        signature_verifier=signature_verifier,
        download_dir=Path(args.download_dir),
        tooling_sha=args.tooling_sha or repo.resolve("HEAD"),
    )
    stamped = common.write_plan(Path(args.output), plan, schema_name=finish.FINISH_SCHEMA)
    common.print_json(stamped)
    return 0


def cmd_finish_create_draft(args: argparse.Namespace) -> int:
    plan = common.read_plan(Path(args.plan), schema_name=finish.FINISH_SCHEMA)
    repo = _repo(args.repo)
    github = gh.GhCliGitHubClient()
    report = finish.create_draft(plan, repo=repo, github=github)
    common.print_json(report)
    return 0


def cmd_finish_plan_publication(args: argparse.Namespace) -> int:
    plan = common.read_plan(Path(args.plan), schema_name=finish.FINISH_SCHEMA)
    github = gh.GhCliGitHubClient()
    report = finish.plan_publication(plan, github=github)
    common.print_json(report)
    return 0


def cmd_finish_publish(args: argparse.Namespace) -> int:
    plan = common.read_plan(Path(args.plan), schema_name=finish.FINISH_SCHEMA)
    github = gh.GhCliGitHubClient()
    publication = finish.plan_publication(plan, github=github)
    report = finish.publish(plan, publication, github=github)
    common.print_json(report)
    return 0


def cmd_finish_closeout(args: argparse.Namespace) -> int:
    repo = _repo(args.repo)
    repo.fetch()
    tags = list(repo.remote_tags().keys())
    client = GhCliMilestoneClient()
    if args.dry_run:
        report = finish.plan_closeout(milestone_client=client, tags=tags)
    else:
        report = finish.apply_closeout(milestone_client=client, tags=tags)
    common.print_json(report)
    return 0


def cmd_inspect(args: argparse.Namespace) -> int:
    repo = _repo(args.repo)
    repo.fetch()
    report: dict = {"releaseBranch": args.release_branch}
    ref = f"refs/remotes/origin/{args.release_branch}"
    report["branchExists"] = repo.ref_exists(ref)
    if report["branchExists"]:
        report["branchSha"] = repo.resolve(ref)
    if args.version:
        github = gh.GhCliGitHubClient()
        tag = f"v{args.version}"
        report["tag"] = tag
        report["tagSha"] = repo.remote_tags().get(tag)
        existing = github.get_release(tag)
        report["release"] = (
            None
            if existing is None
            else {
                "isDraft": existing.is_draft,
                "isPrerelease": existing.is_prerelease,
                "targetCommitish": existing.target_commitish,
                "url": existing.url,
            }
        )
    common.print_json(report)
    return 0


class GhCliMilestoneClient:
    """The real milestone client, backed by ``gh api``."""

    def __init__(self, repository: str = gh.GITHUB_REPOSITORY):
        self.repository = repository
        self.runner = common.DEFAULT_RUNNER

    def _api(self, args: list[str]):
        import json

        result = self.runner.run(["gh", "api", *args], cwd=Path.cwd())
        return json.loads(result.stdout) if result.stdout.strip() else None

    def milestones(self) -> list[milestones.Milestone]:
        payload = self._api(
            [
                f"repos/{self.repository}/milestones",
                "--paginate", "--slurp", "-f", "state=all", "-f", "per_page=100",
            ]
        )
        pages = payload if isinstance(payload, list) and payload and isinstance(payload[0], list) else [payload or []]
        result = []
        for page in pages:
            for item in page:
                result.append(
                    milestones.Milestone(number=item["number"], title=item["title"], state=item["state"])
                )
        return result

    def create_milestone(self, title, *, due_on, description):
        args = [f"repos/{self.repository}/milestones", "-X", "POST", "-f", f"title={title}"]
        if due_on:
            args.extend(["-f", f"due_on={due_on}"])
        if description:
            args.extend(["-f", f"description={description}"])
        payload = self._api(args)
        return milestones.Milestone(number=payload["number"], title=payload["title"], state=payload["state"])

    def open_milestone_items(self, milestone_number: int):
        payload = self._api(
            [
                f"repos/{self.repository}/issues",
                "--paginate", "--slurp",
                "-f", f"milestone={milestone_number}",
                "-f", "state=open",
                "-f", "per_page=100",
            ]
        )
        pages = payload if isinstance(payload, list) and payload and isinstance(payload[0], list) else [payload or []]
        result = []
        for page in pages:
            for item in page:
                kind = "pull-request" if "pull_request" in item else "issue"
                result.append(
                    milestones.MilestoneItem(
                        number=item["number"], title=item["title"], url=item["html_url"], kind=kind
                    )
                )
        return result

    def update_item_milestone(self, item_number: int, milestone_number: int) -> None:
        self.runner.run(
            [
                "gh", "api", f"repos/{self.repository}/issues/{item_number}",
                "-X", "PATCH", "-F", f"milestone={milestone_number}",
            ],
            cwd=Path.cwd(),
        )

    def close_milestone(self, milestone_number: int) -> None:
        self.runner.run(
            [
                "gh", "api", f"repos/{self.repository}/milestones/{milestone_number}",
                "-X", "PATCH", "-f", "state=closed",
            ],
            cwd=Path.cwd(),
        )

    def closing_issues(self, pull_request_number: int) -> list[int]:
        query = (
            "query($owner:String!,$name:String!,$number:Int!){"
            "repository(owner:$owner,name:$name){pullRequest(number:$number){"
            "closingIssuesReferences(first:50){nodes{number}}}}}"
        )
        owner, name = self.repository.split("/")
        result = self.runner.run(
            [
                "gh", "api", "graphql",
                "-f", f"query={query}",
                "-f", f"owner={owner}",
                "-f", f"name={name}",
                "-F", f"number={pull_request_number}",
            ],
            cwd=Path.cwd(),
        )
        import json

        payload = json.loads(result.stdout)
        nodes = payload["data"]["repository"]["pullRequest"]["closingIssuesReferences"]["nodes"]
        return [node["number"] for node in nodes]


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--repo", type=Path, default=None, help="repository root (defaults to cwd)")
    subparsers = parser.add_subparsers(dest="command", required=True)

    prepare_parser = subparsers.add_parser("prepare")
    prepare_sub = prepare_parser.add_subparsers(dest="prepare_command", required=True)

    plan_parser = prepare_sub.add_parser("plan")
    plan_parser.add_argument("--integration-target", required=True)
    plan_parser.add_argument("--version", default=None)
    plan_parser.add_argument("--approved-base", default=None)
    plan_parser.add_argument("--tooling-sha", default=None)
    plan_parser.add_argument("--output", default="prepare-plan.json")
    plan_parser.set_defaults(func=cmd_prepare_plan)

    apply_parser = prepare_sub.add_parser("apply")
    apply_parser.add_argument("--plan", required=True)
    apply_parser.set_defaults(func=cmd_prepare_apply)

    finish_parser = subparsers.add_parser("finish")
    finish_sub = finish_parser.add_subparsers(dest="finish_command", required=True)

    finish_plan_parser = finish_sub.add_parser("plan")
    finish_plan_parser.add_argument("--version", required=True)
    finish_plan_parser.add_argument("--tooling-sha", default=None)
    finish_plan_parser.add_argument("--download-dir", default="finish-downloads")
    finish_plan_parser.add_argument("--output", default="finish-plan.json")
    finish_plan_parser.set_defaults(func=cmd_finish_plan)

    create_draft_parser = finish_sub.add_parser("create-draft")
    create_draft_parser.add_argument("--plan", required=True)
    create_draft_parser.set_defaults(func=cmd_finish_create_draft)

    plan_publication_parser = finish_sub.add_parser("plan-publication")
    plan_publication_parser.add_argument("--plan", required=True)
    plan_publication_parser.set_defaults(func=cmd_finish_plan_publication)

    publish_parser = finish_sub.add_parser("publish")
    publish_parser.add_argument("--plan", required=True)
    publish_parser.set_defaults(func=cmd_finish_publish)

    closeout_parser = finish_sub.add_parser("closeout")
    closeout_parser.add_argument("--dry-run", action="store_true")
    closeout_parser.set_defaults(func=cmd_finish_closeout)

    inspect_parser = subparsers.add_parser("inspect")
    inspect_parser.add_argument("--release-branch", required=True)
    inspect_parser.add_argument("--version", default=None)
    inspect_parser.set_defaults(func=cmd_inspect)

    return parser


def main(argv: list[str] | None = None) -> int:
    parser = create_parser()
    args = parser.parse_args(argv)
    try:
        return args.func(args)
    except common.ReleaseToolError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
