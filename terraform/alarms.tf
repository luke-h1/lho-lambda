data "aws_caller_identity" "current" {}
data "aws_region" "current" {}

data "archive_file" "alarm_notifier_archive" {
  type        = "zip"
  source_dir  = "${path.module}/../.build/release/alarm-notifier-package"
  output_path = "${path.module}/../alarm-notifier.zip"
}

locals {
  notifications_enabled = nonsensitive(var.discord_webhook_url != "")
  alarm_notifier_name   = "${var.project_name}-alarm-notifier-${var.env}"
  alarm_tags = merge(var.tags, {
    Environment = var.env
  })

  monitored_request_metrics = {
    gateway = {
      alarm_name  = "${aws_apigatewayv2_api.lambda.name}-requests-anomaly"
      description = "Unusual request rate on the now-playing API Gateway (${var.env})"
      namespace   = "AWS/ApiGateway"
      metric_name = "Count"
      dimensions = {
        ApiId = aws_apigatewayv2_api.lambda.id
        Stage = aws_apigatewayv2_stage.lambda.name
      }
    }
    lambda = {
      alarm_name  = "${aws_lambda_function.lambda.function_name}-invocations-anomaly"
      description = "Unusual invocation rate on the now-playing Lambda (${var.env})"
      namespace   = "AWS/Lambda"
      metric_name = "Invocations"
      dimensions = {
        FunctionName = aws_lambda_function.lambda.function_name
      }
    }
    authorizer = {
      alarm_name  = "${aws_lambda_function.api_authorizer.function_name}-invocations-anomaly"
      description = "Unusual invocation rate on the now-playing authorizer Lambda (${var.env})"
      namespace   = "AWS/Lambda"
      metric_name = "Invocations"
      dimensions = {
        FunctionName = aws_lambda_function.api_authorizer.function_name
      }
    }
  }
}

check "alarm_destinations_configured" {
  assert {
    condition     = local.notifications_enabled
    error_message = "Alarm notifications require discord_webhook_url."
  }
}

resource "aws_sns_topic" "alarms" {
  count = local.notifications_enabled ? 1 : 0
  name  = "${var.project_name}-alarms-${var.env}"
  tags  = local.alarm_tags
}

resource "aws_sns_topic_policy" "alarms" {
  count = local.notifications_enabled ? 1 : 0
  arn   = aws_sns_topic.alarms[0].arn

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect    = "Allow"
      Principal = { Service = "cloudwatch.amazonaws.com" }
      Action    = "SNS:Publish"
      Resource  = aws_sns_topic.alarms[0].arn
      Condition = {
        ArnLike      = { "aws:SourceArn" = "arn:aws:cloudwatch:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:alarm:*" }
        StringEquals = { "aws:SourceAccount" = data.aws_caller_identity.current.account_id }
      }
    }]
  })
}

resource "aws_sqs_queue" "alarm_notifier_dlq" {
  count                     = local.notifications_enabled ? 1 : 0
  name                      = "${var.project_name}-alarm-notifier-dlq-${var.env}"
  message_retention_seconds = 14 * 24 * 60 * 60
  tags                      = local.alarm_tags
}

resource "aws_iam_role" "alarm_notifier_exec" {
  count = local.notifications_enabled ? 1 : 0
  name  = "${var.project_name}-${var.env}-alarm-notifier-exec-role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action    = "sts:AssumeRole"
      Effect    = "Allow"
      Principal = { Service = "lambda.amazonaws.com" }
    }]
  })
}

resource "aws_iam_role_policy_attachment" "alarm_notifier_basic" {
  count      = local.notifications_enabled ? 1 : 0
  role       = aws_iam_role.alarm_notifier_exec[0].name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_iam_role_policy" "alarm_notifier_dlq" {
  count = local.notifications_enabled ? 1 : 0
  name  = "${var.project_name}-${var.env}-alarm-notifier-dlq"
  role  = aws_iam_role.alarm_notifier_exec[0].id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect   = "Allow"
      Action   = ["sqs:SendMessage"]
      Resource = [aws_sqs_queue.alarm_notifier_dlq[0].arn]
    }]
  })
}

resource "aws_cloudwatch_log_group" "alarm_notifier_logs" {
  count             = local.notifications_enabled ? 1 : 0
  name              = "/aws/lambda/${local.alarm_notifier_name}"
  retention_in_days = 14
  log_group_class   = "STANDARD"

  tags = {
    Environment = var.env
    Service     = "nowplaying"
    s3export    = "true"
  }
}

