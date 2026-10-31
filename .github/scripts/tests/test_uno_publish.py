#!/usr/bin/env python3
"""Verify the Uno publish layout and deployment-prefix contract."""

import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest

import yaml


ROOT = Path(__file__).resolve().parents[3]
SMOKE_SCRIPTS = (
    ROOT / "samples/Gallery/Uno/scripts/smoke.sh",
    ROOT / "samples/SkiaFiddle/scripts/smoke.sh",
)


class UnoPublishTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.site = self.root / "wwwroot"
        self.site.mkdir()
        self.write_publish_fixture()

    def write_publish_fixture(self):
        (self.site / "_framework").mkdir(exist_ok=True)
        (self.site / "index.html").write_text(
            '<script src="/package_hash/uno-bootstrap.js"></script>\n'
            '<link rel="modulepreload" href="/uno-config.js" />\n'
            '<link rel="modulepreload" href="/_framework/dotnet.js" />\n'
        )
        (self.site / "uno-config.js").write_text(
            'config.uno_app_base = "/package_hash";\n'
            "config.offline_files = ['/', \"/uno-config.js\", \"/index.html\", "
            '"/package_hash/uno-bootstrap.js", "/."];\n'
            'config.environmentVariables["UNO_BOOTSTRAP_WEBAPP_BASE_PATH"] = "/";\n'
        )
        (self.site / "service-worker.js").write_text(
            'import { config } from "/uno-config.js";\n'
            "const WEBAPP_PATH = '/';\n"
        )

    def smoke(self, script):
        tools = self.root / "bin"
        tools.mkdir(exist_ok=True)
        for name, status in (("python3", 0), ("node", 0), ("npm", 17)):
            tool = tools / name
            tool.write_text(f"#!/bin/sh\nexit {status}\n")
            tool.chmod(0o755)
        return subprocess.run(
            [shutil.which("bash"), str(script), str(self.site)],
            env={**os.environ, "PATH": f"{tools}{os.pathsep}{os.environ['PATH']}"},
            capture_output=True,
            text=True,
        )

    def test_root_config_reaches_dependency_setup_in_both_harnesses(self):
        for script in SMOKE_SCRIPTS:
            with self.subTest(script=script):
                result = self.smoke(script)
                self.assertEqual(17, result.returncode, result.stderr)
                self.assertNotIn("expected path missing", result.stderr)

    def test_missing_publish_inputs_fail_before_dependency_setup(self):
        for name in ("index.html", "_framework", "uno-config.js"):
            path = self.site / name
            missing = self.site / f"{name}.missing"
            path.rename(missing)
            try:
                for script in SMOKE_SCRIPTS:
                    with self.subTest(input=name, script=script):
                        result = self.smoke(script)
                        self.assertEqual(1, result.returncode, result.stderr)
                        self.assertIn(f"expected path missing: {path}", result.stderr)
            finally:
                missing.rename(path)

    def test_legacy_config_does_not_replace_required_root_config(self):
        (self.site / "uno-config.js").unlink()
        package = self.site / "package_hash"
        package.mkdir()
        (package / "uno-config.js").write_text("legacy config")
        for script in SMOKE_SCRIPTS:
            with self.subTest(script=script):
                result = self.smoke(script)
                self.assertEqual(1, result.returncode, result.stderr)
                self.assertIn("wwwroot/uno-config.js", result.stderr)

    def rewrite(self, prefix):
        workflow = yaml.safe_load(
            (ROOT / ".github/workflows/build-site.yml").read_text()
        )
        assembly = next(
            step["run"] for step in workflow["jobs"]["build"]["steps"]
            if step.get("name") == "Assemble site"
        )
        helpers = assembly[
            assembly.index("uno_rewrite_file() {"):
            assembly.index("uno_rewrite_dir site/gallery-uno")
        ]
        subprocess.run(
            ["bash", "-euo", "pipefail", "-c",
             helpers + '\nuno_rewrite_dir "$1" "$2"',
             "uno-rewrite-test", str(self.site), prefix],
            check=True,
            capture_output=True,
            text=True,
        )

    def test_rewrites_root_config_and_worker_for_main_and_staging(self):
        for prefix in ("/SkiaSharp/gallery-uno", "/SkiaSharp/staging/123/fiddle"):
            with self.subTest(prefix=prefix):
                # Each prefix starts from the same unmodified publish fixture.
                self.write_publish_fixture()
                compressed = [
                    self.site / f"{name}.{extension}"
                    for name in ("index.html", "uno-config.js", "service-worker.js")
                    for extension in ("br", "gz")
                ]
                for path in compressed:
                    path.write_text("stale compressed content")
                unrelated = self.site / "untouched.js.br"
                unrelated.write_text("unchanged")

                self.rewrite(prefix)
                index = (self.site / "index.html").read_text()
                config = (self.site / "uno-config.js").read_text()
                worker = (self.site / "service-worker.js").read_text()
                self.assertIn(f'href="{prefix}/uno-config.js"', index)
                self.assertIn(f'src="{prefix}/package_hash/uno-bootstrap.js"', index)
                self.assertIn(f'href="{prefix}/_framework/dotnet.js"', index)
                self.assertIn(f'uno_app_base = "{prefix}/package_hash"', config)
                self.assertIn(f"offline_files = ['{prefix}/',", config)
                self.assertIn(f'"{prefix}/uno-config.js"', config)
                self.assertIn(f'WEBAPP_BASE_PATH"] = "{prefix}/"', config)
                self.assertIn(f'from "{prefix}/uno-config.js"', worker)
                self.assertIn(f"const WEBAPP_PATH = '{prefix}/';", worker)
                self.assertFalse(any(path.exists() for path in compressed))
                self.assertEqual("unchanged", unrelated.read_text())

                self.rewrite(prefix)
                self.assertEqual(index, (self.site / "index.html").read_text())
                self.assertEqual(config, (self.site / "uno-config.js").read_text())
                self.assertEqual(worker, (self.site / "service-worker.js").read_text())


if __name__ == "__main__":
    unittest.main()
