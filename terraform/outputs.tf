output "function_name" {
  description = "value of the lambda function name"
  value       = aws_lambda_function.lambda.function_name
}

output "base_url" {
  description = "Base URL of the API Gateway stage"
  # value = module.api_gw.apigatewayv2_api_api_endpoint
  value = aws_apigatewayv2_stage.lambda.invoke_url
}
