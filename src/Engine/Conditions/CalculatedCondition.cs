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
        var whiteOnTurn = condition.WhiteOnTurn;
            
        // Coordinates of the king on turn.
        Coords kingCoords = new();
        
        PiecesOnTurn = new Dictionary<Coords, IPiece>();

        var key = condition.ToString();
        var data = ChessEngine.CalculatedConditionData.TryGetValue(key, out var calculatedConditionData);
        
        if (data)
        {
            // Creating friendly pieces and saving them into dictionary.
            for (var row = 0; row < 8; row++)
            {
                for (var column = 0; column < 8; column++)
                {
                    IPiece piece;
                    
                    var status = condition.Chessboard[row, column].Status;
                    var color = condition.Chessboard[row, column].PieceColor is PieceColor.White;

                    if (color != whiteOnTurn || status is Status.Empty or Status.EnPassant)
                    {
                        continue;
                    }
                    
                    // Getting type of piece and adding it into dictionary.
                    if (status is Status.King)
                    {
                        piece = new King(condition.Chessboard[row, column].PieceColor);
                        kingCoords = new Coords(row, column);
                    }
                    else
                    {
                        piece = status switch
                        {
                            Status.Pawn => new Pawn(color ? PieceColor.White : PieceColor.Black),
                            Status.Rook => new Rook(color ? PieceColor.White : PieceColor.Black),
                            Status.Knight => new Knight(color ? PieceColor.White : PieceColor.Black),
                            Status.Bishop => new Bishop(color ? PieceColor.White : PieceColor.Black),
                            Status.Queen => new Queen(color ? PieceColor.White : PieceColor.Black),
                            _ => throw new Exception("Unexpected status.")
                        };
                    }
                    
                    PiecesOnTurn.Add(new Coords(row, column), piece);
                }
            }
        }
        else
        {
            calculatedConditionData = new CalculatedConditionData();
            
            // Creating pieces, calculating moves and attacks, and saving into collections.
            for (sbyte row = 0; row < 8; row++)
            {
                for (sbyte column = 0; column < 8; column++)
                {
                    IPiece piece;

                    var status = condition.Chessboard[row, column].Status;
                    var color = condition.Chessboard[row, column].PieceColor is PieceColor.White;

                    switch (status)
                    {
                        // Getting type of piece and recalculating.
                        case Status.Empty or Status.EnPassant:
                            continue;
                        case Status.King:
                        {
                            piece = new King(color ? PieceColor.White : PieceColor.Black);
                        
                            if (piece.PieceColor is PieceColor.White == whiteOnTurn)
                            {
                                kingCoords = new Coords(row, column);
                            }

                            break;
                        }
                        default:
                        {
                            piece = status switch
                            {
                                Status.Pawn => new Pawn(color ? PieceColor.White : PieceColor.Black),
                                Status.Rook => new Rook(color ? PieceColor.White : PieceColor.Black),
                                Status.Knight => new Knight(color ? PieceColor.White : PieceColor.Black),
                                Status.Bishop => new Bishop(color ? PieceColor.White : PieceColor.Black),
                                Status.Queen => new Queen(color ? PieceColor.White : PieceColor.Black),
                                _ => throw new Exception("Unexpected status.")
                            };

                            break;
                        }
                    }
                    
                    // If the piece is on turn...
                    if (piece.PieceColor is PieceColor.White == whiteOnTurn)
                    {
                        PiecesOnTurn.Add(new Coords(row, column), piece);
                    }
                    // The piece is not on turn...
                    else
                    {
                        piece.UpdatePossibleAttacks(condition, new Coords(row, column));
                        
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
            for (sbyte column = 0; column < 8; column++)
            {
                var status = condition.Chessboard[row, column].Status;

                if (status is Status.Empty or Status.EnPassant)
                {
                    continue;
                }
                
                var color = condition.Chessboard[row, column].PieceColor is PieceColor.White;
                
                // Getting piece type and calculations.
                IPiece piece = status switch
                {
                    Status.Pawn => new Pawn(color ? PieceColor.White : PieceColor.Black),
                    Status.Rook => new Rook(color ? PieceColor.White : PieceColor.Black),
                    Status.Knight => new Knight(color ? PieceColor.White : PieceColor.Black),
                    Status.Bishop => new Bishop(color ? PieceColor.White : PieceColor.Black),
                    Status.Queen => new Queen(color ? PieceColor.White : PieceColor.Black),
                    Status.King => new King(color ? PieceColor.White : PieceColor.Black),
                    _ => throw new Exception("Unexpected status.")
                };

                if (color == condition.WhiteOnTurn)
                {
                    continue;
                }
                
                piece.UpdatePossibleAttacks(condition, new Coords(row, column));
                
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
