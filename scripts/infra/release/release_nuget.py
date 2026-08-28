"""The NuGet.org public release receipt and package-family consistency gate.

NuGet.org is the source of truth for what actually shipped. This module never
trusts a branch head or a locally recorded version; it queries the NuGet V3
registration/catalog and flat-container APIs for the exact requested version,
downloads and verifies the small anchor package set, and cross-checks every
required package in the family against the same composed version and (for
the SkiaSharp family) the same embedded source commit.
"""

from __future__ import annotations

import gzip
import hashlib
import json
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
    """The real NuGet.org client, using only the standard library.

    ``registration_base``/``flat_container_base`` default to the real
    NuGet.org endpoints but can be overridden (e.g. by tests pointing at a
    local HTTP server) without any monkeypatching of module globals.
    """

    def __init__(
        self,
        *,
        timeout: int = 30,
        registration_base: str = REGISTRATION_BASE,
        flat_container_base: str = FLAT_CONTAINER_BASE,
    ):
        self.timeout = timeout
        self.registration_base = registration_base
        self.flat_container_base = flat_container_base

    def _get_json(self, url: str) -> dict | None:
        try:
            with urllib_request.urlopen(url, timeout=self.timeout) as response:  # noqa: S310
                raw = response.read()
                content_encoding = response.headers.get("Content-Encoding", "")
        except HTTPError as exc:
            if exc.code == 404:
                return None
            raise NuGetError(f"NuGet request to {url} failed: HTTP {exc.code}") from exc
        except URLError as exc:
            raise NuGetError(f"NuGet request to {url} failed: {exc}") from exc

        # NuGet.org's registration5-gz-semver2 resource -- the only
        # SemVer2-capable registration resource it publishes (registration5-
        # semver2, without "-gz-", does not exist; only semver1 does, which
        # omits SemVer2-shaped versions like our "-preview.N.NNNNN" build
        # revisions) -- always compresses its response body and reliably
        # sets this header for it. urllib.request never transparently
        # decompresses a response the way e.g. `requests` does, so this
        # must be handled explicitly here or every registration read
        # silently receives raw gzip bytes instead of JSON. The magic-byte
        # check is a defensive fallback in case a proxy ever strips the
        # header while leaving the body compressed.
        if content_encoding.lower() == "gzip" or raw[:2] == b"\x1f\x8b":
            try:
                raw = gzip.decompress(raw)
            except OSError as exc:
                raise NuGetError(f"NuGet response from {url} is not valid gzip: {exc}") from exc
        try:
            return json.loads(raw.decode("utf-8"))
        except (UnicodeDecodeError, json.JSONDecodeError) as exc:
            raise NuGetError(f"NuGet response from {url} is not valid JSON: {exc}") from exc

    def get_catalog_entry(self, package_id: str, version: str) -> CatalogEntry | None:
        index_url = f"{self.registration_base}/{package_id.lower()}/index.json"
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
                # The registration item's inlined catalogEntry is missing
                # packageHash/packageSize/repository (it carries only
                # listing metadata like "listed", "authors", "tags") --
                # confirmed against nuget.org's real response shape. It
                # must always be dereferenced through its own "@id" to the
                # full catalog leaf; there is no shortcut based on which
                # fields happen to be present inline.
                catalog_id = entry.get("@id")
                if not catalog_id:
                    raise NuGetError(
                        f"{package_id} {version} registration entry has no "
                        "catalogEntry '@id' to dereference"
                    )
                leaf = self._get_json(catalog_id)
                if leaf is None:
                    raise NuGetError(
                        f"{package_id} {version} catalogEntry '@id' {catalog_id} "
                        "could not be resolved"
                    )
                return CatalogEntry.from_json(leaf)
        return None

    def download_package(self, package_id: str, version: str) -> bytes:
        lower_id = package_id.lower()
        lower_version = version.lower()
        url = f"{self.flat_container_base}/{lower_id}/{lower_version}/{lower_id}.{lower_version}.nupkg"
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
        url = f"{self.flat_container_base}/{lower_id}/{lower_version}/{lower_id}.nuspec"
        try:
            with urllib_request.urlopen(url, timeout=self.timeout) as response:  # noqa: S310
                return response.read()
        except HTTPError as exc:
            if exc.code == 404:
                return None
            raise NuGetError(f"could not fetch nuspec for {package_id} {version}: HTTP {exc.code}") from exc
        except URLError as exc:
            raise NuGetError(f"could not fetch nuspec for {package_id} {version}: {exc}") from exc


