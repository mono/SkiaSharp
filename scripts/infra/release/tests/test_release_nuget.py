from __future__ import annotations

import gzip
import http.server
import io
import json
import sys
import threading
import unittest
import zipfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release_nuget as nuget
from release_common import CommandResult, ConflictError, NotReadyError


REPO_ROOT = Path(__file__).resolve().parents[4]


def build_nuspec_xml(
    package_id: str,
    version: str,
    *,
    commit: str | None,
    branch: str | None,
    dependency_groups: list[tuple[str, list[tuple[str, str]]]] | None = None,
) -> str:
    repository = ""
    if commit is not None:
        repository = f'<repository type="git" commit="{commit}" branch="{branch}" url="https://aka.ms/skiasharp-repo" />'
    dependencies = ""
    if dependency_groups:
        groups = []
        for tfm, deps in dependency_groups:
            dep_xml = "".join(
                f'<dependency id="{dep_id}" version="{dep_version}" />'
                for dep_id, dep_version in deps
            )
            groups.append(f'<group targetFramework="{tfm}">{dep_xml}</group>')
        dependencies = f"<dependencies>{''.join(groups)}</dependencies>"
    return (
        '<?xml version="1.0" encoding="utf-8"?>'
        '<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">'
        "<metadata>"
        f"<id>{package_id}</id><version>{version}</version>"
        f"{repository}{dependencies}"
        "</metadata></package>"
    )


def build_nupkg(
    package_id: str,
    version: str,
    *,
    commit: str | None = "a" * 40,
    branch: str | None = "release/3.119.0-preview.1",
    dependency_groups: list[tuple[str, list[tuple[str, str]]]] | None = None,
    payload: bytes = b"binary-content",
) -> bytes:
    buffer = io.BytesIO()
    with zipfile.ZipFile(buffer, "w") as archive:
        archive.writestr(
            f"{package_id}.nuspec",
            build_nuspec_xml(
                package_id, version, commit=commit, branch=branch, dependency_groups=dependency_groups
            ),
        )
        archive.writestr("lib/net8.0/dummy.dll", payload)
    return buffer.getvalue()


def catalog_entry_for(nupkg_bytes: bytes, *, package_id: str, version: str, listed: bool = True, repository: dict | None = None, dependency_groups: list | None = None) -> nuget.CatalogEntry:
    computed_hash = __import__("hashlib").sha512(nupkg_bytes).digest()
    import base64

    if repository is None:
        # Derive the default from the nupkg's own embedded nuspec so the
        # catalog entry's repository field is realistic by default; pass an
        # explicit ``repository`` (including ``""``, matching real
        # NuGet.org catalog leaves for some already-published versions) to
        # simulate the catalog's repository field being absent/unreliable,
        # since it is never cross-checked against the nuspec any more.
        embedded = nuget.parse_nuspec(nupkg_bytes)
        repository = {
            "type": embedded.repository.type,
            "commit": embedded.repository.commit,
            "branch": embedded.repository.branch,
        }
    return nuget.CatalogEntry(
        id=package_id,
        version=version,
        listed=listed,
        package_hash=base64.b64encode(computed_hash).decode("ascii"),
        package_hash_algorithm="SHA512",
        package_size=len(nupkg_bytes),
        dependency_groups=tuple(dependency_groups or [{"targetFramework": "net8.0", "dependencies": []}]),
        repository=repository,
    )


class FakeNuGetClient:
    def __init__(self):
        self.entries: dict[tuple[str, str], nuget.CatalogEntry] = {}
        self.packages: dict[tuple[str, str], bytes] = {}

    def add(self, package_id: str, version: str, nupkg_bytes: bytes, *, entry: nuget.CatalogEntry | None = None):
        self.packages[(package_id, version)] = nupkg_bytes
        self.entries[(package_id, version)] = entry or catalog_entry_for(
            nupkg_bytes, package_id=package_id, version=version
        )

    def get_catalog_entry(self, package_id: str, version: str):
        return self.entries.get((package_id, version))

    def download_package(self, package_id: str, version: str) -> bytes:
        return self.packages[(package_id, version)]

    def get_nuspec(self, package_id: str, version: str) -> bytes | None:
        nupkg = self.packages.get((package_id, version))
        if nupkg is None:
            return None
        with zipfile.ZipFile(io.BytesIO(nupkg)) as archive:
            names = [name for name in archive.namelist() if name.endswith(".nuspec")]
            return archive.read(names[0])


class FakeVersionsFileReader:
    def __init__(self):
        self.files: dict[str, dict[str, str]] = {}
        self.existing_commits: set[str] = set()
        self.branch_membership: dict[str, set[str]] = {}

    def seed(self, commit: str, *, skiasharp_base: str, preview_label: str, harfbuzz_base: str, branch: str):
        self.files[commit] = {
            "scripts/azure-templates-variables.yml": (
                "variables:\n"
                f"  SKIASHARP_VERSION: {skiasharp_base}\n"
                f"  PREVIEW_LABEL: '{preview_label}'\n"
            ),
            "scripts/VERSIONS.txt": (
                f"SkiaSharp                nuget       {skiasharp_base}\n"
                f"HarfBuzzSharp            nuget       {harfbuzz_base}\n"
            ),
        }
        self.existing_commits.add(commit)
        self.branch_membership.setdefault(branch, set()).add(commit)

    def read_file(self, commit: str, path: str) -> str:
        return self.files[commit][path]

    def commit_exists(self, commit: str) -> bool:
        return commit in self.existing_commits

    def branch_contains(self, branch: str, commit: str) -> bool:
        return commit in self.branch_membership.get(branch, set())


class FakeSignatureVerifier:
    def __init__(self, *, should_fail: bool = False):
        self.should_fail = should_fail
        self.calls: list[Path] = []

    def verify(self, nupkg_path: Path, fingerprints: tuple[str, ...]) -> None:
        self.calls.append(nupkg_path)
        if self.should_fail:
            raise nuget.NuGetError("signature verification failed")


