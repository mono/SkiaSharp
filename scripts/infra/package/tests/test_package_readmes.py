from __future__ import annotations

from html import escape
import os
from pathlib import Path
import subprocess
import tempfile
import unittest


ROOT = Path(__file__).resolve().parents[4]
NUGET_PROPS = ROOT / "source" / "SkiaSharp.NuGet.props"
NUGET_TARGETS = ROOT / "source" / "SkiaSharp.NuGet.targets"
BINDING_TARGETS = ROOT / "binding" / "Directory.Build.targets"

CURRENT_REPOSITORY_URL = "https://github.com/mono/SkiaSharp"
CURRENT_DOCUMENTATION_URL = "https://mono.github.io/SkiaSharp"
DESTINATION_REPOSITORY_URL = "https://github.com/dotnet/SkiaSharp"
DESTINATION_DOCUMENTATION_URL = "https://dotnet.github.io/SkiaSharp"

CUSTOM_TEMPLATES = (
    ("SkiaSharp", ROOT / "binding" / "SkiaSharp" / "PACKAGE-README.md"),
    (
        "HarfBuzzSharp",
        ROOT / "binding" / "HarfBuzzSharp" / "PACKAGE-README.md",
    ),
    (
        "SkiaSharp.Views",
        ROOT
        / "source"
        / "SkiaSharp.Views"
        / "SkiaSharp.Views"
        / "PACKAGE-README.md",
    ),
    (
        "SkiaSharp.Views.Blazor",
        ROOT
        / "source"
        / "SkiaSharp.Views"
        / "SkiaSharp.Views.Blazor"
        / "PACKAGE-README.md",
    ),
    (
        "SkiaSharp.Views.Maui.Controls",
        ROOT
        / "source"
        / "SkiaSharp.Views.Maui"
        / "SkiaSharp.Views.Maui.Controls"
        / "PACKAGE-README.md",
    ),
)


def xml_text(value: object) -> str:
    return escape(str(value), quote=False)


def xml_attribute(value: object) -> str:
    return escape(str(value), quote=True)


