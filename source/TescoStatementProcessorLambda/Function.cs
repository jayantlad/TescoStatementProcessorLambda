using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TescoStatementHandler.Factories;
using TescoStatementProcessorLambda.Dtos;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TescoStatementProcessorLambda;

public class Function
{
    private static IHost _host;
    private static IStatementProcessor _statementProcessor;

    public Function()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddDefaultAWSOptions(_.Configuration.GetAWSOptions());
                services.AddAWSService<IAmazonS3>();
                services.AddAWSService<IAmazonDynamoDB>();
                services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
                services.AddScoped<IStatementProcessor, StatementProcessor>();
                services.AddScoped<IStatementRespository, StatementRespository>();
                services.AddScoped<IStatementFactory, StatementFactory>();
            })
            .Build();

        _statementProcessor = _host.Services.GetRequiredService<IStatementProcessor>();
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandlerAsync(Event input, ILambdaContext context)
    {
        await _statementProcessor.ProcessAsync(input, new CancellationToken());
    }
}