#!/usr/bin/env python3
"""Exercise a running Debug gallery through one MAUI DevFlow batch session."""

import argparse
import json
import queue
import re
import subprocess
import threading
import time
from pathlib import Path


class DevFlow:
    def __init__(self, port, output, device):
        self.log = (output / "devflow.log").open("w", encoding="utf-8")
        self.transcript = (output / "commands.jsonl").open("w", encoding="utf-8")
        command = [
            "maui", "devflow", "batch", "--agent-port", str(port),
            "--delay", "150", "--continue-on-error",
        ]
        if device:
            command.extend(["--device", device])
        self.process = subprocess.Popen(
            command,
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            stderr=self.log,
            text=True,
            bufsize=1,
        )
        self.responses = queue.Queue()
        self.reader = threading.Thread(target=self.read_responses, daemon=True)
        self.reader.start()

    def read_responses(self):
        for line in self.process.stdout:
            self.responses.put(line)
        self.responses.put(None)

    def call(self, *args):
        command = "MAUI " + " ".join(json.dumps(str(arg)) for arg in args)
        self.process.stdin.write(command + "\n")
        self.process.stdin.flush()
        try:
            line = self.responses.get(timeout=45)
        except queue.Empty as error:
            raise TimeoutError(f"DevFlow timed out: {command}") from error
        if not line:
            raise RuntimeError(f"DevFlow exited while running: {command}")
        self.transcript.write(line)
        self.transcript.flush()
        result = json.loads(line)
        if result["exit_code"] != 0:
            raise RuntimeError(f"{command}: {result['output']}")
        # The CLI can append a human-readable target diagnostic after the JSON.
        data, _ = json.JSONDecoder().raw_decode(result["output"].lstrip())
        if isinstance(data, dict) and (data.get("success") is False or "error" in data):
            raise RuntimeError(f"{command}: {data}")
        return data

    def query(self, automation_id):
        for attempt in range(5):
            try:
                return self.call(
                    "query", "--automationId", automation_id,
                )
            except RuntimeError as error:
                if "The UI kept changing while DevFlow was reading it" not in str(error) or attempt == 4:
                    raise
                time.sleep(0.1)

    def wait(self, automation_id, predicate=lambda item: item.get("isVisible"), timeout=15):
        deadline = time.monotonic() + timeout
        last = []
        while time.monotonic() < deadline:
            last = self.query(automation_id)
            # Navigation retains prior pages; prefer the instance in the native window.
            for item in sorted(last, key=lambda entry: "windowBounds" not in entry):
                if predicate(item):
                    return item
            time.sleep(0.1)
        raise AssertionError(f"Timed out waiting for {automation_id}: {last}")

    def value(self, automation_id, name):
        target = self.wait(automation_id)
        return self.call("property", target["id"], name)["value"]

    def visible(self, automation_id):
        return any(
            item.get("isVisible")
            and item.get("bounds", {}).get("width", 0) > 0
            and item.get("bounds", {}).get("height", 0) > 0
            for item in self.query(automation_id)
        )

    def wait_gone(self, automation_id, timeout=15):
        deadline = time.monotonic() + timeout
        while time.monotonic() < deadline:
            if not self.query(automation_id):
                return
            time.sleep(0.1)
        raise AssertionError(f"{automation_id} did not disappear")

    def set(self, automation_id, name, value):
        target = self.wait(automation_id)
        self.call("set-property", target["id"], name, str(value).lower() if isinstance(value, bool) else value)

    def tap(self, automation_id):
        target = self.wait(automation_id, lambda item: item.get("isVisible") and item.get("isEnabled"))
        self.call("tap", target["id"])

    def screenshot(self, path):
        self.call("screenshot", "--output", path, "--overwrite")

    def close(self):
        self.process.stdin.close()
        try:
            self.process.wait(timeout=5)
        except subprocess.TimeoutExpired:
            self.process.terminate()
            self.process.wait(timeout=5)
        self.log.close()
        self.transcript.close()


def stable_id(prefix, title):
    return prefix + "".join(c if c.isascii() and c.isalnum() else "-" for c in title.lower())


