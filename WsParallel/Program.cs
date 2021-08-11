using System;
using System.Net.WebSockets;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace WsParallel
{
    public class Program
    {
        static string Url;
        static int Sockets;
        static int counter = 0;

        static async Task Connect()
        {
            Console.WriteLine($"Starting {++counter}");
            var ws = new ClientWebSocket();
            var url = new Uri(Url);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(3000);
            var s = Stopwatch.StartNew();

            try
            {
                await ws.ConnectAsync(url, cts.Token);

                if (ws.State == WebSocketState.Open)
                {
                    Console.WriteLine($"Connected at {s.ElapsedMilliseconds}");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cts.Token);
                }
                else
                    Console.WriteLine($"Failed at {s.ElapsedMilliseconds}");
            }
            catch (OperationCanceledException oe)
            {
                Console.WriteLine($"Connect async took long, exception message: {oe.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Canceled: {cts.IsCancellationRequested}");
                Console.WriteLine($"Exception at {s.ElapsedMilliseconds}, message: {e.Message}, exception: {e.InnerException}");
            }
            s.Stop();
            if (!cts.IsCancellationRequested)
                Console.WriteLine($"Closed at {s.ElapsedMilliseconds}");
            cts.Dispose();
            ws.Dispose();
        }

        static async Task Test()
        {

            try
            {
                int[] arr = new int[Sockets];
                Console.WriteLine($"List Count: {arr.Count()}");
                var tasks = arr.Select(a => Connect());
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static async Task Main(string[] args)
        {
            try
            {
                Url = args[0];
                Sockets = Convert.ToInt32(args[1]);

                await Test();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("End");
        }
    }
}
