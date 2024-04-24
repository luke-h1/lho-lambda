const healthHandler = async (): Promise<string> => {
  return JSON.stringify({
    status: 'OK',
    deployedBy: process.env.DEPLOYED_BY,
    deployedAt: process.env.DEPLOYED_AT,
  });
};
export default healthHandler;
