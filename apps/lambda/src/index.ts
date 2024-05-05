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
  const path = event.path.split('/')[3] as unknown as string;

  // AWSXRay.enableAutomaticMode();

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
