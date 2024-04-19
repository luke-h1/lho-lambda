import { APIGatewayProxyEvent, Context, Handler } from 'aws-lambda';
import AWSXRay from 'aws-xray-sdk';
import routes from './routes';
import buildPath from './utils/buildPath';
import isErrorLike from './utils/isErrorLike';
import LambdaError from './utils/lambdaError';
import lambdaTimeout from './utils/lambdaTimeout';

export const handler: Handler = async (
  event: APIGatewayProxyEvent,
  context: Context,
) => {
  const path =
    // path can either be the last part of the path or the routeKey
    // depending on whether the function is executed from aws or a http call comes thru from the http gateway
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-ignore
    event.requestContext?.path?.split('/').pop() ??
    // @ts-expect-error fix typings for path
    event.routeKey ??
    // @ts-expect-error fix typings for path
    event.rawPath;

  AWSXRay.enableAutomaticMode();

  console.log('path is', path);

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
