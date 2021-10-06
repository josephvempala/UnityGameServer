﻿using Server.client;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Server
{
    internal static class Server
    {
        public static int max_clients { get; private set; }
        public static int port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int client_id, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static Socket TCPListner;
        private static Socket UDPListner;
        private static IPEndPoint ServerEndpoint;
        private static CancellationTokenSource cancellationTokenSource;

        public static void Start(int maximum_clients, int port_no)
        {
            max_clients = maximum_clients;
            port = port_no;
            cancellationTokenSource = new CancellationTokenSource();
            InitializePacketHandlers();

            ServerEndpoint = new IPEndPoint(IPAddress.Any, port);
            TCPListner = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TCPListner.Bind(ServerEndpoint);
            TCPListner.Listen(128);
            _ = Task.Run(TCPListen);

            UDPListner = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            UDPListner.Bind(ServerEndpoint);
            _ = Task.Run(UDPListen);
        }

        public static async Task SendUDPDataAsync(EndPoint endPoint, Packet packet)
        {
            try
            {
                if (endPoint != null)
                {
                    await UDPListner.SendToAsync(new ArraySegment<byte>(packet.ToArray()), SocketFlags.None, endPoint).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error occurred while sending UDP data to {endPoint} : {e}");
            }
        }

        public static void Stop()
        {
            cancellationTokenSource.Cancel();
            foreach (KeyValuePair<int, Client> client in clients)
            {
                client.Value.Disconnect();
            }
        }

        private static async Task UDPListen()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    ArraySegment<byte> data = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(Constants.MAX_BUFFER_SIZE));
                    SocketReceiveFromResult socketReceive = await UDPListner.ReceiveFromAsync(data, SocketFlags.None, ServerEndpoint).ConfigureAwait(false);
                    if (socketReceive.ReceivedBytes < 4)
                    {
                        continue;
                    }
                    int clientId = BitConverter.ToInt32(data.Array, 0);
                    if (clientId <= 0 || clientId > max_clients)
                    {
                        continue;
                    }
                    if (clients[clientId].Udp.endPoint == null)
                    {
                        clients[clientId].Udp.Connect(socketReceive.RemoteEndPoint);
                        continue;
                    }
                    if (socketReceive.RemoteEndPoint.ToString() == clients[clientId].Udp.endPoint.ToString())
                    {
                        byte[] packetBytes = ArrayPool<byte>.Shared.Rent(socketReceive.ReceivedBytes);
                        Buffer.BlockCopy(data.Array, 0, packetBytes, 0, socketReceive.ReceivedBytes);
                        ArrayPool<byte>.Shared.Return(data.Array);
                        clients[clientId].Udp.HandleData(packetBytes);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"Error occurred while listening for UDP packet: {e}");
                }
            }
        }

        private static async Task TCPListen()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                Socket item = await Task.Factory.FromAsync(TCPListner.BeginAccept, TCPListner.EndAccept, TCPListner).ConfigureAwait(false);
                Debug.Log($"{item.RemoteEndPoint} Connecting to server...");
                AddClient(item);
            }
        }

        private static void AddClient(Socket socket)
        {
            for (int i = 1; i <= max_clients; i++)
            {
                if (!clients.ContainsKey(i))
                {
                    Client client = new Client(i);
                    client.Tcp.Connect(socket);
                    clients.Add(i, client);
                    ServerSend.Welcome(i, "Welcome to the server");
                    return;
                }
            }

            Debug.Log($"${socket.RemoteEndPoint} failed to connect, too many players");
        }

        private static void InitializePacketHandlers()
        {
            packetHandlers = new Dictionary<int, PacketHandler>
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.message, ServerHandle.MessageReceived },
                { (int)ClientPackets.playerControls, ServerHandle.PlayerInput },
                { (int)ClientPackets.playerOrientation, ServerHandle.PlayerOrientation }
            };
        }
    }
}