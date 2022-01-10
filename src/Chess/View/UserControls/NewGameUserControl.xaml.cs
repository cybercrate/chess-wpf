using System.Windows;
using System.Windows.Controls;
using Engine.UtilityComponents;

namespace Chess.View.UserControls;

public partial class NewGameUserControl
{
    private readonly bool _newGame;
    
    private bool _playerBlackPieces;
    
    private bool _playerWhitePieces;
    
    private int _blackDifficulty;
    
    private int _whiteDifficulty;
    
    private float _minCalculationTime;
    
    private readonly Action _loadChessUserControl;
    
    public NewGameUserControl(Action loadChessUserControl, bool newGame)
    {
        InitializeComponent();
        
        _newGame = newGame;
        
        if (newGame is false)
        {
            HeaderTextBlock.Text = "Current game settings:";
        }
        
        _playerWhitePieces = Settings.WhiteIsPlayer;
        WhiteComboBox.SelectedIndex = _playerWhitePieces ? 0 : 1;

        _playerBlackPieces = Settings.BlackIsPlayer;
        BlackComboBox.SelectedIndex = _playerBlackPieces ? 0 : 1;

        _whiteDifficulty = Settings.WhiteDifficulty;
        WhiteSlider.Value = _whiteDifficulty - 1;

        _blackDifficulty = Settings.BlackDifficulty;
        BlackSlider.Value = _blackDifficulty - 1;

        _minCalculationTime = Settings.CalculationMinTime;
        MinTimeSlider.Value = _minCalculationTime;

        _loadChessUserControl = loadChessUserControl;
    }

    // Controls events.
    private void BlackComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BlackComboBox.SelectedIndex is 0)
        {
            _playerBlackPieces = true;
            BlackDifficultyLabel.Visibility = Visibility.Hidden;
            BlackSlider.Visibility = Visibility.Hidden;
            BlackStackPanel.Visibility = Visibility.Hidden;
            BlackDifficultyRow.Height = new GridLength(0);

            if (WhiteComboBox.SelectedIndex is not 0)
            {
                return;
            }
            
            MinTimeLabel.Visibility = Visibility.Hidden;
            MinTimeSlider.Visibility = Visibility.Hidden;
            MinTimeStackPanel.Visibility = Visibility.Hidden;
            MinTimeRow.Height = new GridLength(0);
            MinTimeBlackLine.Height = new GridLength(0);
        }
        else
        {
            _playerBlackPieces = false;
            BlackDifficultyLabel.Visibility = Visibility.Visible;
            BlackSlider.Visibility = Visibility.Visible;
            BlackStackPanel.Visibility = Visibility.Visible;
            BlackDifficultyRow.Height = new GridLength(1, GridUnitType.Star);

            MinTimeLabel.Visibility = Visibility.Visible;
            MinTimeSlider.Visibility = Visibility.Visible;
            MinTimeStackPanel.Visibility = Visibility.Visible;
            MinTimeRow.Height = new GridLength(1, GridUnitType.Star);
            MinTimeBlackLine.Height = new GridLength(1);
        }
    }

    private void BlackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
        _blackDifficulty = (int)BlackSlider.Value + 1;

    private void WhiteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (WhiteComboBox.SelectedIndex is 0)
        {
            _playerWhitePieces = true;
            WhiteDifficultyLabel.Visibility = Visibility.Hidden;
            WhiteSlider.Visibility = Visibility.Hidden;
            WhiteStackPanel.Visibility = Visibility.Hidden;
            WhiteDifficultyRow.Height = new GridLength(0);

            if (BlackComboBox.SelectedIndex is not 0)
            {
                return;
            }
            
            MinTimeLabel.Visibility = Visibility.Hidden;
            MinTimeSlider.Visibility = Visibility.Hidden;
            MinTimeStackPanel.Visibility = Visibility.Hidden;
            MinTimeRow.Height = new GridLength(0);
            MinTimeBlackLine.Height = new GridLength(0);
        }
        else
        {
            _playerWhitePieces = false;
            WhiteDifficultyLabel.Visibility = Visibility.Visible;
            WhiteSlider.Visibility = Visibility.Visible;
            WhiteStackPanel.Visibility = Visibility.Visible;
            WhiteDifficultyRow.Height = new GridLength(1, GridUnitType.Star);

            MinTimeLabel.Visibility = Visibility.Visible;
            MinTimeSlider.Visibility = Visibility.Visible;
            MinTimeStackPanel.Visibility = Visibility.Visible;
            MinTimeRow.Height = new GridLength(1, GridUnitType.Star);
            MinTimeBlackLine.Height = new GridLength(1);
        }
    }

    private void WhiteSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
        _whiteDifficulty = (int)WhiteSlider.Value + 1;

    private void MinTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
        _minCalculationTime = (float)MinTimeSlider.Value;

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        Settings.BlackIsPlayer = _playerBlackPieces;
        Settings.WhiteIsPlayer = _playerWhitePieces;
        Settings.BlackDifficulty = _blackDifficulty;
        Settings.WhiteDifficulty = _whiteDifficulty;
        Settings.CalculationMinTime = _minCalculationTime;
        
        if (_newGame)
        {
            Settings.IsNewGame = true;
        }
        
        _loadChessUserControl();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => _loadChessUserControl();
}