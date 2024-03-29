import { LambdaActions } from './lambdaError';

const buildPath = (path: string): LambdaActions | string => {
  switch (path) {
    case 'now-playing' || '/api/now-playing':
      return 'nowPlaying';
    case 'health' || '/api/health':
      return 'health';

    case 'version' || '/api/version':
      return 'version';

    default:
      return path;
  }
};
export default buildPath;
