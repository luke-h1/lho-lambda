# lho-lambda

Lambda for lhowsam.com. Handles Spotify API requests

## Tech stack

- [AWS Lambda](https://aws.amazon.com/lambda/)
- [Terraform](https://www.terraform.io/)
- [C#](https://dotnet.microsoft.com/en-us/languages/csharp)

## Changelog

Release notes are changesets. A pull request adds a `.changeset/*.md` file with:

```bash
bun run changeset
```

Write one or two sentences for the person who uses the API, and pick the bump type. `major`, `minor` and `patch` select the heading (Breaking changes, New, Fixed). A change with nothing to tell a user still has to pass the `Changeset present` check:

```bash
bun run changeset -- --empty
```

Every merge to `main` deploys live. The release workflow then writes the waiting notes into `CHANGELOG.md` under that release's `live.<sha>` version, deletes the files and puts the same notes in the GitHub release. Preview the notes that are waiting:

```bash
bun run release:notes pending
```

Bun is only necessary for the changesets. The .NET build does not use it.

## Commit hooks

```bash
uv tool install pre-commit
uv tool install commitizen
pre-commit install
pre-commit install --hook-type commit-msg
```

Denote a breaking change by adding `!` after the type or scope, for example `feat!: drop legacy payload shape` or `feat(api)!: drop legacy payload shape`.
