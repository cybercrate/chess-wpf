using Engine.Conditions;
using Engine.Pieces.Base;
using Engine.Pieces.Types;
using Engine.UtilityComponents;

namespace Engine.Pieces;

internal sealed class King : Piece
{
    /// <summary>
    /// The king needs to know all possible enemy attack to know where he can move.
    /// </summary>
    public HashSet<Coords> PossibleEnemyAttacks { get; set; } = null!;

    public King(PieceColor color) : base(color)
    {
    }

    /// <summary>
    /// Before updating possible moves, possible enemy attacks need to be updated first.
    /// </summary>
    public override void UpdatePossibleMoves(Condition condition, bool check, Coords coords)
    {
        List<Coords> possibleMoves = new(8);

        if (condition.Draw50 > 99)
        {
            PossibleMoves = possibleMoves.ToArray();
            return;
        }

        // Creating currently processed coords.
        Coords processedCoords = new(coords.Row, coords.Column - 1);

        Status status;
        PieceColor color;
        bool possibleEnemyAttack;

        // Left.
        if (coords.Column > 0)
        {
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);
                
                if (possibleEnemyAttack is false)
                {
                    possibleMoves.Add(processedCoords);
                }
            }
        }

        // Left up.
        processedCoords = new Coords(coords.Row - 1, coords.Column - 1);

        if (coords.Column > 0 && coords.Row > 0)
        {
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);
                
                if (possibleEnemyAttack is false)
                {
                    possibleMoves.Add(processedCoords);
                }
            }
        }

        // Up.
        processedCoords = new Coords(coords.Row - 1, coords.Column);

        if (coords.Row > 0)
        {
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);
                
                if (possibleEnemyAttack is false)
                {
                    possibleMoves.Add(processedCoords);
                }
            }
        }

        // Right up.
        processedCoords = new Coords(coords.Row - 1, coords.Column + 1);

        if (coords.Column < 7 && coords.Row > 0)
        {
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);
                
                if (possibleEnemyAttack is false)
                {
                    possibleMoves.Add(processedCoords);
                }
            }
        }

        // Right.
        processedCoords = new Coords(coords.Row, coords.Column + 1);

        if (coords.Column < 7)
        {
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);
                
                if (possibleEnemyAttack is false)
                {
                    possibleMoves.Add(processedCoords);
                }
            }
        }

        // Right down.
        processedCoords = new Coords(coords.Row + 1, coords.Column + 1);

        if (coords.Column < 7 && coords.Row < 7)
        {
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);
                
                if (possibleEnemyAttack is false)
                {
                    possibleMoves.Add(processedCoords);
                }
            }
        }

        // Down.
        processedCoords = new Coords(coords.Row + 1, coords.Column);

        if (coords.Row < 7)
        {
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);
                
                if (possibleEnemyAttack is false)
                {
                    possibleMoves.Add(processedCoords);
                }
            }
        }

        // Left down.
        processedCoords = new Coords(coords.Row + 1, coords.Column - 1);

        if (coords.Column > 0 && coords.Row < 7)
        {
            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
            color = condition.Chessboard[processedCoords.Row, processedCoords.Column].PieceColor;
            
            if (status is Status.Empty or Status.EnPassant || color != PieceColor)
            {
                possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);
                
                if (possibleEnemyAttack is false)
                {
                    possibleMoves.Add(processedCoords);
                }
            }
        }

        // Castling is possible only of the king is not in check.
        if (check is false)
        {
            if (PieceColor is PieceColor.White)
            {
                if (condition.WhiteKingMoved is false)
                {
                    // Small castling (right).
                    if (condition.WhiteSmallRookMoved is false)
                    {
                        status = condition.Chessboard[coords.Row, coords.Column + 1].Status;
                        possibleEnemyAttack = PossibleEnemyAttacks.Contains(new Coords(coords.Row, coords.Column + 1));

                        // Is Col + 1 square free and safe?
                        if (status is Status.Empty or Status.EnPassant && possibleEnemyAttack is false)
                        {
                            processedCoords = new Coords(coords.Row, coords.Column + 2);

                            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
                            possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);

                            // Is Col + 2 square free and safe?
                            if (status is Status.Empty or Status.EnPassant && possibleEnemyAttack is false)
                            {
                                possibleMoves.Add(processedCoords);
                            }
                        }
                    }

                    // Large castling (left).
                    if (condition.WhiteLargeRookMoved is false)
                    {
                        status = condition.Chessboard[coords.Row, coords.Column - 1].Status;
                        possibleEnemyAttack = PossibleEnemyAttacks.Contains(new Coords(coords.Row, coords.Column - 1));
                        
                        // Is Col - 1 square free and safe?
                        if (status is Status.Empty or Status.EnPassant && possibleEnemyAttack is false)
                        {
                            processedCoords = new Coords(coords.Row, coords.Column - 2);

                            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
                            possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);

                            // Is Col - 2 square free and safe?
                            if (status is Status.Empty or Status.EnPassant && possibleEnemyAttack is false)
                            {
                                status = condition.Chessboard[coords.Row, coords.Column - 3].Status;
                                
                                // Is Col - 3 square free?
                                if (status is Status.Empty or Status.EnPassant)
                                {
                                    possibleMoves.Add(processedCoords);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (condition.BlackKingMoved is false)
                {
                    // Small castling (right).
                    if (condition.BlackSmallRookMoved is false)
                    {
                        status = condition.Chessboard[coords.Row, coords.Column + 1].Status;
                        possibleEnemyAttack = PossibleEnemyAttacks.Contains(new Coords(coords.Row, coords.Column + 1));
                        
                        // Is Col +1 square free and safe?
                        if (status is Status.Empty or Status.EnPassant && possibleEnemyAttack is false)
                        {
                            processedCoords = new Coords(coords.Row, coords.Column + 2);

                            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
                            possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);

                            // Is Col + 2 square free and safe?
                            if (status is Status.Empty or Status.EnPassant && possibleEnemyAttack is false)
                            {
                                possibleMoves.Add(processedCoords);
                            }
                        }
                    }

                    // Large castling (left).
                    if (condition.BlackLargeRookMoved is false)
                    {
                        status = condition.Chessboard[coords.Row, coords.Column - 1].Status;
                        possibleEnemyAttack = PossibleEnemyAttacks.Contains(new Coords(coords.Row, coords.Column - 1));
                        
                        // Is Col - 1 square free and safe?
                        if (status is Status.Empty or Status.EnPassant && possibleEnemyAttack is false)
                        {
                            processedCoords = new Coords(coords.Row, coords.Column - 2);

                            status = condition.Chessboard[processedCoords.Row, processedCoords.Column].Status;
                            possibleEnemyAttack = PossibleEnemyAttacks.Contains(processedCoords);

                            // Is Col - 2 square free and safe?
                            if (status is Status.Empty or Status.EnPassant && possibleEnemyAttack is false)
                            {
                                status = condition.Chessboard[coords.Row, coords.Column - 3].Status;
                                
                                // Is Col - 3 square free?
                                if (status is Status.Empty or Status.EnPassant)
                                {
                                    possibleMoves.Add(processedCoords);
                                }
                            }
                        }
                    }
                }
            }
        }
        // If the king is in check...
        else
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

    /// <summary>
    /// It is necessary to first update possible attacks, then to update possible moves.
    /// </summary>
    public override void UpdatePossibleAttacks(Condition condition, Coords coords)
    {
        List<Coords> possibleAttacks = new(8);

        // Left.
        if (coords.Column > 0)
        {
            possibleAttacks.Add(new Coords(coords.Row, coords.Column - 1));
        }

        // Left up.
        if (coords.Column > 0 && coords.Row > 0)
        {
            possibleAttacks.Add(new Coords(coords.Row - 1, coords.Column - 1));
        }

        // Up.
        if (coords.Row > 0)
        {
            possibleAttacks.Add(new Coords(coords.Row - 1, coords.Column));
        }

        // Right up.
        if (coords.Column < 7 && coords.Row > 0)
        {
            possibleAttacks.Add(new Coords(coords.Row - 1, coords.Column + 1));
        }

        // Right.
        if (coords.Column < 7)
        {
            possibleAttacks.Add(new Coords(coords.Row, coords.Column + 1));
        }

        // Right down.
        if (coords.Column < 7 && coords.Row < 7)
        {
            possibleAttacks.Add(new Coords(coords.Row + 1, coords.Column + 1));
        }

        // Down.
        if (coords.Row < 7)
        {
            possibleAttacks.Add(new Coords(coords.Row + 1, coords.Column));
        }

        // Left down.
        if (coords.Column > 0 && coords.Row < 7)
        {
            possibleAttacks.Add(new Coords(coords.Row + 1, coords.Column - 1));
        }

        PossibleAttacks = possibleAttacks.ToArray();
    }
}
