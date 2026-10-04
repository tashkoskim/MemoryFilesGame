using MemoryFilesGame.Core;

using System.Runtime.InteropServices;

namespace MemoryFilesGame.Main
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 4 && args[0].Equals("--completed", StringComparison.OrdinalIgnoreCase))
            {
                ShowResults(args);
                return;
            }

            Console.Title = "Memory Files Game";
            CenterConsoleWindow();
            Console.WriteLine("+--------------------------------------+");
            Console.WriteLine("|          MEMORY FILES GAME           |");
            Console.WriteLine("+--------------------------------------+");
            Console.WriteLine();
            Console.WriteLine("Start a game by entering the number of image pairs");
            Console.WriteLine("you want to play with:");
            Console.WriteLine();
            Console.Write("> ");
            if (!int.TryParse(Console.ReadLine(), out int numPairs) || numPairs <= 0)
            {
                Console.Error.WriteLine("Enter a whole number greater than zero.");
                return;
            }

            StartGame(numPairs);
        }

        private static void StartGame(int numPairs)
        {
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

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{Constants.GameFolderPath}\"",
                UseShellExecute = true
            });
        }

        private static void ShowResults(string[] args)
        {
            double seconds = double.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture);
            int moves = int.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture);
            int pairs = int.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture);
            TimeSpan elapsed = TimeSpan.FromSeconds(seconds);

            Console.Title = "Memory Files Game - You Win";
            CenterConsoleWindow();
            int selectedOption = 0;
            Console.CursorVisible = false;

            while (true)
            {
                DrawResults(pairs, moves, elapsed, selectedOption);
                ConsoleKey key = Console.ReadKey(intercept: true).Key;
                if (key is ConsoleKey.LeftArrow or ConsoleKey.UpArrow)
                {
                    selectedOption = 0;
                    continue;
                }

                if (key is ConsoleKey.RightArrow or ConsoleKey.DownArrow)
                {
                    selectedOption = 1;
                    continue;
                }

                if (key == ConsoleKey.Enter)
                {
                    Console.CursorVisible = true;
                    if (selectedOption == 0)
                    {
                        StartGame(pairs);
                    }
                    return;
                }
            }
        }

        private static void DrawResults(int pairs, int moves, TimeSpan elapsed, int selectedOption)
        {
            Console.Clear();
            Console.WriteLine("+--------------------------------------+");
            Console.WriteLine("|               YOU WIN!               |");
            Console.WriteLine("+--------------------------------------+");
            Console.WriteLine();
            Console.WriteLine($"  Pairs:  {pairs}");
            Console.WriteLine($"  Moves:  {moves}");
            Console.WriteLine($"  Time:   {elapsed:mm\\:ss}");
            Console.WriteLine();
            Console.WriteLine(selectedOption == 0 ? "  > Play Again <" : "    Play Again");
            Console.WriteLine(selectedOption == 1 ? "  > Exit <" : "    Exit");
            Console.WriteLine();
            Console.WriteLine("  Use arrow keys and Enter to select.");
        }

        private static void CenterConsoleWindow()
        {
            IntPtr consoleWindow = GetConsoleWindow();
            if (consoleWindow == IntPtr.Zero || !GetWindowRect(consoleWindow, out Rect window) ||
                !SystemParametersInfo(0x0030, 0, out Rect workArea, 0))
            {
                return;
            }

            int x = workArea.Left + ((workArea.Right - workArea.Left) - (window.Right - window.Left)) / 2;
            int y = workArea.Top + ((workArea.Bottom - workArea.Top) - (window.Bottom - window.Top)) / 2;
            SetWindowPos(consoleWindow, IntPtr.Zero, x, y, 0, 0, 0x0001 | 0x0004);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr handle, out Rect rect);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint action, uint parameter, out Rect rect, uint flags);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr handle, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

    }
}
