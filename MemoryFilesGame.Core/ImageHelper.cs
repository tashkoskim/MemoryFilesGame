using System.Drawing;

namespace MemoryFilesGame.Core
{
    public static class ImageHelper
    {
        public static void CreateAsciiImage(MemoryFile fileObj)
        {
            SvgImageSource.Write(fileObj);
            RasterImageWriter.WritePng(fileObj.ImagePath, fileObj.AsciiChar, fileObj.Color);
        }

        public static void CreateMatchedIcon(MemoryFile fileObj) => RasterImageWriter.WriteIcon(fileObj.MatchedIconPath, fileObj.AsciiChar, fileObj.Color);

        public static void CreateCardBackIcon() => RasterImageWriter.WriteIcon(Constants.CardBackIconPath, '?', Color.FromArgb(48, 114, 196));
    }
}
