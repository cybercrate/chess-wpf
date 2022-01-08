using Engine.Conditions;
using Engine.Pieces.Base;
using Engine.Pieces.Types;
using Engine.UtilityComponents;

namespace Engine.Pieces;

internal class King : Piece
{
    /// <summary>
    /// The king needs to know all possible enemy attack to know where he can move.
    /// </summary>
    public HashSet<Coords> PossibleEnemyAttacks { get; set; } = null!;

    public King(bool white) : base(white)
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
        Coords pc = new(coords.Row, coords.Column - 1);

        // Left.
        if (coords.Column > 0)
        {
            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant ||
                condition.Chessboard[pc.Row, pc.Column].White != White)
            {
                if (PossibleEnemyAttacks.Contains(pc) is false)
                {
                    possibleMoves.Add(pc);
                }
            }
        }

        // Left up.
        pc = new Coords(coords.Row - 1, coords.Column - 1);

        if (coords.Column > 0 && coords.Row > 0)
        {
            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant ||
                condition.Chessboard[pc.Row, pc.Column].White != White)
            {
                if (PossibleEnemyAttacks.Contains(pc) is false)
                {
                    possibleMoves.Add(pc);
                }
            }
        }

        // Up.
        pc = new Coords(coords.Row - 1, coords.Column);

        if (coords.Row > 0)
        {
            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant ||
                condition.Chessboard[pc.Row, pc.Column].White != White)
            {
                if (PossibleEnemyAttacks.Contains(pc) is false)
                {
                    possibleMoves.Add(pc);
                }
            }
        }

        // Right up.
        pc = new Coords(coords.Row - 1, coords.Column + 1);

        if (coords.Column < 7 && coords.Row > 0)
        {
            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant ||
                condition.Chessboard[pc.Row, pc.Column].White != White)
            {
                if (PossibleEnemyAttacks.Contains(pc) is false)
                {
                    possibleMoves.Add(pc);
                }
            }
        }

        // Right.
        pc = new Coords(coords.Row, coords.Column + 1);

        if (coords.Column < 7)
        {
            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant ||
                condition.Chessboard[pc.Row, pc.Column].White != White)
            {
                if (PossibleEnemyAttacks.Contains(pc) is false)
                {
                    possibleMoves.Add(pc);
                }
            }
        }

        // Right down.
        pc = new Coords(coords.Row + 1, coords.Column + 1);

        if (coords.Column < 7 && coords.Row < 7)
        {
            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant ||
                condition.Chessboard[pc.Row, pc.Column].White != White)
            {
                if (PossibleEnemyAttacks.Contains(pc) is false)
                {
                    possibleMoves.Add(pc);
                }
            }
        }

        // Down.
        pc = new Coords(coords.Row + 1, coords.Column);

        if (coords.Row < 7)
        {
            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant ||
                condition.Chessboard[pc.Row, pc.Column].White != White)
            {
                if (PossibleEnemyAttacks.Contains(pc) is false)
                {
                    possibleMoves.Add(pc);
                }
            }
        }

        // Left down.
        pc = new Coords(coords.Row + 1, coords.Column - 1);

        if (coords.Column > 0 && coords.Row < 7)
        {
            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant ||
                condition.Chessboard[pc.Row, pc.Column].White != White)
            {
                if (PossibleEnemyAttacks.Contains(pc) is false)
                {
                    possibleMoves.Add(pc);
                }
            }
        }

        // Castling is possible only of the king is not in check.
        if (check is false)
        {
            if (White)
            {
                if (condition.WhiteKingMoved is false)
                {
                    // Small castling (right).
                    if (condition.WhiteSmallRookMoved is false)
                    {
                        // Is Col + 1 square free and safe?
                        if (condition.Chessboard[coords.Row, coords.Column + 1].Status is Status.Empty
                                or Status.EnPassant &&
                            PossibleEnemyAttacks.Contains(new Coords(coords.Row, coords.Column + 1)) is false)
                        {
                            pc = new Coords(coords.Row, coords.Column + 2);

                            // Is Col + 2 square free and safe?
                            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant &&
                                PossibleEnemyAttacks.Contains(pc) is false)
                            {
                                possibleMoves.Add(pc);
                            }
                        }
                    }

                    // Large castling (left).
                    if (condition.WhiteLargeRookMoved is false)
                    {
                        // Is Col - 1 square free and safe?
                        if (condition.Chessboard[coords.Row, coords.Column - 1].Status is Status.Empty
                                or Status.EnPassant &&
                            PossibleEnemyAttacks.Contains(new Coords(coords.Row, coords.Column - 1)) is false)
                        {
                            pc = new Coords(coords.Row, coords.Column - 2);

                            // Is Col - 2 square free and safe?
                            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant &&
                                PossibleEnemyAttacks.Contains(pc) is false)
                            {
                                // Is Col - 3 square free?
                                if (condition.Chessboard[coords.Row, coords.Column - 3].Status is Status.Empty
                                    or Status.EnPassant)
                                {
                                    possibleMoves.Add(pc);
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
                        // Is Col +1 square free and safe?
                        if (condition.Chessboard[coords.Row, coords.Column + 1].Status is Status.Empty
                                or Status.EnPassant &&
                            PossibleEnemyAttacks.Contains(new Coords(coords.Row, coords.Column + 1)) is false)
                        {
                            pc = new Coords(coords.Row, coords.Column + 2);

                            // Is Col + 2 square free and safe?
                            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant &&
                                PossibleEnemyAttacks.Contains(pc) is false)
                            {
                                possibleMoves.Add(pc);
                            }
                        }
                    }

                    // Large castling (left).
                    if (condition.BlackLargeRookMoved is false)
                    {
                        // Is Col - 1 square free and safe?
                        if (condition.Chessboard[coords.Row, coords.Column - 1].Status is Status.Empty
                                or Status.EnPassant &&
                            PossibleEnemyAttacks.Contains(new Coords(coords.Row, coords.Column - 1)) is false)
                        {
                            pc = new Coords(coords.Row, coords.Column - 2);

                            // Is Col - 2 square free and safe?
                            if (condition.Chessboard[pc.Row, pc.Column].Status is Status.Empty or Status.EnPassant &&
                                PossibleEnemyAttacks.Contains(pc) is false)
                            {
                                // Is Col - 3 square free?
                                if (condition.Chessboard[coords.Row, coords.Column - 3].Status is Status.Empty
                                    or Status.EnPassant)
                                {
                                    possibleMoves.Add(pc);
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
                if (ChessEngine.ValidMoveDuringCheck(coords, possibleMoves[i], condition) is false)
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
