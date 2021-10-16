using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Challenge.Protos;
using Grpc.Core;
using Grpc.Net.Client;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

using threading = System.Threading.Channels;

namespace ClientConsole
{
    public class Client : IClient
    {
        public delegate void MyEventHandler(object source, MessageEventArgs args);
        public event MyEventHandler MessageReceivedEvent;
        private bool connectionIsOpen = false;

        private Channel chanel = new("localhost:5000", ChannelCredentials.Insecure);
        private Chat.ChatClient client;
        private AsyncDuplexStreamingCall<MessageRequest, StreamingMessageResponse> call;

        private threading.Channel<MessageRequest> sendChannel = threading.Channel.CreateUnbounded<MessageRequest>();

        public Client()
        {
            client = new Chat.ChatClient(chanel);
            call = client.MessageChanel();
            connectionIsOpen = true;
            Task.Run(async () =>
            {
                try
                {
                    await foreach (var message in call.ResponseStream.ReadAllAsync())
                    {
                        MessageReceivedEvent.Invoke(call.ResponseStream, new MessageEventArgs { Message = message });
                    }
                }
                catch (RpcException ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                await call.RequestStream.CompleteAsync();
                
                connectionIsOpen = false;
                Console.WriteLine("Connection Closed");

            });
            Task.Run(async () =>
            {
                try
                {
                    await foreach (var message in sendChannel.Reader.ReadAllAsync())
                    {
               
                        await call.RequestStream.WriteAsync(message);
                        Console.WriteLine("Sended:" + message.Content);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            });

        }

        public async Task<string> SendAsync(string message)
        {
            if (connectionIsOpen == false)
                throw new Exception("Connection Is Close");
            var id = Guid.NewGuid().ToString();

            var responseTask = Observable.FromEventPattern<MyEventHandler, MessageEventArgs>(
                    h => MessageReceivedEvent += h,
                    h => MessageReceivedEvent -= h)
              
                .Where(a => a.EventArgs.Message.CorrelationId == id)
                .Timeout(new TimeSpan(0,0,10))
                .Select(ep => ep.EventArgs.Message)
                .FirstAsync();

            await sendChannel.Writer.WriteAsync(new MessageRequest() { Content = message, CorrelationId = id });
            var responseMessage = await responseTask;
            return HandelResponseMessage(responseMessage);
        }

        private string HandelResponseMessage(StreamingMessageResponse responseMessage)
        {
            if (responseMessage.MessageCase == StreamingMessageResponse.MessageOneofCase.Error)
                throw new Exception(responseMessage.Error.Message);
            return responseMessage.MessageResponse.Content;
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public StreamingMessageResponse Message;
    }

}


