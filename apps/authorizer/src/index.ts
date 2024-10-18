import { APIGatewayProxyEvent, Handler } from 'aws-lambda';

export const handler: Handler = async (event: APIGatewayProxyEvent) => {
  const response = {
    isAuthorized: false,
    context: {},
  };

  if (event.headers['x-api-key'] === process.env.API_KEY) {
    response.isAuthorized = true;
  }

  return response;
};
