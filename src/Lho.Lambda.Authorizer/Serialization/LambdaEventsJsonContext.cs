using System.Text.Json.Serialization;
using Lho.Lambda.Authorizer.Models;

namespace Lho.Lambda.Authorizer.Serialization;

[JsonSerializable(typeof(AuthorizerRequest))]
[JsonSerializable(typeof(AuthorizerSimpleResponse))]
public partial class LambdaEventsJsonContext : JsonSerializerContext;
