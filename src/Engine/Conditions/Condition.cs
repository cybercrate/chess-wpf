using Engine.Pieces.Base;
using System.Text;
using Engine.UtilityComponents;

namespace Engine.Conditions;

/// <summary>
/// Stores chess condition. Contains only raw data (needed for upcoming analysis).
/// </summary>
public class Condition
{
    /// <summary>
    /// Logical chessboard.
    /// </summary>
    public PieceChar[,] Chessboard { get; }
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
                for (var col = 0; col < 8; col++)
                {
                    if (Chessboard[row, col].Status is 'k' && Chessboard[row, col].White == WhiteOnTurn)
                    {
                        return new Coords((sbyte)row, (sbyte)col);
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
        Chessboard = new PieceChar[8, 8];
        WhiteOnTurn = true;
        Draw50 = 0;
            
        // Creating pieces in default positions.
        for (var row = 0; row < 8; row++)
        {
            for (var column = 0; column < 8; column++)
            {
                Chessboard[row, column] = row switch
                {
                    1 => new PieceChar('p', false),
                    6 => new PieceChar('p'),
                    0 when column is 0 or 7 => new PieceChar('v', false),
                    7 when column is 0 or 7 => new PieceChar('v'),
                    0 when column is 6 or 1 => new PieceChar('j', false),
                    7 when column is 6 or 1 => new PieceChar('j'),
                    0 when column is 5 or 2 => new PieceChar('s', false),
                    7 when column is 5 or 2 => new PieceChar('s'),
                    0 when column is 3 => new PieceChar('d', false),
                    7 when column is 3 => new PieceChar('d'),
                    0 when column is 4 => new PieceChar('k', false),
                    7 when column is 4 => new PieceChar('k'),
                    _ => new PieceChar('n')
                };
            }
        }
    }
        
    /// <summary>
    /// This constructor creates a copy of given condition.
    /// </summary>
    public Condition(Condition condition)
    {
        Chessboard = (PieceChar[,])condition.Chessboard.Clone();
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
            for (var col = 0; col < 8; col++)
            {
                if (Chessboard[row, col].Status is 'n' or 'x')
                {
                    continue;
                }
                    
                sb.Append(row);
                sb.Append(col);
                sb.Append(Chessboard[row, col].White ? 'b' : 'c');
                sb.Append(Chessboard[row, col].Status);
            }
        }
            
        sb.Append(WhiteOnTurn.ToString()[0]);
        sb.Append(WhiteKingMoved.ToString()[0]);
        sb.Append(WhiteSmallRookMoved.ToString()[0]);
        sb.Append(WhiteLargeRookMoved.ToString()[0]);
        sb.Append(BlackKingMoved.ToString()[0]);
        sb.Append(BlackSmallRookMoved.ToString()[0]);
        sb.Append(BlackLargeRookMoved.ToString()[0]);
            
        return sb.ToString();
    }
}
