"""The NuGet.org public release receipt and package-family consistency gate.

NuGet.org is the source of truth for what actually shipped. This module never
trusts a branch head or a locally recorded version; it queries the NuGet V3
registration/catalog and flat-container APIs for the exact requested version,
downloads and verifies the small anchor package set, and cross-checks every
required package in the family against the same composed version and (for
the SkiaSharp family) the same embedded source commit.
"""

from __future__ import annotations

import hashlib
import re
import time
import xml.etree.ElementTree as ElementTree
import zipfile
from dataclasses import dataclass
from pathlib import Path
from typing import Callable, Protocol
from urllib import request as urllib_request
from urllib.error import HTTPError, URLError

from release_common import ConflictError, NotReadyError, PlanError, ReleaseToolError
import release_model as model

REGISTRATION_BASE = "https://api.nuget.org/v3/registration5-gz-semver2"
FLAT_CONTAINER_BASE = "https://api.nuget.org/v3-flatcontainer"

_NUSPEC_NS = "{http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd}"
_NUSPEC_NS_VARIANTS = (
    "{http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd}",
    "{http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd}",
    "{http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd}",
    "",
)

# A NuGet dependency version range's minimum bound: a bare version ("1.2.3"),
# an exact-match range ("[1.2.3]"), or a minimum-inclusive range
# ("[1.2.3, )" / "[1.2.3,2.0.0)"). Only the minimum bound is ever inspected.
_VERSION_RANGE_RE = re.compile(
    r"^\s*(?:\[|\()?\s*(?P<min>\d[\w.\-+]*)?\s*(?:,.*)?\s*(?:\]|\))?\s*$"
)


class NuGetError(ReleaseToolError):
    """A NuGet.org catalog/package response was missing, invalid, or inconsistent."""


@dataclass(frozen=True)
class CatalogEntry:
    """The subset of a NuGet V3 catalog (PackageDetails) entry this tool trusts."""

    id: str
    version: str
    listed: bool
    package_hash: str
    package_hash_algorithm: str
    package_size: int
    dependency_groups: tuple[dict, ...]
    repository: dict | None

    @classmethod
    def from_json(cls, payload: dict) -> "CatalogEntry":
        return cls(
            id=payload.get("id", ""),
            version=payload.get("version", ""),
            listed=bool(payload.get("listed", False)),
            package_hash=payload.get("packageHash", ""),
            package_hash_algorithm=payload.get("packageHashAlgorithm", ""),
            package_size=int(payload.get("packageSize", 0)),
            dependency_groups=tuple(payload.get("dependencyGroups") or ()),
            repository=payload.get("repository"),
        )


class NuGetClient(Protocol):
    def get_catalog_entry(self, package_id: str, version: str) -> CatalogEntry | None: ...

    def download_package(self, package_id: str, version: str) -> bytes: ...

    def get_nuspec(self, package_id: str, version: str) -> bytes | None: ...


