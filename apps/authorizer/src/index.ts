/* eslint-disable prefer-destructuring, no-console */
import {
  APIGatewayRequestAuthorizerEvent,
  APIGatewayAuthorizerResult,
  StatementEffect,
} from 'aws-lambda';

export const handler = async (
  event: APIGatewayRequestAuthorizerEvent,
): Promise<APIGatewayAuthorizerResult> => {
  try {
    // eslint-disable-next-line prefer-destructuring
    const apiKey = event.headers?.['x-api-key'];

    if (apiKey !== process.env.API_KEY) {
      console.info('deny');
      return generatePolicy('user', 'Deny', event.methodArn);
    }

    console.info('allow');
    return generatePolicy('user', 'Allow', event.methodArn);
  } catch (error) {
    // eslint-disable-next-line no-console
    console.error('Error in authorizer:', error);
    return generatePolicy('user', 'Deny', event.methodArn);
  }
};

const generatePolicy = (
  principalId: string,
  effect: StatementEffect,
  resource: string,
): APIGatewayAuthorizerResult => {
  return {
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
};
