using System.Buffers;
using System.Net;
using System.Threading.Tasks;

namespace Server.client
{
    public class UDP
    {
        private readonly int _id;
        public EndPoint EndPoint;

        public UDP(int id)
        {
            this._id = id;
        }

        public void Connect(EndPoint endpoint)
        {
            EndPoint = endpoint;
        }

        public async Task SendAsync(Packet packet)
        {
            await Server.SendUdpDataAsync(EndPoint, packet).ConfigureAwait(false);
        }

        public void HandleData(byte[] data)
        {
            var packet = new Packet(data);
            var clientId = packet.ReadInt();
            var packetId = packet.ReadInt();
            if (Server.PacketHandlers.ContainsKey(packetId))
                TickManager.ExecuteOnTick(() =>
                {
                    Server.PacketHandlers[packetId].Invoke(_id, packet);
                    ArrayPool<byte>.Shared.Return(data);
                    packet.Dispose();
                });
        }

        public void Disconnect()
        {
            EndPoint = null;
        }
    }
}