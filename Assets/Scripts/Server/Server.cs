using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Server.client;
using UnityEngine;

namespace Server
{
    internal static class Server
    {
        public delegate void PacketHandler(int clientID, Packet packet);

        public static readonly ConcurrentDictionary<int, Client> Clients = new ConcurrentDictionary<int, Client>();

        public static Dictionary<int, PacketHandler> PacketHandlers;

        private static Socket _tcpListener;
        private static Socket _udpListener;
        private static IPEndPoint _serverEndpoint;
        private static CancellationTokenSource _cancellationTokenSource;
        public static int MAXClients { get; private set; }
        public static int Port { get; private set; }

        public static void Start(int maximumClients, int portNo)
        {
            MAXClients = maximumClients;
            Port = portNo;
            _cancellationTokenSource = new CancellationTokenSource();
            InitializePacketHandlers();

            _serverEndpoint = new IPEndPoint(IPAddress.Any, Port);
            _tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _tcpListener.Bind(_serverEndpoint);
            _tcpListener.Listen(128);
            _ = Task.Run(TcpListen);

            _udpListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _udpListener.Bind(_serverEndpoint);
            _ = Task.Run(UdpListen);
        }

        public static async Task SendUdpDataAsync(EndPoint endPoint, Packet packet)
        {
            try
            {
                if (endPoint != null)
                    await _udpListener.SendToAsync(new ArraySegment<byte>(packet.ToArray()), SocketFlags.None, endPoint)
                        .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.Log($"Error occurred while sending UDP data to {endPoint} : {e}");
            }
        }

        public static void Stop()
        {
            _cancellationTokenSource.Cancel();
            foreach (var client in Clients) TickManager.ExecuteOnTick(client.Value.Disconnect);
        }

        private static async Task UdpListen()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
                try
                {
                    var data = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(Constants.MAXBufferSize));
                    var socketReceive = await _udpListener.ReceiveFromAsync(data, SocketFlags.None, _serverEndpoint)
                        .ConfigureAwait(false);
                    if (socketReceive.ReceivedBytes < 4) continue;
                    var clientId = BitConverter.ToInt32(data.Array!, 0);
                    if (clientId <= 0 || clientId > MAXClients) continue;
                    if (Clients[clientId].Udp.EndPoint == null)
                    {
                        Clients[clientId].Udp.Connect(socketReceive.RemoteEndPoint);
                        continue;
                    }

                    if (socketReceive.RemoteEndPoint.ToString() == Clients[clientId].Udp.EndPoint.ToString())
                    {
                        var packetBytes = ArrayPool<byte>.Shared.Rent(socketReceive.ReceivedBytes);
                        Buffer.BlockCopy(data.Array!, 0, packetBytes, 0, socketReceive.ReceivedBytes);
                        ArrayPool<byte>.Shared.Return(data.Array);
                        Clients[clientId].Udp.HandleData(packetBytes);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"Error occurred while listening for UDP packet: {e}");
                }
        }

        private static async Task TcpListen()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var item = await Task.Factory.FromAsync(_tcpListener.BeginAccept, _tcpListener.EndAccept, _tcpListener)
                    .ConfigureAwait(false);
                Debug.Log($"{item.RemoteEndPoint} Connecting to server...");
                AddClient(item);
            }
        }

        private static void AddClient(Socket socket)
        {
            for (var i = 1; i <= MAXClients; i++)
                if (!Clients.ContainsKey(i))
                {
                    var client = new Client(i);
                    client.Tcp.Connect(socket);
                    Clients.TryAdd(i, client);
                    ServerSend.Welcome(i, "Welcome to the server");
                    return;
                }

            Debug.Log($"${socket.RemoteEndPoint} failed to connect, too many players");
        }

        private static void InitializePacketHandlers()
        {
            PacketHandlers = new Dictionary<int, PacketHandler>
            {
                {(int) ClientPackets.WelcomeReceived, ServerHandle.WelcomeReceived},
                {(int) ClientPackets.Message, ServerHandle.MessageReceived},
                {(int) ClientPackets.PlayerControls, ServerHandle.PlayerInput},
                {(int) ClientPackets.PlayerOrientation, ServerHandle.PlayerOrientation}
            };
        }
    }
}