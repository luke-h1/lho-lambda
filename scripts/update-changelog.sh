#!/usr/bin/env bash
set -euo pipefail

version="${1:?version is required}"
tag="v${version}"
repo="${GITHUB_REPOSITORY:-luke-h1/lho-lambda}"
date="${CHANGELOG_DATE:-$(date +%F)}"
changelog="${CHANGELOG_FILE:-CHANGELOG.md}"
previous_tag="${2:-}"

if [ -z "$previous_tag" ]; then
  previous_tag="$(git describe --tags --abbrev=0 --match 'v*' 2>/dev/null || true)"
fi

if [ -f "$changelog" ] && grep -q "^# \\[${version}\\]" "$changelog"; then
  echo "CHANGELOG already contains ${version}"
  exit 0
fi

range="HEAD"
compare_url=""
if [ -n "$previous_tag" ]; then
  range="${previous_tag}..HEAD"
  compare_url="https://github.com/${repo}/compare/${previous_tag}...${tag}"
fi

tmp="$(mktemp)"
section="$(mktemp)"
trap 'rm -f "$tmp" "$section"' EXIT

if [ -n "$compare_url" ]; then
  printf '# [%s](%s) (%s)\n\n' "$version" "$compare_url" "$date" > "$section"
else
  printf '# [%s] (%s)\n\n' "$version" "$date" > "$section"
fi

append_group() {
  local title="$1"
  local pattern="$2"
  local output

  output="$(
    git log --reverse --no-merges --pretty=format:'%H%x09%s' "$range" |
      awk -F '\t' -v pattern="$pattern" -v repo="$repo" '
        BEGIN { IGNORECASE = 1 }
        $2 ~ pattern {
          sha = substr($1, 1, 7)
          subject = $2
          scope = ""
          text = subject

          if (match(subject, /^[a-z]+(\([^)]+\))?!?: /)) {
            prefix = substr(subject, 1, RLENGTH - 2)
            text = substr(subject, RLENGTH + 1)
            if (match(prefix, /\([^)]+\)/)) {
              scope = substr(prefix, RSTART + 1, RLENGTH - 2)
            }
          }

          if (scope != "") {
            printf "* **%s:** %s ([%s](https://github.com/%s/commit/%s))\n", scope, text, sha, repo, $1
          } else {
            printf "* %s ([%s](https://github.com/%s/commit/%s))\n", text, sha, repo, $1
          }
        }
      '
  )"

  if [ -n "$output" ]; then
    printf '### %s\n\n%s\n\n' "$title" "$output" >> "$section"
  fi
}

append_group "Features" '^feat(\(.+\))?!?: '
append_group "Bug Fixes" '^fix(\(.+\))?!?: '
append_group "Performance Improvements" '^perf(\(.+\))?!?: '
append_group "Other Changes" '^(chore|docs|refactor|test|ci|build|style|revert)(\(.+\))?!?: '

if ! grep -q '^\* ' "$section"; then
  git log --reverse --no-merges --pretty=format:'* %s ([%h](https://github.com/'"$repo"'/commit/%H))' "$range" >> "$section"
  printf '\n\n' >> "$section"
fi

cat "$section" > "$tmp"
if [ -f "$changelog" ]; then
  cat "$changelog" >> "$tmp"
fi
mv "$tmp" "$changelog"
