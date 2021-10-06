using System.IO;
using UnityEngine;

public struct Controls
{
    public uint tick;
    public Vector2 horizontalMovement;
    public bool jump;
    public bool crouch;
    public bool walk;

    public void Serialize(ref byte[] controls)
    {
        MemoryStream stream = new MemoryStream(controls);
        int initialPosition = (int)stream.Position;
        using (BinaryWriter packet = new BinaryWriter(stream))
        {
            packet.Write(tick);
            packet.Write(horizontalMovement.x);
            packet.Write(horizontalMovement.y);
            packet.Write(jump);
            packet.Write(crouch);
            packet.Write(walk);
        }
    }

    public void Deserialize(in byte[] controls)
    {
        MemoryStream stream = new MemoryStream(controls);
        using (BinaryReader packet = new BinaryReader(stream))
        {
            tick = packet.ReadUInt32();
            horizontalMovement.x = packet.ReadSingle();
            horizontalMovement.y = packet.ReadSingle();
            jump = packet.ReadBoolean();
            crouch = packet.ReadBoolean();
            walk = packet.ReadBoolean();
        }
    }

    public void Reset()
    {
        horizontalMovement = Vector2.zero;
        jump = false;
        crouch = false;
        walk = false;
    }
}
