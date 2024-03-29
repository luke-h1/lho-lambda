#!/bin/bash

# detech the choice
if [ "$1" == "staging" ]; then
  terraform workspace select staging
  cd apps/lambda 
  pnpm build
  cd ../../
  cd terraform
  terraform apply -var-file="envs/staging.tfvars" -auto-approve
elif [ "$1" == "live" ]; then
  terraform workspace select live
  cd apps/lambda 
  pnpm build
  cd ../../
  cd terraform
  terraform apply -var-file="envs/live.tfvars" -auto-approve
else
  echo "Invalid choice"
  exit 1
fi

