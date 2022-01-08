using Engine.Pieces;
using Engine.Pieces.Base;
using Engine.Pieces.Types;
using Engine.UtilityComponents;

namespace Engine.Conditions;

/// <summary>
/// Keeps the calculated data of condition and algorithms for condition calculating.
/// </summary>
internal class CalculatedCondition
{
    /// <summary>
    /// Pieces of calculated condition. Key is made from row and column.
    /// </summary>
    public Dictionary<Coords, IPiece> PiecesOnTurn { get; }
    
    /// <summary>
    /// Whether the white player is on turn.
    /// </summary>
    private bool WhiteOnTurn { get; }
    
    /// <summary>
    /// Is the king that is on turn in check?
    /// </summary>
    public bool Check { get; }
    
    /// <summary>
    /// Player on turn has no possible moves.
    /// </summary>
    public bool DrawMate { get; }
    
    /// <summary>
    /// Constructor creates pieces and calculates possible moves and attacks for them.
    /// Also checks whether a king is in check.
    /// </summary>
    public CalculatedCondition(Condition condition)
    {
        WhiteOnTurn = condition.WhiteOnTurn;
            
        // Coordinates of the king on turn.
        Coords kingCoords = new();
        
        PiecesOnTurn = new Dictionary<Coords, IPiece>();

        var key = condition.ToString();
        
        if (ChessEngine.CalculatedConditionData.TryGetValue(key, out var calculatedConditionData))
        {
            // Creating friendly pieces and saving them into dictionary.
            for (var row = 0; row < 8; row++)
            {
                for (var col = 0; col < 8; col++)
                {
                    IPiece piece;
                    
                    var status = condition.Chessboard[row, col].Status;
                    var white = condition.Chessboard[row, col].White;

                    if (white != WhiteOnTurn || status is Status.Empty or Status.EnPassant)
                    {
                        continue;
                    }
                    
                    // Getting type of piece and adding it into dictionary.
                    if (status is Status.King)
                    {
                        piece = new King(condition.Chessboard[row, col].White);
                        kingCoords = new Coords((sbyte)row, (sbyte)col);
                    }
                    else
                    {
                        piece = status switch
                        {
                            Status.Pawn => new Pawn(white),
                            Status.Rook => new Rook(white),
                            Status.Knight => new Knight(white),
                            Status.Bishop => new Bishop(white),
                            Status.Queen => new Queen(white),
                            _ => throw new Exception("Unexpected status.")
                        };
                    }
                    
                    PiecesOnTurn.Add(new Coords(row, col), piece);
                }
            }
        }
        else
        {
            calculatedConditionData = new CalculatedConditionData();
            
            // Creating pieces, calculating moves and attacks, and saving into collections.
            for (sbyte ra = 0; ra < 8; ra++)
            {
                for (sbyte sl = 0; sl < 8; sl++)
                {
                    IPiece piece;

                    var status = condition.Chessboard[ra, sl].Status;
                    var white = condition.Chessboard[ra, sl].White;

                    switch (status)
                    {
                        // Getting type of piece and recalculating.
                        case Status.Empty or Status.EnPassant:
                            continue;
                        case Status.King:
                        {
                            piece = new King(white);
                        
                            if (piece.White == WhiteOnTurn)
                            {
                                kingCoords = new Coords(ra, sl);
                            }

                            break;
                        }
                        default:
                        {
                            piece = status switch
                            {
                                Status.Pawn => new Pawn(white),
                                Status.Rook => new Rook(white),
                                Status.Knight => new Knight(white),
                                Status.Bishop => new Bishop(white),
                                Status.Queen => new Queen(white),
                                _ => throw new Exception("Unexpected status.")
                            };

                            break;
                        }
                    }
                    
                    // If the piece is on turn...
                    if (piece.White == WhiteOnTurn)
                    {
                        PiecesOnTurn.Add(new Coords(ra, sl), piece);
                    }
                    // The piece is not on turn...
                    else
                    {
                        piece.UpdatePossibleAttacks(condition, new Coords(ra, sl));
                        
                        foreach (Coords possibleAttackCoords in piece.PossibleAttacks)
                        {
                            calculatedConditionData.EnemyPossibleAttacks.Add(possibleAttackCoords);
                        }

                        if (piece is not MovablePiece movingPiece)
                        {
                            continue;
                        }
                        
                        // If the piece encounters enemy piece that has enemy king behind it...
                        if (movingPiece.PieceProtectingKingCoords.Row is not 8)
                        {
                            calculatedConditionData.KingProtectingPiecesCoords.Add(movingPiece
                                .PieceProtectingKingCoords);
                        }
                    }
                }
            }
            
            ChessEngine.CalculatedConditionData.TryAdd(key, calculatedConditionData);
        }

        // Whether the player on turn is in check.
        if (calculatedConditionData!.EnemyPossibleAttacks.Contains(kingCoords))
        {
            Check = true;
        }
        
        // Sets pieces whether they protect king from check.
        foreach (Coords coords in calculatedConditionData.KingProtectingPiecesCoords)
        {
            (PiecesOnTurn[coords] as Piece)!.ProtectingKing = true;
        }
            
        DrawMate = true;
            
        foreach (var (coords, value) in PiecesOnTurn)
        {
            if (value is King king)
            {
                // Updating possible moves to the king.
                king.PossibleEnemyAttacks = calculatedConditionData.EnemyPossibleAttacks;
            }
            
            // Calculating possible moves of pieces.
            value.UpdatePossibleMoves(condition, Check, coords);
            
            if (DrawMate && value.PossibleMoves.Length > 0)
            {
                DrawMate = false;
            }
        }
    }
    
