import healthHandler from '@lambda/handlers/health';
import nowPlayingHandler from '@lambda/handlers/now-playing';
import versionHandler from '@lambda/handlers/version';
import routes from '@lambda/routes';

describe('routes', () => {
  test('should return health response for health route', async () => {
    const result = await routes('health');

    expect(result).toEqual({
      statusCode: 200,
      headers: expect.any(Object),
      body: await healthHandler(),
    });

    expect(result.headers['Cache-Control']).toEqual('no-cache');
  });

  test('should return version response for version route', async () => {
    const result = await routes('version');

    expect(result).toEqual({
      statusCode: 200,
      headers: expect.any(Object),
      body: await versionHandler(),
    });

    expect(result.headers['Cache-Control']).toEqual('no-cache');
  });

  test('should return now-playing response for now-playing route', async () => {
    const result = await routes('now-playing');
    expect(result).toEqual({
      statusCode: 200,
      headers: expect.any(Object),
      body: await nowPlayingHandler(),
    });
    expect(result.headers['Cache-Control']).toEqual(
      'max-age=5, s-maxage=5, stale-while-revalidate=5, stale-if-error=5',
    );
  });

  test('should return 404 if route is not found', async () => {
    const result = await routes('/123/123');
    expect(result).toEqual({
      statusCode: 200,
      headers: expect.any(Object),
      body: JSON.stringify({ message: 'route not found' }, null, 2),
    });
    expect(result.headers['Cache-Control']).toEqual('no-cache');
  });
});
