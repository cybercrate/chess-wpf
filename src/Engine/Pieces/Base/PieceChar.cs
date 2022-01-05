namespace Engine.Pieces.Base;

/// <summary>
/// Piece represented with a char and boolean (color).
/// </summary>
public struct PieceChar
{
    /// <summary>
    /// n - nothing, x - en passant, k - king, d - queen, v - rook, s - bishop, j - knight, p - pawn
    /// </summary>
    public char Status { get; set; }
    
    /// <summary>
    /// true - white piece, false - black piece
    /// </summary>
    public bool White { get; set; }
    
    /// <summary>
    /// Square constructor.
    /// </summary>
    /// <param name="status">
    /// n - nothing, x - en passant, k - king, d - queen, v - rook, s - bishop, j - knight, p - pawn</param>
    /// <param name="white">piece color</param>
    public PieceChar(char status, bool white = true)
    {
        Status = status;
        White = white;
    }
}
