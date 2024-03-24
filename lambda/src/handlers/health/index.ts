const healthHandler = async (): Promise<string> => {
  return JSON.stringify({
    status: "OK",
    message: "Healthy",
  });
};
export default healthHandler;
