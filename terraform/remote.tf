data "terraform_remote_state" "vpc" {
  backend   = "s3"
  workspace = var.env
  config = {
    bucket         = "nowplaying-${var.env}-terraform-state"
    key            = "vpc/${var.env}.tfstate"
    region         = "eu-west-2"
    dynamodb_table = "nowplaying-${var.env}-terraform-state-lock"
    encrypt        = true
  }
}
