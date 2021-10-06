using System.IO;

public struct Orientation
{
    public uint tick;
    public float CameraRotationX;
    public float CharacterRotationY;

    public void Serialize(ref byte[] buffer)
    {
        MemoryStream stream = new MemoryStream(buffer);
        using (BinaryWriter br = new BinaryWriter(stream))
        {
            br.Write(tick);
            br.Write(CameraRotationX);
            br.Write(CharacterRotationY);
        }
    }

    public void Deserialize(in byte[] buffer)
    {
        MemoryStream stream = new MemoryStream(buffer);
        using (BinaryReader br = new BinaryReader(stream))
        {
            tick = br.ReadUInt32();
            CameraRotationX = br.ReadSingle();
            CharacterRotationY = br.ReadSingle();
        }
    }

    public void Reset()
    {
        tick = 0;
        CameraRotationX = 0;
        CharacterRotationY = 0;
    }
}
