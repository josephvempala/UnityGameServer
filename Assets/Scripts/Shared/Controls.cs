using System.IO;
using UnityEngine;

public struct Controls
{
    public uint Tick;
    public Vector2 HorizontalMovement;
    public bool Jump;
    public bool Crouch;
    public bool Walk;

    public byte[] Serialize(byte[] controls)
    {
        var stream = new MemoryStream(controls);
        using var packet = new BinaryWriter(stream);
        packet.Write(Tick);
        packet.Write(HorizontalMovement.x);
        packet.Write(HorizontalMovement.y);
        packet.Write(Jump);
        packet.Write(Crouch);
        packet.Write(Walk);
        return stream.ToArray();
    }

    public void Deserialize(in byte[] controls)
    {
        var stream = new MemoryStream(controls);
        using var packet = new BinaryReader(stream);
        Tick = packet.ReadUInt32();
        HorizontalMovement.x = packet.ReadSingle();
        HorizontalMovement.y = packet.ReadSingle();
        Jump = packet.ReadBoolean();
        Crouch = packet.ReadBoolean();
        Walk = packet.ReadBoolean();
    }

    public void Reset()
    {
        Jump = false;
    }
}