class HttpNuGetClient:
    """The real NuGet.org client, using only the standard library."""

    def __init__(self, *, timeout: int = 30):
        self.timeout = timeout

    def _get_json(self, url: str) -> dict | None:
        try:
            with urllib_request.urlopen(url, timeout=self.timeout) as response:  # noqa: S310
                import json

                return json.loads(response.read().decode("utf-8"))
        except HTTPError as exc:
            if exc.code == 404:
                return None
            raise NuGetError(f"NuGet request to {url} failed: HTTP {exc.code}") from exc
        except URLError as exc:
            raise NuGetError(f"NuGet request to {url} failed: {exc}") from exc

    def get_catalog_entry(self, package_id: str, version: str) -> CatalogEntry | None:
        index_url = f"{REGISTRATION_BASE}/{package_id.lower()}/index.json"
        index = self._get_json(index_url)
        if index is None:
            return None
        for page in index.get("items", []):
            items = page.get("items")
            if items is None:
                page = self._get_json(page["@id"])
                if page is None:
                    continue
                items = page.get("items", [])
            for item in items:
                entry = item.get("catalogEntry", {})
                if entry.get("version") != version:
                    continue
                if "listed" not in entry and "@id" in entry:
                    entry = self._get_json(entry["@id"]) or entry
                return CatalogEntry.from_json(entry)
        return None

    def download_package(self, package_id: str, version: str) -> bytes:
        lower_id = package_id.lower()
        lower_version = version.lower()
        url = f"{FLAT_CONTAINER_BASE}/{lower_id}/{lower_version}/{lower_id}.{lower_version}.nupkg"
        try:
            with urllib_request.urlopen(url, timeout=self.timeout) as response:  # noqa: S310
                return response.read()
        except HTTPError as exc:
            raise NuGetError(f"could not download {package_id} {version}: HTTP {exc.code}") from exc
        except URLError as exc:
            raise NuGetError(f"could not download {package_id} {version}: {exc}") from exc

    def get_nuspec(self, package_id: str, version: str) -> bytes | None:
        """Fetch the small standalone ``.nuspec`` (not the full ``.nupkg``).

        Used to cross-check identity/repository/dependency metadata for every
        required package in the family without downloading every (often
        large, native) package -- only the small anchor set is downloaded in
        full and hash/signature verified.
        """

        lower_id = package_id.lower()
        lower_version = version.lower()
        url = f"{FLAT_CONTAINER_BASE}/{lower_id}/{lower_version}/{lower_id}.nuspec"
        try:
            with urllib_request.urlopen(url, timeout=self.timeout) as response:  # noqa: S310
                return response.read()
        except HTTPError as exc:
            if exc.code == 404:
                return None
            raise NuGetError(f"could not fetch nuspec for {package_id} {version}: HTTP {exc.code}") from exc
        except URLError as exc:
            raise NuGetError(f"could not fetch nuspec for {package_id} {version}: {exc}") from exc


def wait_for_catalog_entry(
    client: NuGetClient,
    package_id: str,
    version: str,
    *,
    attempts: int = 10,
    delay_seconds: float = 30.0,
    sleep: Callable[[float], None] = time.sleep,
) -> CatalogEntry:
    """Bounded poll for NuGet indexing lag; never silently picks another version."""

    last_entry: CatalogEntry | None = None
    for attempt in range(attempts):
        entry = client.get_catalog_entry(package_id, version)
        if entry is not None and entry.listed:
            return entry
        last_entry = entry
        if attempt < attempts - 1:
            sleep(delay_seconds)
    if last_entry is None:
        raise NotReadyError(
            f"{package_id} {version} is not yet visible on NuGet.org after "
            f"{attempts} attempts; rerun once indexing completes"
        )
    raise NotReadyError(
        f"{package_id} {version} is on NuGet.org but not listed after "
        f"{attempts} attempts; rerun once it is listed"
    )


def verify_catalog_entry(entry: CatalogEntry, *, package_id: str, version: str) -> None:
    if entry.id.lower() != package_id.lower() or entry.version != version:
        raise NuGetError(
            f"catalog entry identity mismatch: requested {package_id} {version}, "
            f"got {entry.id} {entry.version}"
        )
    if not entry.listed:
        raise NuGetError(f"{package_id} {version} is not listed on NuGet.org")
    if entry.package_hash_algorithm != "SHA512" or not entry.package_hash:
        raise NuGetError(
            f"{package_id} {version} catalog entry is missing a SHA512 packageHash"
        )
    if entry.package_size <= 0:
        raise NuGetError(f"{package_id} {version} catalog entry is missing packageSize")
    if not entry.dependency_groups and package_id.lower() != "harfbuzzsharp":
        raise NuGetError(f"{package_id} {version} catalog entry has no dependencyGroups")
    if not entry.repository:
        raise NuGetError(f"{package_id} {version} catalog entry has no repository metadata")


@dataclass(frozen=True)
class NuspecRepository:
    type: str | None
    commit: str | None
    branch: str | None
    url: str | None


@dataclass(frozen=True)
class Nuspec:
    id: str
    version: str
    repository: NuspecRepository
    dependency_groups: tuple[dict, ...]


def _strip_ns(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]


def parse_nuspec(nupkg_bytes: bytes, *, expected_entry: str | None = None) -> Nuspec:
    """Parse the ``.nuspec`` inside a downloaded ``.nupkg`` archive.

    Uses only ``xml.etree.ElementTree``: the input is already hash- and
    signature-verified before this is called, and no external entities or
    DTDs are ever resolved by the standard library's expat backend.
    """

    with zipfile.ZipFile(__import__("io").BytesIO(nupkg_bytes)) as archive:
        nuspec_names = [name for name in archive.namelist() if name.endswith(".nuspec")]
        if expected_entry is not None:
            nuspec_names = [name for name in nuspec_names if name == expected_entry]
        if len(nuspec_names) != 1:
            raise NuGetError(f"expected exactly one .nuspec entry, found {nuspec_names}")
        raw = archive.read(nuspec_names[0])
    return parse_standalone_nuspec(raw)


