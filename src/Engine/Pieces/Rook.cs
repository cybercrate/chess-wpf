using Engine.Conditions;
using Engine.Pieces.Base;
using Engine.UtilityComponents;

namespace Engine.Pieces;

internal class Rook : MovablePiece
{
    public Rook(bool white) : base(white)
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
        
        // Left side.
        for (var col = (sbyte)(coords.Column - 1); col >= 0; col--)
        {
            // If the square is empty.
            if (condition.Chessboard[coords.Row, col].Status is 'n' or 'x')
            {
                possibleMoves.Add(new Coords(coords.Row, col));
            }
            // Square has a piece.
            else 
            {
                // Piece is enemy.
                if (condition.Chessboard[coords.Row, col].White != White)
                {
                    possibleMoves.Add(new Coords(coords.Row, col));
                }
                
                break;
            }
        }
        
        // Right side.
        for (var col = (sbyte)(coords.Column + 1); col < 8; col++)
        {
            // If the square is empty.
            if (condition.Chessboard[coords.Row, col].Status is 'n' or 'x')
            {
                possibleMoves.Add(new Coords(coords.Row, col));
            }
            // Square has a piece.
            else 
            {
                // Piece is enemy.
                if (condition.Chessboard[coords.Row, col].White != White)
                {
                    possibleMoves.Add(new Coords(coords.Row, col));
                }
                
                break;
            }
        }
        
        // Top side.
        for (var row = (sbyte)(coords.Row - 1); row >= 0; row--)
        {
            // If the square is empty.
            if (condition.Chessboard[row, coords.Column].Status is 'n' or 'x')
            {
                possibleMoves.Add(new Coords(row, coords.Column));
            }
            // Square has a piece.
            else
            {
                // Piece is enemy.
                if (condition.Chessboard[row, coords.Column].White != White)
                {
                    possibleMoves.Add(new Coords(row, coords.Column));
                }
                
                break;
            }
        }
        
        // Bottom side.
        for (var row = (sbyte)(coords.Row + 1); row < 8; row++)
        {
            // If the square is empty.
            if (condition.Chessboard[row, coords.Column].Status is 'n' or 'x')
            {
                possibleMoves.Add(new Coords(row, coords.Column));
            }
            // Square has a piece.
            else
            {
                // Piece is enemy.
                if (condition.Chessboard[row, coords.Column].White != White)
                {
                    possibleMoves.Add(new Coords(row, coords.Column));
                }
                
                break;
            }
        }
        
        // If the king is in check or possible check after move of this piece...
        if (check || ProtectingKing)
        {
            // Only check preventing moves are legal.
            for (var i = possibleMoves.Count - 1; i >= 0; i--)
            {
                if (ChessEngine.ValidMoveDuringCheck(coords, possibleMoves[i], condition) is false)
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
        
        // Left side.
        for (var col = (sbyte)(coords.Column - 1); col >= 0; col--)
        {
            // If the square is empty.
            if (condition.Chessboard[coords.Row, col].Status is 'n' or 'x')
            {
                possibleAttacks.Add(new Coords(coords.Row, col));
            }
            // Square has a piece.
            else
            {
                possibleAttacks.Add(new Coords(coords.Row, col));
                
                // Checking whether there is enemy king behind the enemy piece.
                if (condition.Chessboard[coords.Row, col].White != White)
                {
                    // Continuing in row after finding the 2nd piece.
                    for (var col2 = (sbyte)(col - 1); col2 >= 0; col2--)
                    {
                        // Piece found.
                        if (condition.Chessboard[coords.Row, col2].Status is 'n' or 'x')
                        {
                            continue;
                        }
                        
                        // King found.
                        if (condition.Chessboard[coords.Row, col2].Status is 'k')
                        {
                            // Enemy king.
                            if (condition.Chessboard[coords.Row, col2].White != White)
                            {
                                PieceProtectingKingCoords = new Coords(coords.Row, col);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                break;
            }
        }
        
        // Right side.
        for (var col = (sbyte)(coords.Column + 1); col < 8; col++)
        {
            // If the square is empty.
            if (condition.Chessboard[coords.Row, col].Status is 'n' or 'x')
            {
                possibleAttacks.Add(new Coords(coords.Row, col));
            }
            // Square has a piece.
            else
            {
                possibleAttacks.Add(new Coords(coords.Row, col));
                
                // Checking whether there is enemy king behind the enemy piece.
                if (condition.Chessboard[coords.Row, col].White != White)
                {
                    // Continuing in row after finding the 2nd piece.
                    for (var col2 = (sbyte)(col + 1); col2 < 8; col2++)
                    {
                        // Piece found.
                        if (condition.Chessboard[coords.Row, col2].Status is 'n' or 'x')
                        {
                            continue;
                        }
                        
                        // King found.
                        if (condition.Chessboard[coords.Row, col2].Status is 'k')
                        {
                            // Enemy king.
                            if (condition.Chessboard[coords.Row, col2].White != White)
                            {
                                PieceProtectingKingCoords = new Coords(coords.Row, col);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                break;
            }
        }
        
        // Top side.
        for (var row = (sbyte)(coords.Row - 1); row >= 0; row--)
        {
            // If the square is empty.
            if (condition.Chessboard[row, coords.Column].Status is 'n' or 'x')
            {
                possibleAttacks.Add(new Coords(row, coords.Column));
            }
            // Square has a piece.
            else
            {
                possibleAttacks.Add(new Coords(row, coords.Column));
                
                // Checking whether there is enemy king behind the enemy piece.
                if (condition.Chessboard[row, coords.Column].White != White)
                {
                    // Continuing in row after finding the 2nd piece.
                    for (var row2 = (sbyte)(row - 1); row2 >= 0; row2--)
                    {
                        // Piece found.
                        if (condition.Chessboard[row2, coords.Column].Status is 'n' or 'x')
                        {
                            continue;
                        }
                        
                        // King found.
                        if (condition.Chessboard[row2, coords.Column].Status is 'k')
                        {
                            // Enemy king.
                            if (condition.Chessboard[row2, coords.Column].White != White)
                            {
                                PieceProtectingKingCoords = new Coords(row, coords.Column);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                break;
            }
        }
        
        // Bottom side.
        for (var row = (sbyte)(coords.Row + 1); row < 8; row++)
        {
            // If the square is empty.
            if (condition.Chessboard[row, coords.Column].Status is 'n' or 'x')
            {
                possibleAttacks.Add(new Coords(row, coords.Column));
            }
            else // Square has a piece.
            {
                possibleAttacks.Add(new Coords(row, coords.Column));
                
                // Checking whether there is enemy king behind the enemy piece.
                if (condition.Chessboard[row, coords.Column].White != White)
                {
                    // Continuing in row after finding the 2nd piece
                    for (var row2 = (sbyte)(row + 1); row2 < 8; row2++)
                    {
                        // Piece found.
                        if (condition.Chessboard[row2, coords.Column].Status is 'n' or 'x')
                        {
                            continue;
                        }
                        
                        // King found.
                        if (condition.Chessboard[row2, coords.Column].Status is 'k')
                        {
                            // Enemy king.
                            if (condition.Chessboard[row2, coords.Column].White != White)
                            {
                                PieceProtectingKingCoords = new Coords(row, coords.Column);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                break;
            }
        }
        PossibleAttacks = possibleAttacks.ToArray();
    }
}
