/* eslint-disable no-console */
import jwt, { Jwt } from 'jsonwebtoken';
import buildPolicy from './buildPolicy';

const validateToken = (_decoded: Jwt, token: string, methodArn: string) => {
  // todo: add attribute to the decoded token
  try {
    const valid = !!jwt.verify(token, process.env.JWT_SECRET);

    if (!valid) {
      throw new Error('Unauthorized');
    }

    console.info('Successfully validated token');
    return buildPolicy('Allow', methodArn);
  } catch (e) {
    console.error('Failed to validate token', e);

    return buildPolicy('Deny', methodArn, 'Unauthorized');
  }
};

export default validateToken;
