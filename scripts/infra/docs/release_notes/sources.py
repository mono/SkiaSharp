"""Git and GitHub-backed fact collection for release notes."""

from __future__ import annotations

import json
from pathlib import Path
import re
import subprocess

from . import common


HARFBUZZ_PATHSPECS = [
    ":(glob)binding/HarfBuzzSharp/**",
    ":(glob)binding/HarfBuzzSharp.NativeAssets.*/**",
    "binding/libHarfBuzzSharp.json",
    "binding/IncludeNativeAssets.HarfBuzzSharp.targets",
    ":(glob)native/*/libHarfBuzzSharp/**",
    ":(glob)tests/Tests/HarfBuzzSharp/**",
]
SKIA_SUBMODULE = Path("externals/skia")
SKIA_REMOTE_URL = "https://github.com/mono/skia.git"
SKIA_PR_PATTERNS = [
    re.compile(
        r"(?:companion|related)\s+(?:skia\s+)?pr[:\s]+"
        r"https?://github\.com/mono/skia/pull/(\d+)",
        re.IGNORECASE,
    ),
    re.compile(r"https?://github\.com/mono/skia/pull/(\d+)"),
    re.compile(r"mono/skia#(\d+)"),
]
_SKIA_SELF_PR_PATTERNS = [
    re.compile(r"^Merge pull request #(\d+)"),
    re.compile(r"\(#(\d+)\)\s*$"),
]
_SKIA_SHA_RE = re.compile(r"^[0-9a-f]{40}$")
_NOREPLY_RE = re.compile(r"^\d+\+(.+)@users\.noreply\.github\.com$")
_BOT_LOGINS = frozenset({
    "github-actions[bot]", "github-actions", "copilot", "dependabot",
})
_GRAPHQL_BATCH = 50
_AUTHOR_CACHE_PATH = (
    common.RELEASES_DIR / "_sources" / "pr-authors.json"
)
_FIXED_ISSUES_CACHE_PATH = (
    common.RELEASES_DIR / "_sources" / "pr-fixed-issues.json"
)
_FIXES_RE = re.compile(
    r"\b(?:close[sd]?|fix(?:e[sd])?|resolve[sd]?)\b\s*:?\s+#(\d+)",
    re.IGNORECASE,
)
PRODUCT_PREFIXES = ("binding/", "source/")
PRODUCT_EXACT = frozenset({"externals/skia"})
MIXED_EXACT = frozenset({"docs", "VERSIONS.txt", "scripts/VERSIONS.txt"})
MIXED_PREFIXES = ("native/", "nuget/")
MIXED_SUFFIXES = (".props", ".targets")
INTERNAL_SUFFIXES = (".sln", ".slnf", ".slnx")
INTERNAL_PREFIXES = (
    ".agents/",
    ".claude/",
    ".config/",
    ".devcontainer/",
    ".github/",
    ".vscode/",
    "benchmarks/",
    "cake/",
    "changelogs/",
    "design/",
    "documentation/",
    "images/",
    "interactive/",
    "samples/",
    "scripts/",
    "tests/",
    "utils/",
    "wiki/",
)
INTERNAL_EXACT = frozenset({
    ".editorconfig",
    ".gitattributes",
    ".gitignore",
    ".gitmodules",
    "AGENTS.md",
    "CLAUDE.md",
    "CODE-OF-CONDUCT.md",
    "CONTRIBUTING.md",
    "External-Dependency-Info.txt",
    "LICENSE.md",
    "LICENSE.txt",
    "README.md",
    "SignList.xml",
    "bootstrapper.ps1",
    "bootstrapper.sh",
    "build.cake",
    "build.ps1",
    "build.sh",
    "cgmanifest.json",
    "es-metadata.yml",
    "externals/.gitignore",
    "externals/depot_tools",
    "global.json",
    "mono.pub",
    "mono.snk",
    "nuget.config",
})