class CatalogEntryValidationTests(unittest.TestCase):
    def test_valid_entry_passes(self):
        nupkg = build_nupkg("SkiaSharp", "3.119.0")
        entry = catalog_entry_for(nupkg, package_id="SkiaSharp", version="3.119.0")
        nuget.verify_catalog_entry(entry, package_id="SkiaSharp", version="3.119.0")

    def test_unlisted_is_rejected(self):
        nupkg = build_nupkg("SkiaSharp", "3.119.0")
        entry = catalog_entry_for(nupkg, package_id="SkiaSharp", version="3.119.0", listed=False)
        with self.assertRaisesRegex(nuget.NuGetError, "not listed"):
            nuget.verify_catalog_entry(entry, package_id="SkiaSharp", version="3.119.0")

    def test_missing_hash_algorithm_is_rejected(self):
        entry = nuget.CatalogEntry(
            id="SkiaSharp", version="3.119.0", listed=True, package_hash="abc",
            package_hash_algorithm="SHA256", package_size=10,
            dependency_groups=({"targetFramework": "net8.0", "dependencies": []},),
            repository={"type": "git", "commit": "a" * 40, "branch": "release/3.119.0"},
        )
        with self.assertRaisesRegex(nuget.NuGetError, "SHA512"):
            nuget.verify_catalog_entry(entry, package_id="SkiaSharp", version="3.119.0")

    def test_identity_mismatch_is_rejected(self):
        nupkg = build_nupkg("SkiaSharp", "3.119.0")
        entry = catalog_entry_for(nupkg, package_id="SkiaSharp", version="3.119.0")
        with self.assertRaisesRegex(nuget.NuGetError, "identity mismatch"):
            nuget.verify_catalog_entry(entry, package_id="SkiaSharp", version="3.119.1")

    def test_missing_dependency_groups_rejected_except_harfbuzzsharp(self):
        entry = nuget.CatalogEntry(
            id="SkiaSharp", version="3.119.0", listed=True, package_hash="abc",
            package_hash_algorithm="SHA512", package_size=10, dependency_groups=(),
            repository={"type": "git", "commit": "a" * 40, "branch": "release/3.119.0"},
        )
        with self.assertRaisesRegex(nuget.NuGetError, "dependencyGroups"):
            nuget.verify_catalog_entry(entry, package_id="SkiaSharp", version="3.119.0")

        harfbuzz_entry = nuget.CatalogEntry(
            id="HarfBuzzSharp", version="1.8.8.1", listed=True, package_hash="abc",
            package_hash_algorithm="SHA512", package_size=10, dependency_groups=(),
            repository={"type": "git", "commit": "a" * 40, "branch": "release/3.119.0"},
        )
        nuget.verify_catalog_entry(harfbuzz_entry, package_id="HarfBuzzSharp", version="1.8.8.1")

    def test_empty_string_repository_is_accepted(self):
        # Real NuGet.org catalog leaves for some already-published versions
        # (observed live for old SkiaSharp releases, e.g. 3.119.0) expose
        # "repository" as an empty string even though the same package's
        # nuspec has full repository metadata -- the catalog's repository
        # field must never be required here; only the nuspec is
        # authoritative for that (see verify_nuspec_repository).
        entry = nuget.CatalogEntry(
            id="SkiaSharp", version="3.119.0", listed=True, package_hash="abc",
            package_hash_algorithm="SHA512", package_size=10,
            dependency_groups=({"targetFramework": "net8.0", "dependencies": []},),
            repository="",
        )
        nuget.verify_catalog_entry(entry, package_id="SkiaSharp", version="3.119.0")

    def test_missing_repository_entirely_is_accepted(self):
        entry = nuget.CatalogEntry(
            id="SkiaSharp", version="3.119.0", listed=True, package_hash="abc",
            package_hash_algorithm="SHA512", package_size=10,
            dependency_groups=({"targetFramework": "net8.0", "dependencies": []},),
            repository=None,
        )
        nuget.verify_catalog_entry(entry, package_id="SkiaSharp", version="3.119.0")


class WaitForCatalogEntryTests(unittest.TestCase):
    def test_returns_immediately_when_already_listed(self):
        client = FakeNuGetClient()
        nupkg = build_nupkg("SkiaSharp", "3.119.0")
        client.add("SkiaSharp", "3.119.0", nupkg)
        sleeps = []
        entry = nuget.wait_for_catalog_entry(
            client, "SkiaSharp", "3.119.0", attempts=3, delay_seconds=1, sleep=sleeps.append
        )
        self.assertEqual(entry.version, "3.119.0")
        self.assertEqual(sleeps, [])

    def test_polls_until_indexed_then_succeeds(self):
        client = FakeNuGetClient()
        sleeps = []
        attempts_before_ready = 2

        class DelayedClient:
            def __init__(self):
                self.calls = 0

            def get_catalog_entry(self, package_id, version):
                self.calls += 1
                if self.calls <= attempts_before_ready:
                    return None
                nupkg = build_nupkg(package_id, version)
                return catalog_entry_for(nupkg, package_id=package_id, version=version)

        delayed = DelayedClient()
        entry = nuget.wait_for_catalog_entry(
            delayed, "SkiaSharp", "3.119.0", attempts=5, delay_seconds=1, sleep=sleeps.append
        )
        self.assertIsNotNone(entry)
        self.assertEqual(len(sleeps), attempts_before_ready)

    def test_raises_not_ready_after_bounded_attempts(self):
        client = FakeNuGetClient()
        sleeps = []
        with self.assertRaises(NotReadyError):
            nuget.wait_for_catalog_entry(
                client, "SkiaSharp", "3.119.0", attempts=3, delay_seconds=1, sleep=sleeps.append
            )
        self.assertEqual(len(sleeps), 2)  # never sleeps after the last attempt


class NuspecParsingTests(unittest.TestCase):
    def test_round_trips_identity_and_repository(self):
        nupkg = build_nupkg(
            "SkiaSharp", "3.119.0", commit="c" * 40, branch="release/3.119.0"
        )
        parsed = nuget.parse_nuspec(nupkg)
        self.assertEqual(parsed.id, "SkiaSharp")
        self.assertEqual(parsed.version, "3.119.0")
        self.assertEqual(parsed.repository.type, "git")
        self.assertEqual(parsed.repository.commit, "c" * 40)
        self.assertEqual(parsed.repository.branch, "release/3.119.0")

    def test_parses_multiple_dependency_groups(self):
        nupkg = build_nupkg(
            "SkiaSharp.HarfBuzz",
            "3.119.0",
            dependency_groups=[
                ("net8.0", [("HarfBuzzSharp", "1.8.8.1")]),
                ("net9.0", [("HarfBuzzSharp", "[1.8.8.1, )")]),
            ],
        )
        parsed = nuget.parse_nuspec(nupkg)
        self.assertEqual(len(parsed.dependency_groups), 2)


class CollapseDependencyMinimumVersionTests(unittest.TestCase):
    def test_collapses_repeated_identical_versions(self):
        groups = (
            {"targetFramework": "net8.0", "dependencies": [{"id": "HarfBuzzSharp", "version": "1.8.8.1"}]},
            {"targetFramework": "net9.0", "dependencies": [{"id": "HarfBuzzSharp", "version": "[1.8.8.1, )"}]},
        )
        self.assertEqual(
            nuget.collapse_dependency_minimum_version(groups, dependency_id="HarfBuzzSharp"), "1.8.8.1"
        )

    def test_rejects_disagreeing_versions_across_groups(self):
        groups = (
            {"targetFramework": "net8.0", "dependencies": [{"id": "HarfBuzzSharp", "version": "1.8.8.1"}]},
            {"targetFramework": "net9.0", "dependencies": [{"id": "HarfBuzzSharp", "version": "1.8.8.2"}]},
        )
        with self.assertRaisesRegex(nuget.NuGetError, "disagrees"):
            nuget.collapse_dependency_minimum_version(groups, dependency_id="HarfBuzzSharp")

    def test_rejects_missing_dependency(self):
        groups = ({"targetFramework": "net8.0", "dependencies": [{"id": "Other", "version": "1.0.0"}]},)
        with self.assertRaisesRegex(nuget.NuGetError, "no dependency group"):
            nuget.collapse_dependency_minimum_version(groups, dependency_id="HarfBuzzSharp")

    def test_parses_exact_range(self):
        groups = ({"targetFramework": "net8.0", "dependencies": [{"id": "X", "version": "[1.2.3]"}]},)
        self.assertEqual(nuget.collapse_dependency_minimum_version(groups, dependency_id="X"), "1.2.3")


