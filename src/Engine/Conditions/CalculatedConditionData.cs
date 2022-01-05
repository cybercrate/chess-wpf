using Engine.UtilityComponents;

namespace Engine.Conditions;

public class CalculatedConditionData
{
    /// <summary>
    /// Possible attacks of pieces that aren't on turn.
    /// </summary>
    public HashSet<Coords> EnemyPossibleAttacks { get; }
        
    /// <summary>
    /// List of coordinates of pieces that protect king from check.
    /// </summary>
    public List<Coords> KingProtectingPiecesCoords { get; }

    public CalculatedConditionData()
    {
        EnemyPossibleAttacks = new HashSet<Coords>();
        KingProtectingPiecesCoords = new List<Coords>();
    }
}
