using System.Text.Json;

namespace MemoryFilesGame.Core;

public sealed class GameState
{
    public string? PendingCardId { get; set; }
    public List<GameCard> Cards { get; set; } = [];
}

public sealed class GameCard
{
    public string CardId { get; set; } = string.Empty;
    public string PairId { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string ShortcutPath { get; set; } = string.Empty;
    public string MatchedIconPath { get; set; } = string.Empty;
    public bool IsMatched { get; set; }
}

public static class GameStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void Save(GameState state)
    {
        File.WriteAllText(Constants.GameStatePath, JsonSerializer.Serialize(state, JsonOptions));
    }

    public static GameState Load() => JsonSerializer.Deserialize<GameState>(File.ReadAllText(Constants.GameStatePath))
        ?? throw new InvalidDataException("The game state is empty.");
}
