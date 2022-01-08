namespace Engine.UtilityComponents;

/// <summary>
/// Coordinates.
/// </summary>
public readonly struct Coords
{
    /// <summary>
    /// Row
    /// </summary>
    public sbyte Row { get; }
    
    /// <summary>
    /// Column
    /// </summary>
    public sbyte Column { get; }
    
    public Coords(int row, int column)
    {
        Row = (sbyte)row;
        Column = (sbyte)column;
    }
}
