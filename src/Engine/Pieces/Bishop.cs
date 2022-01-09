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
        sbyte column;
        
        Status status;
        bool white;
        
        // Left top side.
        for (column = (sbyte)(currentCoords.Column - 1); column >= 0; column--)
        {
            row--;
            
            if (row >= 0)
            {
                status = condition.Chessboard[row, column].Status;
                
                // If the square is empty.
                if (status is Status.Empty or Status.EnPassant)
                {
                    possibleMoves.Add(new Coords(row, column));
                }
                else // Square has a piece.
                {
                    white = condition.Chessboard[row, column].White;
                    
                    // Piece is enemy.
                    if (white != White) 
                    {
                        possibleMoves.Add(new Coords(row, column));
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
        
        for (column = (sbyte)(currentCoords.Column + 1); column < 8; column++)
        {
            row--;
            
            if (row >= 0)
            {
                status = condition.Chessboard[row, column].Status;

                // If the square is empty.
                if (status is Status.Empty or Status.EnPassant)
                {
                    possibleMoves.Add(new Coords(row, column));
                }
                // Square has a piece.
                else 
                {
                    white = condition.Chessboard[row, column].White;
                    
                    // Piece is enemy.
                    if (white != White)
                    {
                        possibleMoves.Add(new Coords(row, column));
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
        
        for (column = (sbyte)(currentCoords.Column - 1); column >= 0; column--)
        {
            row++;
            
            if (row < 8)
            {
                status = condition.Chessboard[row, column].Status;
                
                // If the square is empty.
                if (status is Status.Empty or Status.EnPassant)
                {
                    possibleMoves.Add(new Coords(row, column));
                }
                // Square has a piece.
                else
                {
                    white = condition.Chessboard[row, column].White;
                    
                    // Piece is enemy.
                    if (white != White) 
                    {
                        possibleMoves.Add(new Coords(row, column));
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
        
        for (column = (sbyte)(currentCoords.Column + 1); column < 8; column++)
        {
            row++;
            
            if (row < 8)
            {
                status = condition.Chessboard[row, column].Status;
                
                // If the square is empty.
                if (status is Status.Empty or Status.EnPassant)
                {
                    possibleMoves.Add(new Coords(row, column));
                }
                // Square has a piece.
                else
                {
                    white = condition.Chessboard[row, column].White;
                    
                    // Piece is enemy.
                    if (white != White) 
                    {
                        possibleMoves.Add(new Coords(row, column));
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
                var validMoveDuring = ChessEngine.ValidMoveDuringCheck(currentCoords, possibleMoves[i], condition);
                
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
        
        // For loop columns counter, rows counter.
        var row = coords.Row;
        sbyte column;

        Status status;
        bool white;
        
        // Left top side.
        for (column = (sbyte)(coords.Column - 1); column >= 0; column--)
        {
            row--;
            
            if (row >= 0)
            {
                status = condition.Chessboard[row, column].Status;
                
                // If the square is empty.
                if (status is Status.Empty or Status.EnPassant)
                {
                    possibleAttacks.Add(new Coords(row, column));
                }
                // Square has a piece.
                else 
                {
                    possibleAttacks.Add(new Coords(row, column));

                    white = condition.Chessboard[row, column].White;
                    
                    // Checking whether there is enemy king behind the enemy piece.
                    if (white != White)
                    {
                        // Creating new variables for keeping the piece variables
                        var innerRow = row;
                        sbyte innerColumn;
                        
                        // Continuing in row after finding the 2nd piece
                        for (innerColumn = (sbyte)(column - 1); innerColumn >= 0; innerColumn--)
                        {
                            innerRow--;
                            
                            if (innerRow >= 0)
                            {
                                status = condition.Chessboard[innerRow, innerColumn].Status;
                                
                                // Piece found.
                                if (status is Status.Empty or Status.EnPassant)
                                {
                                    continue;
                                }

                                // King found.
                                if (status is Status.King)
                                {
                                    white = condition.Chessboard[innerRow, innerColumn].White;
                                    
                                    // Enemy king..
                                    if (white != White)
                                    {
                                        PieceProtectingKingCoords = new Coords(row, column);
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
        
        for (column = (sbyte)(coords.Column + 1); column < 8; column++)
        {
            row--;
            if (row >= 0)
            {
                status = condition.Chessboard[row, column].Status;
                
                // If the square is empty.
                if (status is Status.Empty or Status.EnPassant)
                {
                    possibleAttacks.Add(new Coords(row, column));
                }
                else // Square has a piece.
                {
                    possibleAttacks.Add(new Coords(row, column));

                    white = condition.Chessboard[row, column].White;
                    
                    // Checking whether there is enemy king behind the enemy piece.
                    if (white != White)
                    {
                        // Creating new variables for keeping the piece variables.
                        var innerRow = row;
                        sbyte innerColumn;
                        
                        // Continuing in row after finding the 2nd piece.
                        for (innerColumn = (sbyte)(column + 1); innerColumn < 8; innerColumn++)
                        {
                            innerRow--;
                            
                            if (innerRow >= 0)
                            {
                                status = condition.Chessboard[innerRow, innerColumn].Status;
                                
                                // Piece found.
                                if (status is Status.Empty or Status.EnPassant)
                                {
                                    continue;
                                }

                                // King found.
                                if (status is Status.King)
                                {
                                    white = condition.Chessboard[innerRow, innerColumn].White;
                                    
                                    // Enemy king.
                                    if (white != White)
                                    {
                                        PieceProtectingKingCoords = new Coords(row, column);
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
        
        for (column = (sbyte)(coords.Column - 1); column >= 0; column--)
        {
            row++;
            if (row < 8)
            {
                status = condition.Chessboard[row, column].Status;
                
                // If the square is empty.
                if (status is Status.Empty or Status.EnPassant)
                {
                    possibleAttacks.Add(new Coords(row, column));
                }
                // Square has a piece.
                else 
                {
                    possibleAttacks.Add(new Coords(row, column));

                    white = condition.Chessboard[row, column].White;
                    
                    // Checking whether there is enemy king behind the enemy piece.
                    if (white != White)
                    {
                        // Creating new variables for keeping the piece variables.
                        var innerRow = row;
                        sbyte innerColumn;
                        
                        // Continuing in row after finding the 2nd piece.
                        for (innerColumn = (sbyte)(column - 1); innerColumn >= 0; innerColumn--)
                        {
                            innerRow++;
                            
                            if (innerRow < 8)
                            {
                                status = condition.Chessboard[innerRow, innerColumn].Status;
                                
                                // Piece found.
                                if (status is Status.Empty or Status.EnPassant)
                                {
                                    continue;
                                }

                                // King found.
                                if (status is Status.King)
                                {
                                    white = condition.Chessboard[innerRow, innerColumn].White;
                                    
                                    // Enemy king.
                                    if (white != White)
                                    {
                                        PieceProtectingKingCoords = new Coords(row, column);
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
        
        row = coords.Row;
        
        // Right bottom side.
        for (column = (sbyte)(coords.Column + 1); column < 8; column++)
        {
            row++;
            if (row < 8)
            {
                status = condition.Chessboard[row, column].Status;
                
                // If the square is empty.
                if (status is Status.Empty or Status.EnPassant)
                {
                    possibleAttacks.Add(new Coords(row, column));
                }
                // Square has a piece.
                else 
                {
                    possibleAttacks.Add(new Coords(row, column));

                    white = condition.Chessboard[row, column].White;
                    
                    // Checking whether there is enemy king behind the enemy piece.
                    if (white != White)
                    {
                        // Creating new variables for keeping the piece variables.
                        var innerRow = row;
                        sbyte innerColumn;
                        
                        // Continuing in row after finding the 2nd piece.
                        for (innerColumn = (sbyte)(column + 1); innerColumn < 8; innerColumn++)
                        {
                            innerRow++;
                            
                            if (innerRow < 8)
                            {
                                status = condition.Chessboard[innerRow, innerColumn].Status;
                                
                                // Piece found.
                                if (status is Status.Empty or Status.EnPassant)
                                {
                                    continue;
                                }

                                // King found.
                                if (status is Status.King)
                                {
                                    white = condition.Chessboard[innerRow, innerColumn].White;
                                    
                                    // Enemy king.
                                    if (white != White)
                                    {
                                        PieceProtectingKingCoords = new Coords(row, column);
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
