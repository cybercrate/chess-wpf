using Engine.Conditions;
using Engine.Pieces.Types;
using Engine.UtilityComponents;

namespace Engine.AI;

internal static class Emulator
{
    private static readonly Random Random = new();
    private static readonly Dictionary<Status, int> PiecesValues = new();

    /// <summary>
    /// If this field is set to true, the multithreading calculation is stopped asap.
    /// </summary>
    public static bool InterruptHalfTurn;

    static Emulator()
    {
        PiecesValues.Add(Status.Pawn, 1);
        PiecesValues.Add(Status.EnPassant, 1);
        PiecesValues.Add(Status.Knight, 3);
        PiecesValues.Add(Status.Bishop, 3);
        PiecesValues.Add(Status.Rook, 5);
        PiecesValues.Add(Status.Queen, 9);
    }
    
    /// <summary>
    /// Selects halfTurn from calculated condition. Calculation is done in parallel.
    /// </summary>
    public static HalfTurn? BestHalfTurn(CalculatedCondition calcCondition, Condition condition, int depth)
    {
        InterruptHalfTurn = false;
        var possibleHalfTurns = PossibleHalfTurns(calcCondition);

        if (possibleHalfTurns.Count is 0)
        {
            return null;
        }

        ChessEngine.ProgressMaximumStatic = possibleHalfTurns.Count;

        Parallel.For(0, possibleHalfTurns.Count, i =>
        {
            BasicEvaluating(condition, possibleHalfTurns[i]);

            Condition newCondition = new(condition);
            ChessEngine.MovePiece(possibleHalfTurns[i].From, possibleHalfTurns[i].To, newCondition);
            CalculatedCondition newCalculatedCondition = new(newCondition);

            possibleHalfTurns[i].Value -= Minimax(newCalculatedCondition, newCondition, depth - 1);
            Interlocked.Increment(ref ChessEngine.ProgressValueStatic);
        });

        return InterruptHalfTurn ? null : BestHalfTurn(possibleHalfTurns);
    }

    /// <summary>
    /// Finding the best moves and selecting one random of them.
    /// </summary>
    private static HalfTurn BestHalfTurn(IReadOnlyList<HalfTurn> possibleMoves)
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

    /// <summary>
    /// Loads all possible moves of pieces.
    /// </summary>
    private static List<HalfTurn> PossibleHalfTurns(CalculatedCondition calculatedCondition)
    {
        var result = (from piece in calculatedCondition.PiecesOnTurn
            from coords in piece.Value.PossibleMoves
            select new HalfTurn(piece.Key, coords)).ToList();

        return result;
    }

    /// <summary>
    /// Algorithm for finding the best move.
    /// </summary>
    /// <returns>Returns value of the best move.</returns>
    private static int Minimax(CalculatedCondition calcCondition, Condition condition, int depth)
    {
        if (InterruptHalfTurn)
        {
            return int.MaxValue;
        }

        var possibleHalfTurns = PossibleHalfTurns(calcCondition);
        var max = int.MinValue;

        if (depth is 0)
        {
            foreach (HalfTurn halfTurn in possibleHalfTurns)
            {
                BasicEvaluating(condition, halfTurn);
                
                var possibleAttacks = CalculatedCondition
                    .GetDataOfCalculatedSituation(condition)!
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
            BasicEvaluating(condition, halfTurn);

            Condition newCondition = new(condition);
            ChessEngine.MovePiece(halfTurn.From, halfTurn.To, newCondition);
            CalculatedCondition calculatedCondition = new(newCondition);

            if (calcCondition.DrawMate)
            {
                return calcCondition.Check ? 500 : 0;
            }

            halfTurn.Value -= Minimax(calculatedCondition, newCondition, depth - 1);

            if (halfTurn.Value > max)
            {
                max = halfTurn.Value;
            }
        }

        return max;
    }

    private static void BasicEvaluating(Condition condition, HalfTurn halfTurn)
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
