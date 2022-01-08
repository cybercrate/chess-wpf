namespace Engine.Pieces.Types;

/// <summary>
/// Piece represented with a char and boolean (color).
/// </summary>
public struct PieceId
{
    /// <summary>
    /// 
    /// </summary>
    public Status Status { get; set; }
    
    /// <summary>
    /// true - white piece, false - black piece.
    /// </summary>
    public bool White { get; set; }
    
    /// <summary>
    /// Square constructor.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="white">Piece color.</param>
    public PieceId(Status status, bool white = true)
    {
        Status = status;
        White = white;
    }
}
