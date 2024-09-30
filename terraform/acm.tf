# locals {
#   domain_name = var.env == "live" ? "nowplaying.${var.root_domain}" : "nowplaying.${var.env}.${var.root_domain}"
# }

# resource "aws_acm_certificate" "domain" {
#   provider          = aws.eu-west-2
#   domain_name       = local.domain_name
#   validation_method = "DNS"
#   lifecycle {
#     create_before_destroy = true

#     # only set to false because we might need to do a full teardown
#     prevent_destroy = false
#   }
# }

# resource "aws_acm_certificate_validation" "val" {
#   provider        = aws.eu-west-2
#   certificate_arn = aws_acm_certificate.domain.arn
# }



# resource "aws_route53_zone" "zone" {
#   name    = local.domain_name
#   comment = "Managed by Terraform"
# }

# data "aws_route53_zone" "domain" {
#   private_zone = false
#   zone_id      = aws_route53_zone.zone.zone_id
# }
