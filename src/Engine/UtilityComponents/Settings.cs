namespace Engine.UtilityComponents;

public static class Settings
{
    /// <summary>
    /// Player is white?
    /// </summary>
    public static bool WhiteIsPlayer { get; set; } = true;

    /// <summary>
    /// Player is black?
    /// </summary>
    public static bool BlackIsPlayer { get; set; } = true;
    
    /// <summary>
    /// Difficulty of Emulator playing white.
    /// </summary>
    public static int WhiteDifficulty { get; set; } = 2;

    /// <summary>
    /// Difficulty of Emulator playing black.
    /// </summary>
    public static int BlackDifficulty { get; set; } = 2;
    
    /// <summary>
    /// Minimal time (in seconds) of Emulator turn calculation.
    /// </summary>
    public static float CalculationMinTime { get; set; } = 3.0f;

    /// <summary>
    /// Whether new game request was confirmed.
    /// </summary>
    public static bool IsNewGame { get; set; } = true;
}