class NuspecRepositoryVerificationTests(unittest.TestCase):
    """verify_nuspec_repository is nuspec-only: the NuGet.org catalog's own
    "repository" field is never consulted or cross-checked (see item 3 in
    the follow-up review -- real catalog leaves for some already-published
    versions expose it as an empty string even when the nuspec has full
    metadata, so it cannot be a reliable cross-check source)."""

    def test_valid_metadata_is_accepted(self):
        commit = "d" * 40
        nupkg = build_nupkg("SkiaSharp", "3.119.0", commit=commit, branch="release/3.119.0-preview.1")
        nuspec = nuget.parse_nuspec(nupkg)
        result_commit, result_branch = nuget.verify_nuspec_repository(
            nuspec, package_id="SkiaSharp", version="3.119.0"
        )
        self.assertEqual(result_commit, commit)
        self.assertEqual(result_branch, "release/3.119.0-preview.1")

    def test_rejects_non_git_repository_type(self):
        raw = (
            '<?xml version="1.0" encoding="utf-8"?>'
            '<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">'
            "<metadata><id>SkiaSharp</id><version>3.119.0</version>"
            f'<repository type="svn" commit="{"a" * 40}" branch="release/3.119.0" '
            'url="https://aka.ms/skiasharp-repo" />'
            "</metadata></package>"
        )
        nuspec = nuget.parse_standalone_nuspec(raw.encode("utf-8"))
        with self.assertRaisesRegex(nuget.NuGetError, "not 'git'"):
            nuget.verify_nuspec_repository(nuspec, package_id="SkiaSharp", version="3.119.0")

    def test_rejects_short_commit(self):
        nupkg = build_nupkg("SkiaSharp", "3.119.0", commit="abc123", branch="release/3.119.0")
        nuspec = nuget.parse_nuspec(nupkg)
        with self.assertRaisesRegex(nuget.NuGetError, "full SHA"):
            nuget.verify_nuspec_repository(nuspec, package_id="SkiaSharp", version="3.119.0")

    def test_rejects_invalid_branch_grammar(self):
        nupkg = build_nupkg("SkiaSharp", "3.119.0", commit="a" * 40, branch="main")
        nuspec = nuget.parse_nuspec(nupkg)
        with self.assertRaisesRegex(nuget.NuGetError, "grammar"):
            nuget.verify_nuspec_repository(nuspec, package_id="SkiaSharp", version="3.119.0")

    def test_does_not_reference_the_catalog_at_all(self):
        # No catalog_repository parameter exists any more -- this is a
        # signature/behavior check as much as a value check: passing only
        # the nuspec must be enough to fully validate and extract identity.
        import inspect

        signature = inspect.signature(nuget.verify_nuspec_repository)
        self.assertNotIn("catalog_repository", signature.parameters)
        self.assertNotIn("catalog", signature.parameters)


class VerifyAnchorHashTests(unittest.TestCase):
    def test_matching_hash_passes(self):
        nupkg = build_nupkg("SkiaSharp", "3.119.0")
        entry = catalog_entry_for(nupkg, package_id="SkiaSharp", version="3.119.0")
        nuget.verify_anchor_hash(entry, nupkg, package_id="SkiaSharp", version="3.119.0")

    def test_tampered_bytes_are_rejected(self):
        nupkg = build_nupkg("SkiaSharp", "3.119.0")
        entry = catalog_entry_for(nupkg, package_id="SkiaSharp", version="3.119.0")
        tampered = nupkg + b"tampered"
        with self.assertRaisesRegex(nuget.NuGetError, "does not match"):
            nuget.verify_anchor_hash(entry, tampered, package_id="SkiaSharp", version="3.119.0")


MANIFEST = {
    "families": {
        "SkiaSharp": ["SkiaSharp", "SkiaSharp.HarfBuzz", "SkiaSharp.Extra"],
        "HarfBuzzSharp": ["HarfBuzzSharp", "HarfBuzzSharp.NativeAssets.Android"],
    },
    "anchorPackages": ["SkiaSharp", "SkiaSharp.HarfBuzz", "HarfBuzzSharp"],
}


