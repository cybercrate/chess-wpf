﻿using Engine.Conditions;
using Engine.Pieces.Base;
using Engine.Pieces.Types;
using Engine.UtilityComponents;

namespace Engine.Pieces;

internal sealed class Knight : Piece
{
    public Knight(PieceColor color) : base(color)
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

        // Processed coordinates.
        Coords processedCoords;

        Status status;
        PieceColor color;

        // Top side.
        if (coords.Row > 1 && coords.Column > 0)
        {
            processedCoords = new Coords(coords.Row - 2, coords.Column - 1);
            
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;

            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleMoves.Add(processedCoords);
            }
        }

        if (coords.Row > 1 && coords.Column < 7)
        {
            processedCoords = new Coords(coords.Row - 2, coords.Column + 1);

            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleMoves.Add(processedCoords);
            }
        }

        // Bottom side.
        if (coords.Row < 6 && coords.Column > 0)
        {
            processedCoords = new Coords(coords.Row + 2, coords.Column - 1);

            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleMoves.Add(processedCoords);
            }
        }

        if (coords.Row < 6 && coords.Column < 7)
        {
            processedCoords = new Coords(coords.Row + 2, coords.Column + 1);

            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleMoves.Add(processedCoords);
            }
        }

        // Left side.
        if (coords.Row > 0 && coords.Column > 1)
        {
            processedCoords = new Coords(coords.Row - 1, coords.Column - 2);
            
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleMoves.Add(processedCoords);
            }
        }

        if (coords.Row < 7 && coords.Column > 1)
        {
            processedCoords = new Coords(coords.Row + 1, coords.Column - 2);
            
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleMoves.Add(processedCoords);
            }
        }

        // Right side.
        if (coords.Row > 0 && coords.Column < 6)
        {
            processedCoords = new Coords(coords.Row - 1, coords.Column + 2);
            
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleMoves.Add(processedCoords);
            }
        }

        if (coords.Row < 7 && coords.Column < 6)
        {
            processedCoords = new Coords(coords.Row + 1, coords.Column + 2);
            
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;

            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleMoves.Add(processedCoords);
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

        // Top side.
        if (coords.Row > 1 && coords.Column > 0)
        {
            possibleAttacks.Add(new Coords(coords.Row - 2, coords.Column - 1));
        }

        if (coords.Row > 1 && coords.Column < 7)
        {
            possibleAttacks.Add(new Coords(coords.Row - 2, coords.Column + 1));
        }

        // Bottom side.
        if (coords.Row < 6 && coords.Column > 0)
        {
            possibleAttacks.Add(new Coords(coords.Row + 2, coords.Column - 1));
        }

        if (coords.Row < 6 && coords.Column < 7)
        {
            possibleAttacks.Add(new Coords(coords.Row + 2, coords.Column + 1));
        }

        // Left side.
        if (coords.Row > 0 && coords.Column > 1)
        {
            possibleAttacks.Add(new Coords(coords.Row - 1, coords.Column - 2));
        }

        if (coords.Row < 7 && coords.Column > 1)
        {
            possibleAttacks.Add(new Coords(coords.Row + 1, coords.Column - 2));
        }

        // Right side.
        if (coords.Row > 0 && coords.Column < 6)
        {
            possibleAttacks.Add(new Coords(coords.Row - 1, coords.Column + 2));
        }

        if (coords.Row < 7 && coords.Column < 6)
        {
            possibleAttacks.Add(new Coords(coords.Row + 1, coords.Column + 2));
        }

        PossibleAttacks = possibleAttacks.ToArray();
    }
}
