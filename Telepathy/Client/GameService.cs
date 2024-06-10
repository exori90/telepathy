using Microsoft.Extensions.Hosting;

namespace Client;

public class GameService : IHostedService
{
    private readonly GameClient client;

    public GameService()
    {
        // create client & hook up events
        // note that the message ArraySegment<byte> is only valid until returning (allocation free)
        client = new GameClient(1024);

        client.OnConnected = () => Console.WriteLine("Client Connected");
        client.OnData = (message) => Console.WriteLine("Client Data: " + BitConverter.ToString(message.Array, message.Offset, message.Count));
        client.OnDisconnected = () => Console.WriteLine("Client Disconnected");
    }
    public Task StartAsync(CancellationToken stoppingToken)
    {
        // connect
        client.Connect("localhost", 1337);

        // tick to process incoming messages (do this in your update loop)
        // => limit parameter to avoid deadlocks!
        client.Tick(100);

        // send a message to server
        byte[] message = [0xFF];
        client.Send(new ArraySegment<byte>(message));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        // disconnect from the server when we are done
        client.Disconnect();

        return Task.CompletedTask;
    }
}