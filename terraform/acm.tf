# required to add certificates + DNS for the API gateway rather than let cloudflare handle the routing

locals {
  domain_name = var.env == "live" ? "nowplaying.${var.root_domain}" : "nowplaying-staging.${var.root_domain}"
}

data "aws_route53_zone" "domain" {
  private_zone = false
  zone_id      = var.zone_id
}