class VerifyPublicReceiptTests(unittest.TestCase):
    def _seed_family(
        self,
        client: FakeNuGetClient,
        *,
        skiasharp_version: str,
        skia_commit: str,
        branch: str,
        harfbuzzsharp_version: str,
        harfbuzzsharp_commit: str | None = None,
        harfbuzz_min_versions: dict[str, str] | None = None,
        extra_skiasharp_commit: str | None = None,
        harfbuzz_wrapper_commit: str | None = None,
        catalog_repository_override: object = "__unset__",
    ):
        harfbuzzsharp_commit = harfbuzzsharp_commit or skia_commit
        harfbuzz_min_versions = harfbuzz_min_versions or {"net8.0": harfbuzzsharp_version, "net9.0": harfbuzzsharp_version}

        def _catalog_entry(nupkg_bytes, *, package_id, version):
            if catalog_repository_override == "__unset__":
                return None
            return catalog_entry_for(
                nupkg_bytes, package_id=package_id, version=version,
                repository=catalog_repository_override,
            )

        skiasharp_nupkg = build_nupkg("SkiaSharp", skiasharp_version, commit=skia_commit, branch=branch)
        client.add(
            "SkiaSharp", skiasharp_version, skiasharp_nupkg,
            entry=_catalog_entry(skiasharp_nupkg, package_id="SkiaSharp", version=skiasharp_version),
        )

        hbz_dep_groups = [(tfm, [("HarfBuzzSharp", version)]) for tfm, version in harfbuzz_min_versions.items()]
        harfbuzz_wrapper_nupkg = build_nupkg(
            "SkiaSharp.HarfBuzz", skiasharp_version,
            commit=harfbuzz_wrapper_commit or skia_commit, branch=branch,
            dependency_groups=hbz_dep_groups,
        )
        client.add(
            "SkiaSharp.HarfBuzz", skiasharp_version, harfbuzz_wrapper_nupkg,
            entry=_catalog_entry(harfbuzz_wrapper_nupkg, package_id="SkiaSharp.HarfBuzz", version=skiasharp_version),
        )

        extra_commit = extra_skiasharp_commit or skia_commit
        extra_nupkg = build_nupkg("SkiaSharp.Extra", skiasharp_version, commit=extra_commit, branch=branch)
        client.add(
            "SkiaSharp.Extra", skiasharp_version, extra_nupkg,
            entry=_catalog_entry(extra_nupkg, package_id="SkiaSharp.Extra", version=skiasharp_version),
        )

        harfbuzzsharp_nupkg = build_nupkg(
            "HarfBuzzSharp", harfbuzzsharp_version, commit=harfbuzzsharp_commit, branch=branch
        )
        client.add(
            "HarfBuzzSharp", harfbuzzsharp_version, harfbuzzsharp_nupkg,
            entry=_catalog_entry(harfbuzzsharp_nupkg, package_id="HarfBuzzSharp", version=harfbuzzsharp_version),
        )
        harfbuzzsharp_native_nupkg = build_nupkg(
            "HarfBuzzSharp.NativeAssets.Android", harfbuzzsharp_version, commit=harfbuzzsharp_commit, branch=branch
        )
        client.add(
            "HarfBuzzSharp.NativeAssets.Android", harfbuzzsharp_version, harfbuzzsharp_native_nupkg,
            entry=_catalog_entry(
                harfbuzzsharp_native_nupkg, package_id="HarfBuzzSharp.NativeAssets.Android",
                version=harfbuzzsharp_version,
            ),
        )

    def _reader(self, *, commit: str, branch: str, skiasharp_base: str, preview_label: str, harfbuzz_base: str):
        reader = FakeVersionsFileReader()
        reader.seed(
            commit, skiasharp_base=skiasharp_base, preview_label=preview_label,
            harfbuzz_base=harfbuzz_base, branch=branch,
        )
        return reader

    def test_happy_path_preview_release(self, tmp_download_dir: Path | None = None):
        import tempfile

        commit = "a" * 40
        branch = "release/3.119.0-preview.1"
        client = FakeNuGetClient()
        self._seed_family(
            client,
            skiasharp_version="3.119.0-preview.1.42",
            skia_commit=commit,
            branch=branch,
            harfbuzzsharp_version="1.8.8.1-preview.1.42",
        )
        reader = self._reader(
            commit=commit, branch=branch, skiasharp_base="3.119.0",
            preview_label="preview.1", harfbuzz_base="1.8.8.1",
        )
        verifier = FakeSignatureVerifier()
        with tempfile.TemporaryDirectory() as tmp:
            receipt = nuget.verify_public_receipt(
                nuget=client,
                versions_reader=reader,
                requested_version="3.119.0-preview.1.42",
                manifest=MANIFEST,
                download_dir=Path(tmp),
                signature_verifier=verifier,
                fingerprints=("aa",),
            )
        self.assertEqual(receipt.source_commit, commit)
        self.assertEqual(receipt.harfbuzzsharp_version, "1.8.8.1-preview.1.42")
        self.assertEqual(len(receipt.packages), 5)
        self.assertEqual(len(verifier.calls), 3)  # only the 3 anchor packages are signature-verified

    def test_missing_required_package_is_rejected(self):
        import tempfile

        commit = "a" * 40
        branch = "release/3.119.0"
        client = FakeNuGetClient()
        self._seed_family(
            client, skiasharp_version="3.119.0", skia_commit=commit, branch=branch,
            harfbuzzsharp_version="1.8.8.1",
        )
        del client.entries[("SkiaSharp.Extra", "3.119.0")]
        reader = self._reader(
            commit=commit, branch=branch, skiasharp_base="3.119.0", preview_label="stable", harfbuzz_base="1.8.8.1"
        )
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaises(NotReadyError):
                nuget.verify_public_receipt(
                    nuget=client, versions_reader=reader, requested_version="3.119.0",
                    manifest=MANIFEST, download_dir=Path(tmp),
                    signature_verifier=FakeSignatureVerifier(), fingerprints=("aa",),
                    sleep=lambda _seconds: None,
                )

    def test_mixed_skiasharp_source_commits_are_blocked(self):
        import tempfile

        commit = "a" * 40
        other_commit = "b" * 40
        branch = "release/3.119.0"
        client = FakeNuGetClient()
        self._seed_family(
            client, skiasharp_version="3.119.0", skia_commit=commit, branch=branch,
            harfbuzzsharp_version="1.8.8.1", extra_skiasharp_commit=other_commit,
        )
        reader = self._reader(
            commit=commit, branch=branch, skiasharp_base="3.119.0", preview_label="stable", harfbuzz_base="1.8.8.1"
        )
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaisesRegex(nuget.NuGetError, "expected"):
                nuget.verify_public_receipt(
                    nuget=client, versions_reader=reader, requested_version="3.119.0",
                    manifest=MANIFEST, download_dir=Path(tmp),
                    signature_verifier=FakeSignatureVerifier(), fingerprints=("aa",),
                )

    def test_mismatched_harfbuzz_wrapper_anchor_commit_is_blocked(self):
        # SkiaSharp.HarfBuzz is an anchor package within the SkiaSharp family
        # (see MANIFEST); its own source-commit equality check must not be
        # skipped just because it is downloaded/hash-verified like the
        # other anchors.
        import tempfile

        commit = "a" * 40
        other_commit = "c" * 40
        branch = "release/3.119.0"
        client = FakeNuGetClient()
        self._seed_family(
            client, skiasharp_version="3.119.0", skia_commit=commit, branch=branch,
            harfbuzzsharp_version="1.8.8.1", harfbuzz_wrapper_commit=other_commit,
        )
        reader = self._reader(
            commit=commit, branch=branch, skiasharp_base="3.119.0", preview_label="stable", harfbuzz_base="1.8.8.1"
        )
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaisesRegex(nuget.NuGetError, "SkiaSharp.HarfBuzz.*embeds commit"):
                nuget.verify_public_receipt(
                    nuget=client, versions_reader=reader, requested_version="3.119.0",
                    manifest=MANIFEST, download_dir=Path(tmp),
                    signature_verifier=FakeSignatureVerifier(), fingerprints=("aa",),
                )

    def test_empty_string_catalog_repository_is_accepted_when_nuspec_has_full_metadata(self):
        # Real NuGet.org catalog leaves for some already-published versions
        # (observed live for e.g. SkiaSharp 3.119.0) expose "repository" as
        # an empty string on every package in the family even though every
        # nuspec has full repository metadata; the whole receipt must still
        # verify using the nuspec alone.
        import tempfile

        commit = "a" * 40
        branch = "release/3.119.0"
        client = FakeNuGetClient()
        self._seed_family(
            client, skiasharp_version="3.119.0", skia_commit=commit, branch=branch,
            harfbuzzsharp_version="1.8.8.1", catalog_repository_override="",
        )
        reader = self._reader(
            commit=commit, branch=branch, skiasharp_base="3.119.0", preview_label="stable", harfbuzz_base="1.8.8.1"
        )
        with tempfile.TemporaryDirectory() as tmp:
            receipt = nuget.verify_public_receipt(
                nuget=client, versions_reader=reader, requested_version="3.119.0",
                manifest=MANIFEST, download_dir=Path(tmp),
                signature_verifier=FakeSignatureVerifier(), fingerprints=("aa",),
            )
        self.assertEqual(receipt.source_commit, commit)
        self.assertEqual(len(receipt.packages), 5)

    def test_reused_harfbuzzsharp_commit_from_earlier_release_is_allowed(self):
        import tempfile

        commit = "a" * 40
        older_harfbuzz_commit = "c" * 40
        branch = "release/3.119.0"
        client = FakeNuGetClient()
        self._seed_family(
            client, skiasharp_version="3.119.0", skia_commit=commit, branch=branch,
            harfbuzzsharp_version="1.8.8.1", harfbuzzsharp_commit=older_harfbuzz_commit,
        )
        reader = self._reader(
            commit=commit, branch=branch, skiasharp_base="3.119.0", preview_label="stable", harfbuzz_base="1.8.8.1"
        )
        with tempfile.TemporaryDirectory() as tmp:
            receipt = nuget.verify_public_receipt(
                nuget=client, versions_reader=reader, requested_version="3.119.0",
                manifest=MANIFEST, download_dir=Path(tmp),
                signature_verifier=FakeSignatureVerifier(), fingerprints=("aa",),
            )
        harfbuzz_packages = [p for p in receipt.packages if p.id.startswith("HarfBuzzSharp")]
        self.assertTrue(all(p.source_commit == older_harfbuzz_commit for p in harfbuzz_packages))

    def test_disagreeing_harfbuzzsharp_dependency_versions_across_tfms_blocked(self):
        import tempfile

        commit = "a" * 40
        branch = "release/3.119.0"
        client = FakeNuGetClient()
        self._seed_family(
            client, skiasharp_version="3.119.0", skia_commit=commit, branch=branch,
            harfbuzzsharp_version="1.8.8.1",
            harfbuzz_min_versions={"net8.0": "1.8.8.1", "net9.0": "1.8.8.2"},
        )
        reader = self._reader(
            commit=commit, branch=branch, skiasharp_base="3.119.0", preview_label="stable", harfbuzz_base="1.8.8.1"
        )
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaisesRegex(nuget.NuGetError, "disagrees"):
                nuget.verify_public_receipt(
                    nuget=client, versions_reader=reader, requested_version="3.119.0",
                    manifest=MANIFEST, download_dir=Path(tmp),
                    signature_verifier=FakeSignatureVerifier(), fingerprints=("aa",),
                )

    def test_branch_containment_failure_is_a_conflict(self):
        import tempfile

        commit = "a" * 40
        branch = "release/3.119.0"
        client = FakeNuGetClient()
        self._seed_family(
            client, skiasharp_version="3.119.0", skia_commit=commit, branch=branch,
            harfbuzzsharp_version="1.8.8.1",
        )
        reader = self._reader(
            commit=commit, branch=branch, skiasharp_base="3.119.0", preview_label="stable", harfbuzz_base="1.8.8.1"
        )
        reader.branch_membership[branch] = set()  # commit exists but branch does not contain it
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaises(ConflictError):
                nuget.verify_public_receipt(
                    nuget=client, versions_reader=reader, requested_version="3.119.0",
                    manifest=MANIFEST, download_dir=Path(tmp),
                    signature_verifier=FakeSignatureVerifier(), fingerprints=("aa",),
                )

    def test_signature_verification_failure_blocks(self):
        import tempfile

        commit = "a" * 40
        branch = "release/3.119.0"
        client = FakeNuGetClient()
        self._seed_family(
            client, skiasharp_version="3.119.0", skia_commit=commit, branch=branch,
            harfbuzzsharp_version="1.8.8.1",
        )
        reader = self._reader(
            commit=commit, branch=branch, skiasharp_base="3.119.0", preview_label="stable", harfbuzz_base="1.8.8.1"
        )
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaises(nuget.NuGetError):
                nuget.verify_public_receipt(
                    nuget=client, versions_reader=reader, requested_version="3.119.0",
                    manifest=MANIFEST, download_dir=Path(tmp),
                    signature_verifier=FakeSignatureVerifier(should_fail=True), fingerprints=("aa",),
                )


