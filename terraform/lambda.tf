data "archive_file" "lambda_archive" {
  type        = "zip"
  source_dir  = "${path.module}/../apps/lambda/dist"
  output_path = "${path.module}/../lambda.zip"
}

resource "aws_iam_role" "lambda_exec" {
  name = "lho-lambda-${var.env}-exec-role"
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
resource "aws_iam_role_policy_attachment" "lambda_policy" {
  role       = aws_iam_role.lambda_exec.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

# data "aws_iam_policy" "aws_xray_write_only_access" {
#   arn = "arn:aws:iam::aws:policy/AWSXrayWriteOnlyAccess"
# }
# resource "aws_iam_role_policy_attachment" "aws_xray_write_only_access" {
#   role       = aws_iam_role.lambda_exec.name
#   policy_arn = data.aws_iam_policy.aws_xray_write_only_access.arn
# }

resource "aws_lambda_function" "lambda" {
  function_name    = "nowplaying-lambda-${var.env}"
  runtime          = "nodejs20.x"
  handler          = "index.handler"
  role             = aws_iam_role.lambda_exec.arn
  filename         = "${path.module}/../lambda.zip"
  source_code_hash = data.archive_file.lambda_archive.output_base64sha256
  timeout          = 30
  snap_start {
    apply_on = "PublishedVersions"
  }

  tags = {
    Environment = var.env
    Service     = "nowplaying"
    s3export    = var.env == "live" ? "true" : "false"
  }
  description   = "Now playing Lambda ${var.env}"
  memory_size   = 128
  architectures = ["arm64"]
  environment {
    variables = var.env_vars
  }
}

resource "aws_cloudwatch_log_group" "lambda_logs" {
  name              = "/aws/lambda/${aws_lambda_function.lambda.function_name}"
  retention_in_days = 1
  log_group_class   = "INFREQUENT_ACCESS"
  tags = {
    Environment = var.env
    Service     = "nowplaying"
    s3export    = "true"
  }
}
