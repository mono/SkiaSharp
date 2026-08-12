"""Shared paths, configuration, and pure utilities for release notes."""

from __future__ import annotations

import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys


REPO = "mono/SkiaSharp"
RELEASES_DIR = Path("documentation/docfx/releases")
DEFAULT_POLISH_LIST = Path("output/files-to-polish.txt")
VERSIONS_JSON_PATH = Path("scripts/infra/docs/versions.json")
CO_RELEASE_MAP_PATH = RELEASES_DIR / "_sources" / "co-release-map.json"
CHROME_SCHEDULE_URL = (
    "https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone={}"
)
EXACT_RELEASE_TAG_RE = re.compile(
    r"^v\d+(?:\.\d+){2,3}"
    r"(?:-(?:alpha|beta|preview|rc)(?:\.\d+)+)?$"
)
MONTH_ABBR = [
    "Jan", "Feb", "Mar", "Apr", "May", "Jun",
    "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
]

_VERSIONS_CONFIG: dict[str, list[dict]] = {}
_HISTORY_FLOOR: dict | None = None
_CO_RELEASE_MAP: dict | None = None


def log(*args, **kwargs) -> None:
    kwargs["file"] = sys.stderr
    print(*args, **kwargs)


def run(args: list[str], check: bool = True) -> str:
    result = subprocess.run(args, capture_output=True, text=True, check=check)
    return result.stdout.strip()


def write_polish_list(files, path=None) -> None:
    path = Path(path) if path else DEFAULT_POLISH_LIST
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(
        "".join("{}\n".format(file) for file in files),
        encoding="utf-8",
    )
    log("Wrote files-to-polish list ({} file{}) -> {}".format(
        len(files), "" if len(files) == 1 else "s", path
    ))
    for file in files:
        log("  {}".format(file))


def removeprefix(value: str, prefix: str) -> str:
    return value[len(prefix):] if value.startswith(prefix) else value


def minor_group(version: str) -> str:
    parts = version.split(".")
    return "{}.{}".format(parts[0], parts[1]) if len(parts) >= 2 else parts[0]


def version_key(version: str) -> list[int]:
    return [int(number) for number in re.findall(r"\d+", version)]


def core_tuple(core: str) -> tuple[int, int, int, int]:
    parts = (core.split(".") + ["0", "0", "0", "0"])[:4]
    return tuple(int(part) if part.isdigit() else 0 for part in parts)


def get_upcoming_version() -> str | None:
    path = Path("scripts/azure-templates-variables.yml")
    if path.exists():
        for line in path.read_text().splitlines():
            match = re.match(r"\s*SKIASHARP_VERSION:\s*(\S+)", line)
            if match:
                return match.group(1)
    return None


def load_versions_config(family: str = "skiasharp") -> list[dict]:
    cached = _VERSIONS_CONFIG.get(family)
    if cached is not None:
        return cached
    entries = []
    if VERSIONS_JSON_PATH.exists():
        data = json.loads(VERSIONS_JSON_PATH.read_text())
        bucket = data.get(family, {})
        if not isinstance(bucket, dict):
            raise ValueError(
                "versions.json: '{}' must be an object keyed by line; got {}"
                .format(family, type(bucket).__name__)
            )
        for line, fields in bucket.items():
            entry = dict(fields)
            entry["version"] = line
            entries.append(entry)
    _VERSIONS_CONFIG[family] = entries
    return entries


def versions_config_lookup(
    version: str,
    family: str = "skiasharp",
) -> dict | None:
    return next(
        (
            entry for entry in load_versions_config(family)
            if entry.get("version") == version
        ),
        None,
    )


def history_floor(family: str = "skiasharp") -> str | None:
    global _HISTORY_FLOOR
    if _HISTORY_FLOOR is None:
        _HISTORY_FLOOR = {}
        if VERSIONS_JSON_PATH.exists():
            block = json.loads(VERSIONS_JSON_PATH.read_text()).get(
                "history_floor"
            ) or {}
            if isinstance(block, dict):
                _HISTORY_FLOOR = {
                    key: value
                    for key, value in block.items()
                    if isinstance(value, str) and value
                }
    return _HISTORY_FLOOR.get(family)


def is_below_history_floor(
    version: str,
    family: str = "skiasharp",
) -> bool:
    floor = history_floor(family)
    return bool(floor and core_tuple(version) < core_tuple(floor))


def require_scope_at_or_above_history_floor(
    min_core: tuple | None,
    max_core: tuple | None,
    family: str = "skiasharp",
) -> None:
    floor = history_floor(family)
    if not floor:
        return
    floor_core = core_tuple(floor)
    for name, bound in (
        ("--min-version", min_core),
        ("--max-version", max_core),
    ):
        if bound is not None and bound < floor_core:
            raise RuntimeError(
                "{} is below the {} history floor. Lower history_floor.{} "
                "in versions.json before regenerating historical releases."
                .format(name, floor, family)
            )


def load_co_release_map() -> dict:
    global _CO_RELEASE_MAP
    if _CO_RELEASE_MAP is not None:
        return _CO_RELEASE_MAP
    mapping = {}
    if CO_RELEASE_MAP_PATH.exists():
        data = json.loads(CO_RELEASE_MAP_PATH.read_text())
        if isinstance(data, dict):
            mapping = {
                line: harfbuzz
                for line, harfbuzz in data.items()
                if line and harfbuzz
            }
        elif isinstance(data, list):
            mapping = {
                entry["skia_line"]: entry["hb_line"]
                for entry in data
                if entry.get("skia_line") and entry.get("hb_line")
            }
        else:
            raise ValueError(
                "co-release-map.json: expected a JSON object; got {}"
                .format(type(data).__name__)
            )
    _CO_RELEASE_MAP = mapping
    return mapping