def get_version_from_remote_branch(branch: str) -> str | None:
    try:
        content = common.run([
            "git", "show",
            "origin/{}:scripts/azure-templates-variables.yml".format(branch),
        ])
    except subprocess.CalledProcessError:
        return None
    for line in content.splitlines():
        match = re.match(r"\s*SKIASHARP_VERSION:\s*(\S+)", line)
        if match:
            return match.group(1)
    return None


def version_has_stable_tag(version: str) -> bool:
    tags = common.run(
        ["git", "tag", "-l", "v{}*".format(version)],
        check=False,
    )
    for tag in tags.splitlines():
        tag = tag.strip()
        if not tag or "-preview" in tag or "-rc" in tag:
            continue
        rest = tag[len("v" + version):] if tag.startswith("v" + version) else None
        if rest is not None and (not rest or rest.startswith(".")):
            return True
    return False


def list_remote_release_branches() -> list[str]:
    output = common.run(["git", "branch", "-r"], check=False)
    return [
        line.strip()[len("origin/"):]
        for line in output.splitlines()
        if "->" not in line and line.strip().startswith("origin/release/")
    ]


def _load_cache(path: Path) -> dict:
    try:
        return json.loads(path.read_text())
    except (OSError, ValueError):
        return {}


def _save_cache(path: Path, cache: dict) -> None:
    try:
        ordered = {
            key: cache[key]
            for key in sorted(cache, key=lambda value: int(value))
        }
    except ValueError:
        ordered = cache
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(ordered, indent=2) + "\n")


def _graphql_pr_authors(numbers: list[int]) -> dict:
    owner, name = common.REPO.split("/")
    aliases = "\n".join(
        "p{0}: pullRequest(number: {0}) {{ author {{ login }} }}".format(number)
        for number in numbers
    )
    query = (
        'query {{ repository(owner: "{}", name: "{}") {{\n{}\n}} }}'
        .format(owner, name, aliases)
    )
    try:
        output = common.run(
            ["gh", "api", "graphql", "-f", "query=" + query],
            check=False,
        )
        repository = json.loads(output)["data"]["repository"]
    except (FileNotFoundError, ValueError, KeyError, TypeError):
        return {}
    resolved = {}
    for number in numbers:
        node = repository.get("p{}".format(number))
        if node is not None:
            author = node.get("author")
            resolved[number] = author.get("login") if author else None
    return resolved


def resolve_pr_authors(prs: list[dict]) -> list[dict]:
    pending = [
        pr for pr in prs if not (pr.get("author") or {}).get("login")
    ]
    if not pending:
        return prs
    cache = _load_cache(_AUTHOR_CACHE_PATH)
    query = sorted({
        pr["number"] for pr in pending
        if str(pr["number"]) not in cache
    })
    dirty = False
    for offset in range(0, len(query), _GRAPHQL_BATCH):
        for number, login in _graphql_pr_authors(
            query[offset:offset + _GRAPHQL_BATCH]
        ).items():
            cache[str(number)] = login
            dirty = True
    if dirty:
        _save_cache(_AUTHOR_CACHE_PATH, cache)
    for pr in pending:
        login = cache.get(str(pr["number"]))
        if login:
            pr["author"]["login"] = login
    return prs


def _graphql_pr_fixed_issues(numbers: list[int]) -> dict:
    owner, name = common.REPO.split("/")
    aliases = "\n".join(
        "p{0}: pullRequest(number: {0}) {{ closingIssuesReferences(first: 50) "
        "{{ nodes {{ number }} }} }}".format(number)
        for number in numbers
    )
    query = (
        'query {{ repository(owner: "{}", name: "{}") {{\n{}\n}} }}'
        .format(owner, name, aliases)
    )
    try:
        output = common.run(
            ["gh", "api", "graphql", "-f", "query=" + query],
            check=False,
        )
        repository = json.loads(output)["data"]["repository"]
    except (FileNotFoundError, ValueError, KeyError, TypeError):
        return {}
    resolved = {}
    for number in numbers:
        node = repository.get("p{}".format(number))
        if node is None:
            continue
        refs = (node.get("closingIssuesReferences") or {}).get("nodes") or []
        resolved[number] = sorted({
            ref.get("number")
            for ref in refs
            if isinstance(ref.get("number"), int)
        })
    return resolved


