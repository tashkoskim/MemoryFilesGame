using System.Drawing;
using System.Runtime.InteropServices;

namespace MemoryFilesGame.Core
{
    public class GameManager
    {
        public void InitializeGame(int numPairs, string cardOpenerPath)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numPairs);

            if (string.IsNullOrWhiteSpace(cardOpenerPath) || !File.Exists(cardOpenerPath))
            {
                throw new FileNotFoundException("The card opener executable could not be found.", cardOpenerPath);
            }

            CreateDirectories();
            ImageHelper.CreateCardBackIcon();

            List<MemoryFile> files = GenerateMemoryFiles(numPairs);
            foreach (var file in files.GroupBy(file => file.ImagePath).Select(group => group.First()))
            {
                ImageHelper.CreateAsciiImage(file);
                ImageHelper.CreateMatchedIcon(file);
            }

            foreach (var file in files)
            {
                CreateShortcut(file, cardOpenerPath);
            }

            GameStateStore.Save(new GameState
            {
                StartedAt = DateTimeOffset.UtcNow,
                PairCount = numPairs,
                Cards = files.Select(file => new GameCard
                {
                    CardId = file.CardId,
                    PairId = file.PairId,
                    ImagePath = file.ImagePath,
                    ShortcutPath = file.ShortcutPath,
                    MatchedIconPath = file.MatchedIconPath
                }).ToList()
            });
        }

        public GameProgress OpenCard(string cardId)
        {
            GameState game = GameStateStore.Load();
            GameCard selected = game.Cards.SingleOrDefault(card => card.CardId == cardId)
                ?? throw new InvalidDataException("The selected card is not part of this game.");

            if (selected.IsMatched || game.IsCompleted)
            {
                return CreateProgress(game);
            }

            UpdateShortcutIcon(selected);

            if (game.PendingCardId is not null && game.PendingCardId != selected.CardId)
            {
                game.MoveCount++;
                GameCard pending = game.Cards.Single(card => card.CardId == game.PendingCardId);
                if (pending.PairId == selected.PairId)
                {
                    pending.IsMatched = true;
                    selected.IsMatched = true;
                    UpdateShortcutIcon(pending);
                    UpdateShortcutIcon(selected);
                    game.PendingCardId = null;
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(900));
                    RestoreCardBackIcon(pending);
                    RestoreCardBackIcon(selected);
                    game.PendingCardId = null;
                }
            }
            else
            {
                game.PendingCardId = selected.CardId;
            }

            game.IsCompleted = game.Cards.All(card => card.IsMatched);
            GameStateStore.Save(game);
            return CreateProgress(game);
        }

        public void RevealAllCards()
        {
            foreach (GameCard card in GameStateStore.Load().Cards)
            {
                UpdateShortcutIcon(card);
            }
        }

        private static GameProgress CreateProgress(GameState game) => new(
            game.IsCompleted,
            DateTimeOffset.UtcNow - game.StartedAt,
            game.MoveCount,
            game.PairCount);

        private void CreateDirectories()
        {
            ClearDirectory(Constants.GameFolderPath);

            if (Directory.Exists(Constants.GameDataFolderPath))
            {
                Directory.Delete(Constants.GameDataFolderPath, recursive: true);
            }

            Directory.CreateDirectory(Constants.GameFolderPath);
            Directory.CreateDirectory(Constants.GameDataFolderPath);
            Directory.CreateDirectory(Constants.ImagesFolderPath);
            Directory.CreateDirectory(Constants.SvgFolderPath);
            Directory.CreateDirectory(Constants.IconsFolderPath);
        }

        private static void ClearDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return;
            }

            foreach (string entry in Directory.EnumerateFileSystemEntries(path))
            {
                if (Directory.Exists(entry))
                {
                    Directory.Delete(entry, recursive: true);
                }
                else
                {
                    File.Delete(entry);
                }
            }
        }

        private List<MemoryFile> GenerateMemoryFiles(int numPairs)
        {
            // Prefer expressive Unicode symbols; ordinary letters/numbers are only fallbacks for very large games.
            const string availableSymbols = "☺☻♥♦♣♠♡★☆✦✧✪✯☀☁☂☃☄☾☽⚡❄✿❀❁❂☠☢☣⚔⚒⚙⚑⚐✓✔✕✖↑↓←→↖↗↘↙↕↔➜➤➔▲▼◀▶△▽◁▷◆◇■□●○◉◎◌◍◐◑◒◓♔♕♖♗♘♙♚♛♜♝♞♟♪♫♩♬♭♮♯☕☎✉✂✎✐✏✒⌕⌂☜☞☝☟ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            if (numPairs > availableSymbols.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(numPairs), $"At most {availableSymbols.Length} pairs are supported.");
            }

            List<MemoryFile> files = new(numPairs * 2);
            List<char> availableCharacters = availableSymbols
                .Take(numPairs)
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            for (int i = 0; i < numPairs; i++)
            {
                char asciiChar = availableCharacters[i];
                var color = Color.FromArgb(Random.Shared.Next(256), Random.Shared.Next(256), Random.Shared.Next(256));
                
                string pairId = $"pair_{i + 1:D2}";
                string imageName = $"{pairId}.png";

                // Both cards reference the same image, which makes a real matching pair.
                files.Add(new MemoryFile(Guid.NewGuid().ToString("N"), pairId, asciiChar, color, imageName, $"card_{Guid.NewGuid():N}.lnk"));
                files.Add(new MemoryFile(Guid.NewGuid().ToString("N"), pairId, asciiChar, color, imageName, $"card_{Guid.NewGuid():N}.lnk"));
            }

            return files.OrderBy(_ => Random.Shared.Next()).ToList();
        }

        private void CreateShortcut(MemoryFile file, string cardOpenerPath)
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell")
                ?? throw new PlatformNotSupportedException("Windows Script Host is required to create game cards.");
            dynamic wshShell = Activator.CreateInstance(shellType)
                ?? throw new InvalidOperationException("Could not create the Windows shortcut service.");
            dynamic shortcut = wshShell.CreateShortcut(file.ShortcutPath);

            shortcut.TargetPath = cardOpenerPath;
            shortcut.Arguments = $"--open {file.CardId} \"{file.ImagePath}\"";
            shortcut.WorkingDirectory = Constants.GameFolderPath;
            shortcut.IconLocation = $"{Constants.CardBackIconPath},0";
            shortcut.Save();
        }

        private static void UpdateShortcutIcon(GameCard card)
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell")
                ?? throw new PlatformNotSupportedException("Windows Script Host is required to update game cards.");
            dynamic wshShell = Activator.CreateInstance(shellType)
                ?? throw new InvalidOperationException("Could not create the Windows shortcut service.");
            dynamic shortcut = wshShell.CreateShortcut(card.ShortcutPath);
            shortcut.IconLocation = $"{card.MatchedIconPath},0";
            shortcut.Save();
            RefreshExplorerItem(card.ShortcutPath);
        }

        private static void RestoreCardBackIcon(GameCard card)
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell")
                ?? throw new PlatformNotSupportedException("Windows Script Host is required to update game cards.");
            dynamic wshShell = Activator.CreateInstance(shellType)
                ?? throw new InvalidOperationException("Could not create the Windows shortcut service.");
            dynamic shortcut = wshShell.CreateShortcut(card.ShortcutPath);
            shortcut.IconLocation = $"{Constants.CardBackIconPath},0";
            shortcut.Save();
            RefreshExplorerItem(card.ShortcutPath);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern void SHChangeNotify(uint eventId, uint flags, string item1, IntPtr item2);

        private static void RefreshExplorerItem(string path) => SHChangeNotify(0x00002000, 0x00000005, path, IntPtr.Zero);
    }
}
