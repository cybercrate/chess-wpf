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
    
    public Coords(sbyte row, sbyte column)
    {
        Row = row;
        Column = column;
    }
}
