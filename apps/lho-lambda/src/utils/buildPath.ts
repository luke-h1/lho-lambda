import { LambdaActions } from './lambdaError';

const buildPath = (path: string): LambdaActions => {
  switch (path) {
    case 'now-playing':
    case '/api/now-playing':
      return 'nowPlaying';

    case 'health':
    case '/api/health':
      return 'health';

    case 'version':
    case '/api/version':
      return 'version';

    default:
      return 'unknown';
  }
};
export default buildPath;
