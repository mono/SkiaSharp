#!/usr/bin/env python3
"""Publish an approved SkiaSharp GitHub release draft."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys

import release_github as github_release
import release_publish as publish


def execution_command(args, source_sha: str) -> str:
    command = [
        sys.executable,
        ".agents/skills/release-publish/scripts/publish-release.py",
        *github_release.pinned_arguments(args, source_sha),
    ]
    if args.teaser_file:
        command.extend(["--teaser-file", str(args.teaser_file)])
    return publish.shell_command(command)


def audit(args) -> tuple[github_release.ReleaseContext, dict, str | None]:
    context = github_release.load_release(args)
    existing = context.github_release
    paths = github_release.artifact_paths(context.root, context.tag)
    draft = bool(existing and existing.get("isDraft"))
    published = bool(existing and not existing.get("isDraft"))

    generated_log = None
    if draft:
        generated_log = existing.get("body") or ""
        if not generated_log:
            raise publish.PublishError(
                "GitHub draft has no generated release notes"
            )
        github_release.write_generated_artifacts(context, generated_log)
    elif paths["generated"].is_file():
        generated_log = paths["generated"].read_text(encoding="utf-8")

    expected_body = None
    if args.teaser_file:
        if generated_log is None:
            raise publish.PublishError(
                "generated-log.md is required to validate the teaser"
            )
        expected_body = github_release.assemble_release_body(
            args.teaser_file.read_text(encoding="utf-8"),
            generated_log,
            public_version=(
                context.handoff["versions"]["public"]["SkiaSharp"]
            ),
            notes_version=context.release.numeric,
        )
        github_release.write_release_body(context, expected_body)

    body_matches = (
        None
        if not expected_body or not existing
        else github_release.body_sha256(existing.get("body") or "")
        == github_release.body_sha256(expected_body)
    )
    if published and body_matches is False:
        raise publish.PublishError(
            "published GitHub release body differs from the approved teaser"
        )

    if not existing:
        next_action = "create-release-draft"
    elif draft and not expected_body:
        next_action = "write-release-teaser"
    elif draft:
        next_action = "confirm-publish-release"
    else:
        next_action = "start-release-milestones"

    operations = [
        publish.operation(
            "refresh-website-notes",
            "done" if published else (
                "pending"
                if next_action == "confirm-publish-release"
                else "blocked"
            ),
            (
                "Targeted docs workflow accompanied publication"
                if published
                else f"Dispatch {github_release.DOCS_WORKFLOW} for "
                f"{context.release.numeric}"
            ),
        ),
        publish.operation(
            "publish-github-release",
            "done" if published else (
                "pending"
                if next_action == "confirm-publish-release"
                else "blocked"
            ),
            (
                "GitHub release is published"
                if published
                else "Publish the approved draft"
                if draft and expected_body
                else "Write and validate the customer teaser"
                if draft
                else "Create the generated-notes draft first"
            ),
            url=existing.get("url") if existing else None,
        ),
    ]
    report = {
        "schemaVersion": 1,
        "dryRun": bool(args.dry_run),
        "release": github_release.release_details(context),
        "nuget": context.nuget,
        "githubRelease": (
            {
                "state": "draft" if draft else "published",
                "url": existing.get("url"),
                "bodyMatches": body_matches,
            }
            if existing
            else None
        ),
        "generatedLog": (
            github_release.generated_log_metadata(generated_log)
            if generated_log is not None
            else None
        ),
        "operations": operations,
        "nextAction": next_action,
        "warnings": context.status.get("warnings") or [],
        "executionCommand": (
            execution_command(args, context.source_sha)
            if next_action == "confirm-publish-release"
            else None
        ),
        "milestonesCommand": (
            github_release.milestones_command(context.release)
            if next_action == "start-release-milestones"
            else None
        ),
        "artifacts": {
            key: str(value.relative_to(context.root))
            for key, value in paths.items()
        }
        | {
            "expectedBodySha256": (
                github_release.body_sha256(expected_body)
                if expected_body
                else None
            )
        },
    }
    return context, report, expected_body


def execute(
    args,
    context: github_release.ReleaseContext,
    expected_body: str | None,
) -> tuple[github_release.ReleaseContext, dict, str | None]:
    existing = context.github_release
    if existing is None:
        raise publish.PublishError(
            "create the GitHub release draft before publication"
        )
    if existing.get("isDraft"):
        if not args.teaser_file or expected_body is None:
            raise publish.PublishError(
                "write and pass --teaser-file before publication"
            )
        paths = github_release.write_release_body(context, expected_body)
        context.github.dispatch_docs(context.release.numeric)
        context.github.publish_draft(
            tag=context.tag,
            title=context.release.title,
            body_file=paths["body"],
        )

    return audit(args)


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    github_release.add_release_arguments(parser)
    parser.add_argument("--teaser-file", type=Path)
    parser.add_argument("--dry-run", action="store_true")
    return parser


def main() -> int:
    args = create_parser().parse_args()
    try:
        context, report, expected_body = audit(args)
        if not args.dry_run:
            context, report, expected_body = execute(
                args,
                context,
                expected_body,
            )
            report["dryRun"] = False
        print(json.dumps(report, indent=2))
    except (publish.PublishError, OSError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
