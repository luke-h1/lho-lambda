resource "aws_apigatewayv2_api" "lambda" {
  name                         = "now-playing-gw-${var.env}"
  protocol_type                = "HTTP"
  disable_execute_api_endpoint = true
  cors_configuration {
    allow_headers  = ["*"]
    allow_origins  = ["*"]
    allow_methods  = ["*"]
    expose_headers = ["*"]
  }

  tags = merge(var.tags, {
    Environment = var.env
  })
}

resource "aws_apigatewayv2_domain_name" "domain_name" {
  domain_name = var.env == "live" ? "nowplaying.${var.root_domain}" : "nowplaying-${var.env}.${var.root_domain}"

  domain_name_configuration {
    certificate_arn = aws_acm_certificate.cert.arn
    endpoint_type   = "REGIONAL"
    security_policy = "TLS_1_2"
  }
  tags = merge(var.tags, {
    Environment = var.env
  })
}

resource "aws_apigatewayv2_api_mapping" "lambda" {
  api_id      = aws_apigatewayv2_api.lambda.id
  domain_name = aws_apigatewayv2_domain_name.domain_name.domain_name
  stage       = aws_apigatewayv2_stage.lambda.id
}

resource "aws_apigatewayv2_stage" "lambda" {
  api_id      = aws_apigatewayv2_api.lambda.id
  name        = var.env
  auto_deploy = true
  route_settings {
    route_key              = "$default"
    throttling_burst_limit = 10000
    throttling_rate_limit  = 20000
    logging_level          = "OFF"
  }
  default_route_settings {
    throttling_burst_limit = 10000
    throttling_rate_limit  = 20000
    logging_level          = "OFF"
  }
  tags = merge(var.tags, {
    Environment = var.env
  })
}

resource "aws_apigatewayv2_integration" "lambda" {
  api_id             = aws_apigatewayv2_api.lambda.id
  integration_uri    = aws_lambda_function.lambda.invoke_arn
  integration_type   = "AWS_PROXY"
  integration_method = "POST"
}

# Create API Key
resource "aws_apigatewayv2_api_key" "api_key" {
  name        = "now-playing-api-key-${var.env}"
  description = "API key for now-playing service in ${var.env} environment"
  enabled     = true
}

# Create Usage Plan
resource "aws_apigatewayv2_usage_plan" "usage_plan" {
  name = "now-playing-usage-plan-${var.env}"

  api_stages {
    api_id = aws_apigatewayv2_api.lambda.id
    stage  = aws_apigatewayv2_stage.lambda.name
  }

  throttle_settings {
    burst_limit = 10000
    rate_limit  = 20000
  }
}

resource "aws_apigatewayv2_api_key" "api_key" {
  name        = "now-playing-api-key-${var.env}"
  description = "API key for now-playing service in ${var.env} environment"
  enabled     = true
}

resource "aws_apigatewayv2_usage_plan" "usage_plan" {
  name = "now-playing-usage-plan-${var.env}"

  throttle_settings {
    burst_limit = 10000
    rate_limit  = 20000
  }
}

# Associate API Key with Usage Plan
resource "aws_apigatewayv2_usage_plan_key" "usage_plan_key" {
  key_id        = aws_apigatewayv2_api_key.api_key.id
  key_type      = "API_KEY"
  usage_plan_id = aws_apigatewayv2_usage_plan.usage_plan.id
}

data "archive_file" "auth_archive" {
  type        = "zip"
  source_dir  = "${path.module}/../apps/authorizer/dist"
  output_path = "${path.module}/../authorizer.zip"
}

# Define the custom authorizer Lambda function
resource "aws_lambda_function" "custom_authorizer" {
  filename         = "${path.module}/../authorizer.zip"
  function_name    = "custom-authorizer"
  role             = aws_iam_role.lambda_exec.arn
  handler          = "index.handler"
  runtime          = "nodejs20.x"
  source_code_hash = data.archive_file.auth_archive.output_base64sha256

  environment {
    variables = {
      ENV = var.env
    }
  }

  tags = merge(var.tags, {
    Environment = var.env
  })
}

# Add permission for API Gateway to invoke the custom authorizer Lambda
resource "aws_lambda_permission" "custom_authorizer_permission" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.custom_authorizer.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.lambda.execution_arn}/*/*"
}

# Create the custom authorizer in API Gateway
resource "aws_apigatewayv2_authorizer" "custom_authorizer" {
  api_id           = aws_apigatewayv2_api.lambda.id
  authorizer_uri   = aws_lambda_function.custom_authorizer.invoke_arn
  authorizer_type  = "REQUEST"
  identity_sources = ["method.request.header.Authorization"]
  name             = "custom-authorizer"
}

# ROUTES 
##############################################################################
resource "aws_apigatewayv2_route" "lambda_route_health" {
  api_id           = aws_apigatewayv2_api.lambda.id
  target           = "integrations/${aws_apigatewayv2_integration.lambda.id}"
  route_key        = "GET /api/health"
  operation_name   = "health"
  api_key_required = true
  authorizer_id    = aws_apigatewayv2_authorizer.custom_authorizer.id
}

resource "aws_apigatewayv2_route" "lambda_route_health_head" {
  api_id           = aws_apigatewayv2_api.lambda.id
  target           = "integrations/${aws_apigatewayv2_integration.lambda.id}"
  route_key        = "HEAD /api/health"
  api_key_required = true
  operation_name   = "head health"
  authorizer_id    = aws_apigatewayv2_authorizer.custom_authorizer.id
}

resource "aws_apigatewayv2_route" "lambda_route_version" {
  api_id           = aws_apigatewayv2_api.lambda.id
  target           = "integrations/${aws_apigatewayv2_integration.lambda.id}"
  route_key        = "GET /api/version"
  api_key_required = true
  operation_name   = "version"
  authorizer_id    = aws_apigatewayv2_authorizer.custom_authorizer.id
}

resource "aws_apigatewayv2_route" "lambda_route_now_playing" {
  api_id           = aws_apigatewayv2_api.lambda.id
  target           = "integrations/${aws_apigatewayv2_integration.lambda.id}"
  route_key        = "GET /api/now-playing"
  api_key_required = true
  operation_name   = "now-playing"
  authorizer_id    = aws_apigatewayv2_authorizer.custom_authorizer.id
}
##############################################################################

resource "aws_cloudwatch_log_group" "api_gw" {
  name              = "/aws/api_gw/${aws_apigatewayv2_api.lambda.name}"
  retention_in_days = 1
  log_group_class   = "INFREQUENT_ACCESS"

  tags = merge(var.tags, {
    Environment = var.env
  })
}

resource "aws_lambda_permission" "api_gw" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.lambda.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.lambda.execution_arn}/*/*"
}