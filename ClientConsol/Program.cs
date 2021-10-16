using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Challenge.Protos;
using Grpc.Core;

namespace ClientConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
          await Task.Delay(3000);
          Console.WriteLine("Start");
          IClient client = new Client();
          List<Task> tasks = new List<Task>();
          
            for (int i = 0; i < 5; i++)
            {
          
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var message = await client.SendAsync("Hello");
                        Console.WriteLine("Server: " + message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Exception:{e}");

                    }
                }));
          
          
            }
          
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(Task.Run(async () =>
                 {
                        try
                        {
                            var message = await client.SendAsync("Ping");
                            Console.WriteLine("Server: " + message);
                        }
                        catch (Exception e)
                        {
                         Console.WriteLine($"Exception:{e}");

                     }
                 }));
            }
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    tasks.Add(Task.Run(async () =>
                    {
          
                        try
                        {
                            var messagere = await client.SendAsync("error");
                            Console.WriteLine("Server: " + messagere);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Exception:{e}");

                        }
                    }));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception:{e}");

                }
          
            }
            await Task.WhenAll(tasks.OrderBy(a=>Guid.NewGuid()));
            try
            {
                var message = await client.SendAsync("Bye");
                Console.WriteLine("Server: " + message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception:{e}");

            }
            try
            {
                var message = await client.SendAsync("Ping");
                Console.WriteLine("Server: " + message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception:{e}");

            }
            try
            {
                var message = await client.SendAsync("Ping");
                Console.WriteLine("Server: " + message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception:{e}");

            }
            Console.ReadKey();
        }
    }
}