def parse_standalone_nuspec(raw: bytes) -> Nuspec:
    """Parse a standalone ``.nuspec`` document (the flat-container resource,
    not one embedded in a ``.nupkg``). Same trust model as :func:`parse_nuspec`:
    only used after the identity has been checked against a listed, hashed
    catalog entry.
    """

    root = ElementTree.fromstring(raw)  # noqa: S314 - trusted, hash-verified input
    metadata = None
    for child in root:
        if _strip_ns(child.tag) == "metadata":
            metadata = child
            break
    if metadata is None:
        raise NuGetError("nuspec has no <metadata> element")

    def find(tag: str) -> ElementTree.Element | None:
        for child in metadata:
            if _strip_ns(child.tag) == tag:
                return child
        return None

    id_element = find("id")
    version_element = find("version")
    if id_element is None or version_element is None or not id_element.text or not version_element.text:
        raise NuGetError("nuspec is missing <id> or <version>")

    repo_element = find("repository")
    repository = NuspecRepository(
        type=repo_element.get("type") if repo_element is not None else None,
        commit=repo_element.get("commit") if repo_element is not None else None,
        branch=repo_element.get("branch") if repo_element is not None else None,
        url=repo_element.get("url") if repo_element is not None else None,
    )

    dependency_groups: list[dict] = []
    dependencies_element = find("dependencies")
    if dependencies_element is not None:
        has_groups = any(_strip_ns(c.tag) == "group" for c in dependencies_element)
        if has_groups:
            for group in dependencies_element:
                if _strip_ns(group.tag) != "group":
                    continue
                deps = [
                    {"id": d.get("id"), "version": d.get("version")}
                    for d in group
                    if _strip_ns(d.tag) == "dependency"
                ]
                dependency_groups.append(
                    {"targetFramework": group.get("targetFramework"), "dependencies": deps}
                )
        else:
            deps = [
                {"id": d.get("id"), "version": d.get("version")}
                for d in dependencies_element
                if _strip_ns(d.tag) == "dependency"
            ]
            dependency_groups.append({"targetFramework": None, "dependencies": deps})

    return Nuspec(
        id=id_element.text.strip(),
        version=version_element.text.strip(),
        repository=repository,
        dependency_groups=tuple(dependency_groups),
    )


def sha512_hex(data: bytes) -> str:
    return hashlib.sha512(data).hexdigest()


def verify_anchor_hash(entry: CatalogEntry, nupkg_bytes: bytes, *, package_id: str, version: str) -> None:
    import base64

    computed = hashlib.sha512(nupkg_bytes).digest()
    expected = entry.package_hash
    # NuGet's packageHash is base64-encoded SHA512.
    try:
        expected_bytes = base64.b64decode(expected)
    except Exception as exc:  # noqa: BLE001 - re-raised as a typed, explicit error
        raise NuGetError(f"{package_id} {version} has a malformed packageHash") from exc
    if computed != expected_bytes:
        raise NuGetError(
            f"{package_id} {version} downloaded package hash does not match "
            "the NuGet.org catalog packageHash"
        )


class SignatureVerifier(Protocol):
    def verify(self, nupkg_path: Path, fingerprints: tuple[str, ...]) -> None: ...


@dataclass
class DotNetSignatureVerifier:
    """Runs ``dotnet nuget verify --certificate-fingerprint ... --all``."""

    runner: object  # release_common.CommandRunner, kept loosely typed to avoid import cycle

    def verify(self, nupkg_path: Path, fingerprints: tuple[str, ...]) -> None:
        args = ["dotnet", "nuget", "verify", "--all"]
        for fingerprint in fingerprints:
            args.extend(["--certificate-fingerprint", fingerprint])
        args.append(str(nupkg_path))
        result = self.runner.run(args, cwd=nupkg_path.parent, check=False)
        if not result.ok:
            raise NuGetError(
                f"signature verification failed for {nupkg_path.name}: {result.stdout}{result.stderr}"
            )


