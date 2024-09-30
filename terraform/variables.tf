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

variable "root_domain" {
  type        = string
  description = "The root domain for the route53 record"
  default     = "lhowsam.com"
}

variable "deployed_by" {
  type        = string
  description = "The user who deployed the lambda"
}

variable "tags" {
  type        = map(string)
  description = "The tags to apply to the resources"
  default = {
    "Service"   = "NowPlaying"
    "ManagedBy" = "Terraform"
  }
}

variable "git_sha" {
  type        = string
  description = "The git sha of the commit that caused the deploy"
  default     = "unknown"
}
