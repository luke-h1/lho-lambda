resource "aws_dynamodb_table" "dynamodb_terraform_lock" {
  name           = "nowplaying-${var.env}-terraform-state-lock"
  hash_key       = "LockID"
  read_capacity  = 20
  write_capacity = 20
  attribute {
    name = "LockID"
    type = "S"
  }

  tags = merge(var.tags, {
    Name = "nowplaying-${var.env}-terraform-state-lock"
  })
}