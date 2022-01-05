using Chess.View.UserControls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Chess.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly ChessUserControls _chessUserControl;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Chess";
        Icon = new BitmapImage(new Uri("Resources/Images/Icons/MainIcon.ico", UriKind.Relative));

        _chessUserControl = new ChessUserControls(LoadNewGameUserControl);
        LoadChessUserControl();
    }

    private void LoadChessUserControl() => Content = _chessUserControl;

    private void LoadNewGameUserControl(bool newGame) =>
        Content = new NewGameUserControl(LoadChessUserControl, newGame);

    // Controls events.
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton is MouseButton.Left)
        {
            DragMove();
        }
    }
}