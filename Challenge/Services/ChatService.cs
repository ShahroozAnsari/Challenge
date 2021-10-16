using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text.Json;
using System.Threading.Tasks;
using Challenge.Protos;
using threading = System.Threading.Channels;

namespace Challenge
{
    public class ChatService : Protos.Chat.ChatBase
    {
        private readonly ILogger<ChatService> _logger;
        private threading.Channel<StreamingMessageResponse> responseChannel = threading.Channel.CreateUnbounded<StreamingMessageResponse>();
        private IServerStreamWriter<StreamingMessageResponse> responseStream;

        public ChatService(ILogger<ChatService> logger)
        {
            _logger = logger;
            Task.Run(async () =>
            {
                try
                {
                    await foreach (var message in responseChannel.Reader.ReadAllAsync())
                    {
                        await responseStream.WriteAsync(message);
                        _logger.LogInformation($"{Environment.NewLine}GRPC Response{Environment.NewLine}Method: SyncResponse {Environment.NewLine}Data: {JsonSerializer.Serialize(message)}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            });
        }


        public override async Task MessageChanel(IAsyncStreamReader<MessageRequest> requestStream,
            IServerStreamWriter<StreamingMessageResponse> responseStream,
            ServerCallContext context)
        {
            this.responseStream = responseStream;

            await foreach (var request in requestStream.ReadAllAsync())
            {


                _logger.LogInformation($"{Environment.NewLine}GRPC Request{Environment.NewLine}Method: SyncResponse {Environment.NewLine}Data: {JsonSerializer.Serialize(request)}");

                if (request.Content == "Hello")
                    _ = Task.Run(async () =>
                    {
                        var response = new StreamingMessageResponse
                        {
                            CorrelationId = request.CorrelationId,
                            MessageResponse = new MessageResponse { Content = "Hi" }
                        };
                        await Task.Delay(1000);
                        await responseChannel.Writer.WriteAsync(response);
                    });
                else if (request.Content == "Ping")
                {
                    _ = Task.Run(async () =>
                      {
                          var response = new StreamingMessageResponse
                          {
                              CorrelationId = request.CorrelationId,
                              MessageResponse = new MessageResponse { Content = "Pong" }
                          };
                          await responseChannel.Writer.WriteAsync(response);
                      });
                }
                else if (request.Content == "Bye")
                {
                    _ = Task.Run(async () =>
                    {
                        var response = new StreamingMessageResponse
                        {
                            CorrelationId = request.CorrelationId,
                            MessageResponse = new MessageResponse {Content = "Bye"}
                        };
                        await responseChannel.Writer.WriteAsync(response);
                    });
                    break;
                }
                else
                {
                    _ = Task.Run(async () =>
                    {
                        var response = new StreamingMessageResponse()
                        {
                            CorrelationId = request.CorrelationId,
                            Error = new Error { Code = (int)StatusCode.InvalidArgument, Message = "Invalid Request" },

                        };

                        await responseChannel.Writer.WriteAsync(response);
                        _logger.LogInformation(
                            $"{Environment.NewLine}GRPC Request{Environment.NewLine}Method: Exception {Environment.NewLine}Data: {response}");
                    });
                }

            }
            _logger.LogInformation($"{Environment.NewLine}GRPC Response{Environment.NewLine}Method: ConnectionClosed");
        }

    }
}
