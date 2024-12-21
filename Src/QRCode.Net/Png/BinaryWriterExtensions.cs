namespace QRCode.Net.Png;

// Extension method for writing big-endian integers
public static class BinaryWriterExtensions
{
    public static void WriteInt32BE(this BinaryWriter writer, int value)
    {
        writer.Write((byte)((value >> 24) & 0xFF));
        writer.Write((byte)((value >> 16) & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
        writer.Write((byte)(value & 0xFF));
    }
}