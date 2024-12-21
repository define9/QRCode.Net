using System.IO.Compression;

namespace QRCode.Net.Png;

public class PngQrCodeGenerator
{
    public static PngQrCodeGenerator Instance { get; } = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="filePath"></param>
    /// <param name="scale"></param>
    public Stream GeneratePng(bool[,] matrix, int scale = 10)
    {
        int width = matrix.GetLength(0);
        int height = matrix.GetLength(1);

        // Scaled dimensions
        int scaledWidth = width * scale;
        int scaledHeight = height * scale;

        var ms = new MemoryStream();
        var bw = new BinaryWriter(ms);

        // Write PNG signature
        bw.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

        // IHDR Chunk
        WriteChunk(bw, "IHDR", writer =>
        {
            writer.WriteInt32BE(scaledWidth); // Width
            writer.WriteInt32BE(scaledHeight); // Height
            writer.Write((byte)8); // Bit depth
            writer.Write((byte)0); // Color type (grayscale)
            writer.Write((byte)0); // Compression method
            writer.Write((byte)0); // Filter method
            writer.Write((byte)0); // Interlace method
        });

        // IDAT Chunk (Image Data)
        byte[] imageData = CreateImageData(matrix, scale);
        byte[] compressedData = Compress(imageData);
        WriteChunk(bw, "IDAT", writer =>
        {
            writer.Write(compressedData);
        });

        // IEND Chunk
        WriteChunk(bw, "IEND", writer => { });

        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    private byte[] CreateImageData(bool[,] matrix, int scale)
    {
        int width = matrix.GetLength(0);
        int height = matrix.GetLength(1);
        int scaledWidth = width * scale;

        using (var ms = new MemoryStream())
        {
            for (int y = 0; y < height; y++)
            {
                for (int ys = 0; ys < scale; ys++) // Scale rows
                {
                    ms.WriteByte(0); // No filter
                    for (int x = 0; x < width; x++)
                    {
                        for (int xs = 0; xs < scale; xs++) // Scale columns
                        {
                            byte color = matrix[y, x] ? (byte)0 : (byte)255; // Black or White
                            ms.WriteByte(color);
                        }
                    }
                }
            }
            return ms.ToArray();
        }
    }

    private byte[] Compress(byte[] data)
    {
        using (var ms = new MemoryStream())
        {
            using (var deflate = new DeflateStream(ms, CompressionLevel.Optimal))
            {
                deflate.Write(data, 0, data.Length);
            }
            return ms.ToArray();
        }
    }

    private void WriteChunk(BinaryWriter bw, string type, Action<BinaryWriter> writeData)
    {
        using (var ms = new MemoryStream())
        using (var chunkWriter = new BinaryWriter(ms))
        {
            // Write chunk data
            writeData(chunkWriter);
            byte[] chunkData = ms.ToArray();

            // Write length
            bw.WriteInt32BE(chunkData.Length);

            // Write type
            bw.Write(type.ToCharArray());

            // Write data
            bw.Write(chunkData);

            // Write CRC
            bw.WriteInt32BE(CalculateCrc(type, chunkData));
        }
    }

    private int CalculateCrc(string type, byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        byte[] typeBytes = System.Text.Encoding.ASCII.GetBytes(type);

        foreach (byte b in typeBytes) crc = UpdateCrc(crc, b);
        foreach (byte b in data) crc = UpdateCrc(crc, b);

        return (int)(crc ^ 0xFFFFFFFF);
    }

    private uint UpdateCrc(uint crc, byte b)
    {
        uint[] crcTable = new uint[256];
        for (int i = 0; i < 256; i++)
        {
            uint c = (uint)i;
            for (int j = 0; j < 8; j++)
            {
                if ((c & 1) != 0)
                    c = 0xEDB88320 ^ c >> 1;
                else
                    c >>= 1;
            }
            crcTable[i] = c;
        }

        return crcTable[(crc ^ b) & 0xFF] ^ crc >> 8;
    }
}
