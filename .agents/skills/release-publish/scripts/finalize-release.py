#!/usr/bin/env python3
"""Audit or finalize the GitHub-facing SkiaSharp release operations."""

from __future__ import annotations

import argparse
from dataclasses import dataclass
import hashlib
import json
from pathlib import Path
import re
import shutil
import sys

import release_publish as publish


GITHUB_REPOSITORY = "mono/SkiaSharp"
DOCS_WORKFLOW = "Sync - Release Notes & API Diffs"
SAMPLES_WORKFLOW = "Sync - Samples"
TEASER_LINKS_MARKER = "<!-- RELEASE_LINKS -->"
TAG_RE = re.compile(
    r"^v(?P<numeric>\d+\.\d+\.\d+(?:\.\d+)?)"
    r"(?:-(?P<channel>preview|rc)\.(?P<iteration>\d+)"
    r"\.(?P<build>\d+))?$"
)


@dataclass(frozen=True)
class TagVersion:
    name: str
    numeric: tuple[int, ...]
    channel: str | None
    iteration: int
    build: int

    @classmethod
    def parse(cls, value: str) -> TagVersion | None:
        match = TAG_RE.fullmatch(value)
        if not match:
            return None
        return cls(
            name=value,
            numeric=tuple(
                int(part) for part in match.group("numeric").split(".")
            ),
            channel=match.group("channel"),
            iteration=int(match.group("iteration") or 0),
            build=int(match.group("build") or 0),
        )

    @property
    def sort_key(self) -> tuple:
        channel_rank = {"preview": 0, "rc": 1, None: 2}[self.channel]
        return self.numeric, channel_rank, self.iteration, self.build


def previous_tag_candidates(
    release: publish.ReleaseVersion,
    current_tag: str,
    tags: list[str],
) -> list[str]:
    current = TagVersion.parse(current_tag)
    if current is None:
        raise publish.PublishError(f"invalid current tag {current_tag}")
    parsed = [
        item
        for tag in tags
        if tag != current_tag
        if (item := TagVersion.parse(tag))
    ]
    same_version = sorted(
        (
            item
            for item in parsed
            if item.numeric == release.parts
            and item.channel is not None
            and item.sort_key < current.sort_key
        ),
        key=lambda item: item.sort_key,
        reverse=True,
    )
    older_stable = sorted(
        (
            item
            for item in parsed
            if item.channel is None and item.numeric < release.parts
        ),
        key=lambda item: item.sort_key,
        reverse=True,
    )
    result: list[str] = []
    for item in [*same_version, *older_stable]:
        if item.name not in result:
            result.append(item.name)
        if len(result) == 10:
            break
    return result


