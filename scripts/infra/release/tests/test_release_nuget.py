from __future__ import annotations

import io
import re
import sys
import unittest
import zipfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release_nuget as nuget
from release_common import ConflictError, NotReadyError


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
        # catalog entry and nuspec agree unless a test deliberately passes a
        # conflicting ``repository`` to simulate catalog/nuspec disagreement.
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


class RepositoryMetadataTests(unittest.TestCase):
    def test_agreeing_metadata_is_accepted(self):
        commit = "d" * 40
        nupkg = build_nupkg("SkiaSharp", "3.119.0", commit=commit, branch="release/3.119.0-preview.1")
        nuspec = nuget.parse_nuspec(nupkg)
        result_commit, result_branch = nuget.verify_repository_metadata(
            nuspec=nuspec,
            catalog_repository={"type": "git", "commit": commit, "branch": "release/3.119.0-preview.1"},
            package_id="SkiaSharp",
            version="3.119.0",
        )
        self.assertEqual(result_commit, commit)
        self.assertEqual(result_branch, "release/3.119.0-preview.1")

    def test_rejects_non_git_repository_type(self):
        nupkg = build_nupkg("SkiaSharp", "3.119.0")
        nuspec = nuget.parse_nuspec(nupkg)
        object.__setattr__(nuspec.repository, "type", "git")  # sanity: dataclass is frozen
        with self.assertRaisesRegex(nuget.NuGetError, "not 'git'"):
            nuget.verify_repository_metadata(
                nuspec=nuspec,
                catalog_repository={"type": "svn", "commit": "a" * 40, "branch": "release/3.119.0"},
                package_id="SkiaSharp",
                version="3.119.0",
            )

    def test_rejects_short_commit(self):
        nupkg = build_nupkg("SkiaSharp", "3.119.0", commit="abc123", branch="release/3.119.0")
        nuspec = nuget.parse_nuspec(nupkg)
        with self.assertRaisesRegex(nuget.NuGetError, "full SHA"):
            nuget.verify_repository_metadata(
                nuspec=nuspec,
                catalog_repository={"type": "git", "commit": "abc123", "branch": "release/3.119.0"},
                package_id="SkiaSharp",
                version="3.119.0",
            )

    def test_rejects_nuspec_catalog_commit_disagreement(self):
        nupkg = build_nupkg("SkiaSharp", "3.119.0", commit="a" * 40, branch="release/3.119.0")
        nuspec = nuget.parse_nuspec(nupkg)
        with self.assertRaisesRegex(nuget.NuGetError, "disagree"):
            nuget.verify_repository_metadata(
                nuspec=nuspec,
                catalog_repository={"type": "git", "commit": "b" * 40, "branch": "release/3.119.0"},
                package_id="SkiaSharp",
                version="3.119.0",
            )

    def test_rejects_invalid_branch_grammar(self):
        nupkg = build_nupkg("SkiaSharp", "3.119.0", commit="a" * 40, branch="main")
        nuspec = nuget.parse_nuspec(nupkg)
        with self.assertRaisesRegex(nuget.NuGetError, "grammar"):
            nuget.verify_repository_metadata(
                nuspec=nuspec,
                catalog_repository={"type": "git", "commit": "a" * 40, "branch": "main"},
                package_id="SkiaSharp",
                version="3.119.0",
            )


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
    ):
        harfbuzzsharp_commit = harfbuzzsharp_commit or skia_commit
        harfbuzz_min_versions = harfbuzz_min_versions or {"net8.0": harfbuzzsharp_version, "net9.0": harfbuzzsharp_version}
        skiasharp_nupkg = build_nupkg("SkiaSharp", skiasharp_version, commit=skia_commit, branch=branch)
        client.add("SkiaSharp", skiasharp_version, skiasharp_nupkg)

        hbz_dep_groups = [(tfm, [("HarfBuzzSharp", version)]) for tfm, version in harfbuzz_min_versions.items()]
        harfbuzz_wrapper_nupkg = build_nupkg(
            "SkiaSharp.HarfBuzz", skiasharp_version, commit=skia_commit, branch=branch,
            dependency_groups=hbz_dep_groups,
        )
        client.add("SkiaSharp.HarfBuzz", skiasharp_version, harfbuzz_wrapper_nupkg)

        extra_commit = extra_skiasharp_commit or skia_commit
        extra_nupkg = build_nupkg("SkiaSharp.Extra", skiasharp_version, commit=extra_commit, branch=branch)
        client.add("SkiaSharp.Extra", skiasharp_version, extra_nupkg)

        harfbuzzsharp_nupkg = build_nupkg(
            "HarfBuzzSharp", harfbuzzsharp_version, commit=harfbuzzsharp_commit, branch=branch
        )
        client.add("HarfBuzzSharp", harfbuzzsharp_version, harfbuzzsharp_nupkg)
        harfbuzzsharp_native_nupkg = build_nupkg(
            "HarfBuzzSharp.NativeAssets.Android", harfbuzzsharp_version, commit=harfbuzzsharp_commit, branch=branch
        )
        client.add("HarfBuzzSharp.NativeAssets.Android", harfbuzzsharp_version, harfbuzzsharp_native_nupkg)

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


class PublicPackagesManifestTests(unittest.TestCase):
    def test_public_packages_manifest_matches_versions_txt(self):
        import json

        manifest_path = Path(__file__).resolve().parent.parent / "public-packages.json"
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        versions_text = (REPO_ROOT / "scripts" / "VERSIONS.txt").read_text(encoding="utf-8")
        found = set(
            re.findall(r"^(SkiaSharp\S*|HarfBuzzSharp\S*)\s+nuget\s+\S+\s*$", versions_text, re.MULTILINE)
        )
        declared = set(manifest["families"]["SkiaSharp"]) | set(manifest["families"]["HarfBuzzSharp"])
        self.assertEqual(
            found, declared,
            "public-packages.json is out of sync with the nuget lines in scripts/VERSIONS.txt",
        )

    def test_anchor_packages_are_declared_in_families(self):
        import json

        manifest_path = Path(__file__).resolve().parent.parent / "public-packages.json"
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        declared = set(manifest["families"]["SkiaSharp"]) | set(manifest["families"]["HarfBuzzSharp"])
        for anchor in manifest["anchorPackages"]:
            self.assertIn(anchor, declared)


class TrustedCertificatesTests(unittest.TestCase):
    def test_fingerprints_are_valid_sha256_hex(self):
        certificates_path = Path(__file__).resolve().parent.parent / "trusted-signing-certificates.json"
        fingerprints = nuget.load_fingerprints(certificates_path)
        self.assertGreaterEqual(len(fingerprints), 1)
        for fingerprint in fingerprints:
            self.assertRegex(fingerprint, r"^[0-9A-F]{64}$")


if __name__ == "__main__":
    unittest.main()
