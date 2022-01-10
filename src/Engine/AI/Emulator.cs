using Engine.Conditions;
using Engine.Pieces.Types;
using Engine.UtilityComponents;

namespace Engine.AI;

internal static class Emulator
{
    private static readonly Random Random = new();

    private static readonly Dictionary<Status, int> PiecesValues = new()
    {
        { Status.Pawn, 1 },
        { Status.EnPassant, 1 },
        { Status.Knight, 3 },
        { Status.Bishop, 3 },
        { Status.Rook, 5 },
        { Status.Queen, 9 }
    };

    /// <summary>
    /// Multithreading calculation identifier.
    /// If set true calculation stopped as soon as possible.
    /// </summary>
    public static bool InterruptHalfTurn;

    /// <summary>
    /// Selects half turn from calculated condition. Calculation is done in parallel.
    /// </summary>
    public static HalfTurn? FindBestHalfTurn(CalculatedCondition calculatedCondition, Condition condition, int depth)
    {
        InterruptHalfTurn = false;
        var possibleHalfTurns = GetPossibleHalfTurns(calculatedCondition);

        if (possibleHalfTurns.Count is 0)
        {
            return null;
        }

        ChessEngine.ProgressMaximumStatic = possibleHalfTurns.Count;

        Parallel.For(0, possibleHalfTurns.Count, i =>
        {
            Evaluate(condition, possibleHalfTurns[i]);

            Condition newCondition = new(condition);
            ChessEngine.MovePiece(possibleHalfTurns[i].From, possibleHalfTurns[i].To, newCondition);
            CalculatedCondition newCalculatedCondition = new(newCondition);

            possibleHalfTurns[i].Value -= GetMinimax(newCalculatedCondition, newCondition, depth - 1);
            Interlocked.Increment(ref ChessEngine.ProgressValueStatic);
        });

        return InterruptHalfTurn ? null : FindBestHalfTurn(possibleHalfTurns);
    }

    // Finds the best moves and selecting one random of them.
    private static HalfTurn FindBestHalfTurn(IReadOnlyList<HalfTurn> possibleMoves)
    {
        List<HalfTurn> bestMoves = new() {possibleMoves[0]};

        for (var i = 1; i < possibleMoves.Count; i++)
        {
            if (possibleMoves[i].Value > bestMoves[0].Value)
            {
                bestMoves = new List<HalfTurn> { possibleMoves[i] };
            }
            else if (possibleMoves[i].Value == bestMoves[0].Value)
            {
                bestMoves.Add(possibleMoves[i]);
            }
        }

        return bestMoves[Random.Next(bestMoves.Count)];
    }
    
    // Loads all possible moves of pieces.
    private static List<HalfTurn> GetPossibleHalfTurns(CalculatedCondition calculatedCondition)
    {
        var result = (from piece in calculatedCondition.PiecesOnTurn
            from coords in piece.Value.PossibleMoves
            select new HalfTurn(piece.Key, coords)).ToList();

        return result;
    }
    
    // Finds the best move.
    private static int GetMinimax(CalculatedCondition calculatedCondition, Condition condition, int depth)
    {
        if (InterruptHalfTurn)
        {
            return int.MaxValue;
        }

        var possibleHalfTurns = GetPossibleHalfTurns(calculatedCondition);
        var max = int.MinValue;

        if (depth is 0)
        {
            foreach (HalfTurn halfTurn in possibleHalfTurns)
            {
                Evaluate(condition, halfTurn);
                
                var possibleAttacks = CalculatedCondition.GetDataOfCalculatedSituation(condition)!
                    .EnemyPossibleAttacks.Contains(halfTurn.To);

                if (possibleAttacks)
                {
                    var pieceValue = PiecesValues[condition.Chessboard[halfTurn.From.Row, halfTurn.From.Column].Status];
                    halfTurn.Value -= pieceValue;
                }

                if (max < halfTurn.Value)
                {
                    max = halfTurn.Value;
                }
            }

            return max;
        }

        foreach (HalfTurn halfTurn in possibleHalfTurns)
        {
            Evaluate(condition, halfTurn);

            Condition newCondition = new(condition);
            ChessEngine.MovePiece(halfTurn.From, halfTurn.To, newCondition);
            CalculatedCondition currentCalculatedCondition = new(newCondition);

            if (calculatedCondition.DrawMate)
            {
                return calculatedCondition.Check ? 500 : 0;
            }

            halfTurn.Value -= GetMinimax(currentCalculatedCondition, newCondition, depth - 1);

            if (halfTurn.Value > max)
            {
                max = halfTurn.Value;
            }
        }

        return max;
    }

    private static void Evaluate(Condition condition, HalfTurn halfTurn)
    {
        var toStatus = condition.Chessboard[halfTurn.To.Row, halfTurn.To.Column].Status;
        var fromStatus = condition.Chessboard[halfTurn.From.Row, halfTurn.From.Column].Status;

        // Evaluating taking enemy piece.
        if (toStatus is not Status.Empty)
        {
            // Evaluating en passant (only pawn can take pawn en passant).
            if (toStatus is not Status.EnPassant && fromStatus is Status.Pawn)
            {
                halfTurn.Value += PiecesValues[toStatus];
            }
        }

        if (fromStatus is not Status.Pawn)
        {
            return;
        }
        
        // Evaluating pawn at the ends of row.
        if (halfTurn.To.Row is 0 or 7)
        {
            halfTurn.Value += 4;
        }
    }
}
