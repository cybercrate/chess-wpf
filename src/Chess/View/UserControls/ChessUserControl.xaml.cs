using Engine;
using Engine.UtilityComponents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Chess.View.UserControls;

/// <summary>
/// Interaction logic for ChessUserControl.xaml
/// </summary>
public partial class ChessUserControls
{
    private readonly ChessEngine _chessEngine;
    
    private readonly Action<bool> _loadNewGameUserControl;
    
    private readonly System.Windows.Media.Effects.BlurEffect _objBlur = new() { Radius = 4 };
    
    public ChessUserControls(Action<bool> loadNewGameUserControl)
    {
        InitializeComponent();
        
        _resizeTimer.Tick += ResizeTimer_Tick;
        
        // Create chessboard made of buttons.
        var buttons = new Button[8, 8];
        
        for (sbyte row = 0; row < 8; row++)
        {
            for (sbyte column = 0; column < 8; column++)
            {
                Button button = new();
                button.SetValue(Grid.ColumnProperty, (int)column);
                button.SetValue(Grid.RowProperty, (int)row);
                button.SetValue(Border.BorderThicknessProperty, new Thickness(0));
                button.Focusable = false;
                button.Padding = new Thickness(0);
                button.Tag = new Coords(row, column);

                button.Background = row % 2 != column % 2 ? Brushes.MediumSeaGreen : Brushes.Ivory;
                    
                Grid grid = new();                    
                grid.Children.Add(new UIElement());
                grid.Children.Add(new UIElement());
                
                button.Content = grid;
                buttons[row, column] = button;
                
                Grid.Children.Add(button);
            }
        }

        _chessEngine = new ChessEngine(
            buttons,
            TextBlockOnTurn,
            StatusTextBlock,
            BackMenuItem,
            ForwardMenuItem,
            WrapPanelWhite,
            WrapPanelBlack,
            ProgressBar,
            OpenPieceChoice);
        
        _loadNewGameUserControl = loadNewGameUserControl;
        DataContext = _chessEngine;
    }

    private char OpenPieceChoice(bool white)
    {
        PieceSelection ps = new(Grid.ActualWidth, white) { Owner = Application.Current.MainWindow };
        
        if (Application.Current.MainWindow is null)
        {
            return ps.Status;
        }
        
        Application.Current.MainWindow.Effect = _objBlur;
        ps.ShowDialog();
        Application.Current.MainWindow.Effect = null;

        return ps.Status;
    }
    
    /// <summary>
    /// Timer that allows to run code after window size change.
    /// </summary>
    private readonly DispatcherTimer _resizeTimer = new() { Interval = TimeSpan.FromMilliseconds(10) };
    
    /// <summary>
    /// This code is called 10ms after window size change.
    /// </summary>
    private void ResizeTimer_Tick(object? sender, EventArgs e)
    {
        _resizeTimer.IsEnabled = false;
        _chessEngine.ImageSizeWrapPanel = Grid.ActualHeight * SizeRatio;
            
        if (WrapPanelWhite.Children.Count > 0 && WrapPanelWhite.Children[0] is TextBlock is false)
        {
            foreach (Image image in WrapPanelWhite.Children)
            {
                image.Height = _chessEngine.ImageSizeWrapPanel;
                image.Width = _chessEngine.ImageSizeWrapPanel;
            }
        }

        if (WrapPanelBlack.Children.Count > 0 && WrapPanelBlack.Children[0] is TextBlock is false)
        {
            foreach (Image image in WrapPanelBlack.Children)
            {
                image.Height = _chessEngine.ImageSizeWrapPanel;
                image.Width = _chessEngine.ImageSizeWrapPanel;
            }
        }
    }

    /// <summary>
    /// Taken piece image ratio to the whole play board.
    /// </summary>
    private const double SizeRatio = 0.105;
    
    /// <summary>
    /// Modifies controls layout based on window size.
    /// </summary>
    private void GridSizeRatio_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var width = GridSizeRatio.ActualWidth;
        var height = GridSizeRatio.ActualHeight;
        var widthWithoutBorders = width * 0.3;
        
        if (width - widthWithoutBorders > height)
        {
            var margin = width - height;
            GridSizeRatio.RowDefinitions[3].Height = new GridLength(0);
            GridSizeRatio.ColumnDefinitions[3].Width = new GridLength(margin);
        }
        else
        {
            var margin = height - width;
            GridSizeRatio.ColumnDefinitions[3].Width = new GridLength(widthWithoutBorders);
            GridSizeRatio.RowDefinitions[3].Height = new GridLength(margin + widthWithoutBorders);
        }
        
        _resizeTimer.Stop();
        _resizeTimer.Start();
    }
    
    /// <summary>
    /// Square click event.
    /// </summary>
    private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        _chessEngine.ButtonClick((Coords)(e.Source as Button)!.Tag);

    /// <summary>
    /// First recalculation of taken pieces image.
    /// </summary>
    private void GridSizeRatio_Loaded(object sender, RoutedEventArgs e) =>
        _chessEngine.ImageSizeWrapPanel = Grid.ActualHeight * SizeRatio;

    /// <summary>
    /// New game button click event.
    /// </summary>
    private void NewMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _loadNewGameUserControl(true);
        _chessEngine.LoadedNewGameUserControl();
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _loadNewGameUserControl(false);
        _chessEngine.LoadedNewGameUserControl();
    }

    private void OpenMenuItem_Click(object sender, RoutedEventArgs e) => _chessEngine.LoadFromFile();

    private void SaveMenuItem_Click(object sender, RoutedEventArgs e) => _chessEngine.SaveIntoFile();
        
    private void CloseMenuItem_Click(object sender, RoutedEventArgs e) => Application.Current.MainWindow!.Close();
        
    private void BackMenuItem_Click(object sender, RoutedEventArgs e) => _chessEngine.Back();
        
    private void ForwardMenuItem_Click(object sender, RoutedEventArgs e) => _chessEngine.Forward();
        
    private void UserControl_Loaded(object sender, RoutedEventArgs e) => _chessEngine.LoadedChessUserControl();
}
