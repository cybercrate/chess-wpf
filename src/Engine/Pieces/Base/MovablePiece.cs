using Engine.Pieces.Types;
using Engine.UtilityComponents;

namespace Engine.Pieces.Base;

internal abstract class MovablePiece : Piece
{
    /// <summary>
    /// Coordinates of piece that protects the king from check.
    /// If there is no such piece, the coordinates are set to 8;8.
    /// </summary>
    public Coords PieceProtectingKingCoords { get; set; }

    protected MovablePiece(PieceColor color) : base(color)
    {
    }
}
