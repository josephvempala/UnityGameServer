using System.Text;
using UnityEngine;

namespace Server
{
    internal static class ServerHandle
    {
        public static void WelcomeReceived(int client_id, Packet packet)
        {
            string username = packet.ReadString();
            Debug.Log($"{client_id} logged in with username: {username}");
            Server.clients[client_id].SendIntoGame(username);
        }

        public static void MessageReceived(int client_id, Packet packet)
        {
            string message = packet.ReadString();
            Debug.Log($"{Server.clients[client_id].player.username}: {message}");
            ServerSend.Message(client_id, message);
        }

        public static void PlayerInput(int client_id, Packet packet)
        {
            var inputsLength = packet.ReadInt();
            byte[] inputs = packet.ReadBytes(inputsLength);
            Server.clients[client_id].player.inputManager.RecieveControls(inputs);
        }
    }
}
