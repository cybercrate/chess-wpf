using Engine.Pieces.Types;
using System.Windows.Media.Imaging;

namespace Engine.ResourceManagement;

public static class ImageLoader
{
    private const string RelativePath = $"{ResourcePath.ResourcesRelativePath}/Chessmen";
    
    private const string PawnWhite = "PawnWhite.png";
    private const string PawnBlack = "PawnBlack.png";
    private const string KnightWhite = "KnightWhite.png";
    private const string KnightBlack = "KnightBlack.png";
    private const string BishopWhite = "BishopWhite.png";
    private const string BishopBlack = "BishopBlack.png";
    private const string RookWhite = "RookWhite.png";
    private const string RookBlack = "RookBlack.png";
    private const string QueenWhite = "QueenWhite.png";
    private const string QueenBlack = "QueenBlack.png";
    private const string KingWhite = "KingWhite.png";
    private const string KingBlack = "KingBlack.png";
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static BitmapImage GetImage(Chessman type) => new(new Uri(GeneratePath(type), UriKind.Relative));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static BitmapImage GetImage(Status status, PieceColor color) =>
        new(new Uri(GeneratePath(status, color), UriKind.Relative));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static string GeneratePath(Chessman type)
    {
        var imageSource = type switch
        {
            Chessman.PawnWhite => PawnWhite,
            Chessman.PawnBlack => PawnBlack,
            Chessman.KnightWhite => KnightWhite,
            Chessman.KnightBlack => KnightBlack,
            Chessman.BishopWhite => BishopWhite,
            Chessman.BishopBlack => BishopBlack,
            Chessman.RookWhite => RookWhite,
            Chessman.RookBlack => RookBlack,
            Chessman.QueenWhite => QueenWhite,
            Chessman.QueenBlack => QueenBlack,
            Chessman.KingWhite => KingWhite,
            Chessman.KingBlack => KingBlack,
            _ => throw new Exception()
        };
        
        return $"/{RelativePath}/{imageSource}";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static string GeneratePath(Status status, PieceColor color)
    {
        var imageSource = status switch
        {
            Status.Pawn or Status.EnPassant => color is PieceColor.White ? PawnWhite : PawnBlack,
            Status.Rook => color is PieceColor.White ? RookWhite : RookBlack,
            Status.Knight => color is PieceColor.White ? KnightWhite : KnightBlack,
            Status.Bishop => color is PieceColor.White ? BishopWhite : BishopBlack,
            Status.Queen => color is PieceColor.White ? QueenWhite : QueenBlack,
            Status.King => color is PieceColor.White ? KingWhite : KingBlack,
            _ => throw new Exception()
        };

        return $"/{RelativePath}/{imageSource}";
    }
}
