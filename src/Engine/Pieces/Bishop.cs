using Engine.Conditions;
using Engine.Pieces.Base;
using Engine.Pieces.Types;
using Engine.UtilityComponents;

namespace Engine.Pieces;

internal class Bishop: MovablePiece
{
    public Bishop(bool white) : base(white)
    {
    }
        
    public override void UpdatePossibleMoves(Condition condition, bool check, Coords currentCoords)
    {
        List<Coords> possibleMoves = new();
        
        if (condition.Draw50 > 99)
        {
            PossibleMoves = possibleMoves.ToArray();
            return;
        }
        
        // For loop columns counter, rows counter.
        var row = currentCoords.Row;
        sbyte col;
        
        // Left top side.
        for (col = (sbyte)(currentCoords.Column - 1); col >= 0; col--)
        {
            row--;
            
            if (row >= 0)
            {
                // If the square is empty.
                if (condition.Chessboard[row, col].Status is Status.Empty or Status.EnPassant)
                {
                    possibleMoves.Add(new Coords(row, col));
                }
                else // Square has a piece.
                {
                    // Piece is enemy.
                    if (condition.Chessboard[row, col].White != White) 
                    {
                        possibleMoves.Add(new Coords(row, col));
                    }
                    
                    break;
                }
            }
            else
            {
                break;
            }
        }
        
        // Right top side.
        row = currentCoords.Row;
        
        for (col = (sbyte)(currentCoords.Column + 1); col < 8; col++)
        {
            row--;
            
            if (row >= 0)
            {
                // If the square is empty.
                if (condition.Chessboard[row, col].Status is Status.Empty or Status.EnPassant)
                {
                    possibleMoves.Add(new Coords(row, col));
                }
                // Square has a piece.
                else 
                {
                    // Piece is enemy.
                    if (condition.Chessboard[row, col].White != White)
                    {
                        possibleMoves.Add(new Coords(row, col));
                    }
                    break;
                }
            }
            else
            {
                break;
            }
        }
        
        // Left bottom side.
        row = currentCoords.Row;
        
        for (col = (sbyte)(currentCoords.Column - 1); col >= 0; col--)
        {
            row++;
            
            if (row < 8)
            {
                // If the square is empty.
                if (condition.Chessboard[row, col].Status is Status.Empty or Status.EnPassant)
                {
                    possibleMoves.Add(new Coords(row, col));
                }
                // Square has a piece.
                else
                {
                    // Piece is enemy.
                    if (condition.Chessboard[row, col].White != White) 
                    {
                        possibleMoves.Add(new Coords(row, col));
                    }
                    break;
                }
            }
            else
            {
                break;
            }
        }
        
        // Right bottom side.
        row = currentCoords.Row;
        
        for (col = (sbyte)(currentCoords.Column + 1); col < 8; col++)
        {
            row++;
            
            if (row < 8)
            {
                // If the square is empty.
                if (condition.Chessboard[row, col].Status is Status.Empty or Status.EnPassant)
                {
                    possibleMoves.Add(new Coords(row, col));
                }
                // Square has a piece.
                else 
                {
                    // Piece is enemy.
                    if (condition.Chessboard[row, col].White != White) 
                    {
                        possibleMoves.Add(new Coords(row, col));
                    }
                    
                    break;
                }
            }
            else
            {
                break;
            }
        }
        
        // If the king is in check or possible check after move of this piece...
        if (check || ProtectingKing)
        {
            // only check preventing moves are legal.
            for (var i = possibleMoves.Count - 1; i >= 0; i--)
            {
                if (ChessEngine.ValidMoveDuringCheck(currentCoords, possibleMoves[i], condition) is false)
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
        
        // For loop columns counter, rows counter.
        var row = coords.Row;
        sbyte col;
        
        // Left top side.
        for (col = (sbyte)(coords.Column - 1); col >= 0; col--)
        {
            row--;
            
            if (row >= 0)
            {
                // If the square is empty.
                if (condition.Chessboard[row, col].Status is Status.Empty or Status.EnPassant)
                {
                    possibleAttacks.Add(new Coords(row, col));
                }
                // Square has a piece.
                else 
                {
                    possibleAttacks.Add(new Coords(row, col));
                    
                    // Checking whether there is enemy king behind the enemy piece.
                    if (condition.Chessboard[row, col].White != White)
                    {
                        // Creating new variables for keeping the piece variables
                        var row2 = row;
                        sbyte col2;
                        
                        // Continuing in row after finding the 2nd piece
                        for (col2 = (sbyte)(col - 1); col2 >= 0; col2--)
                        {
                            row2--;
                            if (row2 >= 0)
                            {
                                // Piece found.
                                if (condition.Chessboard[row2, col2].Status is Status.Empty or Status.EnPassant)
                                {
                                    continue;
                                }
                                
                                // King found.
                                if (condition.Chessboard[row2, col2].Status is Status.King)
                                {
                                    // Enemy king..
                                    if (condition.Chessboard[row2, col2].White != White)
                                    {
                                        PieceProtectingKingCoords = new Coords(row, col);
                                    }
                                }
                                else
                                {
                                    break;
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
            else
            {
                break;
            }
        }
        
        // Right top side.
        row = coords.Row;
        
        for (col = (sbyte)(coords.Column + 1); col < 8; col++)
        {
            row--;
            if (row >= 0)
            {
                // If the square is empty.
                if (condition.Chessboard[row, col].Status is Status.Empty or Status.EnPassant)
                {
                    possibleAttacks.Add(new Coords(row, col));
                }
                else // Square has a piece.
                {
                    possibleAttacks.Add(new Coords(row, col));
                    
                    // Checking whether there is enemy king behind the enemy piece.
                    if (condition.Chessboard[row, col].White != White)
                    {
                        // Creating new variables for keeping the piece variables.
                        var row2 = row;
                        sbyte col2;
                        
                        // Continuing in row after finding the 2nd piece.
                        for (col2 = (sbyte)(col + 1); col2 < 8; col2++)
                        {
                            row2--;
                            if (row2 >= 0)
                            {
                                // Piece found.
                                if (condition.Chessboard[row2, col2].Status is Status.Empty or Status.EnPassant)
                                {
                                    continue;
                                }
                                
                                // King found.
                                if (condition.Chessboard[row2, col2].Status is Status.King)
                                {
                                    // Enemy king.
                                    if (condition.Chessboard[row2, col2].White != White)
                                    {
                                        PieceProtectingKingCoords = new Coords(row, col);
                                    }
                                }
                                else
                                {
                                    break;
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
            else
            {
                break;
            }
        }
        
        // Left bottom side.
        row = coords.Row;
        
        for (col = (sbyte)(coords.Column - 1); col >= 0; col--)
        {
            row++;
            if (row < 8)
            {
                // If the square is empty.
                if (condition.Chessboard[row, col].Status is Status.Empty or Status.EnPassant)
                {
                    possibleAttacks.Add(new Coords(row, col));
                }
                // Square has a piece.
                else 
                {
                    possibleAttacks.Add(new Coords(row, col));
                    
                    // Checking whether there is enemy king behind the enemy piece.
                    if (condition.Chessboard[row, col].White != White)
                    {
                        // Creating new variables for keeping the piece variables.
                        var row2 = row;
                        sbyte col2;
                        
                        // Continuing in row after finding the 2nd piece.
                        for (col2 = (sbyte)(col - 1); col2 >= 0; col2--)
                        {
                            row2++;
                            
                            if (row2 < 8)
                            {
                                // Piece found.
                                if (condition.Chessboard[row2, col2].Status is Status.Empty or Status.EnPassant)
                                {
                                    continue;
                                }
                                
                                // King found.
                                if (condition.Chessboard[row2, col2].Status is Status.King)
                                {
                                    // Enemy king.
                                    if (condition.Chessboard[row2, col2].White != White)
                                    {
                                        PieceProtectingKingCoords = new Coords(row, col);
                                    }
                                }
                                else
                                {
                                    break;
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
            else
            {
                break;
            }
        }
        
        // Right bottom side.
        row = coords.Row;
        for (col = (sbyte)(coords.Column + 1); col < 8; col++)
        {
            row++;
            if (row < 8)
            {
                // If the square is empty.
                if (condition.Chessboard[row, col].Status is Status.Empty or Status.EnPassant)
                {
                    possibleAttacks.Add(new Coords(row, col));
                }
                // Square has a piece.
                else 
                {
                    possibleAttacks.Add(new Coords(row, col));
                    
                    // Checking whether there is enemy king behind the enemy piece.
                    if (condition.Chessboard[row, col].White != White)
                    {
                        // Creating new variables for keeping the piece variables.
                        var row2 = row;
                        sbyte col2;
                        
                        // Continuing in row after finding the 2nd piece.
                        for (col2 = (sbyte)(col + 1); col2 < 8; col2++)
                        {
                            row2++;
                            
                            if (row2 < 8)
                            {
                                // Piece found.
                                if (condition.Chessboard[row2, col2].Status is Status.Empty or Status.EnPassant)
                                {
                                    continue;
                                }
                                
                                // King found.
                                if (condition.Chessboard[row2, col2].Status is Status.King)
                                {
                                    // Enemy king.
                                    if (condition.Chessboard[row2, col2].White != White)
                                    {
                                        PieceProtectingKingCoords = new Coords(row, col);
                                    }
                                }
                                else
                                {
                                    break;
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
            else
            {
                break;
            }
        }
        
        PossibleAttacks = possibleAttacks.ToArray();
    }
}
