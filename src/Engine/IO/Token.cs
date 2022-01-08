namespace Engine.IO;

/// <summary>
/// Symbolic representation of chessboard square statuses.
/// </summary>
public static class Token
{
    public const char White = 'w';
    public const char Black = 'b';
    public const char EnPassant = 'e';
    
    public const char King = 'K';
    public const char Queen = 'Q';
    public const char Rook = 'R';
    public const char Bishop = 'B';
    public const char Knight = 'N';
    public const char Pawn = 'P';

    public const char Empty = '[';
    public const char Result = 'T';
    public const char Invalid = '\uffff';
    public const char Return = '\r';
}
