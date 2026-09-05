#!/usr/bin/env bash

set -euo pipefail

gitmodules="${1:-.gitmodules}"

if ! docs_url=$(
  git config -f "$gitmodules" --get submodule.docs.url || exit $?
  printf '\034'
); then
  echo "::error::Unable to read submodule.docs.url from $gitmodules." >&2
  exit 1
fi
docs_url=${docs_url%$'\034'}
docs_url=${docs_url%$'\n'}

github_prefix="https://github.com/"
if [[ "$docs_url" != "$github_prefix"* ]]; then
  echo "::error::Unsupported docs submodule URL; expected https://github.com/owner/repository." >&2
  exit 1
fi

docs_repository=${docs_url#"$github_prefix"}
docs_repository=${docs_repository%.git}

if [[ "$docs_repository" != */* || "$docs_repository" == */*/* ]]; then
  echo "::error::Malformed docs submodule URL; expected exactly one owner and repository segment." >&2
  exit 1
fi

owner=${docs_repository%%/*}
repository=${docs_repository#*/}

if (( ${#owner} > 39 )) ||
    [[ "$owner" == *--* ]] ||
    [[ ! "$owner" =~ ^[A-Za-z0-9]([A-Za-z0-9-]*[A-Za-z0-9])?$ ]]; then
  echo "::error::Invalid GitHub owner in docs submodule URL; use 1-39 alphanumeric characters or single hyphens, beginning and ending alphanumeric." >&2
  exit 1
fi

if (( ${#repository} > 100 )) ||
    [[ ! "$repository" =~ ^[A-Za-z0-9._-]+$ ]] ||
    [[ "$repository" == "." || "$repository" == ".." ]]; then
  echo "::error::Invalid GitHub repository in docs submodule URL; use 1-100 alphanumeric, dot, underscore, or hyphen characters, excluding '.' and '..'." >&2
  exit 1
fi

echo "repository=$docs_repository"
echo "url=https://github.com/$docs_repository"
