using Engine.Pieces.Base;
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
    /// <exception cref="Exception"></exception>
    private static string GeneratePath(ChessmanType type)
    {
        var imageSource = type switch
        {
            ChessmanType.PawnWhite => PawnWhite,
            ChessmanType.PawnBlack => PawnBlack,
            ChessmanType.KnightWhite => KnightWhite,
            ChessmanType.KnightBlack => KnightBlack,
            ChessmanType.BishopWhite => BishopWhite,
            ChessmanType.BishopBlack => BishopBlack,
            ChessmanType.RookWhite => RookWhite,
            ChessmanType.RookBlack => RookBlack,
            ChessmanType.QueenWhite => QueenWhite,
            ChessmanType.QueenBlack => QueenBlack,
            ChessmanType.KingWhite => KingWhite,
            ChessmanType.KingBlack => KingBlack,
            _ => throw new Exception()
        };
        
        return $"/{RelativePath}/{imageSource}";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="white"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static string GeneratePath(char type, bool white)
    {
        var imageSource = type switch
        {
            'p' or 'x' => white ? PawnWhite : PawnBlack,
            'v' => white ? RookWhite : RookBlack,
            'j' => white ? KnightWhite : KnightBlack,
            's' => white ? BishopWhite : BishopBlack,
            'd' => white ? QueenWhite : QueenBlack,
            'k' => white ? KingWhite : KingBlack,
            _ => throw new Exception()
        };

        return $"/{RelativePath}/{imageSource}";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static BitmapImage GetImage(ChessmanType type) => new(new Uri(GeneratePath(type), UriKind.Relative));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="white"></param>
    /// <returns></returns>
    public static BitmapImage GetImage(char type, bool white) =>
        new(new Uri(GeneratePath(type, white), UriKind.Relative));
}
