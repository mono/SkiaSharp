import base64
import importlib.util
from pathlib import Path
import shutil
import struct
import sys
import tempfile
import unittest
import zlib

spec = importlib.util.spec_from_file_location("golden_images", Path(__file__).with_name("extract-visual-goldens.py"))
goldens = importlib.util.module_from_spec(spec)
spec.loader.exec_module(goldens)


def png(value=0):
    def chunk(kind, data):
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data))
    return (goldens.PNG + chunk(b"IHDR", struct.pack(">IIBBBBB", 1, 1, 8, 6, 0, 0, 0)) +
            chunk(b"IDAT", zlib.compress(b"\x00" + bytes([value, 0, 0, 255]))) + chunk(b"IEND", b""))


def marker(key="Visual/mac/base/visual-text.raster.arm64.png", role="actual", data=None):
    return (f"##SKIA-GOLDEN## role={role} key={key} size=1x1 "
            f"base64={base64.b64encode(png() if data is None else data).decode()}")


class GoldenImageTests(unittest.TestCase):
    def test_safe_canonical_paths(self):
        for key in ("Samples/mac/base/console.png", "Visual/portable/base/visual-text.raster.png",
                    "Samples/ubuntu/22.04.5/sample.firefox.x64.png"):
            goldens.validate_key(key)
        for key in ("../Visual/mac/base/sample.png", "/Visual/mac/base/sample.png",
                    "Visual/mac/../sample.png", "Visual/mac/latest/sample.png",
                    "Visual/mac/base/con.png", "Samples/Mac/base/sample.png",
                    "Visual/mac/base/sample..png", "Visual/mac/base/sample.a.b.c.png",
                    "Visual\\mac\\base\\sample.png", "Visual/mac/2147483648/sample.png",
                    "Visual/mac/base/sample.x64.firefox.png", "Visual/mac/base/sample.x64.arm64.png",
                    "Visual/mac/base/sample.firefox.unknown.png"):
            with self.subTest(key=key), self.assertRaises(ValueError):
                goldens.validate_key(key)

    def test_png_integrity_and_marker_dimensions(self):
        self.assertEqual((1, 1), goldens.png_size(png()))
        for data in (b"not png", png()[:24], png()[:-1], png() + b"extra", png()[:40] + b"bad" + png()[43:]):
            with self.subTest(data=data), self.assertRaises(ValueError):
                goldens.png_size(data)
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "run.trx"
            path.write_text(marker().replace("size=1x1", "size=2x1"))
            with self.assertRaisesRegex(ValueError, "dimensions"):
                list(goldens.records(path))

    def test_malformed_records_fail_but_failure_prose_is_not_a_record(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "run.trx"
            path.write_text("##SKIA-GOLDEN## role=actual key=invalid")
            with self.assertRaisesRegex(ValueError, "Malformed"):
                list(goldens.records(path))
            path.write_text(marker() + "\nAssertion failed: PNG retained as a ##SKIA-GOLDEN## role=actual marker.")
            self.assertEqual(1, len(goldens.proposals([path])))

    def test_conflicting_captures_fail_instead_of_choosing_last_writer(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "run.trx"
            path.write_text(marker(data=png(0)) + "\n" + marker(data=png(1)))
            with self.assertRaisesRegex(ValueError, "Conflicting"):
                goldens.proposals([path])

    def test_diagnostics_keep_roles_retries_and_runs_separate(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            files = []
            for name in ("first", "second"):
                path = root / name / "run.trx"
                path.parent.mkdir()
                path.write_text("\n".join(marker(role=role) for role in ("actual", "reference", "diff", "actual")))
                files.append(path)
            output = root / "images"
            self.assertEqual(8, goldens.export_images(files, output))
            self.assertEqual(8, len(list(output.rglob("*.png"))))
            self.assertEqual(2, len(list(output.iterdir())))

    def test_staging_never_overwrites_a_reference_and_dry_run_writes_nothing(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            path = root / "run.trx"
            path.write_text(marker())
            output = root / "proposals"
            self.assertEqual(0, goldens.main([str(path), "--out", str(output), "--dry-run"]))
            self.assertFalse(output.exists())
            self.assertEqual(0, goldens.main([str(path), "--out", str(output)]))
            path.write_text(marker(data=png(1)))
            self.assertEqual(1, goldens.main([str(path), "--out", str(output)]))
            self.assertEqual(png(), next(output.rglob("*.png")).read_bytes())

    @unittest.skipIf(sys.platform == "win32", "Creating symlinks requires Windows host privileges")
    def test_output_symlinks_cannot_escape_destination(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            destination = root / "output"
            outside = root / "outside"
            destination.mkdir()
            outside.mkdir()
            (destination / "Visual").symlink_to(outside, target_is_directory=True)
            with self.assertRaisesRegex(ValueError, "escapes"):
                goldens.contained(destination, "Visual/mac/base/sample.png")

    @unittest.skipIf(sys.platform == "win32", "Creating symlinks requires Windows host privileges")
    def test_final_diagnostic_filename_and_run_directory_cannot_escape(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            path = root / "run.trx"
            path.write_text(marker())
            output = root / "images"
            goldens.export_images([path], output)
            image = next(output.rglob("*.png"))
            image.unlink()
            outside = root / "outside.png"
            outside.write_bytes(b"must not change")
            image.symlink_to(outside)
            with self.assertRaisesRegex(ValueError, "escapes"):
                goldens.export_images([path], output)
            self.assertEqual(b"must not change", outside.read_bytes())
            image.unlink()
            run = next(output.iterdir())
            shutil.rmtree(run)
            external_run = root / "external-run"
            external_run.mkdir()
            run.symlink_to(external_run, target_is_directory=True)
            with self.assertRaisesRegex(ValueError, "escapes"):
                goldens.export_images([path], output)
            self.assertEqual([], list(external_run.iterdir()))


if __name__ == "__main__":
    unittest.main()
