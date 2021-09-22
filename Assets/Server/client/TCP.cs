﻿using System;
using System.Buffers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Server.client
{
    public class TCP
    {
        private CancellationTokenSource cancellationTokenSource;
        private Socket socket;
        private NetworkStream stream;
        private Packet receivedData;
        private int id;

        public TCP(int id)
        {
            this.id = id;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void Connect(Socket socket)
        {
            this.socket = socket;
            socket.ReceiveBufferSize = Constants.MAX_BUFFER_SIZE;
            socket.SendBufferSize = Constants.MAX_BUFFER_SIZE;

            stream = new NetworkStream(socket);

            _ = Task.Run(ReceiveLoop);
        }

        public async Task SendAsync(Packet packet)
        {
            byte[] buffer = packet.ToArray();
            await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
        }

        private async Task ReceiveLoop()
        {
            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    receivedData = new Packet();
                    byte[] receiveBuffer = ArrayPool<byte>.Shared.Rent(Constants.MAX_BUFFER_SIZE);
                    int bytes_read = await stream.ReadAsync(receiveBuffer, 0, Constants.MAX_BUFFER_SIZE).ConfigureAwait(false);
                    if (bytes_read == 0)
                    {
                        Server.clients[id].Disconnect();
                        continue;
                    }

                    byte[] data_read = ArrayPool<byte>.Shared.Rent(bytes_read);
                    Buffer.BlockCopy(receiveBuffer, 0, data_read, 0, bytes_read);
                    receivedData.Reset(HandleData(data_read));
                }
            }
            catch (Exception ex)
            {
                Server.clients[id].Disconnect();
                Debug.Log(ex.Message);
            }

        }

        private bool HandleData(byte[] data)
        {
            int packet_length = 0;
            receivedData.SetBytes(data);

            if (receivedData.UnreadLength >= 4)
            {
                packet_length = receivedData.ReadInt();
                if (packet_length == 0)
                {
                    ArrayPool<byte>.Shared.Return(data);
                    return true;
                }
            }
            while (packet_length > 0 && packet_length <= receivedData.UnreadLength)
            {
                byte[] packet_Bytes = receivedData.ReadBytes(packet_length);
                Packet packet = new Packet(packet_Bytes);
                int packet_id = packet.ReadInt();
                if (Server.packetHandlers.ContainsKey(packet_id))
                {
                    TickManager.ExecuteOnTick(() =>
                    {
                        Server.packetHandlers[packet_id].Invoke(id, packet);
                        packet.Dispose();
                    });
                }

                packet_length = 0;
                if (receivedData.UnreadLength >= 4)
                {
                    packet_length = receivedData.ReadInt();
                    if (packet_length == 0)
                    {
                        ArrayPool<byte>.Shared.Return(data);
                        return true;
                    }
                }
            }
            if (packet_length <= 1)
            {
                ArrayPool<byte>.Shared.Return(data);
                return true;
            }
            return false;
        }

        public void Disconnect()
        {
            cancellationTokenSource.Cancel();
            socket.Close();
            stream.Close();
            receivedData.Dispose();
        }
    }
}
