using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Lho.Lambda.Serialization;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<LambdaEventsJsonContext>))]