def verify_repository_metadata(
    *,
    nuspec: Nuspec,
    catalog_repository: dict,
    package_id: str,
    version: str,
) -> tuple[str, str]:
    """Cross-check nuspec and catalog repository metadata; return (commit, branch)."""

    repo = nuspec.repository
    if repo.type != "git" or catalog_repository.get("type") != "git":
        raise NuGetError(f"{package_id} {version} repository type is not 'git'")
    if not repo.commit or not re.fullmatch(r"[0-9a-fA-F]{40}", repo.commit):
        raise NuGetError(f"{package_id} {version} nuspec repository commit is not a full SHA")
    if repo.commit != catalog_repository.get("commit"):
        raise NuGetError(
            f"{package_id} {version} nuspec and catalog repository commits disagree"
        )
    if not repo.branch or not model.RELEASE_BRANCH_RE.fullmatch(repo.branch):
        raise NuGetError(
            f"{package_id} {version} repository branch {repo.branch!r} does not "
            "match the exact release-branch grammar"
        )
    if repo.branch != catalog_repository.get("branch"):
        raise NuGetError(
            f"{package_id} {version} nuspec and catalog repository branches disagree"
        )
    # Deliberately not trusted: the RepositoryUrl is a Microsoft fwlink, not a
    # verifiable anchor. Binding happens via the resolved commit, not this URL.
    return repo.commit, repo.branch


def _min_version_from_range(range_text: str) -> str:
    match = _VERSION_RANGE_RE.fullmatch(range_text.strip())
    if not match or not match.group("min"):
        raise NuGetError(f"unparseable NuGet dependency version range: {range_text!r}")
    return match.group("min")


def collapse_dependency_minimum_version(dependency_groups: tuple[dict, ...], *, dependency_id: str) -> str:
    """Collapse a dependency's minimum version across every TFM group.

    Requires every group that references ``dependency_id`` to agree on
    exactly one distinct minimum version; raises otherwise.
    """

    minimums: set[str] = set()
    found = False
    for group in dependency_groups:
        for dependency in group.get("dependencies") or []:
            if dependency.get("id") == dependency_id:
                found = True
                minimums.add(_min_version_from_range(dependency.get("version") or ""))
    if not found:
        raise NuGetError(f"no dependency group references {dependency_id}")
    if len(minimums) != 1:
        raise NuGetError(
            f"{dependency_id} minimum version disagrees across target frameworks: "
            f"{sorted(minimums)}"
        )
    return next(iter(minimums))


@dataclass(frozen=True)
class PackageReceipt:
    id: str
    version: str
    source_commit: str
    source_branch: str


@dataclass(frozen=True)
class PublicReceipt:
    skiasharp_version: str
    base: str
    label: str
    build_revision: str | None
    source_commit: str
    source_branch: str
    harfbuzzsharp_version: str
    packages: tuple[PackageReceipt, ...]
    warnings: tuple[str, ...]


def load_manifest(path: Path) -> dict:
    import json

    return json.loads(path.read_text(encoding="utf-8"))


def load_fingerprints(path: Path) -> tuple[str, ...]:
    import json

    data = json.loads(path.read_text(encoding="utf-8"))
    return tuple(cert["fingerprint"] for cert in data["certificates"])


class VersionsFileReader(Protocol):
    """Reads scripts/VERSIONS.txt and scripts/azure-templates-variables.yml
    at an exact commit, without checking out a working tree."""

    def read_file(self, commit: str, path: str) -> str: ...

    def commit_exists(self, commit: str) -> bool: ...

    def branch_contains(self, branch: str, commit: str) -> bool: ...


_SKIASHARP_VERSION_RE = re.compile(r"^\s*SKIASHARP_VERSION:\s*['\"]?([^'\"\s]+)", re.MULTILINE)
_PREVIEW_LABEL_RE = re.compile(r"^\s*PREVIEW_LABEL:\s*['\"]?([^'\"\r\n]+)", re.MULTILINE)
_SKIA_NUGET_RE = re.compile(r"^SkiaSharp\s+nuget\s+(\S+)", re.MULTILINE)
_HARFBUZZ_NUGET_RE = re.compile(r"^HarfBuzzSharp\s+nuget\s+(\S+)", re.MULTILINE)


