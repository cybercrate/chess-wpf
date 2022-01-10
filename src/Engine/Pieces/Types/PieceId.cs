namespace Engine.Pieces.Types;

/// <summary>
/// Represents piece identifier.
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
    public PieceColor PieceColor { get; set; }

    /// <summary>
    /// Square constructor.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="color">Piece color.</param>
    public PieceId(Status status, PieceColor color)
    {
        Status = status;
        PieceColor = color;
    }
}
