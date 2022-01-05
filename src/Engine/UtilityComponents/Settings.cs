namespace Engine.UtilityComponents;

public static class Settings
{
    /// <summary>
    /// Player is white?
    /// </summary>
    public static bool PlayerIsWhite { get; set; }
        
    /// <summary>
    /// Player is black?
    /// </summary>
    public static bool PlayerIsBlack { get; set; }
        
    /// <summary>
    /// Difficulty of Emulator playing black.
    /// </summary>
    public static int BlackDifficulty { get; set; }
        
    /// <summary>
    /// Difficulty of Emulator playing white.
    /// </summary>
    public static int WhiteDifficulty { get; set; }
        
    /// <summary>
    /// Minimal time (in seconds) of Emulator turn calculation.
    /// </summary>
    public static float CalculationMinTime { get; set; }
        
    /// <summary>
    /// Whether new game request was confirmed.
    /// </summary>
    public static bool NewGame { get; set; }
        
    /// <summary>
    /// Default settings.
    /// </summary>
    static Settings()
    {
        PlayerIsWhite = PlayerIsBlack = true;
        BlackDifficulty = WhiteDifficulty = 2;
        CalculationMinTime = 3.0f;
        NewGame = true;
    }
}
