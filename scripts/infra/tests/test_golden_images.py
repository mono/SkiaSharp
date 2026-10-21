import base64
import importlib.util
from pathlib import Path
import struct
import tempfile
import unittest
import zlib

spec = importlib.util.spec_from_file_location("golden_images", Path(__file__).with_name("extract-visual-goldens.py"))
goldens = importlib.util.module_from_spec(spec)
spec.loader.exec_module(goldens)


def png(value):
    def chunk(kind, data):
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data))
    return (goldens.PNG + chunk(b"IHDR", struct.pack(">IIBBBBB", 1, 1, 8, 6, 0, 0, 0)) +
            chunk(b"IDAT", zlib.compress(b"\x00" + bytes([value, 0, 0, 255]))) + chunk(b"IEND", b""))


class GoldenImageTests(unittest.TestCase):
    def test_shared_path_conformance(self):
        fixture = Path(__file__).resolve().parents[3] / "tests/Fixtures/golden-paths.tsv"
        for row in fixture.read_text().splitlines():
            expected, key = row.split("\t")
            with self.subTest(key=key):
                if expected == "valid":
                    goldens.validate_key(key)
                else:
                    with self.assertRaises(ValueError):
                        goldens.validate_key(key)

    def test_conflicting_captures_never_choose_last_writer(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "run.trx"
            key = "mac/12.6/blazor-cpu.x64.png"
            path.write_text("\n".join(
                f"##SKIA-GOLDEN-V2## role=actual key={key} selected={key} size=1x1 base64={base64.b64encode(png(v)).decode()}"
                for v in (0, 1)))
            with self.assertRaisesRegex(ValueError, "Conflicting"):
                goldens.proposals([path])

    def test_legacy_capture_cannot_seed_new_identity(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "run.trx"
            path.write_text(f"##SKIA-VISUAL-ACTUAL## path=raster.macos/Text.png size=1x1 base64={base64.b64encode(png(0)).decode()}")
            self.assertEqual({}, goldens.proposals([path]))
            self.assertEqual(1, goldens.export_images([path], Path(folder) / "images"))

    def test_source_packaged_assets_must_match(self):
        with tempfile.TemporaryDirectory() as folder:
            source, output = Path(folder) / "source", Path(folder) / "output"
            key = "mac/base/blazor-cpu.png"
            for root, value in ((source, 0), (output, 1)):
                file = root / key
                file.parent.mkdir(parents=True)
                file.write_bytes(png(value))
            with self.assertRaisesRegex(ValueError, "Packaged asset"):
                goldens.validate_assets(source, output)


if __name__ == "__main__":
    unittest.main()
