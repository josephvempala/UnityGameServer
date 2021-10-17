using Server.client;

namespace Server
{
    public static class ServerSend
    {
        private static void SendTcpPacket(int clientId, Packet packet)
        {
            packet.WriteLength();
            _ = Server.Clients[clientId].Tcp.SendAsync(packet).ConfigureAwait(false);
        }

        private static void SendTCPPacketToAllButClient(int clientId, Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Server.Clients)
                if (client.Key != clientId)
                    _ = client.Value.Tcp.SendAsync(packet).ConfigureAwait(false);
        }

        private static void SendTCPPacketToAll(Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Server.Clients) _ = client.Value.Tcp.SendAsync(packet).ConfigureAwait(false);
        }

        private static void SendUDPPacket(int clientId, Packet packet)
        {
            _ = Server.Clients[clientId].Udp.SendAsync(packet).ConfigureAwait(false);
        }

        private static void SendUDPPacketToAllButClient(int clientId, Packet packet)
        {
            foreach (var client in Server.Clients)
                if (client.Key != clientId)
                    _ = client.Value.Udp.SendAsync(packet).ConfigureAwait(false);
        }

        private static void SendUdpPacketToAll(Packet packet)
        {
            foreach (var client in Server.Clients) _ = client.Value.Udp.SendAsync(packet).ConfigureAwait(false);
        }

        public static void Welcome(int clientId, string message)
        {
            using (var packet = new Packet((int) ServerPackets.Welcome))
            {
                packet.Write(clientId);
                packet.Write(message);
                SendTcpPacket(clientId, packet);
            }
        }

        public static void Message(int clientId, string message)
        {
            using (var packet = new Packet((int) ServerPackets.Message))
            {
                packet.Write(clientId);
                packet.Write(message);
                SendUdpPacketToAll(packet);
            }
        }

        public static void SpawnPlayer(int toClient, Player player)
        {
            using (var packet = new Packet((int) ServerPackets.SpawnPlayer))
            {
                packet.Write(player.id);
                packet.Write(player.username);
                packet.Write(player.transform.position);
                packet.Write(player.transform.rotation);

                SendTcpPacket(toClient, packet);
            }

            ;
        }

        public static void PlayerState(int client, Player player)
        {
            using (var packet = new Packet((int) ServerPackets.PlayerState))
            {
                packet.Write(player.id);
                packet.Write(player.tick);
                packet.Write(player.transform.localPosition);
                packet.Write(player.transform.localRotation);

                SendUdpPacketToAll(packet);
            }
        }
    }
}