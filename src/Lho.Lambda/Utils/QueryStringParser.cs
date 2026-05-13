using System.Web;

namespace Lho.Lambda.Utils;

public static class QueryStringParser
{
  public static string Get(string? rawQueryString, string key, string defaultValue, IReadOnlyCollection<string>? allowedValues = null)
  {
    if (string.IsNullOrEmpty(rawQueryString))
    {
      return defaultValue;
    }

    var query = HttpUtility.ParseQueryString(rawQueryString);
    var value = query[key];
    if (string.IsNullOrEmpty(value))
    {
      return defaultValue;
    }

    return allowedValues is not null && !allowedValues.Contains(value) ? defaultValue : value;
  }
}
