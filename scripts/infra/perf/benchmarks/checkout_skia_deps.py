#!/usr/bin/env python3
"""Populate Skia's DEPS checkouts by pinned commit, without cloning remote HEAD."""

import ast
import concurrent.futures
import pathlib
import re
import subprocess
import sys


def checkout(skia_path, directory, spec):
    repo, separator, commit = spec.rpartition("@")
    if not separator or not re.fullmatch(r"[0-9a-f]{40}", commit):
        raise ValueError(f"Invalid DEPS pin for {directory}: {spec}")

    path = (skia_path / directory).resolve()
    if not path.is_relative_to(skia_path.resolve()) or path == skia_path.resolve():
        raise ValueError(f"DEPS checkout outside Skia: {directory}")
    if path.exists():
        raise FileExistsError(f"DEPS checkout already exists: {path}")
    path.mkdir(parents=True)
    subprocess.run(["git", "init", "-q", str(path)], check=True)
    subprocess.run(["git", "-C", str(path), "remote", "add", "origin", repo], check=True)
    subprocess.run(["git", "-C", str(path), "fetch", "-q", "--depth=1", "origin", commit], check=True)
    subprocess.run(["git", "-C", str(path), "checkout", "-q", "--detach", commit], check=True)
    print(f"{directory} @ {commit}", flush=True)


def checkout_deps(skia_path):
    assignments = {
        target.id: statement.value
        for statement in ast.parse((skia_path / "DEPS").read_text()).body
        if isinstance(statement, ast.Assign)
        for target in statement.targets
        if isinstance(target, ast.Name)
    }
    variables = ast.literal_eval(assignments["vars"])

    def value(node):
        if isinstance(node, ast.Constant) and isinstance(node.value, str):
            return node.value
        if isinstance(node, ast.BinOp) and isinstance(node.op, ast.Add):
            return value(node.left) + value(node.right)
        if (isinstance(node, ast.Call) and isinstance(node.func, ast.Name)
                and node.func.id == "Var" and len(node.args) == 1 and not node.keywords):
            return variables[value(node.args[0])]
        raise ValueError("Unsupported DEPS dependency expression")

    dependencies = [(value(key), value(spec)) for key, spec in
                    zip(assignments["deps"].keys, assignments["deps"].values)
                    if not isinstance(spec, ast.Dict)
                    and not (isinstance(spec, ast.Constant) and not isinstance(spec.value, str))]
    with concurrent.futures.ThreadPoolExecutor(max_workers=8) as pool:
        futures = {pool.submit(checkout, skia_path, directory, spec): directory
                   for directory, spec in dependencies}
        for future in concurrent.futures.as_completed(futures):
            directory = futures[future]
            try:
                future.result()
            except Exception as error:
                raise RuntimeError(f"Failed to fetch DEPS dependency {directory}") from error


if __name__ == "__main__":
    checkout_deps(pathlib.Path(sys.argv[1]).resolve())
