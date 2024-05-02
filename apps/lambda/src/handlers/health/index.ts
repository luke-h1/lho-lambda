const healthHandler = async (): Promise<string> => {
  return JSON.stringify(
    {
      status: 'OK',
    },
    null,
    2,
  );
};
export default healthHandler;
