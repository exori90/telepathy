using Common;
using Microsoft.Extensions.Hosting;

namespace Server;

public class GameService : IHostedService
{
    public readonly GameServer server;
    private Timer? _timer = null;

    public GameService()
    {
        // create server & hook up events
        // note that the message ArraySegment<byte> is only valid until returning (allocation free)
        server = new GameServer(1024);

        server.OnConnected = (connectionId) => Log.Info(connectionId + " Connected");
        server.OnData = (connectionId, message) => Log.Info(connectionId + " Data: " + BitConverter.ToString(message.Array, message.Offset, message.Count));
        server.OnDisconnected = (connectionId) => Log.Info(connectionId + " Disconnected");
    }
    public Task StartAsync(CancellationToken stoppingToken)
    {
        // start
        server.Start(1337);

        // tick to process incoming messages (do this in your update loop)
        // => limit parameter to avoid deadlocks!
        server.Tick(100);

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        Log.Info("[Telepathy] Send a message to client 0");

        // send a message to client with connectionId = 0 (first one)
        byte[] message = [0x42, 0x13, 0x37];
        server.Send(0, new ArraySegment<byte>(message));
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);

        // stop the server when you don't need it anymore
        server.Stop();

        return Task.CompletedTask;
    }
}