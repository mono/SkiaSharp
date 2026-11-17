import contextlib
import io
import json
import stat
import tempfile
import unittest
from pathlib import Path
from unittest import mock

import run_devskim
from check_devskim import BANNED_FUNCTION_TAG, banned_findings, main


def report(results):
    return {
        "version": "2.1.0",
        "runs": [{"tool": {"driver": {"name": "devskim"}}, "results": results}],
    }


def finding(tags=None, level="warning", uri=None, line=1, text="rewind"):
    result = {
        "ruleId": "DS154189",
        "level": level,
        "message": {"text": "Banned C function detected"},
        "properties": {"tags": [BANNED_FUNCTION_TAG] if tags is None else tags},
    }
    if uri:
        result["locations"] = [{
            "physicalLocation": {
                "artifactLocation": {"uri": uri},
                "region": {"startLine": line, "snippet": {"text": text}},
            },
        }]
    return result


class GateTests(unittest.TestCase):
    def test_clean_report(self):
        self.assertEqual([], banned_findings(report([])))

    def test_all_severities_are_gated(self):
        for level in ["error", "warning", "note", "none"]:
            with self.subTest(level=level):
                result = finding(level=level)
                self.assertEqual([result], banned_findings(report([result])))

    def test_other_findings_are_not_banned_functions(self):
        self.assertEqual([], banned_findings(report([finding(["Other.Tag"])])))

    def test_multiple_runs(self):
        document = report([])
        document["runs"] += report([finding()])["runs"]
        self.assertEqual(1, len(banned_findings(document)))

    def test_suppression_does_not_bypass_gate(self):
        result = finding()
        result["suppressions"] = [{"kind": "inSource", "status": "accepted"}]
        self.assertEqual([result], banned_findings(report([result])))

    def test_malformed_reports_fail(self):
        documents = [
            {},
            {"version": "2.1.0", "runs": []},
            {"version": "2.1.0", "runs": [{}]},
            {"version": "2.1.0", "runs": [{"tool": None}]},
            report(None),
            report([{}]),
            report([{"ruleId": "DS154189", "properties": {}}]),
            report([{"ruleId": "DS154189", "properties": None}]),
            report([finding(tags="not-a-list")]),
        ]
        for document in documents:
            with self.subTest(document=document), self.assertRaises(ValueError):
                banned_findings(document)

    def test_execution_errors_fail(self):
        for invocation in [
            {"executionSuccessful": False},
            {"executionSuccessful": True, "toolExecutionNotifications": [{"level": "error"}]},
        ]:
            document = report([])
            document["runs"][0]["invocations"] = [invocation]
            with self.subTest(invocation=invocation), self.assertRaises(ValueError):
                banned_findings(document)

    def test_exit_codes(self):
        with tempfile.TemporaryDirectory() as directory:
            directory = Path(directory)
            path = directory / "report.sarif"
            for document, expected in [(report([]), 0), (report([finding(uri="a.c")]), 1), ({}, 2)]:
                path.write_text(json.dumps(document), encoding="utf-8")
                with contextlib.redirect_stdout(io.StringIO()), contextlib.redirect_stderr(io.StringIO()):
                    self.assertEqual(expected, main([str(path)]))
            for contents in ["", "{not json"]:
                path.write_text(contents, encoding="utf-8")
                with contextlib.redirect_stderr(io.StringIO()) as errors:
                    self.assertEqual(2, main([str(path)]))
                self.assertIn("DevSkim report error", errors.getvalue())

            # A missing report fails even when the other reports are valid.
            missing = directory / "missing.sarif"
            path.write_text(json.dumps(report([])), encoding="utf-8")
            with contextlib.redirect_stdout(io.StringIO()), contextlib.redirect_stderr(io.StringIO()) as errors:
                self.assertEqual(2, main([str(path), str(missing)]))
            self.assertIn(str(missing), errors.getvalue())

    def test_findings_are_printed_with_their_location(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "report.sarif"
            path.write_text(json.dumps(report([finding(uri="runtime/example.c", line=42, text="printf")])), encoding="utf-8")
            with contextlib.redirect_stdout(io.StringIO()) as output:
                self.assertEqual(1, main([str(path)]))
            self.assertIn("runtime/example.c:42: DS154189: Banned C function detected (printf)", output.getvalue())


class ExceptionTests(unittest.TestCase):
    EXCEPTION = {
        "path": "src/stream.cpp",
        "rule": "DS154189",
        "text": "return stream->rewind();",
        "reason": "Member function.",
    }

    def run_gate(self, source, result, exceptions):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "src").mkdir()
            for name in ["stream.cpp", "other.cpp"]:
                (root / "src" / name).write_text(source, encoding="utf-8")
            (root / "report.sarif").write_text(json.dumps(report([result])), encoding="utf-8")
            (root / "exceptions.json").write_text(json.dumps({"exceptions": exceptions}), encoding="utf-8")
            with contextlib.redirect_stdout(io.StringIO()), contextlib.redirect_stderr(io.StringIO()):
                return main(["--root", str(root), "--exceptions", str(root / "exceptions.json"), str(root / "report.sarif")])

    def test_exception_excuses_the_exact_line(self):
        source = "int a;\n    return stream->rewind();  \n"
        self.assertEqual(0, self.run_gate(source, finding(uri="src/stream.cpp", line=2), [self.EXCEPTION]))

    def test_exception_excuses_nothing_else(self):
        source = "int a;\n    return stream->rewind();\nrewind(file);\n"
        cases = {
            "other line": (finding(uri="src/stream.cpp", line=3), [self.EXCEPTION]),
            "line past the end": (finding(uri="src/stream.cpp", line=9), [self.EXCEPTION]),
            "no line": (finding(uri="src/stream.cpp", line=0), [self.EXCEPTION]),
            "other file": (finding(uri="src/other.cpp", line=2), [self.EXCEPTION]),
            "other rule": (dict(finding(uri="src/stream.cpp", line=2), ruleId="DS185832"), [self.EXCEPTION]),
            "no exception": (finding(uri="src/stream.cpp", line=2), []),
        }
        for name, (result, exceptions) in cases.items():
            with self.subTest(name):
                self.assertEqual(1, self.run_gate(source, result, exceptions))

    def test_invalid_exceptions_fail(self):
        source = "return stream->rewind();\n"
        for exception in [dict(self.EXCEPTION, reason=" "), {k: v for k, v in self.EXCEPTION.items() if k != "text"}]:
            with self.subTest(exception=exception):
                self.assertEqual(2, self.run_gate(source, finding(uri="src/stream.cpp"), [exception]))


