import { APIGatewayAuthorizerResult } from 'aws-lambda';

const buildPolicy = (
  effect: 'Allow' | 'Deny',
  methodArn: string,
  featureFlagType?: string,
): APIGatewayAuthorizerResult => {
  return {
    principalId: featureFlagType ?? '',
    policyDocument: {
      Version: '2012-10-17',
      Statement: [
        {
          Action: 'execute-api:Invoke',
          Effect: effect,
          Resource: methodArn,
        },
      ],
    },
    context: {
      // ensure we pass the request body to the lambda function
      ...(featureFlagType && { featureFlagType }),
    },
  };
};
export default buildPolicy;
