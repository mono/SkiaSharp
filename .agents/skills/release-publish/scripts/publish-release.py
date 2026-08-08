#!/usr/bin/env python3
"""Publish an approved SkiaSharp GitHub release draft."""

from __future__ import annotations

import argparse
import json
import sys

import release_github as github_release
import release_publish as publish


def execution_command(args, source_sha: str) -> str:
    return publish.shell_command(
        [
        sys.executable,
        ".agents/skills/release-publish/scripts/publish-release.py",
        *github_release.pinned_arguments(args, source_sha),
        ]
    )


def audit(args) -> tuple[github_release.ReleaseContext, dict]:
    context = github_release.load_release(args)
    existing = context.github_release
    draft = bool(existing and existing.get("isDraft"))
    published = bool(existing and not existing.get("isDraft"))

    generated_log = None
    if existing:
        body = existing.get("body") or ""
        if not body:
            raise publish.PublishError(
                "GitHub release has no generated release notes"
            )
        generated_log = github_release.extract_generated_notes(body)

    if not existing:
        next_action = "create-release-draft"
    elif draft:
        next_action = "confirm-publish-release"
    else:
        next_action = "start-release-milestones"

    operations = [
        publish.operation(
            "dispatch-release-notes",
            "done" if published else (
                "pending"
                if next_action == "confirm-publish-release"
                else "blocked"
            ),
            (
                "Targeted docs workflow dispatched after publication"
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
                else                 "Publish the marked generated-notes draft"
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
                "bodySha256": github_release.body_sha256(
                    existing.get("body") or ""
                ),
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
    }
    return context, report


def execute(
    args,
    context: github_release.ReleaseContext,
) -> tuple[github_release.ReleaseContext, dict]:
    existing = context.github_release
    if existing is None:
        raise publish.PublishError(
            "create the GitHub release draft before publication"
        )
    if existing.get("isDraft"):
        context.github.publish_draft(
            tag=context.tag,
            title=context.release.title,
        )
        context.github.dispatch_docs(context.release.numeric)

    return audit(args)


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    github_release.add_release_arguments(parser)
    parser.add_argument("--dry-run", action="store_true")
    return parser


def main() -> int:
    args = create_parser().parse_args()
    try:
        context, report = audit(args)
        if not args.dry_run:
            context, report = execute(
                args,
                context,
            )
            report["dryRun"] = False
        print(json.dumps(report, indent=2))
    except (publish.PublishError, OSError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
