data "archive_file" "authorizer_archive" {
  type        = "zip"
  source_dir  = "${path.module}/../apps/authorizer/dist"
  output_path = "${path.module}/../authorizer.zip"
}

resource "aws_iam_role" "authorizer_exec" {
  name = "authorizer-lambda-${var.env}-exec-role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Sid    = ""
      Principal = {
        Service = "lambda.amazonaws.com"
      }
      }
    ]
  })
}

resource "aws_lambda_function" "authorizer" {
  function_name    = "nowplaying-authorizer-${var.env}"
  runtime          = "nodejs20.x"
  handler          = "index.handler"
  role             = aws_iam_role.lambda_exec.arn
  filename         = "${path.module}/../authorizer.zip"
  source_code_hash = data.archive_file.authorizer_archive.output_base64sha256
  timeout          = 10
  tags = {
    Environment = var.env
    Service     = "nowplaying-authorizer"
    s3export    = var.env == "live" ? "true" : "false"
  }
  description   = "Authorizer now playing ${var.env}"
  memory_size   = 128
  architectures = ["arm64"]

  environment {
    variables = {
      DEPLOYED_AT = timestamp()
      DEPLOYED_BY = var.deployed_by
      JWT_SECRET  = var.jwt_secret
    }
  }
}