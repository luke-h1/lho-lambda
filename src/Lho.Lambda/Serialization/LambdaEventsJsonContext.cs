using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json.Serialization;

namespace Lho.Lambda.Serialization;

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class LambdaEventsJsonContext : JsonSerializerContext;