def read_versions_at_commit(reader: VersionsFileReader, commit: str) -> tuple[str, str, str]:
    """Return (skiasharp_base, preview_label, harfbuzzsharp_base) at ``commit``."""

    variables = reader.read_file(commit, "scripts/azure-templates-variables.yml")
    versions = reader.read_file(commit, "scripts/VERSIONS.txt")
    version_match = _SKIASHARP_VERSION_RE.search(variables)
    label_match = _PREVIEW_LABEL_RE.search(variables)
    skia_match = _SKIA_NUGET_RE.search(versions)
    harfbuzz_match = _HARFBUZZ_NUGET_RE.search(versions)
    if not (version_match and label_match and skia_match and harfbuzz_match):
        raise NuGetError(f"could not read version state at {commit}")
    return skia_match.group(1), label_match.group(1).strip(), harfbuzz_match.group(1)


def _fetch_and_verify_nuspec(nuget: NuGetClient, *, package_id: str, version: str) -> Nuspec:
    raw = nuget.get_nuspec(package_id, version)
    if raw is None:
        raise NuGetError(f"{package_id} {version} has no standalone nuspec on NuGet.org")
    nuspec = parse_standalone_nuspec(raw)
    if nuspec.id != package_id or nuspec.version != version:
        raise NuGetError(
            f"{package_id} nuspec identity {nuspec.id} {nuspec.version} does not "
            f"match the requested {package_id} {version}"
        )
    return nuspec


