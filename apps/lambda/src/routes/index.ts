import healthHandler from '@lambda/handlers/health';
import nowPlayingHandler from '@lambda/handlers/now-playing';
import versionHandler from '@lambda/handlers/version';

export type RoutePath = '/api/health' | '/api/version' | '/api/now-playing';

const routes = async (path: RoutePath) => {
  let response: unknown;
  const includeCacheHeader = path === '/api/now-playing';

  const revalidate = 2;

  switch (path) {
    /**
     * @see terraform/gateway.tf for a list of valid routes
     */
    case '/api/health':
      response = await healthHandler();
      break;

    case '/api/version':
      response = await versionHandler();
      break;

    case '/api/now-playing':
      response = await nowPlayingHandler();
      break;

    default:
      response = JSON.stringify({ message: 'route not found' }, null, 2);
      break;
  }

  return {
    statusCode: 200,
    headers: {
      'Content-Type': 'application/json',
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET,OPTIONS,POST,PUT,DELETE',
      'Cache-Control': includeCacheHeader
        ? `max-age=${revalidate}, s-maxage=${revalidate}, stale-while-revalidate=${revalidate}, stale-if-error=${revalidate}`
        : 'no-cache',
    },
    body: response,
  };
};
export default routes;
