import { Context } from "aws-lambda";

const lambdaTimeout = (context: Context) =>
  new Promise((res, rej) => {
    setTimeout(() => {
      rej("Lambda timed out");
    }, context.getRemainingTimeInMillis() - 1000);
  });
export default lambdaTimeout;
