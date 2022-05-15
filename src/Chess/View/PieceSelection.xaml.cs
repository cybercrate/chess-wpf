using Engine.Pieces.Types;
using Engine.ResourceManagement;
using Engine.ResourceManagement.Types;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Chess.View;

/// <summary>
/// Interaction logic for PieceSelection.xaml
/// </summary>
public partial class PieceSelection
{
    public Status Status { get; private set; }

    public PieceSelection(double chessboardWidth, bool white)
    {
        InitializeComponent();

        Icon = ImageLoader.GenerateImage(IconType.Window);
        Height = chessboardWidth / 8 * 1.3 + 10;
        Width = chessboardWidth / 2 * 1.3 + 10;

        for (var i = 0; i < 4; i++)
        {
            Image image = new()
            {
                IsHitTestVisible = false
            };
                
            image.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);
                
            switch(i)
            {
                case 0:
                    image.Source = ImageLoader.GenerateImage(white ? PieceImageType.RookWhite : PieceImageType.RookBlack);
                    RookButton.Content = image;
                    break;
                case 1:
                    image.Source = ImageLoader.GenerateImage(white ? PieceImageType.KnightWhite : PieceImageType.KnightBlack);
                    KnightButton.Content = image;
                    break;
                case 2:
                    image.Source = ImageLoader.GenerateImage(white ? PieceImageType.BishopWhite : PieceImageType.BishopBlack);
                    BishopButton.Content = image;
                    break;
                case 3:
                    image.Source = ImageLoader.GenerateImage(white ? PieceImageType.QueenWhite : PieceImageType.QueenBlack);
                    QueenButton.Content = image;
                    break;
            }
        }
    }

    private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.Source is Button button)
        {
            if (Equals(button, RookButton))
            {
                Status = Status.Rook;
            }
            else if (Equals(button, KnightButton))
            {
                Status = Status.Knight;
            }
            else if (Equals(button, BishopButton))
            {
                Status = Status.Bishop;
            }
            else if (Equals(button, QueenButton))
            {
                Status = Status.Queen;
            }
                
            Close();
        }
        else
        {
            DragMove();
        }
    }
}