def verify_public_receipt(
    *,
    nuget: NuGetClient,
    versions_reader: VersionsFileReader,
    requested_version: str,
    manifest: dict,
    download_dir: Path,
    signature_verifier: SignatureVerifier,
    fingerprints: tuple[str, ...],
    sleep: Callable[[float], None] = time.sleep,
) -> PublicReceipt:
    """Verify the full public NuGet.org receipt for ``requested_version``.

    Implements the "Package-family consistency" and "Anchor and source
    commit" sections of the release-automation plan.
    """

    anchors = tuple(manifest["anchorPackages"])
    families = manifest["families"]
    # HarfBuzzSharp's own version is only known after the SkiaSharp anchor's
    # embedded commit is resolved and scripts/VERSIONS.txt is read there, so
    # it cannot be fetched in the same pass as the other two anchors.
    skiasharp_version_anchors = tuple(a for a in anchors if a != "HarfBuzzSharp")

    warnings: list[str] = []

    def _verify_anchor(package_id: str, version: str) -> tuple[CatalogEntry, Nuspec]:
        entry = wait_for_catalog_entry(nuget, package_id, version, sleep=sleep)
        verify_catalog_entry(entry, package_id=package_id, version=version)
        nupkg_bytes = nuget.download_package(package_id, version)
        verify_anchor_hash(entry, nupkg_bytes, package_id=package_id, version=version)
        download_dir.mkdir(parents=True, exist_ok=True)
        nupkg_path = download_dir / f"{package_id}.{version}.nupkg"
        nupkg_path.write_bytes(nupkg_bytes)
        signature_verifier.verify(nupkg_path, fingerprints)
        nuspec = parse_nuspec(nupkg_bytes)
        if nuspec.id != package_id or nuspec.version != version:
            raise NuGetError(
                f"{package_id} nuspec identity {nuspec.id} {nuspec.version} does "
                f"not match the requested {package_id} {version}"
            )
        return entry, nuspec

    anchor_evidence: dict[str, tuple[CatalogEntry, Nuspec]] = {}
    for package_id in skiasharp_version_anchors:
        anchor_evidence[package_id] = _verify_anchor(package_id, requested_version)

    skia_entry, skia_nuspec = anchor_evidence["SkiaSharp"]
    source_commit, source_branch = verify_repository_metadata(
        nuspec=skia_nuspec,
        catalog_repository=skia_entry.repository or {},
        package_id="SkiaSharp",
        version=requested_version,
    )

    if not versions_reader.commit_exists(source_commit):
        raise NuGetError(
            f"the SkiaSharp package's embedded commit {source_commit} does not "
            "exist in mono/SkiaSharp"
        )
    if not versions_reader.branch_contains(source_branch, source_commit):
        raise ConflictError(
            f"the SkiaSharp package's embedded commit {source_commit} is not "
            f"contained by {source_branch}"
        )

    release = model.parse_release_branch(source_branch)
    base, build_revision = release.validate_public_version(requested_version)

    skia_base, preview_label, harfbuzz_base = read_versions_at_commit(versions_reader, source_commit)
    if skia_base != base:
        raise NuGetError(
            f"SKIASHARP_VERSION at {source_commit} is {skia_base}, expected {base}"
        )
    if release.stable:
        if preview_label != "stable" and preview_label != "preview.0":
            # A stable cut's tooling commit may still read PREVIEW_LABEL as the
            # value it had at branch time; only the composed public version
            # (bare, no suffix) is authoritative for a stable release.
            warnings.append(
                f"PREVIEW_LABEL at {source_commit} is {preview_label!r} for a "
                "stable release; the composed public version was still bare"
            )
    else:
        expected_label = release.label
        if preview_label != expected_label:
            raise NuGetError(
                f"PREVIEW_LABEL at {source_commit} is {preview_label!r}, "
                f"expected {expected_label!r}"
            )

    _, hbz_nuspec = anchor_evidence["SkiaSharp.HarfBuzz"]
    collapsed_min = collapse_dependency_minimum_version(
        hbz_nuspec.dependency_groups, dependency_id="HarfBuzzSharp"
    )
    if release.stable:
        expected_harfbuzzsharp_version = harfbuzz_base
    else:
        expected_harfbuzzsharp_version = model.compose_public_version(
            harfbuzz_base, release.label, build_revision or ""
        )
    if collapsed_min != expected_harfbuzzsharp_version:
        raise NuGetError(
            f"SkiaSharp.HarfBuzz depends on HarfBuzzSharp {collapsed_min}, expected "
            f"{expected_harfbuzzsharp_version}"
        )

    if "HarfBuzzSharp" in anchors:
        anchor_evidence["HarfBuzzSharp"] = _verify_anchor("HarfBuzzSharp", expected_harfbuzzsharp_version)

    packages: list[PackageReceipt] = []
    for package_id in families["SkiaSharp"]:
        if package_id in anchor_evidence:
            entry, nuspec = anchor_evidence[package_id]
        else:
            entry = wait_for_catalog_entry(nuget, package_id, requested_version, sleep=sleep)
            verify_catalog_entry(entry, package_id=package_id, version=requested_version)
            nuspec = _fetch_and_verify_nuspec(
                nuget, package_id=package_id, version=requested_version
            )
            nuspec_commit, _ = verify_repository_metadata(
                nuspec=nuspec,
                catalog_repository=entry.repository or {},
                package_id=package_id,
                version=requested_version,
            )
            if nuspec_commit != source_commit:
                raise NuGetError(
                    f"{package_id} {requested_version} embeds commit "
                    f"{nuspec_commit}, expected {source_commit}"
                )
        packages.append(
            PackageReceipt(
                id=package_id,
                version=requested_version,
                source_commit=source_commit,
                source_branch=source_branch,
            )
        )

    for package_id in families["HarfBuzzSharp"]:
        if package_id in anchor_evidence:
            entry, nuspec = anchor_evidence[package_id]
        else:
            entry = wait_for_catalog_entry(nuget, package_id, expected_harfbuzzsharp_version, sleep=sleep)
            verify_catalog_entry(entry, package_id=package_id, version=expected_harfbuzzsharp_version)
            nuspec = _fetch_and_verify_nuspec(
                nuget, package_id=package_id, version=expected_harfbuzzsharp_version
            )
        # HarfBuzzSharp packages may legitimately keep an older embedded
        # commit: an unchanged HarfBuzzSharp version can be reused across
        # SkiaSharp releases, so only its own internal nuspec/catalog
        # agreement is checked, not equality with source_commit.
        package_commit, package_branch = verify_repository_metadata(
            nuspec=nuspec,
            catalog_repository=entry.repository or {},
            package_id=package_id,
            version=expected_harfbuzzsharp_version,
        )
        packages.append(
            PackageReceipt(
                id=package_id,
                version=expected_harfbuzzsharp_version,
                source_commit=package_commit,
                source_branch=package_branch,
            )
        )

    return PublicReceipt(
        skiasharp_version=requested_version,
        base=base,
        label=release.label,
        build_revision=build_revision,
        source_commit=source_commit,
        source_branch=source_branch,
        harfbuzzsharp_version=expected_harfbuzzsharp_version,
        packages=tuple(packages),
        warnings=tuple(warnings),
    )
