
using System;
using UnityEngine;

namespace Server.client
{
    public class Client
    {
        private int id;
        public TCP Tcp;
        public UDP Udp;
        public Player player;

        public Client(int client_id)
        {
            id = client_id;
            Tcp = new TCP(id);
            Udp = new UDP(id);
        }

        public void SendIntoGame(string playername)
        {
            player = ServerManager.instance.InstantiatePlayer();
            player.Initialize(id, playername);

            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null && client.id != id)
                {
                    ServerSend.SpawnPlayer(id, client.player);
                }
            }

            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    ServerSend.SpawnPlayer(client.id, player);
                }
            }
        }

        public void Disconnect()
        {
            Tcp.Disconnect();
            Udp.Disconnect();
            try
            {
                UnityEngine.Object.Destroy(player.gameObject);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
            Debug.Log($"{player.username} has left");
            player = null;
            Server.clients.TryRemove(id, out Client client);
        }
    }
}