DEFAULT_POLL_DEADLINE_SECONDS = 1200.0  # 20 minutes
DEFAULT_POLL_INTERVAL_SECONDS = 30.0


def poll_catalog_entries(
    client: NuGetClient,
    requests: list[tuple[str, str]],
    *,
    deadline_at: float,
    poll_interval: float = DEFAULT_POLL_INTERVAL_SECONDS,
    clock: Callable[[], float] = time.monotonic,
    sleep: Callable[[float], None] = time.sleep,
) -> dict[tuple[str, str], CatalogEntry]:
    """Poll a batch of ``(package_id, version)`` pairs against one shared
    wall-clock deadline, never each pair independently re-driving its own
    fixed attempt count.

    Makes one immediate pass over every requested pair; whichever are
    missing or not yet listed are retried together on the next pass -- the
    resolved subset is never re-checked -- until either every pair
    resolves or ``deadline_at`` (an absolute timestamp comparable with
    ``clock()``, shared across every batch in one ``verify_public_receipt``
    call) is reached. This replaces the old per-package "10 attempts x 30s"
    loop, which could serially burn up to ~4.5 minutes *per package* --
    with dozens of packages in a family, that added up to hours of
    sequential waiting even though NuGet almost always indexes an entire
    publish batch together.
    """

    start = clock()
    resolved: dict[tuple[str, str], CatalogEntry] = {}
    pending = list(requests)
    while True:
        still_pending = []
        for package_id, version in pending:
            entry = client.get_catalog_entry(package_id, version)
            if entry is not None and entry.listed:
                resolved[(package_id, version)] = entry
            else:
                still_pending.append((package_id, version))
        pending = still_pending
        if not pending:
            return resolved
        now = clock()
        if now >= deadline_at:
            elapsed = now - start
            deadline_seconds = deadline_at - start
            missing = tuple({"id": package_id, "version": version} for package_id, version in pending)
            names = ", ".join(f"{package_id} {version}" for package_id, version in pending)
            raise NotReadyError(
                f"{len(pending)} package(s) not yet visible/listed on NuGet.org "
                f"after {elapsed:.0f}s (deadline {deadline_seconds:.0f}s): {names}; "
                "rerun once indexing completes",
                missing=missing,
                elapsed_seconds=elapsed,
                deadline_seconds=deadline_seconds,
            )
        sleep(min(poll_interval, max(0.0, deadline_at - now)))


