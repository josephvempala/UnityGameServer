using System;
using System.Buffers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Server.client
{
    public class TCP
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly int _id;
        private Packet _receivedData;
        private Socket _socket;
        private NetworkStream _stream;

        public TCP(int id)
        {
            this._id = id;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Connect(Socket socket)
        {
            this._socket = socket;
            socket.ReceiveBufferSize = Constants.MAXBufferSize;
            socket.SendBufferSize = Constants.MAXBufferSize;

            _stream = new NetworkStream(socket);

            _ = Task.Run(ReceiveLoop);
        }

        public async Task SendAsync(Packet packet)
        {
            var buffer = packet.ToArray();
            await _stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
        }

        private async Task ReceiveLoop()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    _receivedData = new Packet();
                    var receiveBuffer = ArrayPool<byte>.Shared.Rent(Constants.MAXBufferSize);
                    var bytesRead = await _stream.ReadAsync(receiveBuffer, 0, Constants.MAXBufferSize)
                        .ConfigureAwait(false);
                    if (bytesRead == 0)
                    {
                        TickManager.ExecuteOnTick(() => { Server.Clients[_id].Disconnect(); });
                        return;
                    }

                    var dataRead = ArrayPool<byte>.Shared.Rent(bytesRead);
                    Buffer.BlockCopy(receiveBuffer, 0, dataRead, 0, bytesRead);
                    _receivedData.Reset(HandleData(dataRead));
                }
            }
            catch (Exception ex)
            {
                TickManager.ExecuteOnTick(() => { Server.Clients[_id].Disconnect(); });
                Debug.Log(ex.Message);
            }
        }

        private bool HandleData(byte[] data)
        {
            var packetLength = 0;
            _receivedData.SetBytes(data);

            if (_receivedData.UnreadLength >= 4)
            {
                packetLength = _receivedData.ReadInt();
                if (packetLength == 0)
                {
                    ArrayPool<byte>.Shared.Return(data);
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= _receivedData.UnreadLength)
            {
                var packetBytes = _receivedData.ReadBytes(packetLength);
                var packet = new Packet(packetBytes);
                var packetID = packet.ReadInt();
                if (Server.PacketHandlers.ContainsKey(packetID))
                    TickManager.ExecuteOnTick(() =>
                    {
                        Server.PacketHandlers[packetID].Invoke(_id, packet);
                        packet.Dispose();
                    });

                packetLength = 0;
                if (_receivedData.UnreadLength >= 4)
                {
                    packetLength = _receivedData.ReadInt();
                    if (packetLength == 0)
                    {
                        ArrayPool<byte>.Shared.Return(data);
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                ArrayPool<byte>.Shared.Return(data);
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            _cancellationTokenSource.Cancel();
            _socket.Close();
            _stream.Close();
            _receivedData.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}