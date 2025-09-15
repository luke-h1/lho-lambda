using System.Reflection;
using LhoLambda.Models;
using Microsoft.AspNetCore.Mvc;

namespace LhoLambda.Controllers;

[ApiController]
public class VersionController : ControllerBase
{
  [HttpGet("api/version")]
  public ActionResult<VersionResponse> Version()
  {
    // Try to get version from environment variable first, then fall back to assembly version
    var version = Environment.GetEnvironmentVariable("VERSION") 
                  ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() 
                  ?? "Unknown";

    var response = new VersionResponse
    {
      Version = version,
      DeployedAt = Environment.GetEnvironmentVariable("DEPLOYED_AT"),
      DeployedBy = Environment.GetEnvironmentVariable("DEPLOYED_BY"),
      GitSha = Environment.GetEnvironmentVariable("GIT_SHA") ?? "unknown"
    };

    Response.Headers.Append("Access-Control-Allow-Origin", "*");
    Response.Headers.Append("Access-Control-Allow-Methods", "GET,OPTIONS,POST,PUT,DELETE");
    Response.Headers.Append("Cache-Control", "no-cache");

    return Ok(response);
  }
}