resource "aws_lambda_function" "alarm_notifier" {
  count            = local.notifications_enabled ? 1 : 0
  function_name    = local.alarm_notifier_name
  runtime          = "dotnet10"
  handler          = "Lho.Lambda.AlarmNotifier::Lho.Lambda.AlarmNotifier.Functions.AlarmNotifierFunction::FunctionHandler"
  role             = aws_iam_role.alarm_notifier_exec[0].arn
  filename         = data.archive_file.alarm_notifier_archive.output_path
  source_code_hash = data.archive_file.alarm_notifier_archive.output_base64sha256
  timeout          = 30
  memory_size      = 256
  architectures    = ["x86_64"]
  description      = "Now playing CloudWatch alarm notifier ${var.env}"

  environment {
    variables = {
      DISCORD_WEBHOOK_URL = var.discord_webhook_url
      SERVICE_NAME        = "now-playing"
      ENVIRONMENT         = var.env
      VERSION             = var.app_version
      DOTNET_ROLL_FORWARD = "Major"
      GIT_SHA             = var.git_sha
    }
  }

  tags = local.alarm_tags

  depends_on = [
    aws_cloudwatch_log_group.alarm_notifier_logs,
    aws_iam_role_policy.alarm_notifier_dlq,
    aws_iam_role_policy_attachment.alarm_notifier_basic,
  ]
}

resource "aws_lambda_function_event_invoke_config" "alarm_notifier" {
  count                        = local.notifications_enabled ? 1 : 0
  function_name                = aws_lambda_function.alarm_notifier[0].function_name
  maximum_retry_attempts       = 2
  maximum_event_age_in_seconds = 3600

  destination_config {
    on_failure {
      destination = aws_sqs_queue.alarm_notifier_dlq[0].arn
    }
  }
}

resource "aws_sns_topic_subscription" "alarm_notifier" {
  count     = local.notifications_enabled ? 1 : 0
  topic_arn = aws_sns_topic.alarms[0].arn
  protocol  = "lambda"
  endpoint  = aws_lambda_function.alarm_notifier[0].arn
}

resource "aws_lambda_permission" "alarm_notifier_sns" {
  count         = local.notifications_enabled ? 1 : 0
  statement_id  = "AllowExecutionFromSNS"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.alarm_notifier[0].function_name
  principal     = "sns.amazonaws.com"
  source_arn    = aws_sns_topic.alarms[0].arn
}

resource "aws_cloudwatch_metric_alarm" "requests_anomaly" {
  for_each = local.notifications_enabled ? local.monitored_request_metrics : {}

  alarm_name          = each.value.alarm_name
  alarm_description   = each.value.description
  comparison_operator = "GreaterThanUpperThreshold"
  evaluation_periods  = 2
  threshold_metric_id = "ad1"
  treat_missing_data  = "notBreaching"
  alarm_actions       = [aws_sns_topic.alarms[0].arn]

  metric_query {
    id          = "ad1"
    expression  = "ANOMALY_DETECTION_BAND(m1, 2)"
    label       = "${each.value.metric_name} (expected)"
    return_data = true
  }

  metric_query {
    id          = "m1"
    return_data = true
    metric {
      metric_name = each.value.metric_name
      namespace   = each.value.namespace
      period      = 60
      stat        = "Sum"
      dimensions  = each.value.dimensions
    }
  }

  tags = local.alarm_tags
}

resource "aws_cloudwatch_metric_alarm" "alarm_notifier_errors" {
  count               = local.notifications_enabled ? 1 : 0
  alarm_name          = "${aws_lambda_function.alarm_notifier[0].function_name}-errors"
  alarm_description   = "Alarm notifier Lambda is failing (${var.env})"
  comparison_operator = "GreaterThanOrEqualToThreshold"
  evaluation_periods  = 1
  metric_name         = "Errors"
  namespace           = "AWS/Lambda"
  period              = 60
  statistic           = "Sum"
  threshold           = 1
  treat_missing_data  = "notBreaching"
  alarm_actions       = [aws_sns_topic.alarms[0].arn]

  dimensions = {
    FunctionName = aws_lambda_function.alarm_notifier[0].function_name
  }

  tags = local.alarm_tags
}

resource "aws_cloudwatch_metric_alarm" "alarm_notifier_dlq_depth" {
  count               = local.notifications_enabled ? 1 : 0
  alarm_name          = "${aws_sqs_queue.alarm_notifier_dlq[0].name}-depth"
  alarm_description   = "Alarm notifier DLQ has messages (${var.env})"
  comparison_operator = "GreaterThanOrEqualToThreshold"
  evaluation_periods  = 1
  metric_name         = "ApproximateNumberOfMessagesVisible"
  namespace           = "AWS/SQS"
  period              = 60
  statistic           = "Maximum"
  threshold           = 1
  treat_missing_data  = "notBreaching"
  alarm_actions       = [aws_sns_topic.alarms[0].arn]

  dimensions = {
    QueueName = aws_sqs_queue.alarm_notifier_dlq[0].name
  }

  tags = local.alarm_tags
}