class VersionsTxtFamilyExtractionTests(unittest.TestCase):
    """Unit tests for the authoritative parser, independent of the real
    scripts/VERSIONS.txt so family/section-boundary edge cases are exact."""

    def test_groups_packages_under_their_family_header(self):
        text = (
            "# native sources\n"
            "harfbuzz                                        release     14.2.1\n"
            "\n"
            "# nuget versions\n"
            "# SkiaSharp\n"
            "SkiaSharp                                       nuget       4.152.0\n"
            "SkiaSharp.Views                                 nuget       4.152.0\n"
            "# HarfBuzzSharp\n"
            "HarfBuzzSharp                                   nuget       14.2.1.200\n"
        )
        families = nuget.extract_versions_txt_families(text)
        self.assertEqual(families["SkiaSharp"], ["SkiaSharp", "SkiaSharp.Views"])
        self.assertEqual(families["HarfBuzzSharp"], ["HarfBuzzSharp"])

    def test_ignores_non_nuget_lines_and_comments(self):
        text = (
            "# nuget versions\n"
            "# SkiaSharp\n"
            "# a comment about SkiaSharp\n"
            "SkiaSharp               assembly    4.152.0.0\n"
            "SkiaSharp                                       nuget       4.152.0\n"
        )
        families = nuget.extract_versions_txt_families(text)
        self.assertEqual(families["SkiaSharp"], ["SkiaSharp"])

    def test_rejects_missing_nuget_versions_section(self):
        with self.assertRaisesRegex(nuget.NuGetError, "nuget versions"):
            nuget.extract_versions_txt_families("# native sources\nfoo release 1\n")

    def test_rejects_package_line_before_any_family_header(self):
        text = "# nuget versions\nSkiaSharp nuget 4.152.0\n# SkiaSharp\n"
        with self.assertRaisesRegex(nuget.NuGetError, "before any"):
            nuget.extract_versions_txt_families(text)

    def test_rejects_section_with_no_family_headers(self):
        text = "# nuget versions\nnot a package line\n"
        with self.assertRaises(nuget.NuGetError):
            nuget.extract_versions_txt_families(text)

    def test_matches_the_real_versions_txt_lines_92_to_133(self):
        versions_text = (REPO_ROOT / "scripts" / "VERSIONS.txt").read_text(encoding="utf-8")
        lines = versions_text.splitlines()
        # The "# nuget versions" section (header on line 90, 1-based) and its
        # "# SkiaSharp"/"# HarfBuzzSharp" sub-headers (lines 91 and 123) are
        # what make the block on lines 92-133 parseable at all; those 42
        # lines currently enumerate exactly 31 SkiaSharp-family and 10
        # HarfBuzzSharp-family public NuGet IDs (41 package lines total, plus
        # the "# HarfBuzzSharp" sub-header itself on line 123).
        header_index = next(i for i, line in enumerate(lines) if line.strip() == "# nuget versions")
        self.assertEqual(header_index, 89)  # line 90, 1-based
        block = "\n".join(lines[header_index:133])
        families = nuget.extract_versions_txt_families(block)
        full_file_families = nuget.extract_versions_txt_families(versions_text)
        self.assertEqual(families, full_file_families)
        self.assertEqual(len(families["SkiaSharp"]), 31)
        self.assertEqual(len(families["HarfBuzzSharp"]), 10)


