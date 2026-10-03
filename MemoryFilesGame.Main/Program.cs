using MemoryFilesGame.Core;

namespace MemoryFilesGame.Main
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the number of pairs of images for the game:");
            if (!int.TryParse(Console.ReadLine(), out int numPairs) || numPairs <= 0)
            {
                Console.Error.WriteLine("Enter a whole number greater than zero.");
                return;
            }

            var gameManager = new GameManager();
            try
            {
                string cardOpenerPath = Path.Combine(AppContext.BaseDirectory, "MemoryFilesGame.ImageOpener.exe");
                gameManager.InitializeGame(numPairs, cardOpenerPath);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"The game could not be initialized: {exception.Message}");
                return;
            }

            Console.WriteLine($"Game initialized! You can open: {Constants.GameFolderPath}");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{Constants.GameFolderPath}\"",
                UseShellExecute = true
            });
        }

    }
}
