# lho-lambda

Lambda for lhowsam.com. Handles Spotify API requests

## Tech stack

- [AWS Lambda](https://aws.amazon.com/lambda/)
- [Terraform](https://www.terraform.io/)
- [C#](https://dotnet.microsoft.com/en-us/languages/csharp)

## Changelog & version

Changelog is generated from conventional commits. On push to `main`, the release workflow updates `CHANGELOG.md` and commits it.

```bash
uv tool install pre-commit
uv tool install commitizen
pre-commit install
pre-commit install --hook-type commit-msg
```

Denote a breaking change by adding `!` after the type or scope, for example `feat!: drop legacy payload shape` or `feat(api)!: drop legacy payload shape`.
