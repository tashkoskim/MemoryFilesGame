namespace MemoryFilesGame.Core
{
    public static class Constants
    {
        public const string GameFolderName = "MemoryFilesGame";
        public const string ImagesFolderName = "TmpImages";
        public const string SvgFolderName = "SvgSources";
        public const string IconsFolderName = "Icons";
        public static readonly string GameFolderPath = Path.Combine(Path.GetTempPath(), GameFolderName);
        public static readonly string GameDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), GameFolderName);
        // Keep every game artifact together so a new game can safely replace the old one.
        public static readonly string ImagesFolderPath = Path.Combine(GameDataFolderPath, ImagesFolderName);
        public static readonly string SvgFolderPath = Path.Combine(GameDataFolderPath, SvgFolderName);
        public static readonly string IconsFolderPath = Path.Combine(GameDataFolderPath, IconsFolderName);
        public static readonly string CardBackIconPath = Path.Combine(IconsFolderPath, "card-back.ico");

        public const string GameLogName = "GameLog.txt";
        public static readonly string GameLogPath = Path.Combine(GameDataFolderPath, GameLogName);
        public static readonly string GameStatePath = Path.Combine(GameDataFolderPath, "GameState.json");
    }
}