def verify_catalog_entry(entry: CatalogEntry, *, package_id: str, version: str) -> None:
    """Validate the catalog leaf's own trustworthy fields.

    ``entry.repository`` is deliberately never required or checked here:
    real NuGet.org catalog leaves for some already-published versions
    (observed live for old SkiaSharp releases, e.g. 3.119.0) expose it as
    an empty string even though the same package's nuspec has full
    repository metadata, so it is not a reliable source either for
    "is repository metadata present" or for cross-checking against the
    nuspec. Only the nuspec is authoritative for repository metadata --
    see :func:`verify_nuspec_repository`.
    """

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
    """Runs ``<dotnet_command> nuget verify --all --certificate-fingerprint ...``.

    ``dotnet_command`` defaults to a bare ``("dotnet",)`` (relying on PATH),
    but callers running inside this repository should pass the resolved
    Arcade wrapper instead (``eng/common/dotnet.sh``/``dotnet.cmd``), which
    is how this was live-validated against a real published package
    (``./eng/common/dotnet.sh nuget verify --all``), so the pinned SDK/
    runtime is used rather than whatever ``dotnet`` happens to be on PATH.
    """

    runner: object  # release_common.CommandRunner, kept loosely typed to avoid import cycle
    dotnet_command: tuple[str, ...] = ("dotnet",)

    def verify(self, nupkg_path: Path, fingerprints: tuple[str, ...]) -> None:
        if not fingerprints:
            raise NuGetError("no trusted certificate fingerprints were provided")
        # Make the path absolute before building argv: nupkg_path is
        # normally relative (download_dir defaults to a relative path), and
        # passing both a relative argv path *and* cwd=nupkg_path.parent
        # (also relative) doubles the directory once the subprocess actually
        # resolves the argv path against that new cwd (observed live:
        # "Could not find a part of the path '.../finish-downloads/
        # finish-downloads/SkiaSharp.<version>.nupkg'"). Path.absolute() (not
        # .resolve()) is used deliberately: it only prepends the cwd to a
        # relative path and never touches an already-absolute one, so it
        # can't change an existing absolute path's text via symlink
        # resolution (e.g. macOS's /tmp -> /private/tmp).
        resolved_path = nupkg_path.absolute()
        args = [*self.dotnet_command, "nuget", "verify", "--all"]
        for fingerprint in fingerprints:
            args.extend(["--certificate-fingerprint", fingerprint])
        args.append(str(resolved_path))
        # dotnet nuget verify can be slow on a cold SDK (JIT warmup, trust
        # chain building, certificate revocation checks over the network),
        # well past the runner's 120s default -- give it a generous,
        # explicit budget rather than risk a spurious timeout mid-verify.
        result = self.runner.run(args, cwd=resolved_path.parent, check=False, timeout=900)
        if not result.ok:
            raise NuGetError(
                f"signature verification failed for {nupkg_path.name}: {result.stdout}{result.stderr}"
            )