class GitHub:
    def __init__(self) -> None:
        self.gh_path = shutil.which("gh")
        if not self.gh_path:
            raise publish.PublishError("GitHub CLI 'gh' was not found on PATH")

    def json(self, args: list[str], *, timeout: int = 120):
        return publish.run_json(
            [self.gh_path, *args],
            timeout=timeout,
        )

    def release(self, tag: str) -> dict | None:
        result = publish.run(
            [
                self.gh_path,
                "release",
                "view",
                tag,
                "--repo",
                GITHUB_REPOSITORY,
                "--json",
                (
                    "tagName,name,isDraft,isPrerelease,targetCommitish,"
                    "body,url,publishedAt,createdAt"
                ),
            ],
            check=False,
        )
        if result.returncode != 0:
            combined = f"{result.stdout}\n{result.stderr}".lower()
            if "release not found" in combined or "http 404" in combined:
                return None
            raise publish.PublishError(
                f"could not query GitHub release {tag}: "
                f"{result.stderr.strip() or result.stdout.strip()}"
            )
        return publish.parse_json_output(result.stdout)

    def generate_notes(
        self,
        tag: str,
        source_sha: str,
        previous_tag: str,
    ) -> dict:
        return self.json(
            [
                "api",
                f"repos/{GITHUB_REPOSITORY}/releases/generate-notes",
                "-X",
                "POST",
                "-f",
                f"tag_name={tag}",
                "-f",
                f"target_commitish={source_sha}",
                "-f",
                f"previous_tag_name={previous_tag}",
            ]
        )

    def dispatch_docs(self, version: str) -> None:
        publish.run(
            [
                self.gh_path,
                "workflow",
                "run",
                DOCS_WORKFLOW,
                "--repo",
                GITHUB_REPOSITORY,
                "--ref",
                "main",
                "-f",
                f"min_version={version}",
                "-f",
                f"max_version={version}",
                "-f",
                "source_branch=main",
            ]
        )

    def create_release(
        self,
        *,
        tag: str,
        title: str,
        source_sha: str,
        notes_file: Path,
        prerelease: bool,
    ) -> None:
        args = [
            self.gh_path,
            "release",
            "create",
            tag,
            "--repo",
            GITHUB_REPOSITORY,
            "--title",
            title,
            "--notes-file",
            str(notes_file),
            "--target",
            source_sha,
            "--verify-tag",
        ]
        if prerelease:
            args.extend(["--prerelease", "--latest=false"])
        publish.run(args)

    def publish_draft(
        self,
        *,
        tag: str,
        title: str,
        body_file: Path,
    ) -> None:
        publish.run(
            [
                self.gh_path,
                "release",
                "edit",
                tag,
                "--repo",
                GITHUB_REPOSITORY,
                "--title",
                title,
                "--notes-file",
                str(body_file),
                "--verify-tag",
                "--draft=false",
            ]
        )

    def sample_run(self, tag: str) -> dict | None:
        runs = self.json(
            [
                "run",
                "list",
                "--repo",
                GITHUB_REPOSITORY,
                "--workflow",
                SAMPLES_WORKFLOW,
                "--event",
                "release",
                "--branch",
                tag,
                "--limit",
                "1",
                "--json",
                "databaseId,status,conclusion,url,createdAt",
            ]
        )
        return runs[0] if runs else None

def generated_log_parts(body: str) -> tuple[str | None, str, int]:
    compare_url = None
    kept = []
    count = 0
    new_contributors = False
    for line in body.splitlines():
        stripped = line.strip()
        if stripped == "## What's Changed":
            continue
        if stripped == "## New Contributors":
            new_contributors = True
        prefix = "**Full Changelog**: "
        if stripped.startswith(prefix):
            compare_url = stripped[len(prefix) :].strip()
            continue
        kept.append(line)
        if (
            not new_contributors
            and stripped.startswith("* ")
            and " by @" in stripped
            and "/pull/" in stripped
        ):
            count += 1
    changes = "\n".join(kept).strip()
    if not changes:
        changes = "No pull requests were merged after the previous release."
    return compare_url, changes, count


def assemble_release_body(
    teaser: str,
    generated_log: str,
    *,
    public_version: str,
    notes_version: str,
) -> str:
    if teaser.count(TEASER_LINKS_MARKER) != 1:
        raise publish.PublishError(
            f"teaser must contain exactly one {TEASER_LINKS_MARKER}"
        )
    if "<details" in teaser or "**Full Changelog**:" in teaser:
        raise publish.PublishError(
            "teaser must not contain the folded log or full changelog line"
        )
    if "```" in teaser:
        raise publish.PublishError("teaser must not contain a code fence")
    if re.search(
        r"\b(?:CVE-\d|security (?:fix|release)|vulnerabilit)",
        teaser,
        re.IGNORECASE,
    ):
        raise publish.PublishError(
            "teaser must not advertise security or vulnerability details"
        )
    if "Replace this comment" in teaser:
        raise publish.PublishError("teaser subtitle has not been written")
    first_line = next(
        (line.strip() for line in teaser.splitlines() if line.strip()),
        "",
    )
    if not first_line or first_line.startswith(("#", "<!--")):
        raise publish.PublishError(
            "teaser must start with a plain-language subtitle"
        )
    compare_url, changes, count = generated_log_parts(generated_log)
    links = [
        (
            "\U0001f4e6 [NuGet]"
            f"(https://www.nuget.org/packages/SkiaSharp/{public_version})"
        ),
        (
            "\U0001f4d6 [Release notes]"
            "(https://mono.github.io/SkiaSharp/docs/releases/"
            f"{notes_version}.html)"
        ),
    ]
    if compare_url:
        links.append(f"\U0001f500 [Full changelog]({compare_url})")
    top = teaser.strip().replace(
        TEASER_LINKS_MARKER,
        " \u00b7 ".join(links),
    )
    return (
        f"{top}\n\n---\n\n"
        f"<details><summary>All changes ({count} pull requests)</summary>"
        f"\n\n{changes}\n\n</details>\n"
    )


