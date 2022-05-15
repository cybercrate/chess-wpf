using Engine.Pieces.Types;
using Engine.ResourceManagement.Types;
using System.Windows.Media.Imaging;

namespace Engine.ResourceManagement;

public static class ImageLoader
{
    private static Dictionary<IconType, string>? _icons = null;
    private static Dictionary<PieceImageType, string>? _pieceImages = null;

    public static void SetResources(Dictionary<IconType, string> icons, Dictionary<PieceImageType, string> peces)
    {
        _icons = icons;
        _pieceImages = peces;
    }
    
    private static string GetPath(IconType type)
    {
        string? path = null;
        _icons?.TryGetValue(type, out path);

        return path!;
    }

    private static string GetPath(PieceImageType type)
    {
        string? path = null;
        _pieceImages?.TryGetValue(type, out path);

        return path!;
    }

    public static BitmapImage GenerateImage(IconType type) => new(new Uri(GetPath(type), UriKind.Relative));

    public static BitmapImage GenerateImage(Status status, PieceColor color) =>
        new(new Uri($"/{GetPath(GetSourceType(status, color))}", UriKind.Relative));

    public static BitmapImage GenerateImage(PieceImageType piece) =>
        new(new Uri($"/{GetPath(piece)}", UriKind.Relative));

    private static PieceImageType GetSourceType(Status status, PieceColor color) => status switch
    {
        Status.Pawn or Status.EnPassant => color is PieceColor.White ? PieceImageType.PawnWhite : PieceImageType.PawnBlack,
        Status.Rook => color is PieceColor.White ? PieceImageType.RookWhite : PieceImageType.RookBlack,
        Status.Knight => color is PieceColor.White ? PieceImageType.KnightWhite : PieceImageType.KnightBlack,
        Status.Bishop => color is PieceColor.White ? PieceImageType.BishopWhite : PieceImageType.BishopBlack,
        Status.Queen => color is PieceColor.White ? PieceImageType.QueenWhite : PieceImageType.QueenBlack,
        Status.King => color is PieceColor.White ? PieceImageType.KingWhite : PieceImageType.KingBlack,
        _ => throw new Exception()
    };
}
