using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using SecApiReportDataLoader.Models;
using SecApiReportDataLoader.Services;
using System;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SecApiReportDataLoader
{
    public class Function
    {
        private readonly IDeserializer _deserializer;
        private readonly ReportDataLoaderService _reportDataLoaderService;

        public Function()
        {
            _deserializer = new Deserializer();
            _reportDataLoaderService = new ReportDataLoaderService();
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var msg in evnt.Records)
            {
                await ProcessMessageAsync(msg, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage msg, ILambdaContext context)
        {
            void Log(string logMsg)
            {
                context.Logger.LogLine($"[RequestId: {context.AwsRequestId}]: {logMsg}");
            }

            Log($">>>>> Processing message {msg.Body}");

            LambdaTriggerMessage triggerMessage = null;
            try
            {
                triggerMessage = _deserializer.Get(msg);
            }
            catch (Exception ex)
            {
                Log($"Failed to deserialize lambda's trigger message: {ex}");
                return;
            }

            await _reportDataLoaderService.Load(triggerMessage, Log);

            Log($"Finished processing. <<<<<");
        }
    }
}
