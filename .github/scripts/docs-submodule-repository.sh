#!/usr/bin/env bash

set -euo pipefail

gitmodules="${1:-.gitmodules}"

if ! docs_url=$(git config -f "$gitmodules" --get submodule.docs.url); then
  echo "::error::Unable to read submodule.docs.url from $gitmodules." >&2
  exit 1
fi

case "$docs_url" in
  https://github.com/*/*)
    docs_repository=${docs_url#https://github.com/}
    docs_repository=${docs_repository%.git}
    ;;
  *)
    echo "::error::Unsupported docs submodule URL '$docs_url'; expected an HTTPS GitHub URL." >&2
    exit 1
    ;;
esac

if [[ ! "$docs_repository" =~ ^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$ ]]; then
  echo "::error::Unable to derive an owner/repository slug from docs submodule URL '$docs_url'." >&2
  exit 1
fi

echo "repository=$docs_repository"
echo "url=https://github.com/$docs_repository"
