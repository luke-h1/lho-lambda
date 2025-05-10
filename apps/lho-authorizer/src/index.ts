/* eslint-disable prefer-destructuring, no-console */
import {
  APIGatewayRequestAuthorizerEvent,
  APIGatewayAuthorizerResult,
  StatementEffect,
} from 'aws-lambda';

const sendDiscordNotification = async (message: string) => {
  if (!process.env.DISCORD_WEBHOOK_URL) {
    console.warn('Discord webhook URL not configured');
    return;
  }

  try {
    await fetch(process.env.DISCORD_WEBHOOK_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        content: message,
      }),
    });
  } catch (error) {
    console.error('Failed to send Discord notification:', error);
  }
};

export const handler = async (
  event: APIGatewayRequestAuthorizerEvent,
): Promise<APIGatewayAuthorizerResult> => {
  try {
    const consumer = event.headers?.['x-consumer'];
    const validConsumer = ['lhowsam-dev', 'lhowsam-prod', 'lhowsam-local'];

    const referer =
      event.headers?.referer || event.headers?.Referer || 'unknown';

    // eslint-disable-next-line prefer-destructuring
    const apiKey = event.headers?.['x-api-key'];

    if (apiKey !== process.env.API_KEY) {
      console.info('deny');
      await sendDiscordNotification(
        `🚫 Access denied for consumer: ${consumer ?? 'unknown'}. IP address: ${event?.requestContext?.identity?.sourceIp ?? 'UNKNOWN'} | Referer: ${referer} | env: ${process.env.ENVIRONMENT}`,
      );
      return generatePolicy('user', 'Deny', event.methodArn);
    }

    if (
      !validConsumer?.includes(consumer ?? '') &&
      process.env.API_KEY === apiKey
    ) {
      await sendDiscordNotification(
        `⚠️ Invalid consumer: ${consumer ?? 'unknown'} or referer: ${referer} with valid API key. IP address: ${event?.requestContext?.identity?.sourceIp ?? 'UNKNOWN'} | env: ${process.env.ENVIRONMENT}`,
      );
    }

    console.info('allow');
    return generatePolicy('user', 'Allow', event.methodArn);
  } catch (error) {
    // eslint-disable-next-line no-console
    console.error('Error in authorizer:', error);
    await sendDiscordNotification(
      `❌ Error in authorizer: ${error instanceof Error ? error.message : 'Unknown error'}`,
    );
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
