using System.IO;

public struct Orientation
{
    public uint Tick;
    public float CameraRotationX;
    public float CharacterRotationY;

    public byte[] Serialize(byte[] buffer)
    {
        var stream = new MemoryStream(buffer);
        using var br = new BinaryWriter(stream);
        br.Write(Tick);
        br.Write(CameraRotationX);
        br.Write(CharacterRotationY);
        return stream.ToArray();
    }

    public void Deserialize(in byte[] buffer)
    {
        var stream = new MemoryStream(buffer);
        using var br = new BinaryReader(stream);
        Tick = br.ReadUInt32();
        CameraRotationX = br.ReadSingle();
        CharacterRotationY = br.ReadSingle();
    }

    public void Reset()
    {
        Tick = 0;
        CameraRotationX = 0;
        CharacterRotationY = 0;
    }
}