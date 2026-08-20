#!/usr/bin/env python3
"""Shared GitHub release helpers for SkiaSharp publication."""

from __future__ import annotations

from dataclasses import dataclass
import hashlib
from pathlib import Path
import re
import shutil
import sys

import release_publish as publish


GITHUB_REPOSITORY = "mono/SkiaSharp"
DOCS_WORKFLOW = "Sync - Release Notes & API Diffs"
TEASER_LINKS_MARKER = "<!-- RELEASE_LINKS -->"
TAG_RE = re.compile(
    r"^v(?P<numeric>\d+\.\d+\.\d+(?:\.\d+)?)"
    r"(?:-(?P<channel>preview|rc)(?P<suffix>(?:\.\d+)+))?$"
)


@dataclass(frozen=True)
class TagVersion:
    name: str
    numeric: tuple[int, ...]
    channel: str | None
    prerelease: tuple[int, ...]

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
            prerelease=tuple(
                int(part)
                for part in (match.group("suffix") or "").lstrip(".").split(".")
                if part
            ),
        )

    @property
    def sort_key(self) -> tuple:
        channel_rank = {"preview": 0, "rc": 1, None: 2}[self.channel]
        return self.numeric, channel_rank, self.prerelease


def previous_release_tag(
    current_tag: str,
    tags: list[str],
) -> str:
    current = TagVersion.parse(current_tag)
    if current is None:
        raise publish.PublishError(f"invalid current tag {current_tag}")
    previous = [
        item
        for tag in tags
        if tag != current_tag
        if (item := TagVersion.parse(tag))
        if item.sort_key < current.sort_key
    ]
    if not previous:
        raise publish.PublishError(
            f"no release tag precedes {current_tag}"
        )
    return max(previous, key=lambda item: item.sort_key).name


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

    def create_draft(
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
            "--draft",
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
class ReleaseContext:
    root: Path
    repository: publish.GitRepository
    release: publish.ReleaseVersion
    status: dict
    handoff: dict
    source_sha: str
    nuget: dict
    tag: str
    tags: dict[str, str]
    github: GitHub
    github_release: dict | None


def load_release(args) -> ReleaseContext:
    repo = publish.GitRepository.discover()
    release = publish.ReleaseVersion.parse(args.release_branch)
    status = publish.status_report(repo.root, release.branch)
    handoff = publish.validate_status_handoff(
        status,
        release,
        expected_sha=args.expect_source_sha,
        expected_build_run=args.expect_build_run,
        expected_tests_run=args.expect_tests_run,
        expected_bar_build=args.expect_bar_build,
    )
    source_sha = repo.resolve_release_sha(
        release.branch,
        args.expect_source_sha,
    )
    nuget = publish.NuGet().check(handoff["versions"])
    if nuget["state"] != "ready":
        raise publish.PublishError(
            "both exact public packages must be on NuGet.org before "
            "GitHub release publication"
        )
    public_skia = handoff["versions"]["public"]["SkiaSharp"]
    tag = f"v{public_skia}"
    tags = repo.remote_tags()
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
        target = github_release.get("targetCommitish")
        if target and target != source_sha:
            raise publish.PublishError(
                f"GitHub release targets {target}, expected {source_sha}"
            )
    return ReleaseContext(
        root=repo.root,
        repository=repo,
        release=release,
        status=status,
        handoff=handoff,
        source_sha=source_sha,
        nuget=nuget,
        tag=tag,
        tags=tags,
        github=github,
        github_release=github_release,
    )


def pinned_arguments(args, source_sha: str | None = None) -> list[str]:
    return [
        args.release_branch,
        "--expect-source-sha",
        source_sha or args.expect_source_sha,
        "--expect-build-run",
        str(args.expect_build_run),
        "--expect-tests-run",
        str(args.expect_tests_run),
        "--expect-bar-build",
        str(args.expect_bar_build),
    ]


def add_release_arguments(parser) -> None:
    parser.add_argument("release_branch")
    parser.add_argument("--expect-source-sha", required=True)
    parser.add_argument("--expect-build-run", required=True, type=int)
    parser.add_argument("--expect-tests-run", required=True, type=int)
    parser.add_argument("--expect-bar-build", required=True, type=int)


def artifact_paths(root: Path, tag: str) -> dict[str, Path]:
    directory = root / "output" / "release" / tag
    return {
        "directory": directory,
        "generated": directory / "generated-log.md",
        "teaser": directory / "teaser.md",
        "body": directory / "release-body.md",
    }


def write_generated_artifacts(
    context: ReleaseContext,
    generated_log: str,
) -> dict[str, Path]:
    paths = artifact_paths(context.root, context.tag)
    paths["directory"].mkdir(parents=True, exist_ok=True)
    paths["generated"].write_text(generated_log, encoding="utf-8")
    if not paths["teaser"].exists():
        paths["teaser"].write_text(
            (
                "<!-- Replace this comment with one neutral subtitle line. -->"
                f"\n\n{TEASER_LINKS_MARKER}\n"
            ),
            encoding="utf-8",
        )
    return paths


def write_release_body(
    context: ReleaseContext,
    body: str,
) -> dict[str, Path]:
    paths = artifact_paths(context.root, context.tag)
    paths["directory"].mkdir(parents=True, exist_ok=True)
    paths["body"].write_text(body, encoding="utf-8")
    return paths


def generated_log_metadata(body: str) -> dict:
    compare_url, _, count = generated_log_parts(body)
    return {
        "pullRequestCount": count,
        "compareUrl": compare_url,
        "sha256": body_sha256(body),
    }


def release_details(context: ReleaseContext) -> dict:
    return {
        "branch": context.release.branch,
        "version": context.release.raw,
        "type": context.release.release_type,
        "sourceSha": context.source_sha,
        "buildRunId": context.handoff["build"]["runId"],
        "testsRunId": context.handoff["tests"]["runId"],
        "barBuildId": context.handoff["bar"]["id"],
        "publicPackages": context.handoff["versions"]["public"],
        "tag": context.tag,
        "title": context.release.title,
    }


def milestones_command(release: publish.ReleaseVersion) -> str:
    return publish.shell_command(
        [
            sys.executable,
            ".agents/skills/release-milestones/scripts/"
            "reconcile-release-assignments.py",
            "--version",
            release.numeric,
            "--dry-run",
        ]
    )
