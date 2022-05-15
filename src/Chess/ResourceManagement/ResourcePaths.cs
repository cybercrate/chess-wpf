using Engine.ResourceManagement.Types;

namespace Chess.ResourceManagement;

internal static class ResourcePaths
{
    private const string _resourcesRelativePath = "Resources/Images";
    private const string _iconRelativePath = $"{_resourcesRelativePath}/Icons";
    private const string _pieceImagesRelativePath = $"{_resourcesRelativePath}/Chessmen";

    internal static Dictionary<IconType, string> _icons = new()
    {
        { IconType.Back, $"{_iconRelativePath}/Back.png" },
        { IconType.Exit, $"{_iconRelativePath}/Exit.png" },
        { IconType.Forward, $"{_iconRelativePath}/Forward.png" },
        { IconType.New, $"{_iconRelativePath}/New.png" },
        { IconType.Open, $"{_iconRelativePath}/Open.png" },
        { IconType.Save, $"{_iconRelativePath}/Save.png" },
        { IconType.Settings, $"{_iconRelativePath}/Settings.png" },
        { IconType.Window, $"{_iconRelativePath}/App.ico" }
    };

    internal static Dictionary<PieceImageType, string> _pieceImages = new()
    {
        { PieceImageType.PawnWhite, $"{_pieceImagesRelativePath}/PawnWhite.png" },
        { PieceImageType.PawnBlack, $"{_pieceImagesRelativePath}/PawnBlack.png" },
        { PieceImageType.KnightWhite, $"{_pieceImagesRelativePath}/KnightWhite.png" },
        { PieceImageType.KnightBlack, $"{_pieceImagesRelativePath}/KnightBlack.png" },
        { PieceImageType.BishopWhite, $"{_pieceImagesRelativePath}/BishopWhite.png" },
        { PieceImageType.BishopBlack, $"{_pieceImagesRelativePath}/BishopBlack.png" },
        { PieceImageType.RookWhite, $"{_pieceImagesRelativePath}/RookWhite.png" },
        { PieceImageType.RookBlack, $"{_pieceImagesRelativePath}/RookBlack.png" },
        { PieceImageType.QueenWhite, $"{_pieceImagesRelativePath}/QueenWhite.png" },
        { PieceImageType.QueenBlack, $"{_pieceImagesRelativePath}/QueenBlack.png" },
        { PieceImageType.KingWhite, $"{_pieceImagesRelativePath}/KingWhite.png" },
        { PieceImageType.KingBlack, $"{_pieceImagesRelativePath}/KingBlack.png" }
    };
}
