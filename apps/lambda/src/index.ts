/* eslint-disable prefer-destructuring */
import { APIGatewayProxyEvent, Context, Handler } from 'aws-lambda';
// import AWSXRay from 'aws-xray-sdk';
import routes from './routes';
import buildPath from './utils/buildPath';
import isErrorLike from './utils/isErrorLike';
import LambdaError from './utils/lambdaError';
import lambdaTimeout from './utils/lambdaTimeout';

export const handler: Handler = async (
  event: APIGatewayProxyEvent,
  context: Context,
) => {
  // AWSXRay.enableAutomaticMode();
  // eslint-disable-next-line @typescript-eslint/ban-ts-comment
  // @ts-ignore
  const path = event.rawPath;

  try {
    return await Promise.race([routes(path), lambdaTimeout(context)]).then(
      value => value,
    );
  } catch (e) {
    const errorBody = isErrorLike(e)
      ? new LambdaError({
          message: e.message,
          name: e.name,
          code: 500,
          action: buildPath(path),
          statusCode: 500,
          stack: e.stack,
        })
      : e;

    return {
      statusCode: 500,
      headers: {
        'Content-Type': 'application/json',
        'Access-Control-Allow-Origin': '*',
      },
      body: errorBody,
    };
  }
};
