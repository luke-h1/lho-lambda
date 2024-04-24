import { APIGatewayTokenAuthorizerEvent, Handler } from 'aws-lambda';
import jwt from 'jsonwebtoken';
import buildPolicy from './utils/buildPolicy';
import validateToken from './utils/validateToken';

export const handler: Handler = async (
  event: APIGatewayTokenAuthorizerEvent,
) => {
  const { authorizationToken } = event;

  if (!authorizationToken) {
    return buildPolicy('Deny', event.methodArn, 'Unauthorized');
  }

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [_type, token] = authorizationToken.split(' ');

  try {
    const decoded = jwt.decode(token as string, {
      complete: true,
    });

    if (!decoded) {
      return buildPolicy('Deny', event.methodArn, 'Unauthorized');
    }

    if (!token) {
      // eslint-disable-next-line no-console
      console.warn('No token provided');
      return buildPolicy('Deny', event.methodArn, 'Unauthorized');
    }

    return validateToken(decoded, token, event.methodArn);
  } catch (e) {
    // eslint-disable-next-line no-console
    console.error('Failed to validate token', e);

    return buildPolicy('Deny', event.methodArn, 'Unauthorized');
  }
};
