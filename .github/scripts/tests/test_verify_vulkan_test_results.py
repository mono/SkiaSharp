import importlib.util
import pathlib
import tempfile
import unittest


SCRIPT = pathlib.Path(__file__).parents[1] / "verify-vulkan-test-results.py"
SPEC = importlib.util.spec_from_file_location("verify_vulkan_test_results", SCRIPT)
MODULE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(MODULE)


def trx(*results):
    rows = "\n".join(
        f'<UnitTestResult testName="{name}" outcome="{outcome}" />'
        for name, outcome in results
    )
    return f"""<?xml version="1.0" encoding="utf-8"?>
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
    {rows}
  </Results>
</TestRun>
"""


class VerifyVulkanTestResultsTests(unittest.TestCase):
    def write_trx(self, contents):
        directory = tempfile.TemporaryDirectory()
        self.addCleanup(directory.cleanup)
        path = pathlib.Path(directory.name) / "TestResults.trx"
        path.write_text(contents, encoding="utf-8")
        return path

    def test_accepts_passing_ganesh_and_graphite_results(self):
        path = self.write_trx(
            trx(
                (
                    "SkiaSharp.Vulkan.Tests.GRContextTest.CreateVkContextIsValid",
                    "Passed",
                ),
                (
                    "SkiaSharp.Vulkan.Tests.GraphiteVkBackendContextTest."
                    "GraphiteVkContextIsCreatedFromRawHandles",
                    "Passed",
                ),
                ("SkiaSharp.Vulkan.Tests.SmokeTest.VulkanSmoke", "Skipped"),
            )
        )

        results = MODULE.read_results(path)
        required = MODULE.verify_results(results)

        self.assertEqual(3, len(results))
        self.assertEqual({"Ganesh Vulkan", "Graphite Vulkan"}, set(required))

    def test_rejects_skipped_required_result(self):
        path = self.write_trx(
            trx(
                (
                    "SkiaSharp.Vulkan.Tests.GRContextTest.CreateVkContextIsValid",
                    "Skipped",
                ),
                (
                    "SkiaSharp.Vulkan.Tests.GraphiteVkBackendContextTest."
                    "GraphiteVkContextIsCreatedFromRawHandles",
                    "Passed",
                ),
            )
        )

        with self.assertRaisesRegex(ValueError, "Ganesh Vulkan test did not pass"):
            MODULE.verify_results(MODULE.read_results(path))

    def test_rejects_missing_required_result(self):
        path = self.write_trx(
            trx(
                (
                    "SkiaSharp.Vulkan.Tests.GRContextTest.CreateVkContextIsValid",
                    "Passed",
                ),
            )
        )

        with self.assertRaisesRegex(ValueError, "Graphite Vulkan test is missing"):
            MODULE.verify_results(MODULE.read_results(path))

    def test_rejects_empty_trx(self):
        path = self.write_trx(trx())

        with self.assertRaisesRegex(ValueError, "contains no test results"):
            MODULE.verify_results(MODULE.read_results(path))


if __name__ == "__main__":
    unittest.main()
