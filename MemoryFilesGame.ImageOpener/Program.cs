using MemoryFilesGame.Core;

using System.Diagnostics;
using System.Globalization;

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
                var gameManager = new GameManager();
                GameProgress progress = gameManager.OpenCard(args[1]);
                LogImageAccess(Path.GetFileName(args[2]));

                if (progress.IsCompleted)
                {
                    // Explicitly reveal the final card before the results screen appears.
                    gameManager.RevealAllCards();
                    Thread.Sleep(900);
                    ShowCompletion(progress);
                }
            }
            catch
            {
                // A card may belong to a previous game session; silently ignore it.
            }
        }

        private static void ShowCompletion(GameProgress progress)
        {
            string mainAppPath = Path.Combine(AppContext.BaseDirectory, "MemoryFilesGame.Main.exe");
            Process.Start(new ProcessStartInfo
            {
                FileName = mainAppPath,
                Arguments = $"--completed {progress.Elapsed.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture)} {progress.MoveCount} {progress.PairCount}",
                UseShellExecute = true
            });
        }

        private static void LogImageAccess(string imageName)
        {
            File.AppendAllText(Constants.GameLogPath, $"{DateTimeOffset.Now:O}: {imageName} opened.{Environment.NewLine}");
        }
    }
}
