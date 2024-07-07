# lho-lambda

Supporting lambdas for lhowsam.com. Handles now-playing and other API requests that supports lhowsam.com

## Tech stack

- [AWS Lambda](https://aws.amazon.com/lambda/)
- [Terraform](https://www.terraform.io/)
- [Node.js](https://nodejs.org/en/)

## setup

Install correct version of node and pnpm

```
nvm install && nvm use
```

```
PNPM_VERSION=$(node -e "console.log(require('./package.json').engines.pnpm)")
curl -fsSL https://get.pnpm.io/install.sh | env PNPM_VERSION=$PNPM_VERSION sh -
```
