import healthHandler from "@lambda/handlers/health";
import nowPlayingHandler from "@lambda/handlers/now-playing";
import versionHandler from "@lambda/handlers/version";

const routes = async (path: string) => {
  let response: unknown;
  const includeCacheHeader = path === "now-playing";
  const revalidate = 6;

  console.log("path is", path);

  switch (path) {
    case "health" || "/api/health":
      response = await healthHandler();
      break;

    case "version" || "/api/version":
      response = await versionHandler();
      break;

    case "now-playing" || "/api/now-playing":
      response = await nowPlayingHandler();
      break;

    default:
      response = JSON.stringify({ message: "route not found" });
      break;
  }

  return {
    statusCode: 200,
    headers: {
      "Content-Type": "application/json",
      "Access-Control-Allow-Origin": "*",
      "Access-Control-Allow-Methods": "GET,OPTIONS,POST,PUT,DELETE",
      "Cache-Control": includeCacheHeader
        ? `max-age=${revalidate}, s-maxage=${revalidate}, stale-while-revalidate=${revalidate}, stale-if-error=${revalidate}`
        : "no-cache",
    },
    body: response,
  };
};
export default routes;
