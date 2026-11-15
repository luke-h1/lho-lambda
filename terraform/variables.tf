variable "project_name" {
  type        = string
  description = "the name of the project"
  default     = "now-playing"
}

variable "env" {
  type        = string
  description = "The environment to deploy to"
}

variable "spotify_client_id" {
  type        = string
  description = "Spotify client ID"
  sensitive   = true
}

variable "spotify_client_secret" {
  type        = string
  description = "Spotify client secret"
  sensitive   = true
}

variable "spotify_refresh_token" {
  type        = string
  description = "Spotify refresh token"
  sensitive   = true
}

variable "lastfm_api_key" {
  type        = string
  description = "Last.fm API key"
  sensitive   = true
}

variable "lastfm_username" {
  type        = string
  description = "Last.fm username"
  sensitive   = true
}

variable "zone_id" {
  type        = string
  description = "The zone id for the route53 record"
}

variable "root_domain" {
  type        = string
  description = "The root domain for the route53 record"
  default     = "lhowsam.com"
}

variable "sub_domain" {
  type        = string
  description = "The sub domain for the route53 record"
}

variable "private_key" {
  type        = string
  description = "The private key for the certificate"
}

variable "certificate_body" {
  type        = string
  description = "The certificate body for the certificate"
}

variable "certificate_chain" {
  type        = string
  description = "The certificate chain for the certificate"
}

variable "deployed_by" {
  type        = string
  description = "The user who deployed the lambda"
}

variable "tags" {
  type        = map(string)
  description = "The tags to apply to the resources"
  default = {
    "Service"   = "now-playing"
    "ManagedBy" = "Terraform"
  }
}

variable "app_version" {
  type        = string
  description = "The version of the application"
  default     = "unknown"
}

variable "git_sha" {
  type        = string
  description = "The git sha of the commit that caused the deploy"
  default     = "unknown"
}

variable "sentry_dsn" {
  type        = string
  description = "Sentry DSN for Lambda error monitoring"
  sensitive   = true
  default     = ""
}

variable "api_key" {
  description = "API key for securing the API Gateway endpoints"
  type        = string
  sensitive   = true
}

variable "invocations_alarm_threshold" {
  type        = number
  description = "Invocations per minute that trips the high-invocation alarm (3 of 5 periods)."
  default     = 30

  validation {
    condition     = var.invocations_alarm_threshold >= 10 && floor(var.invocations_alarm_threshold) == var.invocations_alarm_threshold
    error_message = "invocations_alarm_threshold must be an integer >= 10."
  }
}

variable "discord_webhook_url" {
  description = "Discord webhook URL that receives CloudWatch alarm notifications"
  type        = string
  sensitive   = true
}
