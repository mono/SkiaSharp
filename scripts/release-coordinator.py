#!/usr/bin/env python3
"""Run the SkiaSharp release workflow as four resumable local phases.

Examples:
  python3 scripts/release-coordinator.py start main
  python3 scripts/release-coordinator.py packages release/4.152.0-preview.1
  python3 scripts/release-coordinator.py release release/4.152.0-preview.1
  python3 scripts/release-coordinator.py finish release/4.152.0-preview.1

All phases are read-only by default. Pass the phase-specific execution flag
only after reviewing the preceding report.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import re
import shlex
import subprocess
import sys


ROOT = Path(__file__).resolve().parent.parent
PYTHON = sys.executable
RELEASE_BRANCH_RE = re.compile(
    r"^release/(?P<numeric>\d+\.\d+\.\d+(?:\.\d+)?)"
    r"(?:-(?:preview|rc)\.\d+)?$"
)


class CoordinatorError(RuntimeError):
    """The local release phase could not be planned or advanced safely."""


def run_json(
    command: list[str],
    *,
    stream_stderr: bool = False,
    timeout: int = 7500,
) -> dict:
    result = subprocess.run(
        command,
        cwd=ROOT,
        stdout=subprocess.PIPE,
        stderr=None if stream_stderr else subprocess.PIPE,
        text=True,
        timeout=timeout,
    )
    if result.returncode != 0:
        detail = (
            ""
            if stream_stderr
            else (result.stderr or "").strip()
        )
        raise CoordinatorError(
            f"command failed ({result.returncode}): {shlex.join(command)}"
            + (f"\n{detail}" if detail else "")
        )
    try:
        return json.loads(result.stdout)
    except json.JSONDecodeError as error:
        raise CoordinatorError(
            f"command returned invalid JSON: {shlex.join(command)}"
        ) from error


def set_option(command: list[str], name: str, value: str) -> None:
    if name in command:
        index = command.index(name)
        if index + 1 >= len(command):
            raise CoordinatorError(f"{name} has no value")
        command[index + 1] = value
    else:
        command.extend([name, value])


def add_flag(command: list[str], name: str) -> None:
    if name not in command:
        command.append(name)


def replace_file_option(
    command: list[str],
    name: str,
    value: Path,
) -> None:
    set_option(command, name, str(value.resolve()))


def detect_publication(target: str) -> dict:
    return run_json(
        [
            PYTHON,
            (
                ".agents/skills/release-publish/scripts/"
                "detect-release-publish.py"
            ),
            target,
        ]
    )


def start_phase(args) -> dict:
    detection = None
    version = args.version
    if not version:
        detection = run_json(
            [
                PYTHON,
                (
                    ".agents/skills/release-branch/scripts/"
                    "detect-release-version.py"
                ),
                args.target,
            ]
        )
        version = detection["releaseVersion"]
    command = [
        PYTHON,
        (
            ".agents/skills/release-branch/scripts/"
            "create-release-branches.py"
        ),
        version,
        "--dry-run",
    ]
    plan = run_json(command)
    result = None
    if args.execute:
        execution = plan.get("executionCommand")
        result = (
            run_json(shlex.split(execution), stream_stderr=True)
            if execution
            else plan
        )
    return {
        "schemaVersion": 1,
        "phase": "start",
        "dryRun": not args.execute,
        "target": args.target,
        "releaseVersion": version,
        "detection": detection,
        "plan": plan,
        "result": result,
        "nextCommand": (
            f"{PYTHON} scripts/release-coordinator.py packages "
            f"release/{version}"
        ),
    }


def packages_phase(args) -> dict:
    context = detect_publication(args.target)
    audit_command = shlex.split(context["pushAuditCommand"])
    set_option(
        audit_command,
        "--verification",
        args.verification,
    )
    plan = run_json(audit_command)
    allowed = {
        "confirm-publish-packages",
        "approve-publish-run",
        "wait-for-nuget",
        "start-release-draft",
    }
    if plan["nextAction"] not in allowed:
        raise CoordinatorError(
            f"package publication needs recovery: {plan['nextAction']}"
        )
    result = None
    if args.execute:
        command_text = (
            plan.get("executionCommand")
            or plan.get("resumeCommand")
        )
        if command_text:
            command = shlex.split(command_text)
            set_option(
                command,
                "--verification",
                args.verification,
            )
            add_flag(command, "--wait")
            result = run_json(command, stream_stderr=True)
        else:
            result = plan
        if result["nextAction"] != "start-release-draft":
            raise CoordinatorError(
                f"package publication ended at {result['nextAction']}; "
                "rerun this phase to resume"
            )
    return {
        "schemaVersion": 1,
        "phase": "packages",
        "dryRun": not args.execute,
        "verificationMode": args.verification,
        "context": context,
        "plan": plan,
        "result": result,
        "nextCommand": (
            f"{PYTHON} scripts/release-coordinator.py release "
            f"{context['releaseBranch']}"
        ),
    }


def release_phase(args) -> dict:
    context = detect_publication(args.target)
    draft = run_json(shlex.split(context["draftAuditCommand"]))
    allowed = {
        "confirm-create-release-draft",
        "write-release-teaser",
        "audit-release-publication",
    }
    if draft["nextAction"] not in allowed:
        raise CoordinatorError(
            f"release draft needs recovery: {draft['nextAction']}"
        )

    draft_result = draft
    draft_created = False
    if args.execute_draft and draft.get("executionCommand"):
        draft_result = run_json(
            shlex.split(draft["executionCommand"]),
            stream_stderr=True,
        )
        draft_created = True

    publication = None
    publication_result = None
    publication_command = draft_result.get("publishAuditCommand")
    default_teaser = (
        ROOT / draft_result["artifacts"]["teaser"]
        if draft_result.get("artifacts")
        else None
    )
    teaser_ready = bool(
        default_teaser
        and default_teaser.is_file()
        and "Replace this comment"
        not in default_teaser.read_text(encoding="utf-8")
    )
    should_plan_publication = bool(
        args.teaser_file
        or teaser_ready
        or draft_result["nextAction"] == "audit-release-publication"
    )
    if publication_command and not draft_created and should_plan_publication:
        command = shlex.split(publication_command)
        if args.teaser_file:
            if not args.teaser_file.is_file():
                raise CoordinatorError(
                    f"teaser file does not exist: {args.teaser_file}"
                )
            replace_file_option(
                command,
                "--teaser-file",
                args.teaser_file,
            )
        publication = run_json(command)
        if args.publish:
            execution = publication.get("executionCommand")
            publication_result = (
                run_json(
                    shlex.split(execution),
                    stream_stderr=True,
                )
                if execution
                else publication
            )
            if (
                publication_result["nextAction"]
                != "start-release-milestones"
            ):
                raise CoordinatorError(
                    "release publication ended at "
                    f"{publication_result['nextAction']}"
                )
    elif args.publish:
        raise CoordinatorError(
            "publication plan is unavailable; create the draft and complete "
            "the teaser first"
        )

    return {
        "schemaVersion": 1,
        "phase": "release",
        "dryRun": not (args.execute_draft or args.publish),
        "context": context,
        "draft": draft,
        "draftResult": draft_result,
        "publication": publication,
        "publicationResult": publication_result,
        "nextCommand": (
            f"{PYTHON} scripts/release-coordinator.py finish "
            f"{context['releaseBranch']}"
        ),
    }


def release_numeric(branch: str) -> str:
    match = RELEASE_BRANCH_RE.fullmatch(branch)
    if not match:
        raise CoordinatorError(
            f"invalid exact release branch: {branch}"
        )
    return match.group("numeric")


def finish_phase(args) -> dict:
    context = detect_publication(args.target)
    numeric = release_numeric(context["releaseBranch"])
    reconcile_command = [
        PYTHON,
        (
            ".agents/skills/release-milestones/scripts/"
            "reconcile-release-assignments.py"
        ),
        "--version",
        numeric,
        "--dry-run",
    ]
    reconcile = run_json(reconcile_command)
    advance_command = [
        PYTHON,
        (
            ".agents/skills/release-milestones/scripts/"
            "advance-release-milestones.py"
        ),
        "--dry-run",
    ]
    advance = run_json(advance_command)

    reconcile_result = None
    advance_result = None
    if args.execute:
        reconciliation = reconcile.get("executionCommand")
        reconcile_result = (
            run_json(
                shlex.split(reconciliation),
                stream_stderr=True,
            )
            if reconciliation
            else reconcile
        )
        if reconcile_result["nextAction"] != "complete":
            raise CoordinatorError(
                "assignment reconciliation ended at "
                f"{reconcile_result['nextAction']}"
            )

        current_advance = run_json(advance_command)
        compared = (
            "operations",
            "closureOperations",
            "warnings",
            "nextAction",
        )
        if any(
            current_advance[field] != advance[field]
            for field in compared
        ):
            raise CoordinatorError(
                "milestone state changed after reconciliation; rerun finish "
                "to review the current advancement plan"
            )
        advancement = current_advance.get("executionCommand")
        advance_result = (
            run_json(
                shlex.split(advancement),
                stream_stderr=True,
            )
            if advancement
            else current_advance
        )
        if advance_result["nextAction"] != "complete":
            raise CoordinatorError(
                f"milestone advancement ended at "
                f"{advance_result['nextAction']}"
            )

    return {
        "schemaVersion": 1,
        "phase": "finish",
        "dryRun": not args.execute,
        "context": context,
        "reconcile": reconcile,
        "advance": advance,
        "reconcileResult": reconcile_result,
        "advanceResult": advance_result,
    }


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    subparsers = parser.add_subparsers(dest="command", required=True)

    start = subparsers.add_parser(
        "start",
        aliases=["a"],
        help="A: resolve and create paired release branches",
    )
    start.add_argument("target", nargs="?", default="main")
    start.add_argument("--version")
    start.add_argument("--execute", action="store_true")
    start.set_defaults(handler=start_phase)

    packages = subparsers.add_parser(
        "packages",
        aliases=["b"],
        help="B: recover/publish the exact packages",
    )
    packages.add_argument("target")
    packages.add_argument("--execute", action="store_true")
    packages.add_argument(
        "--verification",
        choices=("nuget", "azure"),
        default="nuget",
    )
    packages.set_defaults(handler=packages_phase)

    release = subparsers.add_parser(
        "release",
        aliases=["c"],
        help="C: create the draft, validate the teaser, and publish",
    )
    release.add_argument("target")
    release.add_argument("--execute-draft", action="store_true")
    release.add_argument("--teaser-file", type=Path)
    release.add_argument("--publish", action="store_true")
    release.set_defaults(handler=release_phase)

    finish = subparsers.add_parser(
        "finish",
        aliases=["d"],
        help="D: reconcile assignments and advance milestones",
    )
    finish.add_argument("target")
    finish.add_argument("--execute", action="store_true")
    finish.set_defaults(handler=finish_phase)
    return parser


def main() -> int:
    args = create_parser().parse_args()
    try:
        report = args.handler(args)
        print(json.dumps(report, indent=2))
    except (
        CoordinatorError,
        OSError,
        subprocess.SubprocessError,
    ) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
