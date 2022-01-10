using Engine.IO;
using Engine.Pieces.Types;
using Engine.UtilityComponents;
using System.Text;

namespace Engine.Conditions;

/// <summary>
/// Chess condition storage.
/// Contains only raw data for upcoming analysis.
/// </summary>
public class Condition
{
    /// <summary>
    /// Logical chessboard.
    /// </summary>
    public PieceId[,] Chessboard { get; }
    /// <summary>
    /// Specifies whether white player is on turn.
    /// </summary>
    public bool WhiteOnTurn { get; set; }
    /// <summary>
    /// How many turns were executed without taking a piece or moving a pawn.
    /// If 50 of such turns have been played, draw condition occurs.
    /// </summary>
    public int Draw50 { get; set; }
    /// <summary>
    /// Returns coordinates of king that is on turn.
    /// </summary>
    public Coords KingCoords
    {
        get
        {
            for (var row = 0; row < 8; row++)
            {
                for (var column = 0; column < 8; column++)
                {
                    var status = Chessboard[row, column].Status;
                    var color = Chessboard[row, column].PieceColor;
                    
                    if (status is Status.King && color is PieceColor.White == WhiteOnTurn)
                    {
                        return new Coords(row, column);
                    }
                }
            }
                
            throw new Exception("King not found!");
        }
    }
        
    /// <summary>
    /// Indicates whether at least once it was moved with white king (for castling).
    /// </summary>
    public bool WhiteKingMoved { get; set; }
    /// <summary>
    /// Indicates whether at least once it was moved with white right rook (for castling).
    /// </summary>
    public bool WhiteSmallRookMoved { get; set; }
    /// <summary>
    /// Indicates whether at least once it was moved with white left rook (for castling).
    /// </summary>
    public bool WhiteLargeRookMoved { get; set; }
    /// <summary>
    /// Indicates whether at least once it was moved with black king (for castling).
    /// </summary>
    public bool BlackKingMoved { get; set; }
    /// <summary>
    /// Indicates whether at least once it was moved with black right rook (for castling).
    /// </summary>
    public bool BlackSmallRookMoved { get; set; }
    /// <summary>
    /// Indicates whether at least once it was moved with black left rook (for castling).
    /// </summary>
    public bool BlackLargeRookMoved { get; set; }

    /// <summary>
    /// Default constructor sets up pieces into default position and sets the turn to the white player.
    /// </summary>
    public Condition()
    {
        Chessboard = new PieceId[8, 8];
        WhiteOnTurn = true;
        Draw50 = 0;
            
        // Creating pieces in default positions.
        for (var row = 0; row < 8; row++)
        {
            for (var column = 0; column < 8; column++)
            {
                Chessboard[row, column] = row switch
                {
                    1 => new PieceId(Status.Pawn, PieceColor.Black),
                    6 => new PieceId(Status.Pawn, PieceColor.White),
                    0 when column is 0 or 7 => new PieceId(Status.Rook, PieceColor.Black),
                    7 when column is 0 or 7 => new PieceId(Status.Rook, PieceColor.White),
                    0 when column is 6 or 1 => new PieceId(Status.Knight, PieceColor.Black),
                    7 when column is 6 or 1 => new PieceId(Status.Knight, PieceColor.White),
                    0 when column is 5 or 2 => new PieceId(Status.Bishop, PieceColor.Black),
                    7 when column is 5 or 2 => new PieceId(Status.Bishop, PieceColor.White),
                    0 when column is 3 => new PieceId(Status.Queen, PieceColor.Black),
                    7 when column is 3 => new PieceId(Status.Queen, PieceColor.White),
                    0 when column is 4 => new PieceId(Status.King, PieceColor.Black),
                    7 when column is 4 => new PieceId(Status.King, PieceColor.White),
                    _ => new PieceId(Status.Empty, PieceColor.None)
                };
            }
        }
    }
        
    /// <summary>
    /// This constructor creates a copy of given condition.
    /// </summary>
    public Condition(Condition condition)
    {
        Chessboard = (PieceId[,])condition.Chessboard.Clone();
        WhiteOnTurn = condition.WhiteOnTurn;
        Draw50 = condition.Draw50;
        WhiteKingMoved = condition.WhiteKingMoved;
        WhiteSmallRookMoved = condition.WhiteSmallRookMoved;
        WhiteLargeRookMoved = condition.WhiteLargeRookMoved;
        BlackKingMoved = condition.BlackKingMoved;
        BlackSmallRookMoved = condition.BlackSmallRookMoved;
        BlackLargeRookMoved = condition.BlackLargeRookMoved;
    }

    /// <summary>
    /// Returns unique string of the condition.
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new();
            
        for (var row = 0; row < 8; row++)
        {
            for (var column = 0; column < 8; column++)
            {
                if (Chessboard[row, column].Status is Status.Empty or Status.EnPassant)
                {
                    continue;
                }
                    
                sb.Append(row);
                sb.Append(column);
                sb.Append(Chessboard[row, column].PieceColor is PieceColor.White ? Token.White : Token.Black);
                sb.Append(Chessboard[row, column].Status);
            }
        }

        var flags = new[]
        {
            WhiteOnTurn ? Token.True : Token.False,
            WhiteKingMoved ? Token.True : Token.False,
            WhiteSmallRookMoved ? Token.True : Token.False,
            WhiteLargeRookMoved ? Token.True : Token.False,
            BlackKingMoved ? Token.True : Token.False,
            BlackSmallRookMoved ? Token.True : Token.False,
            BlackLargeRookMoved ? Token.True : Token.False
        };

        sb.Append(new string(flags));

        return sb.ToString();
    }
}
