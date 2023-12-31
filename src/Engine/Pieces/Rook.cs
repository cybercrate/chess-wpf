﻿using Engine.Conditions;
using Engine.Pieces.Base;
using Engine.Pieces.Types;
using Engine.UtilityComponents;

namespace Engine.Pieces;

internal sealed class Rook : MovablePiece
{
    public Rook(PieceColor color) : base(color)
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

        Status status;
        PieceColor color;
        
        // Left side.
        for (var column = (sbyte)(coords.Column - 1); column >= 0; column--)
        {
            status = condition.Chessboard[coords.Row, column].Status;
            
            // If the square is empty.
            if (status is Status.Empty or Status.EnPassant)
            {
                possibleMoves.Add(new Coords(coords.Row, column));
            }
            // Square has a piece.
            else
            {
                color = condition.Chessboard[coords.Row, column].PieceColor;
                
                // Piece is enemy.
                if (color != PieceColor)
                {
                    possibleMoves.Add(new Coords(coords.Row, column));
                }
                
                break;
            }
        }
        
        // Right side.
        for (var column = (sbyte)(coords.Column + 1); column < 8; column++)
        {
            status = condition.Chessboard[coords.Row, column].Status;
            
            // If the square is empty.
            if (status is Status.Empty or Status.EnPassant)
            {
                possibleMoves.Add(new Coords(coords.Row, column));
            }
            // Square has a piece.
            else
            {
                color = condition.Chessboard[coords.Row, column].PieceColor;
                
                // Piece is enemy.
                if (color != PieceColor)
                {
                    possibleMoves.Add(new Coords(coords.Row, column));
                }
                
                break;
            }
        }
        
        // Top side.
        for (var row = (sbyte)(coords.Row - 1); row >= 0; row--)
        {
            status = condition.Chessboard[row, coords.Column].Status;
            
            // If the square is empty.
            if (status is Status.Empty or Status.EnPassant)
            {
                possibleMoves.Add(new Coords(row, coords.Column));
            }
            // Square has a piece.
            else
            {
                color = condition.Chessboard[row, coords.Column].PieceColor;
                
                // Piece is enemy.
                if (color != PieceColor)
                {
                    possibleMoves.Add(new Coords(row, coords.Column));
                }
                
                break;
            }
        }
        
        // Bottom side.
        for (var row = (sbyte)(coords.Row + 1); row < 8; row++)
        {
            status = condition.Chessboard[row, coords.Column].Status;
            
            // If the square is empty.
            if (status is Status.Empty or Status.EnPassant)
            {
                possibleMoves.Add(new Coords(row, coords.Column));
            }
            // Square has a piece.
            else
            {
                color = condition.Chessboard[row, coords.Column].PieceColor;
                
                // Piece is enemy.
                if (color != PieceColor)
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

        Status status;
        PieceColor color;
        
        // Left side.
        for (var column = (sbyte)(coords.Column - 1); column >= 0; column--)
        {
            status = condition.Chessboard[coords.Row, column].Status;
            
            // If the square is empty.
            if (status is Status.Empty or Status.EnPassant)
            {
                possibleAttacks.Add(new Coords(coords.Row, column));
            }
            // Square has a piece.
            else
            {
                possibleAttacks.Add(new Coords(coords.Row, column));

                color = condition.Chessboard[coords.Row, column].PieceColor;
                
                // Checking whether there is enemy king behind the enemy piece.
                if (color != PieceColor)
                {
                    // Continuing in row after finding the 2nd piece.
                    for (var innerColumn = (sbyte)(column - 1); innerColumn >= 0; innerColumn--)
                    {
                        status = condition.Chessboard[coords.Row, innerColumn].Status;
                        
                        // Piece found.
                        if (status is Status.Empty or Status.EnPassant)
                        {
                            continue;
                        }
                        
                        // King found.
                        if (status is Status.King)
                        {
                            color = condition.Chessboard[coords.Row, innerColumn].PieceColor;
                            
                            // Enemy king.
                            if (color != PieceColor)
                            {
                                PieceProtectingKingCoords = new Coords(coords.Row, column);
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
        for (var column = (sbyte)(coords.Column + 1); column < 8; column++)
        {
            status = condition.Chessboard[coords.Row, column].Status;
            
            // If the square is empty.
            if (status is Status.Empty or Status.EnPassant)
            {
                possibleAttacks.Add(new Coords(coords.Row, column));
            }
            // Square has a piece.
            else
            {
                possibleAttacks.Add(new Coords(coords.Row, column));

                color = condition.Chessboard[coords.Row, column].PieceColor;
                
                // Checking whether there is enemy king behind the enemy piece.
                if (color != PieceColor)
                {
                    // Continuing in row after finding the 2nd piece.
                    for (var innerColumn = (sbyte)(column + 1); innerColumn < 8; innerColumn++)
                    {
                        status = condition.Chessboard[coords.Row, innerColumn].Status;
                        
                        // Piece found.
                        if (status is Status.Empty or Status.EnPassant)
                        {
                            continue;
                        }

                        // King found.
                        if (status is Status.King)
                        {
                            color = condition.Chessboard[coords.Row, innerColumn].PieceColor;
                            
                            // Enemy king.
                            if (color != PieceColor)
                            {
                                PieceProtectingKingCoords = new Coords(coords.Row, column);
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
            status = condition.Chessboard[row, coords.Column].Status;
            
            // If the square is empty.
            if (status is Status.Empty or Status.EnPassant)
            {
                possibleAttacks.Add(new Coords(row, coords.Column));
            }
            // Square has a piece.
            else
            {
                possibleAttacks.Add(new Coords(row, coords.Column));

                color = condition.Chessboard[row, coords.Column].PieceColor;
                
                // Checking whether there is enemy king behind the enemy piece.
                if (color != PieceColor)
                {
                    // Continuing in row after finding the 2nd piece.
                    for (var innerRow = (sbyte)(row - 1); innerRow >= 0; innerRow--)
                    {
                        status = condition.Chessboard[innerRow, coords.Column].Status;
                        
                        // Piece found.
                        if (status is Status.Empty or Status.EnPassant)
                        {
                            continue;
                        }

                        // King found.
                        if (status is Status.King)
                        {
                            color = condition.Chessboard[innerRow, coords.Column].PieceColor;
                            
                            // Enemy king.
                            if (color != PieceColor)
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
            status = condition.Chessboard[row, coords.Column].Status;
            
            // If the square is empty.
            if (status is Status.Empty or Status.EnPassant)
            {
                possibleAttacks.Add(new Coords(row, coords.Column));
            }
            else // Square has a piece.
            {
                possibleAttacks.Add(new Coords(row, coords.Column));

                color = condition.Chessboard[row, coords.Column].PieceColor;
                
                // Checking whether there is enemy king behind the enemy piece.
                if (color != PieceColor)
                {
                    // Continuing in row after finding the 2nd piece
                    for (var innerRow = (sbyte)(row + 1); innerRow < 8; innerRow++)
                    {
                        status = condition.Chessboard[innerRow, coords.Column].Status;
                        
                        // Piece found.
                        if (status is Status.Empty or Status.EnPassant)
                        {
                            continue;
                        }

                        // King found.
                        if (status is Status.King)
                        {
                            color = condition.Chessboard[innerRow, coords.Column].PieceColor;
                            
                            // Enemy king.
                            if (color != PieceColor)
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
