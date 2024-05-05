variable "env" {
  type        = string
  description = "The environment to deploy to"
}

variable "env_vars" {
  type        = map(string)
  description = "The environment variables to set on lambda"
  validation {
    condition     = contains(keys(var.env_vars), "SPOTIFY_CLIENT_ID") && contains(keys(var.env_vars), "SPOTIFY_CLIENT_SECRET") && contains(keys(var.env_vars), "SPOTIFY_REFRESH_TOKEN")
    error_message = "env_vars must contain keys: SPOTIFY_CLIENT_ID, SPOTIFY_CLIENT_SECRET, SPOTIFY_REFRESH_TOKEN"
  }
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


variable "routes" {
  type = map(object({
    data_trace_enabled       = bool
    logging_level            = string
    detailed_metrics_enabled = bool
    throttling_burst_limit   = number
    throttling_rate_limit    = number
  }))
  description = "The route settings for the api gateway"
  default = {
    "GET /api/health" = {
      throttling_burst_limit = 10000
      throttling_rate_limit  = 20000
    },
    "HEAD /api/health" = {
      throttling_burst_limit   = 10000
      throttling_rate_limit    = 20000
      data_trace_enabled       = false
      detailed_metrics_enabled = false
      logging_level            = "OFF"
    }
  }
}