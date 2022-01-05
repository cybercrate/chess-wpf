namespace Engine.UtilityComponents;

/// <summary>
/// 
/// </summary>
internal class HalfTurn
{
    /// <summary>
    /// 
    /// </summary>
    public Coords From { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public Coords To { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public int Value { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public HalfTurn(Coords from, Coords to)
    {
        From = from;
        To = to;
        Value = 0;
    }
}