def body_sha256(body: str) -> str:
    return hashlib.sha256(body.encode("utf-8")).hexdigest()


@dataclass
class FinalizeContext:
    root: Path
    release: publish.ReleaseVersion
    source_sha: str
    tag: str
    previous_tag: str | None
    generated_log: str | None
    expected_body: str | None
    report: dict


def load_release(args):
    repo = publish.GitRepository.discover()
    release = publish.ReleaseVersion.parse(args.release_branch)
    status = publish.status_report(repo.root, release.branch)
    handoff = publish.validate_status_handoff(
        status,
        release,
        expected_sha=args.expect_source_sha,
        expected_managed_run=args.expect_managed_run,
        expected_tests_run=args.expect_tests_run,
    )
    source_sha = repo.resolve_release_sha(
        release.branch,
        args.expect_source_sha,
    )
    nuget = publish.NuGet().check(handoff["versions"])
    if nuget["state"] != "ready":
        raise publish.PublishError(
            "both exact public packages must be on NuGet.org before "
            "release finalization"
        )
    return repo, release, status, handoff, source_sha, nuget


def execution_command(args, source_sha: str) -> str | None:
    if not args.previous_tag:
        return None
    command = [
        sys.executable,
        ".agents/skills/release-publish/scripts/finalize-release.py",
        args.release_branch,
        "--expect-source-sha",
        source_sha,
        "--expect-managed-run",
        str(args.expect_managed_run),
        "--expect-tests-run",
        str(args.expect_tests_run),
        "--previous-tag",
        args.previous_tag,
    ]
    if args.teaser_file:
        command.extend(["--teaser-file", str(args.teaser_file)])
    return publish.shell_command(command)


def artifact_paths(root: Path, tag: str) -> dict[str, Path]:
    directory = root / "output" / "release" / tag
    return {
        "directory": directory,
        "generated": directory / "generated-log.md",
        "teaser": directory / "teaser.md",
        "body": directory / "release-body.md",
    }


def write_artifacts(
    context: FinalizeContext,
    teaser_file: Path | None,
) -> dict[str, Path]:
    paths = artifact_paths(context.root, context.tag)
    paths["directory"].mkdir(parents=True, exist_ok=True)
    if context.generated_log is not None:
        paths["generated"].write_text(
            context.generated_log,
            encoding="utf-8",
        )
    if not paths["teaser"].exists():
        paths["teaser"].write_text(
            (
                "<!-- Replace this comment with one neutral subtitle line. -->"
                f"\n\n{TEASER_LINKS_MARKER}\n"
            ),
            encoding="utf-8",
        )
    if teaser_file and context.expected_body:
        paths["body"].write_text(
            context.expected_body,
            encoding="utf-8",
        )
    return paths


def finalization_states(
    *,
    previous_tag: bool,
    body_ready: bool,
    tag_exists: bool,
    published: bool,
    sample_run: dict | None,
) -> dict:
    ready = previous_tag and body_ready
    if sample_run:
        sample_status = (
            "done"
            if sample_run.get("conclusion") == "success"
            else "running"
            if sample_run.get("status") != "completed"
            else "failed"
        )
    else:
        sample_status = "running" if published else "blocked"

    if published:
        if sample_status == "running":
            next_action = "wait-for-samples"
        elif sample_status == "failed":
            next_action = "investigate-samples"
        else:
            next_action = "start-release-milestones"
    elif not previous_tag:
        next_action = "select-previous-tag"
    elif not body_ready:
        next_action = "write-release-teaser"
    else:
        next_action = "confirm-finalize-release"

    return {
        "tag": "done" if tag_exists else (
            "pending" if ready else "blocked"
        ),
        "docs": "done" if published else (
            "pending" if ready else "blocked"
        ),
        "teaser": "done" if published or body_ready else (
            "awaiting-user" if previous_tag else "blocked"
        ),
        "release": "done" if published else (
            "pending" if ready else "blocked"
        ),
        "samples": sample_status,
        "nextAction": next_action,
    }