def resolve_fixed_issues(prs: list[dict]) -> list[dict]:
    numbers = sorted({pr["number"] for pr in prs if pr.get("number")})
    if not numbers:
        return prs
    cache = _load_cache(_FIXED_ISSUES_CACHE_PATH)
    query = [number for number in numbers if str(number) not in cache]
    dirty = False
    for offset in range(0, len(query), _GRAPHQL_BATCH):
        for number, issues in _graphql_pr_fixed_issues(
            query[offset:offset + _GRAPHQL_BATCH]
        ).items():
            cache[str(number)] = issues
            dirty = True
    if dirty:
        _save_cache(_FIXED_ISSUES_CACHE_PATH, cache)
    for pr in prs:
        number = pr.get("number")
        if not number:
            continue
        issues = set(cache.get(str(number)) or [])
        issues.update(int(value) for value in _FIXES_RE.findall(pr.get("body") or ""))
        pr["fixes"] = sorted(issues)
    return prs


def _ensure_skia_repo() -> bool:
    gitdir = SKIA_SUBMODULE / ".git"
    if not gitdir.exists():
        SKIA_SUBMODULE.mkdir(parents=True, exist_ok=True)
        common.run(["git", "init", "-q", str(SKIA_SUBMODULE)], check=False)
    if not gitdir.exists():
        return False
    remotes = common.run(
        ["git", "-C", str(SKIA_SUBMODULE), "remote"],
        check=False,
    ).split()
    if "origin" not in remotes:
        common.run([
            "git", "-C", str(SKIA_SUBMODULE), "remote", "add",
            "origin", SKIA_REMOTE_URL,
        ], check=False)
    return True


def resolve_skia_links(prs: list[dict]) -> list[dict]:
    pending = []
    for pr in prs:
        if pr.get("skiaPr") or not pr.get("commit"):
            continue
        revisions = common.run([
            "git", "rev-parse",
            "{}:externals/skia".format(pr["commit"]),
            "{}^:externals/skia".format(pr["commit"]),
        ], check=False).split()
        if len(revisions) == 2 and revisions[0] != revisions[1]:
            if _SKIA_SHA_RE.fullmatch(revisions[0]):
                pending.append((pr, revisions[0]))
    if not pending or not _ensure_skia_repo():
        return prs
    missing = sorted({
        sha for _, sha in pending
        if subprocess.run(
            ["git", "-C", str(SKIA_SUBMODULE), "cat-file", "-e", sha],
            capture_output=True,
        ).returncode != 0
    })
    if missing:
        common.run([
            "git", "-C", str(SKIA_SUBMODULE), "fetch", "-q",
            "--depth=1", "--filter=blob:none", "origin", *missing,
        ], check=False)
    for pr, sha in pending:
        subject = common.run([
            "git", "-C", str(SKIA_SUBMODULE), "log", "-1",
            "--format=%s", sha,
        ], check=False)
        for pattern in _SKIA_SELF_PR_PATTERNS:
            match = pattern.search(subject)
            if match:
                pr["skiaPr"] = int(match.group(1))
                break
    return prs


def _path_category(path: str) -> str:
    if path.endswith(INTERNAL_SUFFIXES):
        return "internal"
    if path.endswith(MIXED_SUFFIXES):
        return "mixed"
    if path in PRODUCT_EXACT or path.startswith(PRODUCT_PREFIXES):
        return "product"
    if path in MIXED_EXACT or path.startswith(MIXED_PREFIXES):
        return "mixed"
    if path in INTERNAL_EXACT or path.startswith(INTERNAL_PREFIXES):
        return "internal"
    raise ValueError(
        "Unclassified release-notes path {!r}. Add it to the explicit product, "
        "mixed, or internal path sets in release_notes/sources.py.".format(path)
    )


