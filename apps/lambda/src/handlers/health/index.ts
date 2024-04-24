const healthHandler = async (): Promise<string> => {
  return JSON.stringify({
    status: 'OK',
  });
};
export default healthHandler;
