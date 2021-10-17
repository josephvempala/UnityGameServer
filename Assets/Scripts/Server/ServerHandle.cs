using UnityEngine;

namespace Server
{
    internal static class ServerHandle
    {
        public static void WelcomeReceived(int clientID, Packet packet)
        {
            var username = packet.ReadString();
            Debug.Log($"{clientID.ToString()} logged in with username: {username}");
            Server.Clients[clientID].SendIntoGame(username);
        }

        public static void MessageReceived(int clientID, Packet packet)
        {
            var message = packet.ReadString();
            Debug.Log($"{Server.Clients[clientID].Player.username}: {message}");
            ServerSend.Message(clientID, message);
        }

        public static void PlayerInput(int clientID, Packet packet)
        {
            var inputsLength = packet.ReadInt();
            var inputs = packet.ReadBytes(inputsLength);
            Server.Clients[clientID].Player.inputManager.ReceiveControls(inputs);
        }

        public static void PlayerOrientation(int clientID, Packet packet)
        {
            var inputsLength = packet.ReadInt();
            var orientation = packet.ReadBytes(inputsLength);
            Server.Clients[clientID].Player.inputManager.ReceiveOrientation(orientation);
        }
    }
}