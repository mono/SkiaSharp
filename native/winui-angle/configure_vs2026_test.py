import pathlib
import tempfile
import unittest

from configure_vs2026 import configure


class ConfigureTests(unittest.TestCase):
    def test_pins_compiler_and_ignores_v145_redist(self):
        with tempfile.TemporaryDirectory() as directory:
            root = pathlib.Path(directory)
            angle = root / "angle"
            vs = root / "vs"
            files = {
                angle / "build/toolchain/win/setup_toolchain.py":
                    "    args.append(SDK_VERSION)\n    variables = _LoadEnvFromBat(args)\n",
                angle / "build/vs_toolchain.py": "  return FindVCComponentRoot('Redist')\n",
                vs / "VC/Tools/MSVC/14.44.35207/bin/Hostx64/arm64/cl.exe": "",
                vs / "VC/Redist/MSVC/14.44.35207/arm64/Microsoft.VC143.CRT/msvcp140.dll": "",
                vs / "VC/Redist/MSVC/14.51.36231/arm64/Microsoft.VC145.CRT/msvcp140.dll": "",
            }
            for path, contents in files.items():
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(contents)
            configure(angle, vs, "14.44.35207")
            configure(angle, vs, "14.44.35207")
            self.assertIn("-vcvars_ver=14.44.35207", next(iter(files)).read_text())
            runtime = (angle / "build/vs_toolchain.py").read_text()
            self.assertIn("14.44.35207", runtime)
            self.assertNotIn("14.51", runtime)
            (angle / "build/toolchain/win/setup_toolchain.py").write_text("changed upstream")
            with self.assertRaisesRegex(RuntimeError, "toolchain anchor"):
                configure(angle, vs, "14.44.35207")

    def test_missing_compiler_fails(self):
        with tempfile.TemporaryDirectory() as directory:
            with self.assertRaisesRegex(RuntimeError, "Missing selected v143 compiler"):
                configure(pathlib.Path(directory), pathlib.Path(directory), "14.44.35207")


if __name__ == "__main__":
    unittest.main()