class PublicPackagesManifestTests(unittest.TestCase):
    def test_public_packages_manifest_matches_versions_txt(self):
        import json

        manifest_path = Path(__file__).resolve().parent.parent / "public-packages.json"
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        versions_text = (REPO_ROOT / "scripts" / "VERSIONS.txt").read_text(encoding="utf-8")
        # Authoritative source: the "# SkiaSharp"/"# HarfBuzzSharp" families
        # declared in scripts/VERSIONS.txt's "# nuget versions" section, not a
        # `<PackageId>` inference from project files.
        expected = nuget.extract_versions_txt_families(versions_text)
        for family in ("SkiaSharp", "HarfBuzzSharp"):
            self.assertEqual(
                set(manifest["families"][family]),
                set(expected[family]),
                f"public-packages.json[{family!r}] is out of sync with "
                "scripts/VERSIONS.txt's '# nuget versions' section",
            )

    def test_anchor_packages_are_declared_in_families(self):
        import json

        manifest_path = Path(__file__).resolve().parent.parent / "public-packages.json"
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        declared = set(manifest["families"]["SkiaSharp"]) | set(manifest["families"]["HarfBuzzSharp"])
        for anchor in manifest["anchorPackages"]:
            self.assertIn(anchor, declared)


class FakeCommandRunnerForVerify:
    def __init__(self, *, returncode: int = 0):
        self.returncode = returncode
        self.calls: list[list[str]] = []

    def run(self, args, *, cwd, check=True, timeout=120, input=None):
        self.calls.append(list(args))
        return CommandResult(args=tuple(args), returncode=self.returncode, stdout="", stderr="")


class DotNetSignatureVerifierTests(unittest.TestCase):
    def test_passes_all_fingerprints_and_dotnet_command_prefix(self):
        runner = FakeCommandRunnerForVerify()
        verifier = nuget.DotNetSignatureVerifier(runner=runner, dotnet_command=("/repo/eng/common/dotnet.sh",))
        nupkg = Path("/tmp/does-not-matter/SkiaSharp.1.0.0.nupkg")
        verifier.verify(nupkg, ("AAAA", "BBBB"))
        self.assertEqual(len(runner.calls), 1)
        args = runner.calls[0]
        self.assertEqual(args[0], "/repo/eng/common/dotnet.sh")
        self.assertIn("--all", args)
        self.assertEqual(args.count("--certificate-fingerprint"), 2)
        self.assertIn("AAAA", args)
        self.assertIn("BBBB", args)
        self.assertEqual(args[-1], str(nupkg))

    def test_defaults_to_bare_dotnet(self):
        runner = FakeCommandRunnerForVerify()
        verifier = nuget.DotNetSignatureVerifier(runner=runner)
        verifier.verify(Path("/tmp/pkg.nupkg"), ("AAAA",))
        self.assertEqual(runner.calls[0][0], "dotnet")

    def test_rejects_empty_fingerprint_list(self):
        runner = FakeCommandRunnerForVerify()
        verifier = nuget.DotNetSignatureVerifier(runner=runner)
        with self.assertRaises(nuget.NuGetError):
            verifier.verify(Path("/tmp/pkg.nupkg"), ())
        self.assertEqual(runner.calls, [])

    def test_raises_on_verification_failure(self):
        runner = FakeCommandRunnerForVerify(returncode=1)
        verifier = nuget.DotNetSignatureVerifier(runner=runner)
        with self.assertRaises(nuget.NuGetError):
            verifier.verify(Path("/tmp/pkg.nupkg"), ("AAAA",))


class DotnetCommandResolutionTests(unittest.TestCase):
    def test_prefers_arcade_wrapper_when_present(self):
        import tempfile

        import release as cli

        with tempfile.TemporaryDirectory() as tmp:
            repo_root = Path(tmp)
            (repo_root / "eng" / "common").mkdir(parents=True)
            wrapper_name = "dotnet.cmd" if sys.platform.startswith("win") else "dotnet.sh"
            wrapper = repo_root / "eng" / "common" / wrapper_name
            wrapper.write_text("#!/bin/sh\n", encoding="utf-8")
            self.assertEqual(cli._dotnet_command(repo_root), (str(wrapper),))

    def test_falls_back_to_path_dotnet_when_wrapper_missing(self):
        import tempfile

        import release as cli

        with tempfile.TemporaryDirectory() as tmp:
            self.assertEqual(cli._dotnet_command(Path(tmp)), ("dotnet",))


class TrustedCertificatesTests(unittest.TestCase):
    """Structural checks against the real, checked-in reviewed certificate
    file. These deliberately never assert an exact fingerprint value or an
    exact certificate count: rotation is additive (old fingerprints are
    kept alongside new ones), so a future renewal must not need a test
    edit here. Only load_certificates()'s own structural validation (regex
    format, known role, dedupe, date format) is exercised precisely, using
    synthetic fixtures below."""

    def _load(self):
        certificates_path = Path(__file__).resolve().parent.parent / "trusted-signing-certificates.json"
        return nuget.load_certificates(certificates_path)

    def test_fingerprints_are_valid_sha256_hex(self):
        certificates = self._load()
        self.assertGreaterEqual(len(certificates), 1)
        for certificate in certificates:
            self.assertRegex(certificate.fingerprint, r"^[0-9A-F]{64}$")

    def test_fingerprints_are_unique(self):
        certificates = self._load()
        fingerprints = [c.fingerprint for c in certificates]
        self.assertEqual(len(fingerprints), len(set(fingerprints)))

    def test_every_certificate_has_a_known_role(self):
        for certificate in self._load():
            self.assertIn(certificate.role, nuget.KNOWN_CERTIFICATE_ROLES)

    def test_at_least_one_author_and_one_repository_certificate(self):
        # SkiaSharp packages are dual-signed (Microsoft author signature +
        # NuGet.org repository signature); `dotnet nuget verify --all`
        # needs a trusted fingerprint of each role or it fails on whichever
        # signature has no match. This is a property of the anchor
        # packages, not a fixed count, so adding/rotating certificates of
        # either role never breaks this test.
        roles = {certificate.role for certificate in self._load()}
        self.assertIn("author", roles)
        self.assertIn("repository", roles)

    def test_load_fingerprints_returns_every_certificate(self):
        certificates = self._load()
        fingerprints = nuget.load_fingerprints(
            Path(__file__).resolve().parent.parent / "trusted-signing-certificates.json"
        )
        self.assertEqual(set(fingerprints), {c.fingerprint for c in certificates})