def pr_category(files: set[str], title: str = "") -> str:
    del title
    if not files:
        return "internal"
    categories = {_path_category(path) for path in files}
    if "product" in categories:
        return "product"
    if "mixed" in categories:
        return "mixed"
    return "internal"


def is_bot_login(login: str | None) -> bool:
    if not login:
        return False
    normalized = login.casefold()
    return normalized in _BOT_LOGINS or normalized.endswith("[bot]")


def contributor_roster(prs: list[dict]) -> list[tuple[str, list[int]]]:
    by_login = {}
    for pr in prs:
        if pr.get("category") == "internal":
            continue
        login = (pr.get("author") or {}).get("login")
        if not login or login == "mattleibow" or is_bot_login(login):
            continue
        by_login.setdefault(login, []).append(pr.get("number"))
    roster = [
        (login, sorted(number for number in numbers if number))
        for login, numbers in by_login.items()
    ]
    roster.sort(key=lambda entry: (-len(entry[1]), entry[0].casefold()))
    return roster


def _files_by_commit(from_ref: str, to_ref: str) -> dict[str, set[str]]:
    output = common.run([
        "git", "log", "-z", "--no-renames", "--name-only",
        "--format=%x1e%H", "{}..{}".format(from_ref, to_ref),
    ])
    result = {}
    for record in output.split("\x1e"):
        fields = record.strip("\0\n").split("\0")
        if not fields or not fields[0].strip():
            continue
        commit = fields[0].strip()
        result[commit] = {
            path.lstrip("\n")
            for path in fields[1:]
            if path.lstrip("\n")
        }
    return result


def _login_from_email(email: str) -> str | None:
    match = _NOREPLY_RE.fullmatch(email)
    return match.group(1) if match else None


def get_prs_from_diff(
    from_ref: str,
    to_ref: str,
    paths: list[str] | None = None,
) -> list[dict]:
    command = [
        "git", "log", "-z",
        "--format=%H%x00%ae%x00%an%x00%s%x00%b",
        "{}..{}".format(from_ref, to_ref),
    ]
    if paths:
        command.extend(["--", *paths])
    fields = common.run(command).split("\0")
    if fields and fields[-1] == "":
        fields.pop()
    if len(fields) % 5:
        raise RuntimeError("Unexpected NUL-delimited git log shape")
    files_by_commit = _files_by_commit(from_ref, to_ref)
    prs = []
    seen = set()
    for offset in range(0, len(fields), 5):
        commit, email, name, subject, body = (
            field.strip() for field in fields[offset:offset + 5]
        )
        match = re.search(r"\(#(\d+)\)\s*$", subject)
        if not match:
            continue
        number = int(match.group(1))
        if number in seen:
            continue
        seen.add(number)
        title = re.sub(r"\s*\(#\d+\)\s*$", "", subject)
        skia_pr = next(
            (
                int(found.group(1))
                for pattern in SKIA_PR_PATTERNS
                if (found := pattern.search(body))
            ),
            None,
        )
        prs.append({
            "title": title,
            "author": {
                "login": _login_from_email(email),
                "name": name,
                "email": email,
            },
            "url": "https://github.com/{}/pull/{}".format(common.REPO, number),
            "number": number,
            "body": body,
            "commit": commit,
            "skiaPr": skia_pr,
            "category": pr_category(files_by_commit.get(commit, set()), title),
        })
    return prs


def release_date_display(version: str) -> str | None:
    tag = "v{}".format(version)
    output = common.run([
        "git", "for-each-ref", "--format=%(creatordate:short)",
        "refs/tags/{}".format(tag),
    ], check=False).strip()
    if not re.fullmatch(r"\d{4}-\d{2}-\d{2}", output):
        output = common.run(
            ["git", "log", "-1", "--format=%cs", tag],
            check=False,
        ).strip()
    if not re.fullmatch(r"\d{4}-\d{2}-\d{2}", output):
        return None
    year, month, day = (int(part) for part in output.split("-"))
    from datetime import datetime
    return "{} {}, {}".format(
        datetime(year, month, 1).strftime("%B"),
        day,
        year,
    )
