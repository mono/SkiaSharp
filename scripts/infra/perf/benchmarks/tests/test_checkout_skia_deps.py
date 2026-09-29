import importlib.util
import pathlib
import subprocess
import tempfile
import unittest
from unittest import mock


SCRIPT = pathlib.Path(__file__).resolve().parents[1] / "checkout_skia_deps.py"
SPEC = importlib.util.spec_from_file_location("checkout_skia_deps", SCRIPT)
MODULE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(MODULE)


class CheckoutSkiaDepsTests(unittest.TestCase):
    def test_parses_real_deps_without_running_it(self):
        skia = SCRIPT.parents[4] / "externals/skia"
        with mock.patch.object(MODULE, "checkout") as checkout:
            MODULE.checkout_deps(skia)
        self.assertGreater(checkout.call_count, 10)

    def test_does_not_execute_deps_code(self):
        with tempfile.TemporaryDirectory() as directory:
            skia = pathlib.Path(directory)
            (skia / "DEPS").write_text(
                "vars = {}\ndeps = {}\nraise RuntimeError('executed')\n")
            MODULE.checkout_deps(skia)

    def create_remote(self, root):
        source = root / "source"
        remote = root / "remote.git"
        subprocess.run(["git", "init", "-q", "-b", "main", str(source)], check=True)
        subprocess.run(["git", "-C", str(source), "-c", "user.name=Test",
                        "-c", "user.email=test@example.com", "commit", "-q",
                        "--allow-empty", "-m", "pinned"], check=True)
        pin = subprocess.check_output(["git", "-C", str(source), "rev-parse", "HEAD"],
                                      text=True).strip()
        subprocess.run(["git", "clone", "-q", "--bare", str(source), str(remote)], check=True)
        return remote, pin

    def test_fetches_pin_without_cloning_broken_default_head(self):
        with tempfile.TemporaryDirectory() as directory:
            root = pathlib.Path(directory)
            remote, pin = self.create_remote(root)
            skia = root / "skia"
            skia.mkdir()
            # Simulate a mirror advertising a default branch whose object is absent.
            (remote / "refs" / "heads" / "master").write_text("f" * 40 + "\n")
            (remote / "HEAD").write_text("ref: refs/heads/master\n")
            (skia / "DEPS").write_text(
                f"vars = {{}}\ndeps = {{'third_party/dep': '{remote.as_uri()}@{pin}'}}\n")

            old_checkout = subprocess.run(
                ["git", "clone", "-q", "--depth=1", "--no-checkout",
                 remote.as_uri(), str(root / "old-checkout")],
                capture_output=True, text=True)
            self.assertNotEqual(0, old_checkout.returncode)
            MODULE.checkout_deps(skia)
            actual = subprocess.check_output(
                ["git", "-C", str(skia / "third_party/dep"), "rev-parse", "HEAD"],
                text=True).strip()
            self.assertEqual(pin, actual)

    def test_rejects_missing_pin(self):
        with tempfile.TemporaryDirectory() as directory:
            root = pathlib.Path(directory)
            remote, _ = self.create_remote(root)
            skia = root / "skia"
            skia.mkdir()
            (skia / "DEPS").write_text(
                f"vars = {{}}\ndeps = {{'third_party/dep': '{remote.as_uri()}@{'f' * 40}'}}\n")

            with self.assertRaisesRegex(RuntimeError, "Failed to fetch DEPS dependency"):
                MODULE.checkout_deps(skia)


if __name__ == "__main__":
    unittest.main()
