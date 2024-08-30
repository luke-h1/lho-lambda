import pkg from '../../../package.json';

const versionHandler = async () => {
  return JSON.stringify(
    {
      version: pkg.version,
      deployedBy: process.env.DEPLOYED_BY,
      deployedAt: process.env.DEPLOYED_AT,
      gitSha: process.env.GIT_SHA ?? 'unknown',
    },
    null,
    2,
  );
};
export default versionHandler;
