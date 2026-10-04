using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MemoryFilesGame.Core;

internal static class RasterImageWriter
{
    private const int IconSize = 128;

    public static void WritePng(string path, char character, Color color)
    {
        using Bitmap bitmap = CreateBitmap(100, character, color);
        bitmap.Save(path, ImageFormat.Png);
    }

    public static void WriteIcon(string path, char character, Color color)
    {
        using Bitmap bitmap = CreateBitmap(IconSize, character, color);
        using BinaryWriter writer = new(File.Create(path));

        writer.Write((ushort)0); writer.Write((ushort)1); writer.Write((ushort)1);
        writer.Write((byte)IconSize); writer.Write((byte)IconSize); writer.Write((byte)0); writer.Write((byte)0);
        writer.Write((ushort)1); writer.Write((ushort)32);
        writer.Write(40 + IconSize * IconSize * 4 + IconSize * 4); writer.Write(22);
        writer.Write(40); writer.Write(IconSize); writer.Write(IconSize * 2); writer.Write((ushort)1); writer.Write((ushort)32);
        writer.Write(0); writer.Write(IconSize * IconSize * 4); writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(0);

        Rectangle bounds = new(0, 0, IconSize, IconSize);
        BitmapData data = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            byte[] row = new byte[IconSize * 4];
            for (int y = IconSize - 1; y >= 0; y--)
            {
                Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), row, 0, row.Length);
                writer.Write(row);
            }
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        writer.Write(new byte[IconSize * 4]);
    }

    private static Bitmap CreateBitmap(int size, char character, Color color)
    {
        Bitmap bitmap = new(size, size, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        using Font font = new("Segoe UI Symbol", size * 0.68f, FontStyle.Regular, GraphicsUnit.Pixel);
        using Brush brush = new SolidBrush(color);
        using StringFormat format = new()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        graphics.DrawString(character.ToString(), font, brush, new RectangleF(0, 0, size, size), format);
        return bitmap;
    }
}
