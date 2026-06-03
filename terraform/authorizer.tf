data "archive_file" "auth_archive" {
  type        = "zip"
  source_dir  = "${path.module}/../.build/release/authorizer-package"
  output_path = "${path.module}/../authorizer.zip"
}

locals {
  authorizer_function_name = "${var.project_name}-api-authorizer-${var.env}"
}

resource "aws_lambda_function" "api_authorizer" {
  filename         = "${path.module}/../authorizer.zip"
  function_name    = local.authorizer_function_name
  role             = aws_iam_role.lambda_exec.arn
  handler          = "Lho.Lambda.Authorizer::Lho.Lambda.Authorizer.Functions.AuthorizerFunction::FunctionHandler"
  source_code_hash = data.archive_file.auth_archive.output_base64sha256
  runtime          = "dotnet10"
  memory_size      = 256
  architectures    = ["x86_64"]
  timeout          = 10

  tracing_config {
    mode = "Active"
  }

  environment {
    variables = {
      API_KEY                 = var.api_key
      SERVICE_NAME            = "now-playing"
      ENVIRONMENT             = var.env
      VERSION                 = var.app_version
      DOTNET_ROLL_FORWARD     = "Major"
      GIT_SHA                 = var.git_sha
      SENTRY_DSN              = var.sentry_dsn
      SENTRY_ENVIRONMENT      = var.env
      SENTRY_RELEASE          = var.app_version
      PUSHGATEWAY_URL         = var.pushgateway_url
      PUSHGATEWAY_AUTH_HEADER = var.pushgateway_auth_header
      PROMETHEUS_JOB          = "now-playing-authorizer"
      METRICS_ENABLED         = tostring(var.monitoring_enabled)
    }
  }

  tags = merge(var.tags, {
    ENVIRONMENT = var.env
  })

  depends_on = [aws_cloudwatch_log_group.auth_logs]
}

resource "aws_cloudwatch_log_group" "auth_logs" {
  name              = "/aws/lambda/${local.authorizer_function_name}"
  retention_in_days = 1
  log_group_class   = "STANDARD"

  tags = {
    Environment = var.env
    Service     = "nowplaying"
    s3export    = "true"
  }
}

resource "aws_apigatewayv2_authorizer" "api_key" {
  api_id                            = aws_apigatewayv2_api.lambda.id
  authorizer_type                   = "REQUEST"
  authorizer_uri                    = aws_lambda_function.api_authorizer.invoke_arn
  identity_sources                  = ["$request.header.x-api-key"]
  name                              = "api-authorizer"
  authorizer_payload_format_version = "2.0"
  authorizer_result_ttl_in_seconds  = 10
  enable_simple_responses           = true
}

resource "aws_lambda_permission" "api_gw_authorizer" {
  statement_id  = "AllowExecutionFromAPIGatewayAuthorizer"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.api_authorizer.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.lambda.execution_arn}/authorizers/${aws_apigatewayv2_authorizer.api_key.id}"
}
