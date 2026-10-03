using System.Drawing;

namespace MemoryFilesGame.Core
{
    public class MemoryFile
    {
        public string CardId { get; }
        public string PairId { get; }
        public char AsciiChar { get; set; }
        public Color Color { get; set; }
        public string ImageName { get; }
        public string ImagePath { get; private set; }
        public string SvgPath { get; }
        public string ShortcutName { get; }
        public string ShortcutPath { get; private set; } 
        public string MatchedIconPath { get; }

        public MemoryFile(string cardId, string pairId, char asciiChar, Color color, string imageName, string shortcutName)
        {
            CardId = cardId;
            PairId = pairId;
            AsciiChar = asciiChar;
            Color = color;
            ImageName = imageName;
            ImagePath = Path.Combine(Constants.ImagesFolderPath, ImageName);
            SvgPath = Path.Combine(Constants.SvgFolderPath, Path.ChangeExtension(ImageName, ".svg"));
            ShortcutName = shortcutName;
            ShortcutPath = Path.Combine(Constants.GameFolderPath, ShortcutName);
            MatchedIconPath = Path.Combine(Constants.IconsFolderPath, $"{PairId}.ico");
        }
    }
}