def verify_nuspec_repository(nuspec: Nuspec, *, package_id: str, version: str) -> tuple[str, str]:
    """Validate and extract ``(commit, branch)`` from a package's own nuspec.

    The NuGet.org catalog's own ``repository`` field is never used as a
    cross-check source (see :func:`verify_catalog_entry`): only the nuspec
    -- hash+signature verified for the anchor packages, and independently
    downloaded for every other family package -- is authoritative for
    repository metadata.
    """

    repo = nuspec.repository
    if repo.type != "git":
        raise NuGetError(f"{package_id} {version} nuspec repository type is not 'git'")
    if not repo.commit or not re.fullmatch(r"[0-9a-fA-F]{40}", repo.commit):
        raise NuGetError(f"{package_id} {version} nuspec repository commit is not a full SHA")
    if not repo.branch or not model.RELEASE_BRANCH_RE.fullmatch(repo.branch):
        raise NuGetError(
            f"{package_id} {version} nuspec repository branch {repo.branch!r} does not "
            "match the exact release-branch grammar"
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


_CERTIFICATE_FINGERPRINT_RE = re.compile(r"^[0-9A-F]{64}$")
_CERTIFICATE_DATE_RE = re.compile(r"^\d{4}-\d{2}-\d{2}$")
KNOWN_CERTIFICATE_ROLES = ("author", "repository")


@dataclass(frozen=True)
class CertificateInfo:
    """One reviewed, pinned NuGet package-signing certificate.

    ``role`` distinguishes an author signature (e.g. Microsoft Corporation)
    from a repository signature (NuGet.org): a dual-signed package needs a
    trusted fingerprint of *each* role present, or ``dotnet nuget verify
    --all`` fails on whichever signature has no match. ``valid_until`` is
    purely informational/audit data -- an expired signing certificate does
    not retroactively invalidate packages already signed with it, so
    verification never filters by it at runtime.
    """

    fingerprint: str
    role: str
    subject: str
    description: str
    valid_from: str | None
    valid_until: str | None


def load_certificates(path: Path) -> tuple[CertificateInfo, ...]:
    """Load and structurally validate the reviewed certificate list.

    Designed for additive rotation: any number of certificates per role is
    accepted (a renewal adds a new entry alongside the old one instead of
    replacing it), so this never assumes exactly one or two entries.
    """

    import json

    data = json.loads(path.read_text(encoding="utf-8"))
    entries = data.get("certificates")
    if not isinstance(entries, list) or not entries:
        raise NuGetError(f"{path} has no certificates")

    certificates: list[CertificateInfo] = []
    seen_fingerprints: set[str] = set()
    for entry in entries:
        fingerprint = entry.get("fingerprint")
        if not isinstance(fingerprint, str) or not _CERTIFICATE_FINGERPRINT_RE.fullmatch(fingerprint):
            raise NuGetError(f"{path} has an invalid certificate fingerprint: {fingerprint!r}")
        if fingerprint in seen_fingerprints:
            raise NuGetError(f"{path} has a duplicate certificate fingerprint: {fingerprint!r}")
        seen_fingerprints.add(fingerprint)

        role = entry.get("role")
        if role not in KNOWN_CERTIFICATE_ROLES:
            raise NuGetError(
                f"{path} certificate {fingerprint!r} has an unknown role {role!r}; "
                f"expected one of {KNOWN_CERTIFICATE_ROLES}"
            )

        for date_field in ("validFrom", "validUntil"):
            value = entry.get(date_field)
            if value is not None and not _CERTIFICATE_DATE_RE.fullmatch(value):
                raise NuGetError(
                    f"{path} certificate {fingerprint!r} has an invalid {date_field}: {value!r}"
                )

        certificates.append(
            CertificateInfo(
                fingerprint=fingerprint,
                role=role,
                subject=entry.get("subject") or "",
                description=entry.get("description") or "",
                valid_from=entry.get("validFrom"),
                valid_until=entry.get("validUntil"),
            )
        )
    return tuple(certificates)


def load_fingerprints(path: Path) -> tuple[str, ...]:
    """Every reviewed fingerprint, any role, in file order.

    This is what gets passed to ``dotnet nuget verify --certificate-fingerprint``:
    the command accepts multiple occurrences of the flag and treats the
    signer as trusted if it matches *any* of them, so the full reviewed set
    (every still-relevant role and rotation generation) is always passed
    together rather than picking a single "current" fingerprint.
    """

    return tuple(certificate.fingerprint for certificate in load_certificates(path))


_NUGET_SECTION_HEADER = "# nuget versions"
_FAMILY_HEADER_RE = re.compile(r"^#\s*(SkiaSharp|HarfBuzzSharp)\s*$")
_NUGET_PACKAGE_LINE_RE = re.compile(r"^(\S+)\s+nuget\s+\S+\s*$")


def extract_versions_txt_families(versions_text: str) -> dict[str, list[str]]:
    """Parse the authoritative package inventory out of ``scripts/VERSIONS.txt``.

    ``public-packages.json`` must be validated exactly against this: the
    ``# nuget versions`` section's ``# SkiaSharp`` / ``# HarfBuzzSharp``
    sub-headers and the ``<id> nuget <version>`` lines beneath each, not a
    bare ``<PackageId>`` inference from project files or an ad hoc prefix
    match. A package's declared family comes from which sub-header it falls
    under in the file, so renaming/relabeling a family in ``VERSIONS.txt``
    is authoritative over the package's own name.
    """

    lines = versions_text.splitlines()
    try:
        start = next(i for i, line in enumerate(lines) if line.strip() == _NUGET_SECTION_HEADER)
    except StopIteration:
        raise NuGetError(
            f"scripts/VERSIONS.txt has no {_NUGET_SECTION_HEADER!r} section"
        ) from None

    families: dict[str, list[str]] = {}
    current_family: str | None = None
    for line in lines[start + 1:]:
        header_match = _FAMILY_HEADER_RE.fullmatch(line.strip())
        if header_match:
            current_family = header_match.group(1)
            families.setdefault(current_family, [])
            continue
        package_match = _NUGET_PACKAGE_LINE_RE.fullmatch(line)
        if package_match is None:
            continue
        if current_family is None:
            raise NuGetError(
                f"scripts/VERSIONS.txt has a nuget package line before any "
                f"'# SkiaSharp'/'# HarfBuzzSharp' header: {line!r}"
            )
        families[current_family].append(package_match.group(1))
    if not families:
        raise NuGetError(
            "scripts/VERSIONS.txt's nuget versions section has no "
            "'# SkiaSharp'/'# HarfBuzzSharp' package families"
        )
    return families


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

VARIABLES_PATH = "scripts/azure-templates-variables.yml"
VERSIONS_PATH = "scripts/VERSIONS.txt"


def read_versions_at_commit(reader: VersionsFileReader, commit: str) -> tuple[str, str, str]:
    """Return (skiasharp_base, preview_label, harfbuzzsharp_base) at ``commit``."""

    variables = reader.read_file(commit, VARIABLES_PATH)
    versions = reader.read_file(commit, VERSIONS_PATH)
    version_match = _SKIASHARP_VERSION_RE.search(variables)
    label_match = _PREVIEW_LABEL_RE.search(variables)
    skia_match = _SKIA_NUGET_RE.search(versions)
    harfbuzz_match = _HARFBUZZ_NUGET_RE.search(versions)
    if not (version_match and label_match and skia_match and harfbuzz_match):
        raise NuGetError(f"could not read version state at {commit}")
    return skia_match.group(1), label_match.group(1).strip(), harfbuzz_match.group(1)


def read_family_ids_at_commit(reader: VersionsFileReader, commit: str) -> dict[str, list[str]]:
    """Return the authoritative SkiaSharp/HarfBuzzSharp package-family ID
    lists exactly as ``scripts/VERSIONS.txt`` declared them at ``commit``.

    A historical release must never be checked against the *current*
    ``public-packages.json``/``VERSIONS.txt`` family lists: a package added
    to a family after that release shipped (e.g. ``SkiaSharp.Vulkan.
    Silk.NET``, added well after SkiaSharp 4.151.1) never existed on
    NuGet.org for that old version, so polling for it would eventually
    raise :class:`NotReadyError` even though the release is perfectly
    valid. Only the commit-exact ``VERSIONS.txt`` is authoritative for
    which packages an already-published release actually requires; the
    manifest's ``anchorPackages`` (which three packages are always
    downloaded/hash/signature-verified) is separate, current-tooling
    policy that does not change per release and is not derived here.
    """

    return extract_versions_txt_families(reader.read_file(commit, VERSIONS_PATH))


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
    deadline_seconds: float = DEFAULT_POLL_DEADLINE_SECONDS,
    poll_interval: float = DEFAULT_POLL_INTERVAL_SECONDS,
    clock: Callable[[], float] = time.monotonic,
    sleep: Callable[[float], None] = time.sleep,
) -> PublicReceipt:
    """Verify the full public NuGet.org receipt for ``requested_version``.

    Implements the "Package-family consistency" and "Anchor and source
    commit" sections of the release-automation plan.

    Every catalog-indexing wait in this function shares one wall-clock
    deadline (``deadline_seconds`` from the call's start, 20 minutes by
    default): the SkiaSharp/SkiaSharp.HarfBuzz bootstrap poll and the
    single batched poll for every remaining package both count against it,
    rather than each package independently retrying its own fixed attempt
    count -- see :func:`poll_catalog_entries`.
    """

    deadline_at = clock() + deadline_seconds

    anchors = tuple(manifest["anchorPackages"])
    # NOTE: manifest["families"] (public-packages.json's *current* family
    # lists) is deliberately never used to decide which packages a
    # historical release requires -- see read_family_ids_at_commit below,
    # which derives that from the exact VERSIONS.txt at the resolved
    # source commit instead. anchorPackages is current tooling policy
    # (which three packages are always fully downloaded/hash/signature-
    # verified) and does not change per release, so it is used as-is.
    # HarfBuzzSharp's own version is only known after the SkiaSharp anchor's
    # embedded commit is resolved and scripts/VERSIONS.txt is read there, so
    # it cannot be fetched in the same pass as the other two anchors.
    skiasharp_version_anchors = tuple(a for a in anchors if a != "HarfBuzzSharp")

    warnings: list[str] = []

    def _finish_anchor_verification(
        package_id: str, version: str, entry: CatalogEntry
    ) -> tuple[CatalogEntry, Nuspec]:
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

    # Bootstrap: SkiaSharp and SkiaSharp.HarfBuzz are both requested at the
    # same version and are both required before anything else can be
    # resolved (the source commit and package families come from their
    # nuspecs), so poll for both together in one shared pass rather than
    # sequentially.
    bootstrap_entries = poll_catalog_entries(
        nuget, [(package_id, requested_version) for package_id in skiasharp_version_anchors],
        deadline_at=deadline_at, poll_interval=poll_interval, clock=clock, sleep=sleep,
    )
    anchor_evidence: dict[str, tuple[CatalogEntry, Nuspec]] = {}
    for package_id in skiasharp_version_anchors:
        anchor_evidence[package_id] = _finish_anchor_verification(
            package_id, requested_version, bootstrap_entries[(package_id, requested_version)]
        )

    _, skia_nuspec = anchor_evidence["SkiaSharp"]
    source_commit, source_branch = verify_nuspec_repository(
        skia_nuspec, package_id="SkiaSharp", version=requested_version
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
    # The exact package families this release requires -- as VERSIONS.txt
    # declared them at the *source* commit, not the current tree. A package
    # added to a family after this release shipped never existed on
    # NuGet.org for this version and must never be polled for.
    families = read_family_ids_at_commit(versions_reader, source_commit)
    for required_family in ("SkiaSharp", "HarfBuzzSharp"):
        if required_family not in families:
            raise NuGetError(
                f"scripts/VERSIONS.txt at {source_commit} has no {required_family!r} "
                "package family section"
            )
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

    # One shared pass over every remaining expected catalog entry: the rest
    # of the SkiaSharp family, the HarfBuzzSharp anchor (if configured as
    # one), and the entire HarfBuzzSharp family -- all polled together
    # against the *same* deadline established at the top of this function,
    # not a fresh budget per package.
    remaining_requests: dict[tuple[str, str], None] = {}
    for package_id in families["SkiaSharp"]:
        if package_id not in anchor_evidence:
            remaining_requests[(package_id, requested_version)] = None
    if "HarfBuzzSharp" in anchors:
        remaining_requests[("HarfBuzzSharp", expected_harfbuzzsharp_version)] = None
    for package_id in families["HarfBuzzSharp"]:
        if package_id not in anchor_evidence:
            remaining_requests[(package_id, expected_harfbuzzsharp_version)] = None
    remaining_entries = poll_catalog_entries(
        nuget, list(remaining_requests),
        deadline_at=deadline_at, poll_interval=poll_interval, clock=clock, sleep=sleep,
    )

    if "HarfBuzzSharp" in anchors:
        anchor_evidence["HarfBuzzSharp"] = _finish_anchor_verification(
            "HarfBuzzSharp", expected_harfbuzzsharp_version,
            remaining_entries[("HarfBuzzSharp", expected_harfbuzzsharp_version)],
        )

    packages: list[PackageReceipt] = []
    for package_id in families["SkiaSharp"]:
        if package_id in anchor_evidence:
            _, nuspec = anchor_evidence[package_id]
        else:
            entry = remaining_entries[(package_id, requested_version)]
            verify_catalog_entry(entry, package_id=package_id, version=requested_version)
            nuspec = _fetch_and_verify_nuspec(
                nuget, package_id=package_id, version=requested_version
            )
        # Every SkiaSharp-family package -- anchor or not -- must embed the
        # same source commit as the SkiaSharp anchor itself. This used to be
        # skipped for anchors (SkiaSharp itself trivially matches since
        # source_commit was derived from its own nuspec, but SkiaSharp.
        # HarfBuzz is also an anchor and its own nuspec was never
        # independently checked here).
        nuspec_commit, _ = verify_nuspec_repository(
            nuspec, package_id=package_id, version=requested_version
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
            _, nuspec = anchor_evidence[package_id]
        else:
            entry = remaining_entries[(package_id, expected_harfbuzzsharp_version)]
            verify_catalog_entry(entry, package_id=package_id, version=expected_harfbuzzsharp_version)
            nuspec = _fetch_and_verify_nuspec(
                nuget, package_id=package_id, version=expected_harfbuzzsharp_version
            )
        # HarfBuzzSharp packages may legitimately keep an older embedded
        # commit: an unchanged HarfBuzzSharp version can be reused across
        # SkiaSharp releases, so only its own nuspec is validated here, not
        # equality with source_commit.
        package_commit, package_branch = verify_nuspec_repository(
            nuspec, package_id=package_id, version=expected_harfbuzzsharp_version
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
