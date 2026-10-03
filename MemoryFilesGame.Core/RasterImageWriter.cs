using System.Drawing;
using System.IO.Compression;
using System.Text;

namespace MemoryFilesGame.Core;

internal static class RasterImageWriter
{
    private static readonly Dictionary<char, string> Glyphs = new()
    {
        ['?'] = "6912404", ['0'] = "699F996", ['1'] = "2622227", ['2'] = "691248F", ['3'] = "E11211E", ['4'] = "124AF22",
        ['5'] = "F88E11E", ['6'] = "788E996", ['7'] = "F124444", ['8'] = "6996996", ['9'] = "699711E",
        ['A'] = "699F999", ['B'] = "E99E99E", ['C'] = "7888887", ['D'] = "E99999E", ['E'] = "F88E88F", ['F'] = "F88E888",
        ['G'] = "788B997", ['H'] = "999F999", ['I'] = "7222227", ['J'] = "1111996", ['K'] = "99A C A99".Replace(" ", ""),
        ['L'] = "888888F", ['M'] = "9FDB999", ['N'] = "9DDB999", ['O'] = "6999996", ['P'] = "E99E888", ['Q'] = "69999A5",
        ['R'] = "E99EA99", ['S'] = "788611E", ['T'] = "F222222", ['U'] = "9999996", ['V'] = "9999964", ['W'] = "999BBD6",
        ['X'] = "9964699", ['Y'] = "9964222", ['Z'] = "F1248F"
    };

    public static void WritePng(string path, char character, Color color) => WritePng(path, CreatePixels(100, character, color), 100);

    public static void WriteIcon(string path, char character, Color color) => WriteIcon(path, CreatePixels(128, character, color));

    private static Color[] CreatePixels(int size, char character, Color foreground)
    {
        Color[] pixels = Enumerable.Repeat(Color.White, size * size).ToArray();
        string glyph = Glyphs[character];
        int scale = Math.Max(1, Math.Min(size / 7, size / 9));
        int startX = (size - 4 * scale) / 2;
        int startY = (size - 7 * scale) / 2;

        for (int row = 0; row < 7; row++)
        for (int column = 0; column < 4; column++)
        {
            int value = Convert.ToInt32(glyph[row].ToString(), 16);
            if ((value & (1 << (3 - column))) == 0) continue;
            for (int y = 0; y < scale; y++)
            for (int x = 0; x < scale; x++)
                pixels[(startY + row * scale + y) * size + startX + column * scale + x] = foreground;
        }
        return pixels;
    }

    private static void WritePng(string path, Color[] pixels, int size)
    {
        byte[] raw = new byte[size * (size * 4 + 1)];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Color pixel = pixels[y * size + x];
            int offset = y * (size * 4 + 1) + 1 + x * 4;
            raw[offset] = pixel.R; raw[offset + 1] = pixel.G; raw[offset + 2] = pixel.B; raw[offset + 3] = pixel.A;
        }

        using FileStream stream = File.Create(path);
        stream.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        WriteChunk(stream, "IHDR", [0, 0, 0, (byte)size, 0, 0, 0, (byte)size, 8, 6, 0, 0, 0]);
        using MemoryStream compressed = new();
        using (ZLibStream zlib = new(compressed, CompressionLevel.SmallestSize, true)) zlib.Write(raw);
        WriteChunk(stream, "IDAT", compressed.ToArray());
        WriteChunk(stream, "IEND", []);
    }

    private static void WriteIcon(string path, Color[] pixels)
    {
        const int size = 128;
        using BinaryWriter writer = new(File.Create(path));
        writer.Write((ushort)0); writer.Write((ushort)1); writer.Write((ushort)1);
        writer.Write((byte)size); writer.Write((byte)size); writer.Write((byte)0); writer.Write((byte)0); writer.Write((ushort)1); writer.Write((ushort)32);
        writer.Write(40 + size * size * 4 + size * 4); writer.Write(22);
        writer.Write(40); writer.Write(size); writer.Write(size * 2); writer.Write((ushort)1); writer.Write((ushort)32);
        writer.Write(0); writer.Write(size * size * 4); writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(0);
        for (int y = size - 1; y >= 0; y--)
        for (int x = 0; x < size; x++)
        {
            Color pixel = pixels[y * size + x];
            writer.Write(pixel.B); writer.Write(pixel.G); writer.Write(pixel.R); writer.Write(pixel.A);
        }
        writer.Write(new byte[size * 4]);
    }

    private static void WriteChunk(Stream stream, string name, byte[] data)
    {
        WriteBigEndian(stream, (uint)data.Length);
        byte[] type = Encoding.ASCII.GetBytes(name);
        stream.Write(type); stream.Write(data);
        WriteBigEndian(stream, Crc32(type.Concat(data)));
    }

    private static void WriteBigEndian(Stream stream, uint value) => stream.Write([(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value]);

    private static uint Crc32(IEnumerable<byte> bytes)
    {
        uint crc = 0xffffffff;
        foreach (byte value in bytes)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) == 1 ? 0xedb88320u : 0);
        }
        return ~crc;
    }
}
