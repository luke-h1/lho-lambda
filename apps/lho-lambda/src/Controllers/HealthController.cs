using Microsoft.AspNetCore.Mvc;

namespace LhoLambda.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
  [HttpGet("api/health")]
  public IActionResult Health()
  {
    var response = new
    {
      status = "OK"
    };

    Response.Headers.Append("Access-Control-Allow-Origin", "*");
    Response.Headers.Append("Access-Control-Allow-Methods", "GET,OPTIONS,POST,PUT,DELETE");
    Response.Headers.Append("Cache-Control", "no-cache");

    return Ok(response);
  }
}
