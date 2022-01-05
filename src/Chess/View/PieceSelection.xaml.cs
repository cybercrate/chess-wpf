using Engine.Pieces.Base;
using Engine.ResourceManagement;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chess.View;

/// <summary>
/// Interaction logic for PieceSelection.xaml
/// </summary>
public partial class PieceSelection
{
    public char Status { get; private set; }

    public PieceSelection(double chessboardWidth, bool white)
    {
        InitializeComponent();
        Icon = new BitmapImage(new Uri("Resources/Images/Icons/MainIcon.ico", UriKind.Relative));
        
        Height = chessboardWidth / 8 * 1.3 + 10;
        Width = chessboardWidth / 2 * 1.3 + 10;

        for (var i = 0; i < 4; i++)
        {
            var image = new Image
            {
                IsHitTestVisible = false
            };
                
            image.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);
                
            switch(i)
            {
                case 0:
                    image.Source = ImageLoader.GetImage(white ? ChessmanType.RookWhite : ChessmanType.RookBlack);
                    RookButton.Content = image;
                    break;
                case 1:
                    image.Source = ImageLoader.GetImage(white ? ChessmanType.KnightWhite : ChessmanType.KnightBlack);
                    KnightButton.Content = image;
                    break;
                case 2:
                    image.Source = ImageLoader.GetImage(white ? ChessmanType.BishopWhite : ChessmanType.BishopBlack);
                    BishopButton.Content = image;
                    break;
                case 3:
                    image.Source = ImageLoader.GetImage(white ? ChessmanType.QueenWhite : ChessmanType.QueenBlack);
                    QueenButton.Content = image;
                    break;
            }
        }
    }

    private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.Source is Button)
        {
            if (e.Source == RookButton)
            {
                Status = 'v';
            }
            else if (e.Source == KnightButton)
            {
                Status = 'j';
            }
            else if (e.Source == BishopButton)
            {
                Status = 's';
            }
            else if (e.Source == QueenButton)
            {
                Status = 'd';
            }
                
            Close();
        }
        else
        {
            DragMove();
        }
    }
}