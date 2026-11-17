# Changesets

Each file in this directory is one release note. A pull request adds one with:

```sh
bun run changeset
```

Write one or two sentences for the person who uses the API. The bump type sets
the heading the note lands under:

| Bump    | Heading          |
| ------- | ---------------- |
| `major` | Breaking changes |
| `minor` | New              |
| `patch` | Fixed            |

A change with nothing to tell a user (a refactor, a test, CI) still has to pass
the check. Add an empty note:

```sh
bun run changeset -- --empty
```

The notes stay here until the next live release. Every merge to `main` deploys
live, and the release job then moves the notes into `CHANGELOG.md` under that
release's `live.<sha>` version and deletes the files. The bump type only picks
the heading. Releases are named after the commit, not a semver number, so no
changeset changes a version.

Preview the notes that are waiting:

```sh
bun run release:notes pending
```