# Stands in for DevSkim: counts the files of the tree, and flags every strcpy in a .c file.
STUB = """\
#!/usr/bin/env python3
import json, os, sys

args = sys.argv[1:]
option = lambda name: args[args.index(name) + 1]
source, base, output = option("--source-code"), option("--base-path"), option("--output-file")
if not os.path.isdir(source):
    print("[ERR] Error: Not a valid file or directory " + source)
    sys.exit(254)

files = [os.path.join(folder, name) for folder, _, names in os.walk(source) for name in names]
results = []
for path in files:
    if path.endswith(".c") and not os.environ.get("STUB_BLIND"):
        for number, line in enumerate(open(path).read().split("\\n"), 1):
            if "strcpy" in line:
                results.append({
                    "ruleId": "DS185832",
                    "message": {"text": "Banned C function detected (strcpy)"},
                    "properties": {"tags": ["API.DangerousAPI.BannedFunction"]},
                    "locations": [{"physicalLocation": {
                        "artifactLocation": {"uri": os.path.relpath(path, base)},
                        "region": {"startLine": number, "snippet": {"text": "strcpy"}},
                    }}],
                })
run = {"tool": {"driver": {"name": "devskim"}}, "results": results}
json.dump({"version": "2.1.0", "runs": [run]}, open(output, "w"))
print("[DBG] Files analyzed: %d" % len(files))
"""


class RunTests(unittest.TestCase):
    def test_analyzed_files(self):
        self.assertEqual(9, run_devskim.analyzed_files("[15:10:59 DBG] Issues found: 1 in 1 files\n[15:10:59 DBG] Files analyzed: 9\n"))
        with self.assertRaises(run_devskim.ScanError):
            run_devskim.analyzed_files("nothing useful")

    def run_scan(self, files, trees=("tree",), blind=False, empty_directories=()):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            stub = root / "devskim"
            stub.write_text(STUB, encoding="utf-8")
            stub.chmod(stub.stat().st_mode | stat.S_IEXEC)
            for name, contents in files.items():
                path = root / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(contents, encoding="utf-8")
            for name in empty_directories:
                (root / name).mkdir()
            environment = {"STUB_BLIND": "1"} if blind else {}
            with mock.patch.dict("os.environ", environment):
                with contextlib.redirect_stdout(io.StringIO()), contextlib.redirect_stderr(io.StringIO()) as errors:
                    code = run_devskim.run(str(stub), root, root / "reports", list(trees), None)
            return code, errors.getvalue(), sorted(path.name for path in (root / "reports").iterdir())

    def test_clean_scan(self):
        code, _, reports = self.run_scan({"tree/a.c": "int a;\n"})
        self.assertEqual(0, code)
        self.assertEqual(["control.log", "control.sarif", "tree.log", "tree.sarif"], reports)

    def test_banned_function_fails(self):
        code, _, _ = self.run_scan({"tree/a.c": "strcpy(a, b);\n"})
        self.assertEqual(1, code)

    def test_empty_tree_fails(self):
        code, errors, _ = self.run_scan({"other/a.c": "int a;\n"}, empty_directories=["tree"])
        self.assertEqual(2, code)
        self.assertIn("analyzed no files", errors)

    def test_missing_tree_fails(self):
        code, errors, _ = self.run_scan({"tree/a.c": "int a;\n"}, trees=("tree", "missing"))
        self.assertEqual(2, code)
        self.assertIn("missing", errors)

    def test_scanner_that_flags_nothing_fails(self):
        code, errors, _ = self.run_scan({"tree/a.c": "strcpy(a, b);\n"}, blind=True)
        self.assertEqual(2, code)
        self.assertIn("control file", errors)


if __name__ == "__main__":
    unittest.main()
