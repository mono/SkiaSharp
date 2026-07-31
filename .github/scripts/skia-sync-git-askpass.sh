#!/usr/bin/env bash

prompt="${1:-}"
if [[ ! "$prompt" =~ ^(Username|Password)\ for\ \'https://([^/\']+)(/[^/\']*)?\':[[:space:]]*$ ]]; then
  exit 1
fi

kind="${BASH_REMATCH[1]}"
authority="${BASH_REMATCH[2]}"
host="${authority##*@}"
if [[ "$host" != "github.com" ]]; then
  exit 1
fi

if [[ "$kind" == "Username" ]]; then
  printf '%s\n' "x-access-token"
else
  printf '%s\n' "${GIT_AUTH_TOKEN:?GIT_AUTH_TOKEN is required}"
fi