def run_checks(flow, output, all_samples, share, report):
    status = flow.call("status")
    assert status["app"]["packageId"] == "com.skiasharp.gallery.maui", "Refusing to drive a different app"
    report["app"] = status

    def checked(message):
        report["checks"].append(message)
        print(f"PASS {message}", flush=True)

    def back():
        flow.tap("sample-back")
        flow.wait_gone("sample-title")
        flow.wait("gallery-result-count")

    def search_id():
        return "gallery-search" if flow.visible("gallery-search") else "gallery-filter-search"

    def clear_filters():
        if flow.visible("gallery-clear"):
            flow.tap("gallery-clear")

    def open_filters():
        if not flow.visible("gallery-filter-sidebar"):
            flow.tap("gallery-filter-trigger")
            flow.wait("gallery-filter-popup")

    def close_filters():
        if flow.visible("gallery-filter-popup"):
            flow.tap("gallery-filter-done")
            flow.wait_gone("gallery-popup")

    def select_tag(tag):
        tag_id = stable_id("tag-", tag)
        flow.call("scroll", "--element", tag_id)
        flow.tap(tag_id)

    if flow.visible("sample-back"):
        back()
    if flow.visible("gallery-popup-scrim"):
        flow.tap("gallery-popup-scrim")
        flow.wait_gone("gallery-popup")
    clear_filters()
    count = flow.wait("gallery-result-count")["text"]
    shown, total = map(int, re.findall(r"\d+", count))
    assert shown == total and total > 0, count
    checked(f"Catalog contains {total} samples")
    flow.screenshot(output / "home.png")
    flow.tap("gallery-info-trigger")
    flow.wait("gallery-info-menu")
    for field in ("skiasharp-version", "harfbuzz-version", "build-timestamp", "build-footer"):
        assert flow.wait(f"gallery-info-{field}")["text"]
    flow.screenshot(output / "gallery-info.png")
    flow.tap("gallery-info-close")
    flow.wait_gone("gallery-popup")
    assert not flow.query("gallery-footer")
    checked("Header Info popup contains build metadata without a permanent footer")

    assert not flow.query("gallery-filter-api-search")
    assert not flow.query("gallery-filter-popup-api-search")
    if status["device"]["platform"] == "MacCatalyst":
        assert flow.wait(search_id())["type"] == "GallerySearchEntry"
        assert float(flow.value(search_id(), "Parent.Parent.StrokeThickness")) == 1
    flow.call("fill", search_id(), "Lottie Player")
    assert flow.wait("gallery-result-count")["text"].startswith("1 of ")
    open_filters()
    assert flow.value("category-general-count", "Text") == "1"
    assert not flow.query("category-documents")
    assert not flow.query("category-text---typography")
    assert not flow.query("tag-skshader")
    if status["device"]["platform"] == "MacCatalyst":
        assert flow.value("tag-animation", "FontFamily") == "Menlo"
    select_tag("Animation")
    close_filters()
    flow.call("fill", search_id(), "__no_such_gallery_sample__")
    flow.wait("gallery-empty")
    assert flow.visible("gallery-active-tag-animation")
    open_filters()
    assert not flow.query("category-general")
    assert not flow.query("tag-animation")
    close_filters()
    clear_filters()
    checked("One global search updates facet counts, hides empty options, and preserves removable filters")

    flow.call("fill", search_id(), "__no_such_gallery_sample__")
    flow.wait("gallery-empty")
    assert flow.wait("gallery-result-count")["text"].startswith("0 of ")
    clear_filters()
    open_filters()
    flow.tap("category-shaders---effects")
    category_count = int(flow.wait("gallery-result-count")["text"].split()[0])
    assert 0 < category_count < total
    flow.tap("category-all")
    select_tag("SKCanvas")
    first_tag_count = int(flow.wait("gallery-result-count")["text"].split()[0])
    select_tag("SKPaint")
    all_count = int(flow.wait("gallery-result-count")["text"].split()[0])
    assert 0 <= all_count <= first_tag_count <= total
    assert not flow.query("gallery-tag-mode")
    assert not flow.query("gallery-type-more")
    assert not flow.query("gallery-method-more")
    close_filters()
    flow.tap("gallery-sort-trigger")
    flow.wait("gallery-sort-menu")
    flow.tap("gallery-sort-alphabetical")
    flow.wait_gone("gallery-popup")
    assert flow.value("gallery-sort-trigger", "Text") == "A to Z"
    clear_filters()
    checked("Search, compact categories/tags, ALL-tag intersection, sort popover, and reset")

    flow.tap("gallery-header-theme")
    flow.tap("gallery-theme-light")
    flow.wait_gone("gallery-popup")
    before = flow.value("gallery-result-count", "TextColor")
    flow.tap("gallery-header-theme")
    flow.tap("gallery-theme-dark")
    flow.wait_gone("gallery-popup")
    assert flow.value("gallery-result-count", "TextColor") != before
    flow.screenshot(output / "home-theme.png")
    flow.tap("gallery-header-theme")
    flow.tap("gallery-theme-system")
    flow.wait_gone("gallery-popup")
    flow.tap("gallery-header-theme")
    assert flow.wait("gallery-theme-system")["text"].lstrip().startswith("✓")
    flow.tap("gallery-popup-scrim")
    flow.wait_gone("gallery-popup")
    checked("Glyph theme menu, readable light/dark resources, and return to OS System mode")

    if status["device"]["idiom"] == "Desktop":
        flow.call("resize", 440, 820)
        flow.wait("gallery-filter-trigger")
        flow.tap("gallery-filter-trigger")
        panel = flow.wait("gallery-popup-panel")
        assert panel["bounds"]["width"] > 0 and panel["bounds"]["height"] > 0
        flow.screenshot(output / "narrow-filters.png")
        flow.tap("gallery-popup-scrim")
        flow.wait_gone("gallery-popup")
        flow.tap("gallery-filter-trigger")
        flow.wait("gallery-filter-popup")
        flow.call("resize", 1200, 850)
        flow.wait_gone("gallery-popup")
        flow.wait("gallery-filter-sidebar")
        checked("Narrow funnel overlay, backdrop dismissal, and wide sidebar restoration on resize")

    def open_sample(title):
        flow.call("fill", search_id(), title)
        card = flow.wait(stable_id("sample-", title))
        if not card["isEnabled"]:
            report["unsupported"].append(title)
            return False
        flow.tap(card["id"])
        flow.wait("sample-title", lambda item: item.get("text") == title)
        return True

    def backend(name):
        flow.tap(f"gallery-header-{name}")
        rendered = flow.wait(
            "sample-render-status",
            lambda item: item.get("text", "").startswith(name.upper()) and "device px" in item["text"],
        )
        canvas = flow.wait(f"sample-canvas-{name}")
        bounds = canvas["bounds"]
        assert bounds["width"] > 0 and bounds["height"] > 0, canvas
        return rendered["text"]

    assert open_sample("Gradient")
    backend("cpu")
    if status["device"]["platform"] == "MacCatalyst":
        assert flow.value("sample-tag-skcanvas", "Content.FontFamily") == "Menlo"
    if status["device"]["idiom"] == "Desktop":
        original_width = flow.wait("sample-canvas-cpu")["bounds"]["width"]
        flow.tap("sample-controls-toggle")
        expanded_canvas = flow.wait(
            "sample-canvas-cpu",
            lambda item: item.get("bounds", {}).get("width", 0) >= original_width + 250,
        )
        assert not flow.visible("sample-controls")
        flow.screenshot(output / "controls-collapsed-wide.png")
        flow.call("resize", 440, 820)
        toggle = flow.wait("sample-controls-toggle")
        canvas = flow.wait("sample-canvas-cpu")
        assert toggle["bounds"]["width"] >= canvas["bounds"]["width"] - 32
        flow.tap("sample-controls-toggle")
        flow.wait("sample-controls")
        flow.tap("sample-controls-toggle")
        flow.call("resize", 1200, 850)
        flow.wait("sample-canvas-cpu", lambda item: item.get("bounds", {}).get("width", 0) >= expanded_canvas["bounds"]["width"] - 2)
        flow.tap("sample-controls-toggle")
        flow.wait("sample-controls")
        flow.wait("sample-canvas-cpu", lambda item: item.get("bounds", {}).get("width", 0) < expanded_canvas["bounds"]["width"] - 200)
        checked("Controls collapse reclaims the wide column and survives stacked layout changes")
    flow.tap("gallery-info-trigger")
    flow.wait("gallery-info-menu")
    flow.tap("gallery-info-close")
    flow.wait_gone("gallery-popup")
    flow.set("control-angle", "Value", 91.4)
    assert float(flow.value("control-angle", "Value")) == 91
    flow.set("control-gradienttype", "SelectedIndex", 2)
    assert int(flow.value("control-gradienttype", "SelectedIndex")) == 2
    flow.screenshot(output / "gradient-cpu.png")
    backend("gpu")
    flow.screenshot(output / "gradient-gpu.png")
    backend("cpu")
    checked("Stepped live slider, picker, and CPU/GPU/CPU surface replacement")
    back()
    assert flow.value(search_id(), "Text") == "Gradient"
    checked("Back navigation preserves search state")

    assert open_sample("Photo Lab")
    backend("gpu")
    flow.set("control-contrast", "IsToggled", True)
    flow.set("control-contrast-amount", "Value", 0.53)
    assert abs(float(flow.value("control-contrast-amount", "Value")) - 0.55) < 0.0001
    flow.set("control-contrast", "IsToggled", False)
    flow.set("control-contrast", "IsToggled", True)
    assert abs(float(flow.value("control-contrast-amount", "Value")) - 0.55) < 0.0001
    flow.screenshot(output / "photo-lab.png")
    checked("Nested effect groups retain stepped child values")
    back()

    assert open_sample("Nine-Patch Scaler")
    backend("cpu")
    assert float(flow.value("control-width", "Value")) == 400
    assert float(flow.value("control-height", "Value")) == 300
    flow.set("control-width", "Value", 413)
    assert float(flow.value("control-width", "Value")) == 410
    checked("XAML slider range initialization preserves sample defaults and live stepping")
    back()

    assert open_sample("Color Fonts")
    assert float(flow.value("control-palette", "Maximum")) > 0
    flow.set("control-palette", "Value", 2)
    backend("gpu")
    checked("Controls use font metadata loaded during initialization")
    back()

    assert open_sample("Lottie Player")
    backend("gpu")
    flow.set("control-playing", "IsToggled", False)
    flow.set("control-playing", "IsToggled", True)
    backend("cpu")
    back()
    assert open_sample("Lottie Player")
    backend("gpu")
    checked("Animated playback, pause/resume, renderer switching, and reopening")
    back()

    if all_samples:
        source = Path(__file__).resolve().parents[2] / "Shared" / "Samples"
        titles = []
        for path in sorted(source.glob("*.cs")):
            match = re.search(r'override\s+string\s+Title\s*=>\s*"([^"]+)"', path.read_text(encoding="utf-8-sig"))
            if match:
                titles.append(match[1])
        assert len(titles) == total, "Smoke catalog does not match the running app's catalog"
        for title in titles:
            if not open_sample(title):
                continue
            if title not in ("PDF Composer", "Create XPS Document"):
                backend("cpu")
                backend("gpu")
            else:
                flow.wait("sample-state", lambda item: item.get("text", "").startswith("Document generated."))
            if title in ("Shader Playground", "Lottie Player", "Color Fonts"):
                flow.screenshot(output / (stable_id("", title) + ".png"))
            checked(f"Render and navigate: {title}")
            back()

    assert open_sample("PDF Composer")
    flow.wait("sample-state", lambda item: item.get("text", "").startswith("Document generated."))
    flow.set("control-pagesize", "SelectedIndex", 2)
    flow.wait("sample-state", lambda item: item.get("text", "").startswith("Document generated."))
    assert str(flow.value("sample-open", "IsEnabled")).lower() == "true"
    assert str(flow.value("sample-share", "IsEnabled")).lower() == "true"
    flow.screenshot(output / "pdf-composer.png")
    checked("PDF generation, regeneration, and native export actions")
    if share:
        flow.tap("sample-share")
        flow.screenshot(output / "native-share.png")
        state = flow.wait("sample-render-status")["text"]
        assert not state.startswith("Export failed"), state
        report["shareRequested"] = True
    else:
        back()
        clear_filters()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--port", type=int, required=True, help="Exact gallery DevFlow port from 'maui devflow list'")
    parser.add_argument("--device", help="Explicit simulator UDID or Android serial when several devices are running")
    parser.add_argument("--output", type=Path, default=Path("output/maui-gallery-smoke"))
    parser.add_argument("--all-samples", action="store_true", help="Render every supported catalog sample on CPU and GPU")
    parser.add_argument("--share", action="store_true", help="Request PDF sharing last; leaves the native share sheet open")
    args = parser.parse_args()
    args.output = args.output.resolve()
    args.output.mkdir(parents=True, exist_ok=True)
    report = {"passed": False, "checks": [], "unsupported": []}
    flow = DevFlow(args.port, args.output, args.device)
    try:
        run_checks(flow, args.output, args.all_samples, args.share, report)
        report["passed"] = True
    except (AssertionError, RuntimeError, TimeoutError, ValueError, OSError) as error:
        report["error"] = str(error)
        print(f"FAIL {error}", flush=True)
    finally:
        flow.close()
        (args.output / "report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    raise SystemExit(0 if report["passed"] else 1)


if __name__ == "__main__":
    main()
