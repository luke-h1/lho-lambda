import { APIGatewayProxyEvent, Context, Handler } from 'aws-lambda';
import routes from './routes';
import buildPath from './utils/buildPath';
import isErrorLike from './utils/isErrorLike';
import LambdaError from './utils/lambdaError';
import lambdaTimeout from './utils/lambdaTimeout';

export const handler: Handler = async (
  event: APIGatewayProxyEvent,
  context: Context,
) => {
  /**
   * The incoming request path can either be the last part of the req ctx path, the route key
   * or the rawPath depending on whether the function is executed from aws or the lambda is
   * executed via a HTTP request via API gateway
   */

  const path =
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-ignore
    event.requestContext?.path?.split('/').pop() ??
    // @ts-expect-error missing aws-lambda types
    event.routeKey ??
    // @ts-expect-error missing aws-lambda types
    event.rawPath;

  // AWSXRay.enableAutomaticMode();

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