def harfbuzz_summary_required(harfbuzz: dict | None) -> bool:
    if not harfbuzz:
        return False
    if harfbuzz.get("prs"):
        return True
    previous = harfbuzz.get("previous_version")
    return bool(previous and previous != harfbuzz.get("version"))


def _sha256_bytes(data: bytes) -> str:
    return "sha256:" + hashlib.sha256(data).hexdigest()


def load_notes_sidecar(stem: str, base_dir: Path) -> dict | None:
    notes_path = base_dir / "_sources" / "{}.notes.md".format(stem)
    if not notes_path.is_file():
        return None
    return {
        "path": "_sources/{}.notes.md".format(stem),
        "sha256": _sha256_bytes(notes_path.read_bytes()),
    }


def load_breaking_companions(line: str, base_dir: Path) -> dict | None:
    folder = base_dir / line
    if not folder.is_dir():
        return None
    files = sorted(folder.glob("**/*.breaking.md"))
    if not files:
        return None
    digest = hashlib.sha256()
    paths = []
    for file in files:
        relative = file.relative_to(base_dir).as_posix()
        paths.append(relative)
        digest.update(relative.encode("utf-8"))
        digest.update(b"\0")
        digest.update(file.read_bytes())
        digest.update(b"\0")
    return {"paths": paths, "sha256": "sha256:" + digest.hexdigest()}


_PRERELEASE_STAGE = {"alpha": 0, "beta": 1, "preview": 2, "rc": 3}
_UNKNOWN_STAGE = 8
_STABLE_STAGE = 9


def release_branch_sort_key(branch: str) -> tuple:
    name = removeprefix(branch, "release/")
    core, _, label = name.partition("-")
    parts = core.split(".")
    if len(parts) < 2 or not parts[0].isdigit() or not parts[1].isdigit():
        return (-1, 0, 0, 0, 0, 0)
    major = int(parts[0])
    minor = int(parts[1])
    third = parts[2] if len(parts) > 2 else "0"
    if third == "x":
        return (major, minor, -1, 0, 0, 0)
    if not third.isdigit():
        return (-1, 0, 0, 0, 0, 0)
    patch = int(third)
    subpatch = int(parts[3]) if len(parts) > 3 and parts[3].isdigit() else 0
    if not label:
        return (major, minor, patch, subpatch, _STABLE_STAGE, 0)
    name_match = re.match(r"[A-Za-z]+", label)
    stage_name = name_match.group(0).lower() if name_match else ""
    numbers = re.findall(r"\d+", label)
    prerelease = int(numbers[0]) if numbers else 0
    stage = _PRERELEASE_STAGE.get(stage_name, _UNKNOWN_STAGE)
    return (major, minor, patch, subpatch, stage, prerelease)


def version_from_branch(branch: str) -> str:
    match = re.match(r"release/(.+)$", branch)
    if not match:
        return branch
    version = match.group(1)
    return version if version.endswith(".x") else version.split("-")[0]


def version_is_superseded(
    version: str,
    family: str = "skiasharp",
) -> bool:
    entry = versions_config_lookup(version, family)
    return bool(entry and entry.get("status") == "superseded")


def resolve_superseded_by(
    version: str,
    family: str = "skiasharp",
) -> str | None:
    entry = versions_config_lookup(version, family)
    if entry and entry.get("status") == "superseded":
        return entry.get("superseded_by")
    return None


def detect_supersedes(
    version: str,
    family: str = "skiasharp",
) -> list[str]:
    rolled = [
        entry["version"]
        for entry in load_versions_config(family)
        if entry.get("status") == "superseded"
        and entry.get("superseded_by") == version
        and entry.get("version")
    ]
    rolled.sort(key=version_key)
    return rolled


def sources_dir(page_path) -> Path:
    return Path(str(page_path)).parent / "_sources"


def data_json_path(page_path) -> Path:
    page = Path(str(page_path))
    return sources_dir(page) / (page.stem + ".data.json")


def context_markdown_path(page_path) -> Path:
    page = Path(str(page_path))
    return sources_dir(page) / (page.stem + ".context.md")


def prose_json_path(page_path) -> Path:
    page = Path(str(page_path))
    return sources_dir(page) / (page.stem + ".prose.json")


def get_version_files() -> tuple[list[str], list[str]]:
    versions = []
    next_versions = []
    for file in RELEASES_DIR.iterdir():
        if (
            file.suffix == ".md"
            and file.name != "index.md"
            and not file.name.endswith(".notes.md")
        ):
            stem = file.stem
            if stem.endswith("-unreleased"):
                next_versions.append(stem[:-11])
            else:
                versions.append(stem)
    versions.sort(key=version_key, reverse=True)
    next_versions.sort(key=version_key, reverse=True)
    return versions, next_versions


def cadence_milestones() -> tuple[int, int, int]:
    source = RELEASES_DIR / "_sources"
    keys = []
    if source.is_dir():
        for file in source.iterdir():
            if not file.name.endswith(".data.json"):
                continue
            stem = file.name[:-len(".data.json")]
            if stem.endswith("-unreleased"):
                stem = stem[:-len("-unreleased")]
            key = version_key(stem)
            if len(key) >= 2:
                keys.append(key)
    next_major = max((key[0] for key in keys), default=4)
    milestones = [key[1] for key in keys if key[0] == next_major]
    current = max(milestones) if milestones else 1
    return next_major, current, current + 1


def validate_shipments(data: dict) -> list[str]:
    tags = [shipment.get("tag") for shipment in data.get("shipments") or []]
    duplicates = sorted({
        tag for tag in tags if tag and tags.count(tag) > 1
    })
    return (
        ["duplicate exact shipment tags: " + ", ".join(duplicates)]
        if duplicates else []
    )
