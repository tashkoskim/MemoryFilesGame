using System.Drawing;

namespace MemoryFilesGame.Core;

internal static class SvgImageSource
{
    public static void Write(MemoryFile file)
    {
        string color = $"#{file.Color.R:X2}{file.Color.G:X2}{file.Color.B:X2}";
        string character = System.Security.SecurityElement.Escape(file.AsciiChar.ToString())!;
        string svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" viewBox="0 0 100 100">
              <rect width="100" height="100" fill="white"/>
              <text x="50" y="50" fill="{color}" font-family="Arial, sans-serif" font-size="72" font-weight="bold" text-anchor="middle" dominant-baseline="central">{character}</text>
            </svg>
            """;

        File.WriteAllText(file.SvgPath, svg);
    }
}
