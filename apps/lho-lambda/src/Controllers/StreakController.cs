using Microsoft.AspNetCore.Mvc;

namespace LhoLambda.Controllers;

[ApiController]
public class StreakController : ControllerBase
{
  [HttpGet("api/streak")]
  public IActionResult Streak()
  {
    var targetDates = new[]
    {
      new DateTime(2025, 6, 20),
      new DateTime(2025, 7, 2),
      new DateTime(2025, 7, 3)
    };

    var distances = targetDates.Select(date => new
    {
      date = date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
      distance = GetDistanceToNow(date)
    }).ToArray();

    var response = new
    {
      distances
    };

    Response.Headers.Append("Access-Control-Allow-Origin", "*");
    Response.Headers.Append("Access-Control-Allow-Methods", "GET,OPTIONS,POST,PUT,DELETE");
    Response.Headers.Append("Cache-Control", "no-cache");

    return Ok(response);
  }

  private static string GetDistanceToNow(DateTime targetDate)
  {
    var now = DateTime.UtcNow;
    var timeSpan = targetDate - now;

    if (timeSpan.TotalDays > 365)
    {
      var years = (int)(timeSpan.TotalDays / 365);
      return $"in {years} year{(years == 1 ? "" : "s")}";
    }

    if (timeSpan.TotalDays > 30)
    {
      var months = (int)(timeSpan.TotalDays / 30);
      return $"in {months} month{(months == 1 ? "" : "s")}";
    }

    if (timeSpan.TotalDays >= 1)
    {
      var days = (int)timeSpan.TotalDays;
      return $"in {days} day{(days == 1 ? "" : "s")}";
    }

    if (timeSpan.TotalHours >= 1)
    {
      var hours = (int)timeSpan.TotalHours;
      return $"in {hours} hour{(hours == 1 ? "" : "s")}";
    }

    if (timeSpan.TotalMinutes >= 1)
    {
      var minutes = (int)timeSpan.TotalMinutes;
      return $"in {minutes} minute{(minutes == 1 ? "" : "s")}";
    }

    // If the date is in the past
    if (timeSpan.TotalSeconds < 0)
    {
      var absTimeSpan = now - targetDate;

      if (absTimeSpan.TotalDays > 365)
      {
        var years = (int)(absTimeSpan.TotalDays / 365);
        return $"{years} year{(years == 1 ? "" : "s")} ago";
      }

      if (absTimeSpan.TotalDays > 30)
      {
        var months = (int)(absTimeSpan.TotalDays / 30);
        return $"{months} month{(months == 1 ? "" : "s")} ago";
      }

      if (absTimeSpan.TotalDays >= 1)
      {
        var days = (int)absTimeSpan.TotalDays;
        return $"{days} day{(days == 1 ? "" : "s")} ago";
      }

      if (absTimeSpan.TotalHours >= 1)
      {
        var hours = (int)absTimeSpan.TotalHours;
        return $"{hours} hour{(hours == 1 ? "" : "s")} ago";
      }

      if (absTimeSpan.TotalMinutes >= 1)
      {
        var minutes = (int)absTimeSpan.TotalMinutes;
        return $"{minutes} minute{(minutes == 1 ? "" : "s")} ago";
      }
    }

    return "now";
  }
}
