using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Server.client
{
    public class Client
    {
        private readonly int _id;
        public Player Player;
        public readonly TCP Tcp;
        public readonly UDP Udp;

        public Client(int clientID)
        {
            _id = clientID;
            Tcp = new TCP(_id);
            Udp = new UDP(_id);
        }

        public void SendIntoGame(string playerName)
        {
            Player = ServerManager.Instance.InstantiatePlayer();
            Player.Initialize(_id, playerName);

            foreach (var client in Server.Clients.Values)
                if (client.Player != null && client._id != _id)
                    ServerSend.SpawnPlayer(_id, client.Player);

            foreach (var client in Server.Clients.Values)
                if (client.Player != null)
                    ServerSend.SpawnPlayer(client._id, Player);
        }

        public void Disconnect()
        {
            Tcp.Disconnect();
            Udp.Disconnect();
            try
            {
                Object.Destroy(Player.gameObject);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }

            Debug.Log($"{Player.username} has left");
            Player = null;
            Server.Clients.TryRemove(_id, out var client);
        }
    }
}