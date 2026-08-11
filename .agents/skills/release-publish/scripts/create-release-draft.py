#!/usr/bin/env python3
"""Create the exact release tag and generated-notes GitHub draft."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys

import release_github as github_release
import release_publish as publish


def execution_command(args, source_sha: str) -> str:
    return publish.shell_command(
        [
            sys.executable,
            ".agents/skills/release-publish/scripts/create-release-draft.py",
            *github_release.pinned_arguments(args, source_sha),
        ]
    )


def publication_audit_command(
    context: github_release.ReleaseContext,
    *,
    teaser_file: Path | None,
) -> str:
    command = [
        sys.executable,
        ".agents/skills/release-publish/scripts/publish-release.py",
        context.release.branch,
        "--expect-source-sha",
        context.source_sha,
        "--expect-managed-run",
        str(context.handoff["managed"]["runId"]),
        "--expect-tests-run",
        str(context.handoff["tests"]["runId"]),
    ]
    if teaser_file:
        command.extend(["--teaser-file", str(teaser_file)])
    command.append("--dry-run")
    return publish.shell_command(command)


def audit(args) -> tuple[github_release.ReleaseContext, dict, str | None]:
    context = github_release.load_release(args)
    previous_tag = github_release.previous_release_tag(
        context.tag,
        list(context.tags),
    )

    existing = context.github_release
    draft = bool(existing and existing.get("isDraft"))
    published = bool(existing and not existing.get("isDraft"))
    generated_log = None
    generated_metadata = None
    if draft:
        generated_log = existing.get("body") or ""
        if not generated_log:
            raise publish.PublishError(
                "GitHub draft has no generated release notes"
            )
        generated_metadata = github_release.generated_log_metadata(
            generated_log
        )
    elif not existing:
        generated = context.github.generate_notes(
            context.tag,
            context.source_sha,
            previous_tag,
        )
        generated_log = generated.get("body") or ""
        generated_metadata = github_release.generated_log_metadata(
            generated_log
        )

    if published:
        next_action = "audit-release-publication"
    elif draft:
        next_action = "write-release-teaser"
    else:
        next_action = "confirm-create-release-draft"

    tag_exists = context.tag in context.tags
    operations = [
        publish.operation(
            "push-tag",
            "done" if tag_exists else (
                "pending"
                if next_action == "confirm-create-release-draft"
                else "blocked"
            ),
            f"{context.tag} -> {context.source_sha}",
        ),
        publish.operation(
            "create-github-draft",
            "done" if existing else (
                "pending"
                if next_action == "confirm-create-release-draft"
                else "blocked"
            ),
            (
                "GitHub release is already published"
                if published
                else "Generated-notes draft exists"
                if draft
                else "Create a draft with exact generated release notes"
            ),
            url=existing.get("url") if existing else None,
        ),
    ]
    paths = github_release.artifact_paths(context.root, context.tag)
    report = {
        "schemaVersion": 1,
        "dryRun": bool(args.dry_run),
        "release": github_release.release_details(context)
        | {
            "previousTag": previous_tag,
        },
        "nuget": context.nuget,
        "githubRelease": (
            {
                "state": "draft" if draft else "published",
                "url": existing.get("url"),
            }
            if existing
            else None
        ),
        "generatedLog": generated_metadata,
        "operations": operations,
        "nextAction": next_action,
        "warnings": context.status.get("warnings") or [],
        "executionCommand": (
            execution_command(args, context.source_sha)
            if next_action == "confirm-create-release-draft"
            else None
        ),
        "publishAuditCommand": (
            publication_audit_command(
                context,
                teaser_file=(
                    paths["teaser"]
                    if next_action == "write-release-teaser"
                    else None
                ),
            )
            if next_action in {
                "write-release-teaser",
                "audit-release-publication",
            }
            else None
        ),
        "artifacts": {
            key: str(value.relative_to(context.root))
            for key, value in paths.items()
        },
    }
    return context, report, generated_log


def execute(
    args,
    context: github_release.ReleaseContext,
    generated_log: str | None,
) -> tuple[github_release.ReleaseContext, dict, str | None]:
    if generated_log is None:
        raise publish.PublishError(
            "generated release notes are required before creating the draft"
        )
    if context.github_release and not context.github_release.get("isDraft"):
        return audit(args)
    if context.tag not in context.tags:
        context.repository.push_tag(context.tag, context.source_sha)
    if context.github_release is None:
        paths = github_release.write_generated_artifacts(
            context,
            generated_log,
        )
        context.github.create_draft(
            tag=context.tag,
            title=context.release.title,
            source_sha=context.source_sha,
            notes_file=paths["generated"],
            prerelease=not context.release.stable,
        )
    return audit(args)


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    github_release.add_release_arguments(parser)
    parser.add_argument("--dry-run", action="store_true")
    return parser


def main() -> int:
    args = create_parser().parse_args()
    try:
        context, report, generated_log = audit(args)
        if not args.dry_run:
            context, report, generated_log = execute(
                args,
                context,
                generated_log,
            )
            report["dryRun"] = False
        if (
            report["nextAction"] == "write-release-teaser"
            and generated_log is not None
        ):
            # A remote draft is the source of truth; refresh its ignored local
            # editorial files even during a read-only remote audit.
            github_release.write_generated_artifacts(context, generated_log)
        print(json.dumps(report, indent=2))
    except (publish.PublishError, OSError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