    /// <summary>
    /// Whether the king on turn has in the current condition in check.
    /// </summary>
    /// <returns></returns>
    public static bool FindOutCheck(Condition condition) =>
        GetDataOfCalculatedSituation(condition)!.EnemyPossibleAttacks.Contains(condition.KingCoords);

    /// <summary>
    /// Returns all possible enemy attack.
    /// </summary>
    public static CalculatedConditionData? GetDataOfCalculatedSituation(Condition condition)
    {
        var key = condition.ToString();
        
        if (ChessEngine.CalculatedConditionData.TryGetValue(key, out var calculatedSituationData))
        {
            return calculatedSituationData;
        }

        calculatedSituationData = new CalculatedConditionData();

        // Creating enemy pieces and calculating possible moves.
        for (sbyte row = 0; row < 8; row++)
        {
            for (sbyte col = 0; col < 8; col++)
            {
                var status = condition.Chessboard[row, col].Status;
                var white = condition.Chessboard[row, col].White;

                if (status is Status.Empty or Status.EnPassant)
                {
                    continue;
                }
                
                // Getting piece type and calculations.
                IPiece piece = status switch
                {
                    Status.Pawn => new Pawn(white),
                    Status.Rook => new Rook(white),
                    Status.Knight => new Knight(white),
                    Status.Bishop => new Bishop(white),
                    Status.Queen => new Queen(white),
                    Status.King => new King(white),
                    _ => throw new Exception("Unexpected status.")
                };

                if (white == condition.WhiteOnTurn)
                {
                    continue;
                }
                
                piece.UpdatePossibleAttacks(condition, new Coords(row, col));
                
                foreach (Coords possibleAttackCoords in piece.PossibleAttacks)
                {
                    calculatedSituationData.EnemyPossibleAttacks.Add(possibleAttackCoords);
                }

                if (piece is not MovablePiece movingPiece)
                {
                    continue;
                }
                
                // If the piece encounters enemy piece that has enemy king behind it...
                if (movingPiece.PieceProtectingKingCoords.Row is not 8)
                {
                    calculatedSituationData.KingProtectingPiecesCoords.Add(movingPiece.PieceProtectingKingCoords);
                }
            }
        }

        ChessEngine.CalculatedConditionData.TryAdd(key, calculatedSituationData);
        return calculatedSituationData;
    }
}
