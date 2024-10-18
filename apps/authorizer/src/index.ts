/* eslint-disable @typescript-eslint/no-unused-vars */
import {
  APIGatewayTokenAuthorizerEvent,
  APIGatewayAuthorizerResult,
  Context,
  Callback,
} from 'aws-lambda';

export const handler = async (
  event: APIGatewayTokenAuthorizerEvent,
  _context: Context,
  _callback: Callback<APIGatewayAuthorizerResult>,
): Promise<APIGatewayAuthorizerResult> => {
  const token = event.authorizationToken;

  // Implement your custom authorization logic here
  if (token === 'allow') {
    return generatePolicy('user', 'Allow', event.methodArn);
  }
  return generatePolicy('user', 'Deny', event.methodArn);
};

const generatePolicy = (
  principalId: string,
  effect: string,
  resource: string,
): APIGatewayAuthorizerResult => {
  const authResponse: APIGatewayAuthorizerResult = {
    principalId,
    policyDocument: {
      Version: '2012-10-17',
      Statement: [
        {
          Action: 'execute-api:Invoke',
          Effect: effect,
          Resource: resource,
        },
      ],
    },
  };
  return authResponse;
};