def audit(args) -> FinalizeContext:
    repo, release, status, handoff, source_sha, nuget = load_release(args)
    public_skia = handoff["versions"]["public"]["SkiaSharp"]
    tag = f"v{public_skia}"
    tags = repo.remote_tags()
    candidates = previous_tag_candidates(
        release,
        tag,
        list(tags),
    )
    previous_tag = args.previous_tag
    if previous_tag:
        if previous_tag == tag or previous_tag not in tags:
            raise publish.PublishError(
                f"previous tag {previous_tag} is not a valid remote tag"
            )
        current = TagVersion.parse(tag)
        previous = TagVersion.parse(previous_tag)
        if (
            current is None
            or previous is None
            or previous.sort_key >= current.sort_key
        ):
            raise publish.PublishError(
                f"previous tag {previous_tag} is not older than {tag}"
            )

    tag_target = tags.get(tag)
    if tag_target and tag_target != source_sha:
        raise publish.PublishError(
            f"remote tag {tag} points to {tag_target}, expected {source_sha}"
        )

    github = GitHub()
    github_release = github.release(tag)
    if github_release:
        if github_release.get("name") != release.title:
            raise publish.PublishError(
                f"GitHub release title is {github_release.get('name')!r}, "
                f"expected {release.title!r}"
            )
        if bool(github_release.get("isPrerelease")) != (not release.stable):
            raise publish.PublishError(
                "GitHub release prerelease flag is incorrect"
            )

    generated_log = None
    generated_metadata = None
    expected_body = None
    if previous_tag:
        generated = github.generate_notes(tag, source_sha, previous_tag)
        generated_log = generated.get("body") or ""
        compare_url, _, count = generated_log_parts(generated_log)
        generated_metadata = {
            "pullRequestCount": count,
            "compareUrl": compare_url,
            "sha256": body_sha256(generated_log),
        }
        if args.teaser_file:
            expected_body = assemble_release_body(
                args.teaser_file.read_text(encoding="utf-8"),
                generated_log,
                public_version=public_skia,
                notes_version=release.numeric,
            )

    draft = bool(github_release and github_release.get("isDraft"))
    published = bool(github_release and not github_release.get("isDraft"))
    body_matches = (
        None
        if not expected_body or not github_release
        else body_sha256(github_release.get("body") or "")
        == body_sha256(expected_body)
    )
    if published and body_matches is False:
        raise publish.PublishError(
            "published GitHub release body differs from the approved teaser"
        )
    sample_run = github.sample_run(tag) if published else None
    states = finalization_states(
        previous_tag=bool(previous_tag),
        body_ready=bool(expected_body),
        tag_exists=bool(tag_target),
        published=published,
        sample_run=sample_run,
    )

    operations = [
        publish.operation(
            "push-tag",
            states["tag"],
            f"{tag} -> {source_sha}",
        ),
        publish.operation(
            "refresh-website-notes",
            states["docs"],
            (
                "Targeted docs workflow accompanied release publication"
                if published
                else f"Dispatch {DOCS_WORKFLOW} for {release.numeric}"
            ),
        ),
        publish.operation(
            "write-release-teaser",
            states["teaser"],
            (
                "Published release already has its customer-facing body"
                if published
                else f"Use approved teaser {args.teaser_file}"
                if expected_body
                else "Classify generated-log.md and write teaser.md"
                if previous_tag
                else "Select a previous tag before generating release notes"
            ),
        ),
        publish.operation(
            "publish-github-release",
            states["release"],
            (
                "GitHub release is published"
                if published
                else "Publish the approved GitHub release"
            ),
            url=github_release.get("url") if github_release else None,
        ),
    ]

    if sample_run:
        sample_state = states["samples"]
        sample_detail = (
            "Sample synchronization succeeded"
            if sample_state == "done"
            else "Sample synchronization is running"
            if sample_state == "running"
            else f"Sample synchronization {sample_run.get('conclusion')}"
        )
    else:
        sample_state = states["samples"]
        sample_detail = (
            "Wait for release-triggered sample synchronization"
            if published
            else "Publishing the release triggers sample synchronization"
        )
    operations.append(
        publish.operation(
            "sync-samples",
            sample_state,
            sample_detail,
            url=sample_run.get("url") if sample_run else None,
        )
    )

    next_action = states["nextAction"]

    paths = artifact_paths(repo.root, tag)
    report = {
        "schemaVersion": 1,
        "dryRun": bool(args.dry_run),
        "release": {
            "branch": release.branch,
            "version": release.raw,
            "type": release.release_type,
            "sourceSha": source_sha,
            "managedRunId": args.expect_managed_run,
            "testsRunId": args.expect_tests_run,
            "publicPackages": handoff["versions"]["public"],
            "tag": tag,
            "title": release.title,
            "previousTag": previous_tag,
            "previousTagCandidates": candidates,
        },
        "nuget": nuget,
        "githubRelease": (
            {
                "state": "draft" if draft else "published",
                "url": github_release.get("url"),
                "bodyMatches": body_matches,
            }
            if github_release
            else None
        ),
        "generatedLog": generated_metadata,
        "samplesRun": sample_run,
        "operations": operations,
        "nextAction": next_action,
        "warnings": status.get("warnings") or [],
        "executionCommand": (
            execution_command(args, source_sha)
            if next_action == "confirm-finalize-release"
            else None
        ),
        "milestonesCommand": (
            publish.shell_command(
                [
                    sys.executable,
                    ".agents/skills/release-milestones/scripts/"
                    "audit-milestones.py",
                    "--version",
                    release.numeric,
                    "--dry-run",
                ]
            )
            if next_action == "start-release-milestones"
            else None
        ),
        "artifacts": {
            key: str(value.relative_to(repo.root))
            for key, value in paths.items()
        }
        | {
            "expectedBodySha256": (
                body_sha256(expected_body)
                if expected_body
                else None
            )
        },
    }
    return FinalizeContext(
        root=repo.root,
        release=release,
        source_sha=source_sha,
        tag=tag,
        previous_tag=previous_tag,
        generated_log=generated_log,
        expected_body=expected_body,
        report=report,
    )