class LoadCertificatesValidationTests(unittest.TestCase):
    """Exercises load_certificates()'s structural validation against small
    synthetic fixture files, independent of the real reviewed list, so
    rotating real certificates never has to touch these."""

    def _write(self, tmp_path: Path, certificates: list[dict]) -> Path:
        import json

        path = tmp_path / "trusted-signing-certificates.json"
        path.write_text(json.dumps({"hashAlgorithm": "SHA256", "certificates": certificates}), encoding="utf-8")
        return path

    def test_accepts_additive_rotation_with_three_generations(self):
        # A third, newly added fingerprint alongside two older ones for the
        # same role must load cleanly: rotation is additive, never a swap.
        import tempfile

        with tempfile.TemporaryDirectory() as tmp:
            path = self._write(
                Path(tmp),
                [
                    {"fingerprint": "A" * 64, "role": "repository", "subject": "NuGet.org", "description": "gen 1", "validFrom": None, "validUntil": "2022-01-01"},
                    {"fingerprint": "B" * 64, "role": "repository", "subject": "NuGet.org", "description": "gen 2", "validFrom": "2022-01-01", "validUntil": "2024-01-01"},
                    {"fingerprint": "C" * 64, "role": "repository", "subject": "NuGet.org", "description": "gen 3", "validFrom": "2024-01-01", "validUntil": None},
                ],
            )
            certificates = nuget.load_certificates(path)
            self.assertEqual(len(certificates), 3)
            self.assertEqual(
                {c.fingerprint for c in certificates}, {"A" * 64, "B" * 64, "C" * 64}
            )

    def test_rejects_duplicate_fingerprint(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp:
            path = self._write(
                Path(tmp),
                [
                    {"fingerprint": "A" * 64, "role": "author", "subject": "x", "description": "d", "validFrom": None, "validUntil": None},
                    {"fingerprint": "A" * 64, "role": "repository", "subject": "y", "description": "d", "validFrom": None, "validUntil": None},
                ],
            )
            with self.assertRaisesRegex(nuget.NuGetError, "duplicate"):
                nuget.load_certificates(path)

    def test_rejects_unknown_role(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp:
            path = self._write(
                Path(tmp),
                [{"fingerprint": "A" * 64, "role": "timestamp", "subject": "x", "description": "d", "validFrom": None, "validUntil": None}],
            )
            with self.assertRaisesRegex(nuget.NuGetError, "unknown role"):
                nuget.load_certificates(path)

    def test_rejects_malformed_fingerprint(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp:
            path = self._write(
                Path(tmp),
                [{"fingerprint": "not-hex", "role": "author", "subject": "x", "description": "d", "validFrom": None, "validUntil": None}],
            )
            with self.assertRaises(nuget.NuGetError):
                nuget.load_certificates(path)

    def test_rejects_malformed_date(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp:
            path = self._write(
                Path(tmp),
                [{"fingerprint": "A" * 64, "role": "author", "subject": "x", "description": "d", "validFrom": None, "validUntil": "not-a-date"}],
            )
            with self.assertRaises(nuget.NuGetError):
                nuget.load_certificates(path)

    def test_rejects_empty_certificate_list(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp:
            path = self._write(Path(tmp), [])
            with self.assertRaises(nuget.NuGetError):
                nuget.load_certificates(path)

    def test_accepts_missing_optional_dates(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp:
            path = self._write(
                Path(tmp),
                [{"fingerprint": "A" * 64, "role": "author", "subject": "x", "description": "d"}],
            )
            certificates = nuget.load_certificates(path)
            self.assertIsNone(certificates[0].valid_from)
            self.assertIsNone(certificates[0].valid_until)


class _RegistrationHandler(http.server.BaseHTTPRequestHandler):
    """Serves canned (optionally gzip-encoded) JSON/bytes responses.

    Subclassed per-test (via ``type(...)``) so each test gets its own
    isolated ``routes``/``gzip_paths`` class state instead of sharing one
    global handler across the whole test module.
    """

    routes: dict[str, bytes] = {}
    gzip_paths: set[str] = set()

    def do_GET(self):  # noqa: N802 - http.server's required method name
        body = self.routes.get(self.path)
        if body is None:
            self.send_response(404)
            self.end_headers()
            return
        self.send_response(200)
        self.send_header("Content-Type", "application/json")
        if self.path in self.gzip_paths:
            self.send_header("Content-Encoding", "gzip")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def log_message(self, format, *args):  # noqa: A002 - silence default stderr logging
        pass


class _LocalNuGetServer:
    """A real local HTTP server standing in for nuget.org for one test.

    Uses the standard library's own ``http.server`` (a real socket, a real
    HTTP/1.1 response with real headers) so ``HttpNuGetClient`` is
    exercised through an actual ``urllib.request.urlopen`` round trip --
    the only way to prove gzip decompression genuinely works, since a
    hand-built fake transport could trivially "pass" a broken
    implementation by never actually gzip-encoding anything.
    """

    def __init__(self):
        handler = type("_Handler", (_RegistrationHandler,), {"routes": {}, "gzip_paths": set()})
        self.handler = handler
        self.httpd = http.server.ThreadingHTTPServer(("127.0.0.1", 0), handler)
        self.thread = threading.Thread(target=self.httpd.serve_forever, daemon=True)
        self.thread.start()

    @property
    def base_url(self) -> str:
        return f"http://127.0.0.1:{self.httpd.server_port}"

    def add_json(self, path: str, payload: object, *, gzip_encoded: bool) -> None:
        raw = json.dumps(payload).encode("utf-8")
        if gzip_encoded:
            raw = gzip.compress(raw)
            self.handler.gzip_paths.add(path)
        self.handler.routes[path] = raw

    def add_bytes(self, path: str, data: bytes) -> None:
        self.handler.routes[path] = data

    def shutdown(self) -> None:
        self.httpd.shutdown()
        self.httpd.server_close()


class HttpNuGetClientRealTransportTests(unittest.TestCase):
    """Item 1 (critical): registration5-gz-semver2 always compresses its
    body; urllib.request never transparently decompresses it. These tests
    run a real local HTTP server and a real HttpNuGetClient against it --
    no mocked transport -- so a regression here (e.g. reverting to
    ``response.read().decode("utf-8")`` without checking Content-Encoding)
    fails with a genuine UnicodeDecodeError/JSONDecodeError, not a
    trivially-satisfied fake.

    Also covers item 2 (critical): the registration item's inlined
    catalogEntry must always be dereferenced through its own "@id" to the
    full catalog leaf, never trusted for packageHash/packageSize just
    because it happens to carry "listed"."""

    def setUp(self):
        self.server = _LocalNuGetServer()
        self.client = nuget.HttpNuGetClient(
            registration_base=f"{self.server.base_url}/registration5-gz-semver2",
            flat_container_base=f"{self.server.base_url}/v3-flatcontainer",
        )

    def tearDown(self):
        self.server.shutdown()

    def _leaf_payload(self, *, package_id: str, version: str, repository) -> dict:
        return {
            "@id": f"{self.server.base_url}/catalog0/data/2024.01.01/{package_id.lower()}.{version}.json",
            "id": package_id,
            "version": version,
            "listed": True,
            "packageHash": "YWJj",  # base64("abc"); value is never checked by this test
            "packageHashAlgorithm": "SHA512",
            "packageSize": 12345,
            "dependencyGroups": [{"targetFramework": "net8.0", "dependencies": []}],
            "repository": repository,
        }

    def test_decompresses_a_real_gzip_response_and_dereferences_the_leaf(self):
        leaf_path = "/catalog0/data/2024.01.01/skiasharp.3.119.0.json"
        leaf_url = f"{self.server.base_url}{leaf_path}"
        index_path = "/registration5-gz-semver2/skiasharp/index.json"
        # The real registration item's inlined catalogEntry has "listed"
        # but never packageHash/packageSize/repository -- only the
        # dereferenced leaf does. If get_catalog_entry ever stopped
        # dereferencing (item 2's regression), this test would return an
        # entry with an empty packageHash instead of failing outright, so
        # the assertions below check the actual hash/size values.
        self.server.add_json(
            index_path,
            {
                "items": [
                    {
                        "items": [
                            {
                                "catalogEntry": {
                                    "@id": leaf_url,
                                    "id": "SkiaSharp",
                                    "version": "3.119.0",
                                    "listed": True,
                                }
                            }
                        ]
                    }
                ]
            },
            gzip_encoded=True,
        )
        self.server.add_json(
            leaf_path,
            self._leaf_payload(
                package_id="SkiaSharp", version="3.119.0",
                repository={"type": "git", "commit": "a" * 40, "branch": "release/3.119.0"},
            ),
            gzip_encoded=True,
        )

        entry = self.client.get_catalog_entry("SkiaSharp", "3.119.0")

        self.assertIsNotNone(entry)
        self.assertTrue(entry.listed)
        self.assertEqual(entry.package_hash, "YWJj")
        self.assertEqual(entry.package_hash_algorithm, "SHA512")
        self.assertEqual(entry.package_size, 12345)
        self.assertEqual(entry.repository["commit"], "a" * 40)

    def test_leaf_response_need_not_be_gzip_encoded(self):
        # Defends against over-assuming: only the registration index/page
        # responses are reliably gzip-compressed by nuget.org; a leaf
        # dereference target must also work when served uncompressed.
        leaf_path = "/catalog0/data/2024.01.01/skiasharp.3.119.0.json"
        leaf_url = f"{self.server.base_url}{leaf_path}"
        index_path = "/registration5-gz-semver2/skiasharp/index.json"
        self.server.add_json(
            index_path,
            {"items": [{"items": [{"catalogEntry": {"@id": leaf_url, "version": "3.119.0", "listed": True}}]}]},
            gzip_encoded=True,
        )
        self.server.add_json(
            leaf_path,
            self._leaf_payload(package_id="SkiaSharp", version="3.119.0", repository=""),
            gzip_encoded=False,
        )

        entry = self.client.get_catalog_entry("SkiaSharp", "3.119.0")
        self.assertIsNotNone(entry)
        self.assertEqual(entry.package_size, 12345)

    def test_dereferences_a_paged_index(self):
        # The real index.json can itself list "page" references (no
        # "items" inlined) that must be fetched before scanning for the
        # matching version.
        leaf_path = "/catalog0/data/2024.01.01/skiasharp.3.119.0.json"
        leaf_url = f"{self.server.base_url}{leaf_path}"
        index_path = "/registration5-gz-semver2/skiasharp/index.json"
        page_path = "/registration5-gz-semver2/skiasharp/page0.json"
        page_url = f"{self.server.base_url}{page_path}"
        self.server.add_json(index_path, {"items": [{"@id": page_url}]}, gzip_encoded=True)
        self.server.add_json(
            page_path,
            {"items": [{"catalogEntry": {"@id": leaf_url, "version": "3.119.0", "listed": True}}]},
            gzip_encoded=True,
        )
        self.server.add_json(
            leaf_path,
            self._leaf_payload(package_id="SkiaSharp", version="3.119.0", repository=""),
            gzip_encoded=True,
        )

        entry = self.client.get_catalog_entry("SkiaSharp", "3.119.0")
        self.assertIsNotNone(entry)
        self.assertEqual(entry.package_size, 12345)

    def test_returns_none_for_a_missing_package(self):
        # No route registered at all for this package's index -> real 404.
        self.assertIsNone(self.client.get_catalog_entry("DoesNotExist", "1.0.0"))

    def test_returns_none_when_requested_version_is_absent(self):
        leaf_url = f"{self.server.base_url}/unused.json"
        index_path = "/registration5-gz-semver2/skiasharp/index.json"
        self.server.add_json(
            index_path,
            {"items": [{"items": [{"catalogEntry": {"@id": leaf_url, "version": "9.9.9", "listed": True}}]}]},
            gzip_encoded=True,
        )
        self.assertIsNone(self.client.get_catalog_entry("SkiaSharp", "3.119.0"))

    def test_malformed_gzip_body_raises_explicit_nuget_error(self):
        # Content-Encoding says gzip but the body is not valid gzip -- must
        # surface as a clear NuGetError, not an unhandled exception.
        index_path = "/registration5-gz-semver2/skiasharp/index.json"
        self.server.add_bytes(index_path, b"not-actually-gzip")
        self.server.handler.gzip_paths.add(index_path)
        with self.assertRaisesRegex(nuget.NuGetError, "not valid gzip"):
            self.client.get_catalog_entry("SkiaSharp", "3.119.0")

    def test_download_package_and_get_nuspec_use_flat_container_base(self):
        lower = "skiasharp"
        nupkg_path = f"/v3-flatcontainer/{lower}/3.119.0/{lower}.3.119.0.nupkg"
        nuspec_path = f"/v3-flatcontainer/{lower}/3.119.0/{lower}.nuspec"
        self.server.add_bytes(nupkg_path, b"fake-nupkg-bytes")
        self.server.add_bytes(nuspec_path, b"<package/>")

        self.assertEqual(self.client.download_package("SkiaSharp", "3.119.0"), b"fake-nupkg-bytes")
        self.assertEqual(self.client.get_nuspec("SkiaSharp", "3.119.0"), b"<package/>")

    def test_get_nuspec_returns_none_for_404(self):
        self.assertIsNone(self.client.get_nuspec("SkiaSharp", "9.9.9"))


if __name__ == "__main__":
    unittest.main()
