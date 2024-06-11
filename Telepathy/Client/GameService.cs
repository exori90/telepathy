using Common;
using Microsoft.Extensions.Hosting;
using System;

namespace Client;

public class GameService : IHostedService
{
    private readonly GameClient client;
    private Timer? _timer = null;

    public GameService()
    {
        // create client & hook up events
        // note that the message ArraySegment<byte> is only valid until returning (allocation free)
        client = new GameClient(1024);

        client.OnConnected = () => Log.Info("Client Connected");
        client.OnData = (message) => Log.Info("Client Data: " + BitConverter.ToString(message.Array, message.Offset, message.Count));
        client.OnDisconnected = () => Log.Info("Client Disconnected");
    }
    public Task StartAsync(CancellationToken stoppingToken)
    {
        // connect
        while (!client.Connected)
        {
            client.Connect("localhost", 1337);

            Thread.Sleep(100);
        }

        // tick to process incoming messages (do this in your update loop)
        // => limit parameter to avoid deadlocks!
        client.Tick(100);

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        Log.Info("[Telepathy] Send a message to server");

        // send a message to server
        byte[] message = [0xFF];
        client.Send(new ArraySegment<byte>(message));
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);

        // disconnect from the server when we are done
        client.Disconnect();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}