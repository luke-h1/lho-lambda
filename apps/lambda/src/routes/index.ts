import healthHandler from '@lambda/handlers/health';
import nowPlayingHandler from '@lambda/handlers/now-playing';
import versionHandler from '@lambda/handlers/version';

const routes = async (path: string) => {
  let response: unknown;
  const includeCacheHeader =
    path === 'now-playing' || path === '/api/now-playing';

  const revalidate = 5;

  switch (path) {
    case 'health':
    case '/api/health':
      response = await healthHandler();
      break;

    case 'version':
    case '/api/version':
      response = await versionHandler();
      break;

    case 'now-playing':
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
