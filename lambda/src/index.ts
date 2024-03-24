import { APIGatewayProxyEvent, Context, Handler } from "aws-lambda";
import lambdaTimeout from "./utils/lambdaTimeout";
import AWSXRay from "aws-xray-sdk";
import routes from "./routes";

export const handler: Handler = async (
  event: APIGatewayProxyEvent,
  context: Context
) => {
  console.log("event", event);
  const path =
    // path can either be the last part of the path or the routeKey
    // depending on whether the function is executed from aws or a http call comes thru from the http gateway
    // @ts-ignore
    event.requestContext?.path?.split("/").pop() ??
    // @ts-expect-error fix typings for path
    event.routeKey ??
    // @ts-expect-error fix typings for path
    event.rawPath;

  AWSXRay.enableAutomaticMode();

  try {
    return await Promise.race([routes(path), lambdaTimeout(context)]).then(
      (value) => value
    );
  } catch (e) {
    return {
      statusCode: 500,
      headers: {
        "Content-Type": "application/json",
        "Access-Control-Allow-Origin": "*",
      },
      // @ts-ignore fix typings for e
      body: JSON.stringify({ message: `lambda errored with error ${e}` }),
    };
  }
};
