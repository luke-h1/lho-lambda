resource "aws_apigatewayv2_api" "lambda" {
  name                         = "now-playing-gw-${var.env}"
  protocol_type                = "HTTP"
  disable_execute_api_endpoint = false
  cors_configuration {
    allow_headers  = ["*"]
    allow_origins  = ["*"]
    allow_methods  = ["*"]
    expose_headers = ["*"]
  }
}

resource "aws_apigatewayv2_domain_name" "domain_name" {
  domain_name = var.env == "live" ? "nowplaying.${var.root_domain}" : "nowplaying-${var.env}.${var.root_domain}"

  domain_name_configuration {
    certificate_arn = aws_acm_certificate.cert.arn
    endpoint_type   = "REGIONAL"
    security_policy = "TLS_1_2"
  }
}

resource "aws_apigatewayv2_api_mapping" "lambda" {
  api_id      = aws_apigatewayv2_api.lambda.id
  domain_name = aws_apigatewayv2_domain_name.domain_name.domain_name
  stage       = aws_apigatewayv2_stage.lambda.id
}

resource "aws_apigatewayv2_stage" "lambda" {
  api_id = aws_apigatewayv2_api.lambda.id
  name   = var.env
  default_route_settings {
    throttling_burst_limit = 10000
    throttling_rate_limit  = 20000
    logging_level          = "OFF"
  }

  dynamic "route_settings" {
    for_each = { for k, v in var.routes : k => v }
    content {
      route_key          = route_settings.value.route_key
      data_trace_enabled = try(route_settings.value.data_trace_enabled, false)
      logging_level      = try(route_settings.value.logging_level, null)

      detailed_metrics_enabled = try(route_settings.value.detailed_metrics_enabled, false)
      throttling_burst_limit   = try(route_settings.value.throttling_burst_limit, null)
      throttling_rate_limit    = try(route_settings.value.throttling_rate_limit, null)
    }
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

}

resource "aws_apigatewayv2_integration" "lambda" {
  api_id             = aws_apigatewayv2_api.lambda.id
  integration_uri    = aws_lambda_function.lambda.invoke_arn
  integration_type   = "AWS_PROXY"
  integration_method = "POST"
}

resource "aws_apigatewayv2_route" "this" {
  for_each = { for k, v in var.routes : k => v }

  api_id    = aws_apigatewayv2_api.lambda.id
  route_key = each.key

  api_key_required                    = try(each.value.api_key_required, null)
  authorization_scopes                = try(split(",", each.value.authorization_scopes), null)
  authorization_type                  = try(each.value.authorization_type, "NONE")
  model_selection_expression          = try(each.value.model_selection_expression, null)
  operation_name                      = try(each.value.operation_name, null)
  route_response_selection_expression = try(each.value.route_response_selection_expression, null)
  target                              = "integrations/${aws_apigatewayv2_integration.lambda.id}"

  # Have been added to the docs. But is WEBSOCKET only(not yet supported)
  # request_models  = try(each.value.request_models, null)
}


# ROUTES 
##############################################################################
# resource "aws_apigatewayv2_route" "lambda_route_health" {
#   api_id         = aws_apigatewayv2_api.lambda.id
#   target         = "integrations/${aws_apigatewayv2_integration.lambda.id}"
#   route_key      = "GET /api/health"
#   operation_name = "health"
# # }
# resource "aws_apigatewayv2_route" "lambda_route_health_head" {
#   api_id           = aws_apigatewayv2_api.lambda.id
#   target           = "integrations/${aws_apigatewayv2_integration.lambda.id}"
#   route_key        = "HEAD /api/health"
#   api_key_required = false
#   operation_name   = "head health"
# }

resource "aws_apigatewayv2_route" "lambda_route_version" {
  api_id           = aws_apigatewayv2_api.lambda.id
  target           = "integrations/${aws_apigatewayv2_integration.lambda.id}"
  route_key        = "GET /api/version"
  api_key_required = false
  operation_name   = "version"
}

resource "aws_apigatewayv2_route" "lambda_route_now_playing" {
  api_id           = aws_apigatewayv2_api.lambda.id
  target           = "integrations/${aws_apigatewayv2_integration.lambda.id}"
  route_key        = "GET /api/now-playing"
  api_key_required = false
  operation_name   = "now-playing"
}
##############################################################################


resource "aws_cloudwatch_log_group" "api_gw" {
  name              = "/aws/api_gw/${aws_apigatewayv2_api.lambda.name}"
  retention_in_days = 1
  log_group_class   = "INFREQUENT_ACCESS"
  tags = {
    Environment = var.env
    Service     = "nowplaying"
    s3export    = "true"
  }
}

resource "aws_lambda_permission" "api_gw" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.lambda.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.lambda.execution_arn}/*/*"
}