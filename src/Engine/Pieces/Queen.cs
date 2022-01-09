using Engine.Conditions;
using Engine.Pieces.Base;
using Engine.UtilityComponents;

namespace Engine.Pieces;

internal class Queen : MovablePiece
{
    public Queen(bool white) : base(white)
    {
    }
        
    public override void UpdatePossibleMoves(Condition condition, bool check, Coords coords)
    {
        List<Coords> possibleMoves = new();
        
        if (condition.Draw50 > 99)
        {
            PossibleMoves = possibleMoves.ToArray();
            return;
        }
        
        Rook rook = new(White);
        rook.UpdatePossibleMoves(condition, check, coords);
        
        Bishop bishop = new(White);
        bishop.UpdatePossibleMoves(condition, check, coords);
        
        possibleMoves.AddRange(rook.PossibleMoves);
        possibleMoves.AddRange(bishop.PossibleMoves);

        // If the king is in check or possible check after move...
        if (check || ProtectingKing)
        {
            // Only check preventing moves are legal.
            for (var i = possibleMoves.Count - 1; i >= 0; i--)
            {
                var validMoveDuring = ChessEngine.ValidMoveDuringCheck(coords, possibleMoves[i], condition);
                
                if (validMoveDuring is false)
                {
                    possibleMoves.RemoveAt(i);
                }
            }
        }

        PossibleMoves = possibleMoves.ToArray();
    }
    
    public override void UpdatePossibleAttacks(Condition condition, Coords coords)
    {
        PieceProtectingKingCoords = new Coords(8, 8);
        List<Coords> possibleAttacks = new();
        
        Rook rook = new(White);
        rook.UpdatePossibleAttacks(condition, coords);
        
        if (rook.PieceProtectingKingCoords.Row is not 8)
        {
            PieceProtectingKingCoords = rook.PieceProtectingKingCoords;
        }
        
        Bishop bishop = new(White);
        bishop.UpdatePossibleAttacks(condition, coords);
        
        if (bishop.PieceProtectingKingCoords.Row is not 8)
        {
            PieceProtectingKingCoords = bishop.PieceProtectingKingCoords;
        }
        
        possibleAttacks.AddRange(rook.PossibleAttacks);
        possibleAttacks.AddRange(bishop.PossibleAttacks);
        
        PossibleAttacks = possibleAttacks.ToArray();
    }
}
