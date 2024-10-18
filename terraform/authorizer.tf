data "archive_file" "auth_lambda_archive" {
  type        = "zip"
  source_dir  = "${path.module}/../apps/authorizer/dist"
  output_path = "${path.module}/../authorizer.zip"
}

resource "aws_lambda_function" "custom_authorizer" {
  filename         = "${path.module}/../authorizer.zip"
  function_name    = "custom-authorizer"
  role             = aws_iam_role.lambda_exec.arn
  handler          = "index.handler"
  runtime          = "nodejs20.x"
  source_code_hash = data.archive_file.auth_lambda_archive.output_base64sha256

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
  api_id           = aws_apigatewayv2_api.lambda_authorizer.id
  authorizer_uri   = aws_lambda_function.custom_authorizer.invoke_arn
  authorizer_type  = "REQUEST"
  identity_sources = ["method.request.header.Authorization"]
  name             = "custom-authorizer"
}


resource "aws_apigatewayv2_api_mapping" "lambda_authorizer" {
  api_id      = aws_apigatewayv2_api.lambda_authorizer.id
  domain_name = aws_apigatewayv2_domain_name.domain_name.domain_name
  stage       = aws_apigatewayv2_stage.lambda.id
}

resource "aws_apigatewayv2_stage" "lambda_authorizer" {
  api_id      = aws_apigatewayv2_api.lambda_authorizer.id
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
  # access_log_settings {
  #   destination_arn = aws_cloudwatch_log_group.api_gw.arn
  #   format = jsonencode({
  #     requestId               = "$context.requestId"
  #     sourceIp                = "$context.identity.sourceIp"
  #     requestTime             = "$context.requestTime"
  #     protocol                = "$context.protocol"
  #     httpMethod              = "$context.httpMethod"
  #     resourcePath            = "$context.resourcePath"
  #     routeKey                = "$context.routeKey"
  #     status                  = "$context.status"
  #     responseLength          = "$context.responseLength"
  #     integrationErrorMessage = "$context.integrationErrorMessage"
  #     }
  #   )
  # }
  tags = merge(var.tags, {
    Environment = var.env
  })
}

resource "aws_apigatewayv2_api" "lambda_authorizer" {
  name                         = "auth-now-playing-gw-${var.env}"
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