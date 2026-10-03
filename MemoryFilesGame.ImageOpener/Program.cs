using MemoryFilesGame.Core;

namespace MemoryFilesGame.ImageOpener
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3 || !args[0].Equals("--open", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                new GameManager().OpenCard(args[1]);
                LogImageAccess(Path.GetFileName(args[2]));
            }
            catch
            {
                // A card may belong to a previous game session; silently ignore it.
            }
        }

        private static void LogImageAccess(string imageName)
        {
            File.AppendAllText(Constants.GameLogPath, $"{DateTimeOffset.Now:O}: {imageName} opened.{Environment.NewLine}");
        }
    }
}
