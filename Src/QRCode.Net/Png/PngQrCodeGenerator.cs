using System.IO.Compression;
using System.Text;

namespace QRCode.Net.Png;

public class PngQrCodeGenerator
{
    public static PngQrCodeGenerator Instance { get; } = new();
    private static readonly uint[] Crc32Table = InitializeCrc32Table();

    // Public method to generate PNG from a boolean matrix
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
            writer.Write((byte)6); // Color type (Truecolor with Alpha - RGBA)
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

    // Create the image data from the boolean matrix with RGBA (Alpha channel added)
    private byte[] CreateImageData(bool[,] matrix, int scale)
    {
        int width = matrix.GetLength(0);
        int height = matrix.GetLength(1);
        int scaledWidth = width * scale;

        using (var ms = new MemoryStream())
        {
            // Iterate over rows
            for (int y = 0; y < height; y++)
            {
                // Scale row by writing repeated rows
                for (int ys = 0; ys < scale; ys++)
                {
                    ms.WriteByte(0); // No filter (Filter type 0)

                    // Iterate over columns
                    for (int x = 0; x < width; x++)
                    {
                        // Scale column by writing repeated pixels
                        for (int xs = 0; xs < scale; xs++)
                        {
                            byte color = matrix[y, x] ? (byte)0 : (byte)255; // Black or White

                            // Write RGBA (4 bytes per pixel)
                            ms.WriteByte(color); // Red channel
                            ms.WriteByte(color); // Green channel
                            ms.WriteByte(color); // Blue channel
                            ms.WriteByte(255);   // Alpha channel (fully opaque)
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
            // C# 的 DeflateStream 虽然是ZLib, 但是DeflateStream不会写头部，所以需要手动写
            // Step 1: Write zlib header (0x78 0x9C) -- commonly used header for default deflate
            ms.WriteByte(0x78);  // Compression method and flags (0x78 means default deflate with zlib header)
            ms.WriteByte(0x9C);  // Compression flags

            using (var deflate = new DeflateStream(ms, CompressionLevel.Optimal))
            {
                deflate.Write(data, 0, data.Length);
            }

            return ms.ToArray();
        }
    }

    // Write a PNG chunk (length, type, data, and CRC)
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

            // Write type (ASCII)
            bw.Write(Encoding.ASCII.GetBytes(type));

            // Write data
            bw.Write(chunkData);

            // Write CRC
            uint crc = CalculateCrc(type, chunkData);
            bw.WriteInt32BE((int)crc);
        }
    }

    // Calculate CRC32 for a chunk type and its data
    private uint CalculateCrc(string type, byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        byte[] typeBytes = Encoding.ASCII.GetBytes(type);

        foreach (byte b in typeBytes)
            crc = UpdateCrc(crc, b);

        foreach (byte b in data)
            crc = UpdateCrc(crc, b);

        return crc ^ 0xFFFFFFFF;
    }

    // Update CRC using the CRC32 lookup table
    private uint UpdateCrc(uint crc, byte b)
    {
        return Crc32Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
    }

    // Initialize the CRC32 lookup table
    private static uint[] InitializeCrc32Table()
    {
        uint[] table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (uint j = 8; j > 0; j--)
            {
                if ((crc & 1) == 1)
                    crc = (crc >> 1) ^ 0xEDB88320;
                else
                    crc >>= 1;
            }
            table[i] = crc;
        }
        return table;
    }
}
