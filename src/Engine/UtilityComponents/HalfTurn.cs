namespace Engine.UtilityComponents;

internal class HalfTurn
{
    public Coords From { get; }
    
    public Coords To { get; }

    public int Value { get; internal set; }
    
    public HalfTurn(Coords from, Coords to)
    {
        From = from;
        To = to;
    }
}
