#!/usr/bin/env python3
"""The stable release-automation CLI entry point.

    release.py prepare plan --integration-target <main|release/X.Y.x> --output <file>
    release.py prepare apply --plan <file> --output <file>
    release.py finish plan --version <exact public version> --output <file>
    release.py finish create-draft --plan <file> --output <file>
    release.py finish plan-publication --plan <file> --output <file>
    release.py finish publish --plan <file> --output <file>
    release.py finish closeout --plan <file> --output <file> [--dry-run]
    release.py inspect --release-branch <release/X.Y.Z...> --output <file>
    release.py render-plan --plan <file> --output <file>

Every "plan" subcommand (``prepare plan``, ``finish plan``) is read-only and
writes a schema-validated, digest-stamped plan artifact to ``--output``.
Every "apply"/"create-draft"/"plan-publication"/"publish"/"closeout"
subcommand takes ``--plan <file>``, revalidates live state before writing,
and never interprets plan fields as commands. In every one of those cases
``--plan`` is the *original* plan file written by ``prepare plan``/
``finish plan`` (schema-validated against ``prepare-plan.schema.json``/
``finish-plan.schema.json``) -- never a result file that ``apply``/
``create-draft``/``publish``/``closeout`` itself wrote. In particular,
``finish publish``, ``finish plan-publication``, and ``finish closeout``
each re-read that same original finish plan on every invocation (not each
other's output), which is what lets every one of them be rerun
independently and idempotently.

Every plan and every command result shares the same standardized
workflow-facing surface, so a thin workflow can read the same fields
regardless of which command produced the file:

- top-level ``toolingSha`` (the trusted tooling commit), ``planDigest``
  (the plan's canonical digest -- results pass through the digest of the
  plan they were produced from), and ``nextAction`` (what the workflow
  should do next: e.g. ``"apply"``, ``"create-draft"``,
  ``"plan-publication"``, ``"publish"``, ``"closeout"``, ``"done"``,
  ``"blocked"``, ``"await-merge"``);
- a nested ``release`` object with ``identity`` (the normalized release
  identity/concurrency key, never including the CI build revision),
  ``version`` (the exact release version -- for finish, the exact
  published public NuGet.org version including the build revision), and
  ``branch`` (the exact release branch).

See ``schemas/prepare-plan.schema.json``, ``schemas/finish-plan.schema.json``,
and ``schemas/result-envelope.schema.json``. ``inspect`` is the one
exception: it has no plan input, so it omits ``toolingSha``/``planDigest``/
``nextAction`` but still reports the same ``release`` object shape.

Every subcommand accepts an optional ``--output <file>`` in addition to its
existing stdout JSON, so a thin workflow step can read an exact file instead
of scraping stdout. ``prepare plan``/``finish plan`` always write their
plan artifact to ``--output`` (required for later "apply"/"create-draft"/
etc. steps to consume); for every other subcommand ``--output`` is optional
and, when given, receives the same JSON object that is printed to stdout.

``render-plan`` is the deterministic summary surface for thin workflows: it
reads any already-validated plan *or* command-result file (auto-detected)
and projects a small, flat, schema-versioned set of fields a workflow can
map directly to job outputs -- ``toolingSha``, ``nextAction``,
``releaseIdentity``, ``releaseBranch``, ``releaseVersion``, and
``planDigest``. See ``schemas/plan-summary.schema.json``.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import release_common as common
import release_finish as finish
import release_github as gh
import release_milestones as milestones
import release_model as model
import release_nuget as nuget
import release_prepare as prepare
import release_summary as summary
from release_git import GitRepository


def _repo(root: Path | None) -> GitRepository:
    start = root or Path.cwd()
    return GitRepository.discover(start)


def _output_path(args: argparse.Namespace) -> Path | None:
    value = getattr(args, "output", None)
    return Path(value) if value else None


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
    github = gh.GhCliGitHubClient()
    report = prepare.apply_prepare_plan(plan, repo=repo, github=github)
    common.emit(report, output=_output_path(args))
    return 0


def _dotnet_command(repo_root: Path) -> tuple[str, ...]:
    """Prefer this repository's pinned Arcade dotnet wrapper over a bare
    ``dotnet`` on PATH, matching how signature verification was live-
    validated (``./eng/common/dotnet.sh nuget verify --all``). Falls back
    to PATH only when the checkout has no ``eng/common`` wrapper script."""

    script_name = "dotnet.cmd" if sys.platform.startswith("win") else "dotnet.sh"
    script = repo_root / "eng" / "common" / script_name
    if script.is_file():
        return (str(script),)
    return ("dotnet",)


def cmd_finish_plan(args: argparse.Namespace) -> int:
    repo = _repo(args.repo)
    package_dir = Path(__file__).resolve().parent
    manifest = nuget.load_manifest(package_dir / "public-packages.json")
    fingerprints = nuget.load_fingerprints(package_dir / "trusted-signing-certificates.json")
    nuget_client = nuget.HttpNuGetClient()
    github = gh.GhCliGitHubClient()
    signature_verifier = nuget.DotNetSignatureVerifier(
        runner=common.DEFAULT_RUNNER, dotnet_command=_dotnet_command(repo.root)
    )
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
    common.emit(report, output=_output_path(args))
    return 0


def cmd_finish_plan_publication(args: argparse.Namespace) -> int:
    plan = common.read_plan(Path(args.plan), schema_name=finish.FINISH_SCHEMA)
    github = gh.GhCliGitHubClient()
    report = finish.plan_publication(plan, github=github)
    common.emit(report, output=_output_path(args))
    return 0


def cmd_finish_publish(args: argparse.Namespace) -> int:
    plan = common.read_plan(Path(args.plan), schema_name=finish.FINISH_SCHEMA)
    github = gh.GhCliGitHubClient()
    publication = finish.plan_publication(plan, github=github)
    report = finish.publish(plan, publication, github=github)
    common.emit(report, output=_output_path(args))
    return 0


def cmd_finish_closeout(args: argparse.Namespace) -> int:
    plan = common.read_plan(Path(args.plan), schema_name=finish.FINISH_SCHEMA)
    repo = _repo(args.repo)
    repo.fetch()
    tags = list(repo.remote_tags().keys())
    client = GhCliMilestoneClient()
    if args.dry_run:
        report = finish.plan_closeout(plan, repo=repo, milestone_client=client, tags=tags)
    else:
        github = gh.GhCliGitHubClient()
        report = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, tags=tags)
    common.emit(report, output=_output_path(args))
    return 0


def cmd_inspect(args: argparse.Namespace) -> int:
    """Read-only recovery/diagnostic report for one release branch/version.

    Unlike every other subcommand, ``inspect`` has no plan input, so its
    report intentionally does not carry the standardized ``toolingSha``/
    ``planDigest``/``nextAction`` envelope (see ``result-envelope.schema.json``)
    -- there is no plan to trace it back to. It still reports a ``release``
    object with ``branch``/``version``/``identity`` for naming consistency
    with every other command's report.
    """

    repo = _repo(args.repo)
    repo.fetch()
    identity = None
    try:
        identity = model.parse_release_branch(args.release_branch).raw
    except common.PlanError:
        identity = None
    report: dict = {
        "release": {
            "branch": args.release_branch,
            "version": args.version,
            "identity": identity,
        },
    }
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
        report["githubRelease"] = (
            None
            if existing is None
            else {
                "isDraft": existing.is_draft,
                "isPrerelease": existing.is_prerelease,
                "targetCommitish": existing.target_commitish,
                "url": existing.url,
            }
        )
    common.emit(report, output=_output_path(args))
    return 0


_SUMMARY_PLAN_SCHEMAS = {"prepare": prepare.PREPARE_SCHEMA, "finish": finish.FINISH_SCHEMA}


def cmd_render_plan(args: argparse.Namespace) -> int:
    """Render the deterministic plan-summary surface for a plan *or*
    command-result file (``--plan`` accepts either -- both are the file a
    thin workflow step just wrote with ``--output``).

    A document whose top-level ``operation`` is ``"prepare"`` or
    ``"finish"`` is treated as a full plan: it is schema-validated and
    digest-verified exactly like ``apply``/``create-draft``/etc. would.
    Anything else is treated as a command result and is schema-validated
    against the standardized result envelope instead (results are not
    separately digested; they carry the source plan's digest unchanged).
    Either way, a tampered or malformed file is rejected before any field
    is projected -- this validation happens identically regardless of
    ``--format``.

    ``--format json`` (the default, for compatibility) emits the flat
    ``schemas/plan-summary.schema.json`` surface. ``--format markdown``
    emits a deterministic, human-readable report built from the same
    validated document, additionally including whichever of
    operations/results/receipt/packages/tag/draft/stable-bump sections are
    present.
    """

    plan_path = Path(args.plan)
    try:
        peeked = json.loads(plan_path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise common.ValidationError(f"could not read {plan_path}: {exc}") from exc
    if not isinstance(peeked, dict):
        raise common.ValidationError(f"{plan_path} must contain a JSON object")

    operation = peeked.get("operation")
    schema_name = _SUMMARY_PLAN_SCHEMAS.get(operation)
    if schema_name is not None:
        document = common.read_plan(plan_path, schema_name=schema_name)
    else:
        common.validate_result_envelope(peeked)
        document = peeked

    if args.format == "markdown":
        rendered_text = summary.render_markdown(document)
        common.emit_text(rendered_text, output=_output_path(args))
    else:
        rendered = summary.summarize_document(document)
        common.validate_against_schema(rendered, summary.SUMMARY_SCHEMA)
        common.emit(rendered, output=_output_path(args))
    return 0


class GhCliMilestoneClient:
    """The real milestone client, backed by ``gh api``.

    Every read here passes ``-X GET`` explicitly: ``gh api`` silently
    switches its default HTTP method from GET to POST as soon as any
    ``-f``/``-F`` flag is present (used here to pass query-string
    parameters like ``state``/``per_page``/``milestone``), so a read that
    relies on the implicit default would actually POST against a
    list/read-only endpoint and fail.
    """

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
                "-X", "GET",
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
                "-X", "GET",
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

    def pull_request_milestone(self, pull_request_number: int) -> str | None:
        payload = self._api(
            ["-X", "GET", f"repos/{self.repository}/pulls/{pull_request_number}"]
        )
        milestone = (payload or {}).get("milestone")
        return milestone["title"] if milestone else None

    def issue_milestone(self, issue_number: int) -> str | None:
        payload = self._api(
            ["-X", "GET", f"repos/{self.repository}/issues/{issue_number}"]
        )
        milestone = (payload or {}).get("milestone")
        return milestone["title"] if milestone else None

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
    apply_parser.add_argument("--output", default=None, help="also write the JSON report to this file")
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
    create_draft_parser.add_argument(
        "--plan", required=True,
        help="the original finish plan JSON file produced by 'finish plan' (not a result file)",
    )
    create_draft_parser.add_argument("--output", default=None, help="also write the JSON report to this file")
    create_draft_parser.set_defaults(func=cmd_finish_create_draft)

    plan_publication_parser = finish_sub.add_parser(
        "plan-publication",
        help="read-only: revalidate the actual remote draft against the original finish plan",
    )
    plan_publication_parser.add_argument(
        "--plan", required=True,
        help="the original finish plan JSON file produced by 'finish plan' (not a result file)",
    )
    plan_publication_parser.add_argument("--output", default=None, help="also write the JSON report to this file")
    plan_publication_parser.set_defaults(func=cmd_finish_plan_publication)

    publish_parser = finish_sub.add_parser("publish")
    publish_parser.add_argument(
        "--plan", required=True,
        help="the original finish plan JSON file produced by 'finish plan' (not a result file)",
    )
    publish_parser.add_argument("--output", default=None, help="also write the JSON report to this file")
    publish_parser.set_defaults(func=cmd_finish_publish)

    closeout_parser = finish_sub.add_parser("closeout")
    closeout_parser.add_argument(
        "--plan", required=True,
        help="the original finish plan JSON file produced by 'finish plan' (not a result file)",
    )
    closeout_parser.add_argument("--dry-run", action="store_true")
    closeout_parser.add_argument("--output", default=None, help="also write the JSON report to this file")
    closeout_parser.set_defaults(func=cmd_finish_closeout)

    inspect_parser = subparsers.add_parser("inspect")
    inspect_parser.add_argument("--release-branch", required=True)
    inspect_parser.add_argument("--version", default=None)
    inspect_parser.add_argument("--output", default=None, help="also write the JSON report to this file")
    inspect_parser.set_defaults(func=cmd_inspect)

    render_plan_parser = subparsers.add_parser(
        "render-plan", help="render a deterministic plan/result summary for thin workflow consumption"
    )
    render_plan_parser.add_argument("--plan", required=True, help="a plan file or a command-result file")
    render_plan_parser.add_argument(
        "--format", choices=("json", "markdown"), default="json",
        help="json (default, for compatibility) or markdown",
    )
    render_plan_parser.add_argument("--output", default=None, help="also write the rendered summary to this file")
    render_plan_parser.set_defaults(func=cmd_render_plan)

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