class PackageReadmeTests(unittest.TestCase):
    def render_readme(
        self,
        package_id: str,
        *,
        template: Path | None = None,
        properties: dict[str, str] | None = None,
        use_binding_targets: bool = False,
    ) -> tuple[str, dict[str, str]]:
        with tempfile.TemporaryDirectory() as directory:
            temp = Path(directory)
            output = temp / "obj"
            output.mkdir()
            values = {
                "PackagingGroup": "SkiaSharp",
                "PackageId": package_id,
                "PackageDescription": "Generated package summary.",
                "IntermediateOutputPath": f"{output}{os.sep}",
                "TargetFramework": "net10.0",
                "IsWindows": "false",
                "IsMacOS": "false",
                "IsLinux": "true",
                "IsNetTizenSupported": "false",
                "IsNetTVOSSupported": "false",
                "IsNetMacOSSupported": "false",
                "SignAssembly": "false",
                "ProduceReferenceAssembly": "false",
            }
            if template is not None:
                values["PackageReadmeSource"] = str(template)
            values.update(properties or {})

            property_xml = "\n".join(
                f"    <{name}>{xml_text(value)}</{name}>"
                for name, value in values.items()
            )
            targets = BINDING_TARGETS if use_binding_targets else NUGET_TARGETS
            project = temp / "PackageReadme.Tests.proj"
            project.write_text(
                f"""<Project>
  <PropertyGroup>
{property_xml}
  </PropertyGroup>
  <Import Project="{xml_attribute(NUGET_PROPS)}" />
  <Import Project="{xml_attribute(targets)}" />
  <Target Name="CapturePackageMetadata">
    <ItemGroup>
      <_PackageMetadata Include="PackageProjectUrl=$(PackageProjectUrl)" />
      <_PackageMetadata Include="PackageReleaseNotes=$(PackageReleaseNotes)" />
      <_PackageMetadata Include="PackageReadmeFile=$(PackageReadmeFile)" />
      <_PackageMetadata Include="RepositoryUrl=$(RepositoryUrl)" />
      <_PackageMetadata Include="RepositoryType=$(RepositoryType)" />
    </ItemGroup>
    <WriteLinesToFile
      File="$(IntermediateOutputPath)metadata.txt"
      Lines="@(_PackageMetadata)"
      Encoding="UTF-8"
      Overwrite="True" />
  </Target>
</Project>
""",
                encoding="utf-8",
            )

            result = subprocess.run(
                [
                    "dotnet",
                    "msbuild",
                    str(project),
                    "/nologo",
                    "/verbosity:minimal",
                    "/target:_GeneratePackageReadmeFile;CapturePackageMetadata",
                ],
                cwd=ROOT,
                capture_output=True,
                text=True,
            )
            self.assertEqual(
                0,
                result.returncode,
                f"MSBuild failed:\nSTDOUT:\n{result.stdout}\nSTDERR:\n{result.stderr}",
            )

            readme = (output / "README.md").read_text(encoding="utf-8-sig")
            metadata = {}
            for line in (output / "metadata.txt").read_text(
                encoding="utf-8-sig"
            ).splitlines():
                key, value = line.split("=", 1)
                metadata[key] = value
            return readme, metadata

    def assert_template_rendered(
        self,
        package_id: str,
        template: Path,
        repository_url: str,
        documentation_url: str,
        *,
        explicit: bool,
    ) -> None:
        properties = {}
        if explicit:
            properties = {
                "SkiaSharpRepositoryUrl": repository_url,
                "SkiaSharpDocumentationUrl": documentation_url,
                "_RepositoryIdentityConfig": str(
                    template.parent / "missing-repository-identity.json"
                ),
            }

        actual, _ = self.render_readme(
            package_id,
            template=template,
            properties=properties,
        )
        expected = (
            template.read_text(encoding="utf-8-sig")
            .replace("{{RepositoryUrl}}", repository_url)
            .replace("{{DocumentationUrl}}", documentation_url)
        )

        self.assertEqual(expected.splitlines(), actual.splitlines())
        self.assertNotIn("{{RepositoryUrl}}", actual)
        self.assertNotIn("{{DocumentationUrl}}", actual)
        self.assertIn(repository_url, actual)
        if "{{DocumentationUrl}}" in template.read_text(encoding="utf-8-sig"):
            self.assertIn(documentation_url, actual)
        if explicit:
            self.assertNotIn(CURRENT_REPOSITORY_URL, actual)
            self.assertNotIn(CURRENT_DOCUMENTATION_URL, actual)

    def test_custom_templates_render_for_current_and_destination_identities(
        self,
    ) -> None:
        scenarios = (
            (
                CURRENT_REPOSITORY_URL,
                CURRENT_DOCUMENTATION_URL,
                False,
            ),
            (
                DESTINATION_REPOSITORY_URL,
                DESTINATION_DOCUMENTATION_URL,
                True,
            ),
        )
        for package_id, template in CUSTOM_TEMPLATES:
            for repository_url, documentation_url, explicit in scenarios:
                with self.subTest(
                    package_id=package_id,
                    repository_url=repository_url,
                ):
                    self.assert_template_rendered(
                        package_id,
                        template,
                        repository_url,
                        documentation_url,
                        explicit=explicit,
                    )

    def test_generated_readme_uses_defaults_and_preserves_metadata(self) -> None:
        readme, metadata = self.render_readme("Test.Package")

        self.assertIn("# Test.Package", readme)
        self.assertIn(
            "https://www.nuget.org/packages/Test.Package",
            readme,
        )
        self.assertIn(CURRENT_REPOSITORY_URL, readme)
        self.assertIn(CURRENT_DOCUMENTATION_URL, readme)
        self.assertEqual(
            {
                "PackageProjectUrl": (
                    "https://go.microsoft.com/fwlink/?linkid=868515"
                ),
                "PackageReleaseNotes": (
                    "Please visit https://go.microsoft.com/fwlink/?linkid=868517 "
                    "to view the release notes."
                ),
                "PackageReadmeFile": "README.md",
                "RepositoryUrl": (
                    "https://go.microsoft.com/fwlink/?linkid=868515"
                ),
                "RepositoryType": "git",
            },
            metadata,
        )

    def test_explicit_identity_overrides_do_not_require_offline_config(
        self,
    ) -> None:
        readme, metadata = self.render_readme(
            "Test.Package",
            properties={
                "SkiaSharpRepositoryUrl": DESTINATION_REPOSITORY_URL,
                "SkiaSharpDocumentationUrl": DESTINATION_DOCUMENTATION_URL,
                "_RepositoryIdentityConfig": str(
                    ROOT / "does-not-exist" / "repository-identity.json"
                ),
            },
        )

        self.assertIn(DESTINATION_REPOSITORY_URL, readme)
        self.assertIn(DESTINATION_DOCUMENTATION_URL, readme)
        self.assertNotIn(CURRENT_REPOSITORY_URL, readme)
        self.assertNotIn(CURRENT_DOCUMENTATION_URL, readme)
        self.assertEqual(
            "https://go.microsoft.com/fwlink/?linkid=868515",
            metadata["PackageProjectUrl"],
        )
        self.assertEqual(
            "https://go.microsoft.com/fwlink/?linkid=868515",
            metadata["RepositoryUrl"],
        )

    def test_harfbuzz_generated_resources_use_repository_override(self) -> None:
        for repository_url, documentation_url, explicit in (
            (
                CURRENT_REPOSITORY_URL,
                CURRENT_DOCUMENTATION_URL,
                False,
            ),
            (
                DESTINATION_REPOSITORY_URL,
                DESTINATION_DOCUMENTATION_URL,
                True,
            ),
        ):
            properties = {"PackagingGroup": "HarfBuzzSharp"}
            if explicit:
                properties.update(
                    {
                        "SkiaSharpRepositoryUrl": repository_url,
                        "SkiaSharpDocumentationUrl": documentation_url,
                        "_RepositoryIdentityConfig": str(
                            ROOT / "missing-repository-identity.json"
                        ),
                    }
                )
            with self.subTest(repository_url=repository_url):
                readme, _ = self.render_readme(
                    "HarfBuzzSharp.NativeAssets.Linux",
                    properties=properties,
                    use_binding_targets=True,
                )
                self.assertIn(
                    f"{repository_url}/blob/main/documentation/dev/packages.md",
                    readme,
                )
                self.assertIn(
                    "https://learn.microsoft.com/dotnet/api/harfbuzzsharp",
                    readme,
                )
                self.assertIn("https://harfbuzz.github.io/", readme)
                self.assertIn(
                    "https://www.nuget.org/packages/SkiaSharp.HarfBuzz",
                    readme,
                )

    def test_generated_readme_round_trips_special_characters(self) -> None:
        summary = (
            'Summary & details; 50% <span title="quoted">"value"</span> - مرحبا'
        )
        notice = (
            '> Notice: A&B; 75% <tag data-value="1 & 2">"quoted"</tag>'
        )
        details = (
            '## Details\n\nXML: `<node attr="A&B">100% complete;</node>`\n\n'
            "Unicode: Καλημέρα κόσμε"
        )
        resources = (
            "- [Docs](https://example.test/docs?a=1&b=2)\n"
            "- Literal `<tag>`; 99% complete"
        )

        readme, _ = self.render_readme(
            "Test.Package",
            properties={
                "PackageDescription": summary,
                "PackageReadmeNotice": notice,
                "PackageReadmeAdditionalDetails": details,
                "PackageReadmeResourceLinks": resources,
            },
        )

        for value in (summary, notice, details, resources):
            with self.subTest(value=value):
                self.assertIn(value, readme)


if __name__ == "__main__":
    unittest.main()
