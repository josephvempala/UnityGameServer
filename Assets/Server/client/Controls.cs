using System.IO;
using UnityEngine;

public struct Controls
{
    public uint tick;
    public Vector2 horizontalMovement;
    public float Yrotation;
    public bool jump;
    public bool crouch;
    public bool walk;

    public void Serialize(ref byte[] controls)
    {
        Stream stream = new MemoryStream(controls);
        using (BinaryWriter packet = new BinaryWriter(stream))
        {
            packet.Write(horizontalMovement.x);
            packet.Write(horizontalMovement.y);
            packet.Write(Yrotation);
            packet.Write(jump);
            packet.Write(crouch);
            packet.Write(walk);
        }
    }

    public void Deserialize(in byte[] controls)
    {
        Stream stream = new MemoryStream(controls);
        using (BinaryReader packet = new BinaryReader(stream))
        {
            horizontalMovement.x = packet.ReadSingle();
            horizontalMovement.y = packet.ReadSingle();
            Yrotation = packet.ReadSingle();
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