def execute(args, context: FinalizeContext) -> FinalizeContext:
    if not context.previous_tag:
        raise publish.PublishError(
            "select and pass --previous-tag before execution"
        )
    repo = publish.GitRepository(context.root)
    github = GitHub()
    tags = repo.remote_tags()
    github_release = github.release(context.tag)
    if github_release and not github_release.get("isDraft"):
        return audit(args)
    if not args.teaser_file or not context.expected_body:
        raise publish.PublishError(
            "write and pass --teaser-file before finalization"
        )
    if context.tag not in tags:
        repo.push_tag(context.tag, context.source_sha)
    paths = write_artifacts(context, args.teaser_file)
    if not github_release or github_release.get("isDraft"):
        github.dispatch_docs(context.release.numeric)
    if github_release is None:
        github.create_release(
            tag=context.tag,
            title=context.release.title,
            source_sha=context.source_sha,
            notes_file=paths["body"],
            prerelease=not context.release.stable,
        )
    elif github_release.get("isDraft"):
        github.publish_draft(
            tag=context.tag,
            title=context.release.title,
            body_file=paths["body"],
        )
    return audit(args)


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("release_branch")
    parser.add_argument("--expect-source-sha", required=True)
    parser.add_argument("--expect-managed-run", required=True, type=int)
    parser.add_argument("--expect-tests-run", required=True, type=int)
    parser.add_argument("--previous-tag")
    parser.add_argument("--teaser-file", type=Path)
    parser.add_argument("--dry-run", action="store_true")
    return parser


def main() -> int:
    args = create_parser().parse_args()
    try:
        context = audit(args)
        if args.dry_run:
            release_state = (
                context.report.get("githubRelease") or {}
            ).get("state")
            if (
                context.previous_tag
                and context.generated_log is not None
                and (release_state != "published" or args.teaser_file)
            ):
                write_artifacts(context, args.teaser_file)
        else:
            context = execute(args, context)
            context.report["dryRun"] = False
        print(json.dumps(context.report, indent=2))
    except (publish.PublishError, OSError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
