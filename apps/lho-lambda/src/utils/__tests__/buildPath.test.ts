import buildPath from '@lambda/utils/buildPath';

describe('buildPath', () => {
  test('should return `nowPlaying` for now-playing path', () => {
    const result = buildPath('/api/now-playing');
    expect(result).toEqual('nowPlaying');
  });

  test('should return `health` for health path', () => {
    const result = buildPath('/api/health');
    expect(result).toEqual('health');
  });
  test('should return `version` for version endpoint', () => {
    const result = buildPath('/api/version');
    expect(result).toEqual('version');
  });

  test('should return `unknown` for un-registered paths', () => {
    const result = buildPath('/123/123');
    expect(result).toEqual('unknown');
  });
});
