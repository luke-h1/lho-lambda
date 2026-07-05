using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Lho.Lambda.Authorizer.Serialization;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<LambdaEventsJsonContext>))]
