using Engine.Conditions;
using Engine.Pieces.Base;
using Engine.Pieces.Types;
using Engine.UtilityComponents;

namespace Engine.Pieces;

internal sealed class Pawn : Piece
{
    public Pawn(PieceColor color) : base(color)
    {
    }

    public override void UpdatePossibleMoves(Condition condition, bool check, Coords coords)
    {
        List<Coords> possibleMoves = new();

        Status status;
        PieceColor color;
        
        if (condition.Draw50 > 99)
        {
            PossibleMoves = possibleMoves.ToArray();
            return;
        }
        
        // Loading all possible moves.
        // The piece is black and moves down.
        if (PieceColor is PieceColor.Black) 
        {
            // Checking whether the pawn isn't in the last row.
            if (coords.Row is not 7)
            {
                status = condition.Chessboard[coords.Row + 1, coords.Column].Status;
                
                // 1st square ahead of the pawn is free.
                if (status is Status.Empty)
                {
                    possibleMoves.Add(new Coords(coords.Row + 1, coords.Column));
                    
                    // The piece is on starting position.
                    if (coords.Row is 1)
                    {
                        status = condition.Chessboard[coords.Row + 2, coords.Column].Status;
                        
                        // 2nd square ahead of the pawn is free.
                        if (status is Status.Empty)
                        {
                            possibleMoves.Add(new Coords(coords.Row + 2, coords.Column));
                        }
                    }
                }
                
                // Checking whether the pawn isn't in the left column.
                if (coords.Column is not 0)
                {
                    status = condition.Chessboard[coords.Row + 1, coords.Column - 1].Status;
                    
                    // On the left attack square is a piece?
                    if (status is not Status.Empty)
                    {
                        color = condition.Chessboard[coords.Row + 1, coords.Column - 1].PieceColor;
                        
                        // Piece is enemy.
                        if (color != PieceColor)
                        {
                            possibleMoves.Add(new Coords(coords.Row + 1, coords.Column - 1));
                        }
                    }
                }
                
                // Checking whether the pawn isn't in the right column.
                if (coords.Column is not 7)
                {
                    status = condition.Chessboard[coords.Row + 1, coords.Column + 1].Status;
                    
                    // On the right attack square is a piece?
                    if (status is not Status.Empty)
                    {
                        color = condition.Chessboard[coords.Row + 1, coords.Column + 1].PieceColor;
                        
                        // Piece is enemy.
                        if (color != PieceColor)
                        {
                            possibleMoves.Add(new Coords(coords.Row + 1, coords.Column + 1));
                        }
                    }
                }
            }
        }
        // The piece is white and moves up.
        else
        {
            // Checking whether a pawn isn't in the last row.
            if (coords.Row is not 0)
            {
                status = condition.Chessboard[coords.Row - 1, coords.Column].Status;
                
                // 1st square ahead of the pawn is free.
                if (status is Status.Empty)
                {
                    possibleMoves.Add(new Coords(coords.Row - 1, coords.Column));
                    
                    // The piece is on starting position.
                    if (coords.Row is 6)
                    {
                        status = condition.Chessboard[coords.Row - 2, coords.Column].Status;
                        
                        // 2nd square ahead of the pawn is free.
                        if (status is Status.Empty)
                        {
                            possibleMoves.Add(new Coords(coords.Row - 2, coords.Column));
                        }
                    }
                }
                
                // Checking whether the pawn isn't in the left column.
                if (coords.Column is not 0)
                {
                    status = condition.Chessboard[coords.Row - 1, coords.Column - 1].Status;
                    
                    // On the left attack square is a piece?
                    if (status is not Status.Empty)
                    {
                        color = condition.Chessboard[coords.Row - 1, coords.Column - 1].PieceColor;
                        
                        // Piece is enemy.
                        if (color != PieceColor)
                        {
                            possibleMoves.Add(new Coords(coords.Row - 1, coords.Column - 1));
                        }
                    }
                }
                
                // Checking whether the pawn isn't in the right column.
                if (coords.Column is not 7)
                {
                    status = condition.Chessboard[coords.Row - 1, coords.Column + 1].Status;

                    // On the right attack square is a piece?
                    if (status is not Status.Empty)
                    {
                        color = condition.Chessboard[coords.Row - 1, coords.Column + 1].PieceColor;
                        
                        // Piece is enemy.
                        if (color != PieceColor)
                        {
                            possibleMoves.Add(new Coords(coords.Row - 1, coords.Column + 1));
                        }
                    }
                }
            }
        }

        // If the king is in check or possible check after move of this piece...
        if (check || ProtectingKing)
        {
            // only check preventing moves are legal.
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
        List<Coords> possibleAttacks = new();
        
        // The piece is black and moves down.
        if (PieceColor is PieceColor.Black) 
        {
            // Checking whether the pawn isn't in the last row.
            if (coords.Row is not 7)
            {
                // Checking whether the pawn isn't in the left column.
                if (coords.Column is not 0)
                {
                    possibleAttacks.Add(new Coords(coords.Row + 1, coords.Column - 1));
                }
                
                // Checking whether the pawn isn't in the right column.
                if (coords.Column is not 7)
                {
                    possibleAttacks.Add(new Coords(coords.Row + 1, coords.Column + 1));
                }
            }
        }
        // The piece is white and moves up.
        else
        {
            // Checking whether the pawn isn't in the last row.
            if (coords.Row is not 0)
            {
                // Checking whether the pawn isn't in the left column.
                if (coords.Column is not 0)
                {
                    possibleAttacks.Add(new Coords(coords.Row - 1, coords.Column - 1));
                }
                
                // Checking whether the pawn isn't in the right column.
                if (coords.Column is not 7)
                {
                    possibleAttacks.Add(new Coords(coords.Row - 1, coords.Column + 1));
                }
            }
        }

        PossibleAttacks = possibleAttacks.ToArray();
    }
}
