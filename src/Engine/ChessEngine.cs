using Engine.AI;
using Engine.Conditions;
using Engine.IO;
using Engine.Pieces.Base;
using Engine.ResourceManagement;
using Engine.UtilityComponents;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Engine;

/// <summary>
/// Wires the logic layer with the user interface.
/// </summary>
public class ChessEngine : INotifyPropertyChanged
{
    private double _turnLength;
    private double _progressValue;
    private double _progressMaximum;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    // The chessboard.
    private readonly Button[,] _buttons;
    
    // Text of color that is on turn.
    private readonly TextBlock _textBlockOnTurn;
    
    // Text block for status displaying (check, mate...).
    private readonly TextBlock _textBlockStatus;
    
    // Menu button for previous turn.
    private readonly MenuItem _back;
    
    // Menu button for previous turn.
    private readonly MenuItem _forward;
    
    // White lost pieces.
    private readonly WrapPanel _whiteLost;
    
    // Black lost pieces.
    private readonly WrapPanel _blackLost;
    
    // Emulator turn calculation progress.
    private readonly Grid _progressBar;
    
    // Keeps all possible moves. Null in case nothing is selected.
    private Coords[]? _selectedCoords;
    
    // Keeps selected Emulator moves. Null in case nothing is selected.
    private Coords[]? _selectedAiCoords;
    
    // Timer that updates UI during turn calculation.
    private readonly DispatcherTimer _timerUpdateUi;
    
    // Opens dialog window with pawn promotion options.
    private readonly Func<bool, char> _openPromotionOptions;
    
    // Logically and memory inexpensive recorded condition.
    private Conditions.Condition _condition = null!;
    
    // Calculated condition, including pieces and their possible moves.
    private CalculatedCondition _calculatedCondition = null!;
    
    // Artificial intelligence.
    private readonly Emulator _emulator = new();
    
    // Task that calculates half turn of Emulator.
    private Task _turnTask = Task.Delay(0);
    
    // History of executed moves by both players.
    private Stack<Conditions.Condition> _history = new();
    
    // Future moves done by both players.
    private readonly Stack<Conditions.Condition> _future = new();
    
    // DateTime for turn length.
    private DateTime _dt;
    
    // How many times player requested to return back to previous turn.
    private int _backCount;
    
    // How many times player requested to go forward to the next turn.
    private int _forwardCount;
    
    /// <summary>
    /// Specifies size of image of the taken piece, based on size of the play area.
    /// The value is recalculated during window size change.
    /// </summary>
    public double ImageSizeWrapPanel { get; set; }
    
    /// <summary>
    /// Turn length in seconds.
    /// </summary>
    public double TurnLength
    {
        get => _turnLength;
        set
        {
            _turnLength = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TurnLength)));
        }
    }

    /// <summary>
    /// ProgressBar value.
    /// </summary>
    public double ProgressValue
    {
        get => _progressValue;
        set
        {
            _progressValue = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressValue)));
        }
    }
    
    /// <summary>
    /// ProgressBar maximum value.
    /// </summary>
    public double ProgressMaximum
    {
        get => _progressMaximum;
        set
        {
            _progressMaximum = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressMaximum)));
        }
    }
    
    public static int ProgressValueStatic;
    public static int ProgressMaximumStatic = 0;

    public static readonly ConcurrentQueuedDictionary<string, CalculatedConditionData> CalculatedConditionData =
        new(50000);
    
    public ChessEngine(
        Button[,] buttons,
        TextBlock textBlockOnTurn,
        TextBlock textBlockStatus,
        MenuItem back,
        MenuItem forward,
        WrapPanel whiteLost,
        WrapPanel blackLost,
        Grid progressBar,
        Func<bool, char> openPromotionOptions)
    {
        _buttons = buttons;
        _textBlockOnTurn = textBlockOnTurn;
        _textBlockStatus = textBlockStatus;
        _back = back;
        _forward = forward;
        _whiteLost = whiteLost;
        _blackLost = blackLost;
        _progressBar = progressBar;
        _openPromotionOptions = openPromotionOptions;
        _timerUpdateUi = new DispatcherTimer(DispatcherPriority.DataBind) { Interval = TimeSpan.FromMilliseconds(125) };
        _timerUpdateUi.Tick += TimerUpdateUi_Tick;
    }
    
    /// <summary>
    /// Logical movement of piece.
    /// </summary>
    public static void MovePiece(Coords from, Coords to, Conditions.Condition condition, bool automaticPromotion = true)
    {
        // Specifies whether to increment draw50 value in condition.
        var draw50 = true;
        
        // Taking pieces and promotions.
        // Checking whether piece has taken a pawn en passant.
        if (condition.Chessboard[from.Row, from.Column].Status is 'p')
        {
            // If the pawn moves, draw50 is invalid.
            draw50 = false;
            
            if (condition.Chessboard[to.Row, to.Column].Status is 'x')
            {
                var row = to.Row is 2 ? to.Row + 1 : to.Row - 1;
                condition.Chessboard[row, to.Column].Status = 'n';
            }
        }
        
        // Removing previous taking en passant.
        for (var i = 0; i < 8; i++)
        {
            if (condition.Chessboard[2, i].Status is 'x')
            {
                condition.Chessboard[2, i].Status = 'n';
                break;
            }

            if (condition.Chessboard[5, i].Status is not 'x')
            {
                continue;
            }
            
            condition.Chessboard[5, i].Status = 'n';
            break;
        }

        switch (condition.Chessboard[from.Row, from.Column].Status)
        {
            // Checking new taking en passant.
            case 'p':
            {
                var move = from.Row - to.Row;
                
                // If the pawn moved 2 squares.
                if (Math.Abs(move) is 2)
                {
                    // If the movement is negative, a white pawn was moved.
                    if (move < 0)
                    {
                        // Creating selection for taking en passant.
                        condition.Chessboard[from.Row + 1, from.Column].Status = 'x';
                        
                        // Setting pawn color to square for taking en passant.
                        condition.Chessboard[from.Row + 1, from.Column].White =
                            condition.Chessboard[from.Row, from.Column].White;
                    }
                    // If positive, a black pawn was moved.
                    else 
                    {
                        // Creating selection for taking en passant.
                        condition.Chessboard[from.Row - 1, from.Column].Status = 'x';
                        
                        // Setting pawn color to square for taking en passant.
                        condition.Chessboard[from.Row - 1, from.Column].White =
                            condition.Chessboard[from.Row, from.Column].White;
                    }
                }

                break;
            }
            
            // Castling.
            case 'v' when from.Row is 0:
            {
                switch (from.Column)
                {
                    case 0:
                        condition.BlackLargeRookMoved = true;
                        break;
                    case 7:
                        condition.BlackSmallRookMoved = true;
                        break;
                }

                break;
            }
            case 'v':
            {
                if (from.Row is 7)
                {
                    switch (from.Column)
                    {
                        case 0:
                            condition.WhiteLargeRookMoved = true;
                            break;
                        case 7:
                            condition.WhiteSmallRookMoved = true;
                            break;
                    }
                }

                break;
            }
            case 'k':
            {
                // Castling.
                if (condition.Chessboard[from.Row, from.Column].White)
                {
                    condition.WhiteKingMoved = true;
                }
                else
                {
                    condition.BlackKingMoved = true;
                }

                var position = from.Column - to.Column;
                
                if (Math.Abs(position) > 1)
                {
                    if (position > 0)
                    {
                        // Left.
                        MovePiece(new Coords(from.Row, (sbyte)(from.Column - 4)),
                            new Coords(from.Row, (sbyte)(from.Column - 1)), condition);
                    }
                    else
                    {
                        // Right.
                        MovePiece(new Coords(from.Row, (sbyte)(from.Column + 3)),
                            new Coords(from.Row, (sbyte)(from.Column + 1)), condition);
                    }
                    
                    // Rook movement caused swapping white on turn.
                    condition.WhiteOnTurn = !condition.WhiteOnTurn;
                }

                break;
            }
        }

        var status = condition.Chessboard[to.Row, to.Column].Status;
        
        if (status is not 'n' and not 'x')
        {
            draw50 = false;
        }

        // Moving piece to new position.
        condition.Chessboard[to.Row, to.Column] = condition.Chessboard[from.Row, from.Column];
        
        // Removing piece from old position.
        condition.Chessboard[from.Row, from.Column].Status = 'n';
        
        // Is necessary to increment draw50 or null.
        if (draw50)
        {
            condition.Draw50++;
        }
        else
        {
            condition.Draw50 = 0;
        }
        
        // Moving pawn to the last row - automatically promotes to queen.
        if (automaticPromotion)
        {
            if (condition.Chessboard[to.Row, to.Column].Status is 'p')
            {
                if (to.Row is 7 or 0)
                {
                    condition.Chessboard[to.Row, to.Column].Status = 'd';
                }
            }
        }
        
        // Swapping color on turn.
        condition.WhiteOnTurn = !condition.WhiteOnTurn;
    }
    
    /// <summary>
    /// Checks whether after the current turn the king will still be in check.
    /// Does not interrupt the passed condition.
    /// </summary>
    public static bool ValidMoveDuringCheck(Coords from, Coords to, Conditions.Condition condition)
    {
        // It's necessary to create a copy, to prevent corrupting the previous condition.
        Conditions.Condition conditionCopy = new(condition);
        
        // Piece movement.
        MovePiece(from, to, conditionCopy);
        
        // Moving back white on turn (for CalculatedCondition algorithm).
        conditionCopy.WhiteOnTurn = !conditionCopy.WhiteOnTurn;
        return !CalculatedCondition.FindOutCheck(conditionCopy);
    }

    /// <summary>
    /// Logical and graphical reset. Resets all fields.
    /// </summary>
    private void NewGame()
    {
        Settings.NewGame = false;
        Emulator.InterruptHalfTurn = true;

        Task.Run(() => { _turnTask.Wait(); })
            .ContinueWith(_ =>
                {
                    _history.Clear();
                    _future.Clear();
                    _condition = new Conditions.Condition();
                    _calculatedCondition = new CalculatedCondition(_condition);
                    DrawCalculatedSituation();
                    _textBlockOnTurn.Text = "white";
                    _textBlockStatus.Text = string.Empty;

                    if (_selectedCoords is not null)
                    {
                        foreach (Coords coords in _selectedCoords)
                        {
                            DeselectSquare(coords);
                        }

                        _selectedCoords = null;
                    }

                    DeselectAiCoords();

                    // Resetting lost pieces.
                    _whiteLost.Children.RemoveRange(0, _whiteLost.Children.Count);
                    _blackLost.Children.RemoveRange(0, _blackLost.Children.Count);
                    _whiteLost.Children.Add(GenerateNoLostPiecesTextBlock());
                    _blackLost.Children.Add(GenerateNoLostPiecesTextBlock());

                    ProgressValue = 0;
                    ProgressMaximum = 1;
                    EmulatorTurn();
                },
                TaskScheduler.FromCurrentSynchronizationContext());
    }

    /// <summary>
    /// Square click.
    /// </summary>
    public void ButtonClick(Coords coords)
    {
        if (PlayerOnTurn() is false)
        {
            return;
        }

        if (_selectedCoords is null)
        {
            // If the square contains a piece of player that is on turn...
            if (_condition.Chessboard[coords.Row, coords.Column].Status is 'n' or 'x' ||
                _condition.Chessboard[coords.Row, coords.Column].White != _condition.WhiteOnTurn)
            {
                return;
            }

            DeselectAiCoords();
            SelectPossibleMoves(coords);
        }
        else
        {
            // Graphically selected squares are unselected.
            foreach (Coords coordsPossibleMove in _selectedCoords)
            {
                DeselectSquare(coordsPossibleMove);
            }
            
            // Whether selected square was clicked.
            var selectedClicked = false;
            
            // Length - 1, because the last coord depends on piece position.
            for (var i = 0; i < _selectedCoords.Length - 1; i++)
            {
                // Selected button clicked?
                if (_selectedCoords[i].Equals(coords) is false)
                {
                    continue;
                }
                
                selectedClicked = true;
                break;
            }
            
            if (selectedClicked)
            {
                // Moves the piece from correct position to the clicked square.
                MovePiece(_selectedCoords[^1], coords);
                _selectedCoords = null;
                
                // Graphically draws the new condition.
                DrawCalculatedSituation();
            }
            // Selected square wasn't clicked.
            else 
            {
                // If different piece was clicked, its square is selected.
                if (_selectedCoords[^1].Equals(coords) is false)
                {
                    // Necessary to null after condition (selectedCoords is used in the condition).
                    _selectedCoords = null;
                    
                    // If the square contains piece of player that is on turn...
                    if (_condition.Chessboard[coords.Row, coords.Column].Status is not 'n' and not 'x' &&
                        _condition.Chessboard[coords.Row, coords.Column].White == _condition.WhiteOnTurn)
                    {
                        SelectPossibleMoves(coords);
                    }
                }
                else
                {
                    // Necessary to null after condition (selectedCoords is used in the previous condition).
                    _selectedCoords = null;
                }
            }
        }
    }
    
    /// <summary>
    /// Moving piece with graphical representation.
    /// If the promotion is false, the user won't be able to choose any promotion option.
    /// </summary>
    private void MovePiece(Coords from, Coords to, bool automaticPromotion = false)
    {
        _future.Clear();
        _history.Push(new Conditions.Condition(_condition));
        
        // Deselection of taken piece.
        // Piece moves to a square with another piece, therefore attacking it.
        if (_condition.Chessboard[to.Row, to.Column].Status is not 'n')
        {
            // It isn't en passant, or it is but a pawn is taken by a pawn.
            if (_condition.Chessboard[to.Row, to.Column].Status is not 'x' ||
                _condition.Chessboard[from.Row, from.Column].Status is 'p')
            {
                // Graphical representation of taken piece.
                if (_condition.Chessboard[to.Row, to.Column].White)
                {
                    // Removing text block if it's still there.
                    if (_whiteLost.Children[0] is TextBlock)
                    {
                        _whiteLost.Children.RemoveAt(0);
                    }
                    
                    // Adding image of taken piece.
                    _whiteLost.Children.Add(GeneratePieceImage(_condition.Chessboard[to.Row, to.Column], true));
                }
                else
                {
                    // Removing text block if it's still there.
                    if (_blackLost.Children[0] is TextBlock)
                    {
                        _blackLost.Children.RemoveAt(0);
                    }

                    // Adding image of taken piece.
                    _blackLost.Children.Add(GeneratePieceImage(_condition.Chessboard[to.Row, to.Column], true));
                }
            }
        }

        // Move the piece in logical layer.
        MovePiece(from, to, _condition, automaticPromotion);
        
        // Pawn at the end of path.
        if (automaticPromotion is false)
        {
            if (_condition.Chessboard[to.Row, to.Column].Status is 'p')
            {
                if (to.Row is 7 or 0)
                {
                    var options = _openPromotionOptions(_condition.Chessboard[to.Row, to.Column].White);
                    _condition.Chessboard[to.Row, to.Column].Status = options;
                }
            }
        }

        _calculatedCondition = new CalculatedCondition(_condition);
        EmulatorTurn();
    }

    /// <summary>
    /// Returns one half turn back.
    /// </summary>
    public void Back()
    {
        if (_backCount < _history.Count)
        {
            _backCount++;
            SetupBack();
        }

        if (_backCount > 1)
        {
            return;
        }

        Emulator.InterruptHalfTurn = true;

        Task.Run(() => { _turnTask.Wait(); })
            .ContinueWith(_ =>
                {
                    // Deselects selected squares.
                    DeselectAiCoords();

                    if (_selectedCoords is not null)
                    {
                        foreach (Coords c in _selectedCoords)
                        {
                            DeselectSquare(c);
                        }

                        _selectedCoords = null;
                    }

                    _back.IsEnabled = false;

                    for (var i = 0; i < _backCount; i++)
                    {
                        var previousSituation = _history.Pop();

                        // Returning taken piece.
                        if (_condition.ToString().Length < previousSituation.ToString().Length)
                        {
                            // Calculating white and black pieces in the current condition.
                            var whiteCurrentCount = _condition.Chessboard.Cast<PieceChar>()
                                .Where(pc => pc.Status is not 'n' && pc.Status is not 'x')
                                .Count(pc => pc.White);

                            // Calculating white and black pieces in the previous condition.
                            var whitePreviousCount = previousSituation.Chessboard.Cast<PieceChar>()
                                .Where(pc => pc.Status is not 'n' && pc.Status is not 'x')
                                .Count(pc => pc.White);

                            // Removing taken piece from wrap panel of the taken piece.
                            var taken = whitePreviousCount > whiteCurrentCount ? _whiteLost : _blackLost;
                            taken.Children.RemoveAt(taken.Children.Count - 1);

                            if (taken.Children.Count is 0)
                            {
                                var tb = GenerateNoLostPiecesTextBlock();
                                taken.Children.Add(tb);
                            }
                        }

                        _future.Push(_condition);
                        _condition = previousSituation;
                    }

                    _backCount = 0;
                    _calculatedCondition = new CalculatedCondition(_condition);

                    DrawCalculatedSituation();

                    if (PlayerOnTurn() is false)
                    {
                        EmulatorTurn();
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext());
    }

    /// <summary>
    /// Moves game one half turn forward.
    /// </summary>
    public void Forward()
    {
        if (_forwardCount < _future.Count)
        {
            _forwardCount++;
            SetupForward();
        }

        if (_forwardCount > 1)
        {
            return;
        }

        Emulator.InterruptHalfTurn = true;

        Task.Run(() => { _turnTask.Wait(); })
            .ContinueWith(_ =>
                {
                    // Deselects selected squares.
                    DeselectAiCoords();

                    if (_selectedCoords is not null)
                    {
                        foreach (var selectedCoords in _selectedCoords)
                        {
                            DeselectSquare(selectedCoords);
                        }

                        _selectedCoords = null;
                    }

                    _forward.IsEnabled = false;

                    for (var i = 0; i < _forwardCount; i++)
                    {
                        var futureSituation = _future.Pop();

                        // Returning taken piece.
                        if (_condition.ToString().Length > futureSituation.ToString().Length)
                        {
                            // White pieces in current check condition.
                            List<PieceChar> whiteCurrent = new();

                            // Black pieces in current check condition.
                            List<PieceChar> blackCurrent = new();

                            // White pieces in future check condition.
                            List<PieceChar> whiteFuture = new();

                            // Black pieces in future check condition.
                            List<PieceChar> blackFuture = new();

                            // Loading white and black pieces in current condition.
                            foreach (PieceChar pc in _condition.Chessboard)
                            {
                                if (pc.Status is 'n' or 'x')
                                {
                                    continue;
                                }

                                if (pc.White)
                                {
                                    whiteCurrent.Add(pc);
                                }
                                else
                                {
                                    blackCurrent.Add(pc);
                                }
                            }

                            // Loading white and black pieces in previous condition.
                            foreach (PieceChar pc in futureSituation.Chessboard)
                            {
                                if (pc.Status is 'n' or 'x')
                                {
                                    continue;
                                }

                                if (pc.White)
                                {
                                    whiteFuture.Add(pc);
                                }
                                else
                                {
                                    blackFuture.Add(pc);
                                }
                            }

                            WrapPanel taken;
                            var takenPc = new PieceChar('n');

                            // Finding taken piece.
                            if (whiteCurrent.Count > whiteFuture.Count)
                            {
                                taken = _whiteLost;

                                foreach (PieceChar pc in whiteCurrent)
                                {
                                    if (whiteFuture.Contains(pc))
                                    {
                                        whiteFuture.Remove(pc);
                                    }
                                    else
                                    {
                                        takenPc = pc;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                taken = _blackLost;

                                foreach (PieceChar pc in blackCurrent)
                                {
                                    if (blackFuture.Contains(pc))
                                        blackFuture.Remove(pc);
                                    else
                                    {
                                        takenPc = pc;
                                        break;
                                    }
                                }
                            }

                            if (taken.Children[0] is TextBlock)
                            {
                                taken.Children.RemoveAt(0);
                            }

                            // Adding taken piece into wrap panel of taken pieces.
                            taken.Children.Add(GeneratePieceImage(takenPc, true));
                        }

                        _history.Push(_condition);
                        _condition = futureSituation;
                    }

                    _forwardCount = 0;
                    _calculatedCondition = new CalculatedCondition(_condition);

                    DrawCalculatedSituation();

                    if (PlayerOnTurn() is false)
                    {
                        EmulatorTurn();
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext());
    }

    /// <summary>
    /// Saves the check condition into text file.
    /// </summary>
    public void SaveIntoFile()
    {
        // If Emulator performs half turn, it is interrupted until the condition is saved.
        if (PlayerOnTurn() is false)
        {
            Emulator.InterruptHalfTurn = true;
        }

        // Generating file name.
        var dt = DateTime.Now;
        var name = $"Chess {dt.Year}-{dt:MM}-{dt:dd} {dt:HH.mm}";

        var sfd = new SaveFileDialog
        {
            Filter = "Text files|*.txt",
            FileName = name
        };

        if (sfd.ShowDialog() is true)
        {
            // Loading taken pieces into logical representation.
            var blackTakenPiece = _blackLost.Children.Cast<UIElement>()
                .TakeWhile(uiElement => uiElement is not TextBlock)
                .Select(uiElement => (PieceChar)(uiElement as Image)!.Tag).ToList();

            var whiteTakenList = _whiteLost.Children.Cast<UIElement>()
                .TakeWhile(uiElement => uiElement is not TextBlock)
                .Select(uiElement => (PieceChar)(uiElement as Image)!.Tag).ToList();

            try
            {
                File file = new(_condition, blackTakenPiece, whiteTakenList, _history);
                file.Save(sfd.FileName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Saving failure", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        // If player is not on turn, the Emulator executes turn after saving.
        if (PlayerOnTurn() is false)
        {
            Task.Run(() => _turnTask.Wait())
                .ContinueWith(_ => EmulatorTurn(), TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
    
    /// <summary>
    /// Load chess condition from file.
    /// </summary>
    public void LoadFromFile()
    {
        // Pausing game and preparing it for resetting.
        // Deselect selected squares.
        DeselectAiCoords();
        
        if (_selectedCoords is not null)
        {
            foreach (Coords selectedCoords in _selectedCoords)
            {
                DeselectSquare(selectedCoords);
            }
            
            _selectedCoords = null;
        }
        
        // If Emulator performs half turn, it is interrupted until the condition is loaded/not loaded.
        if (PlayerOnTurn() is false)
        {
            Emulator.InterruptHalfTurn = true;
        }
        
        var fileDialog = new OpenFileDialog
        {
            Filter = "Text files|*.txt",
            Multiselect = false
        };

        // IF cancel was clicked, then loading is cancelled.
        if (fileDialog.ShowDialog() is not true)
        {
            LoadingUnsuccessful();
            return;
        }

        // Attempt to load.
        File file = new();
        
        try
        {
            file.Load(fileDialog.OpenFile());
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Loading failure", MessageBoxButton.OK, MessageBoxImage.Error);
            LoadingUnsuccessful();
            return;
        }

        // Loading taken pieces.
        _blackLost.Children.Clear();
        
        if (file.BlackTaken.Count is 0)
        {
            _blackLost.Children.Add(GenerateNoLostPiecesTextBlock());
        }
        else
        {
            foreach (PieceChar pc in file.BlackTaken)
            {
                _blackLost.Children.Add(GeneratePieceImage(pc, true));
            }
        }
        
        _whiteLost.Children.Clear();
        
        if (file.WhiteTaken.Count is 0)
        {
            _whiteLost.Children.Add(GenerateNoLostPiecesTextBlock());
        }
        else
        {
            foreach (PieceChar pc in file.WhiteTaken)
            {
                _whiteLost.Children.Add(GeneratePieceImage(pc, true));
            }
        }

        _condition = file.Condition;
        _calculatedCondition = new CalculatedCondition(_condition);
        _history.Clear();
        _history = file.History;
        
        Settings.PlayerIsWhite = true;
        Settings.PlayerIsBlack = true;
        
        DrawCalculatedSituation();
    }
    
    /// <summary>
    /// After the chess engine is loaded, settings need to be checked again.
    /// </summary>
    public void LoadedChessUserControl()
    {
        if (Settings.NewGame)
        {
            NewGame();
        }
        else if (PlayerOnTurn() is false)
        {
            DrawCalculatedSituation();
            
            Task.Run(() => _turnTask.Wait())
                .ContinueWith(_ => EmulatorTurn(), TaskScheduler.FromCurrentSynchronizationContext());
        }
        else
        {
            DrawCalculatedSituation();
        }
    }
    
    /// <summary>
    /// Interrupts executed half turn (if there is any) and then loads user control of new game.
    /// </summary>
    public void LoadedNewGameUserControl()
    {
        if (PlayerOnTurn() is false)
        {
            Emulator.InterruptHalfTurn = true;
        }
    }

    private bool PlayerOnTurn() =>
        _condition.WhiteOnTurn && Settings.PlayerIsWhite || _condition.WhiteOnTurn is false && Settings.PlayerIsBlack;

    /// <summary>
    /// Draws condition of chessboard.
    /// </summary>
    private void DrawCalculatedSituation()
    {
        for (var row = 0; row < 8; row++)
        {
            for (var column = 0; column < 8; column++)
            {
                // Free square.
                if (_condition.Chessboard[row, column].Status is 'n' or 'x')
                {
                    if ((_buttons[row, column].Content as Grid)!.Children[1] is not Image)
                    {
                        continue;
                    }
                    
                    (_buttons[row, column].Content as Grid)!.Children.RemoveRange(0, 2);
                    (_buttons[row, column].Content as Grid)!.Children.Add(new UIElement());
                    (_buttons[row, column].Content as Grid)!.Children.Add(new UIElement());
                }
                // Other pieces.
                else
                {
                    // Displays piece on square.
                    var image = GeneratePieceImage(_condition.Chessboard[row, column]);
                    
                    (_buttons[row, column].Content as Grid)!.Children.RemoveRange(0, 2);
                    (_buttons[row, column].Content as Grid)!.Children.Add(new UIElement());
                    (_buttons[row, column].Content as Grid)!.Children.Add(image);
                }
            }
        }

        // Selected squares are drawn.
        if (_selectedAiCoords is not null)
        {
            foreach (Coords coords in _selectedAiCoords)
            {
                SelectSquare(coords, true);
            }
        }
        
        if (_selectedCoords is not null)
        {
            for (var i = 0; i < _selectedCoords.Length - 1; i++)
            {
                SelectSquare(_selectedCoords[i]);
            }
            
            SelectSquare(_selectedCoords[^1], false, true);
        }

        _textBlockOnTurn.Text = _condition.WhiteOnTurn ? "white" : "black";

        if (_condition.Draw50 > 99)
        {
            _textBlockStatus.Text += "Draw\n(50 turns rule)";
        }
        else if (_calculatedCondition.Check)
        {
            _textBlockStatus.Text = _calculatedCondition.DrawMate ? "Mate" : "Check";
        }
        else
        {
            _textBlockStatus.Text = _calculatedCondition.DrawMate is false ? string.Empty : "Draw";
        }

        SetupBack();
        SetupForward();

        if (Settings.PlayerIsWhite && Settings.PlayerIsBlack)
        {
            if (_progressBar.Visibility == Visibility.Visible)
            {
                _progressBar.Visibility = Visibility.Hidden; 
            }
        }
        else if (_progressBar.Visibility == Visibility.Hidden)
        {
            _progressBar.Visibility = Visibility.Visible;
        }
    }
    
    /// <summary>
    /// Graphically sets up the back MenuItem.
    /// </summary>
    private void SetupBack()
    {
        _back.IsEnabled = _history.Count - _backCount > 0;

        if (Settings.PlayerIsWhite is false || Settings.PlayerIsBlack is false)
        {
            _back.Header = $"Back ({_backCount}/{_history.Count})";
        }
        else
        {
            _back.Header = _history.Count > 0 ? $"Back ({_history.Count})" : "Back";
        }
    }

    /// <summary>
    /// Graphically sets up the forward MenuItem.
    /// </summary>
    private void SetupForward()
    {
        _forward.IsEnabled = _future.Count - _forwardCount > 0;

        if (Settings.PlayerIsWhite is false || Settings.PlayerIsBlack is false)
        {
            _forward.Header = $"Forward ({_forwardCount}/{_future.Count})";
        }
        else
        {
            _forward.Header = _future.Count > 0 ? $"Forward ({_future.Count})" : "Forward";
        }
    }
    
    /// <summary>
    /// According to the square status, it generates and selects possible moves.
    /// </summary>
    /// <param name="coords">Square coords</param>
    private void SelectPossibleMoves(Coords coords)
    {
        var piece = _calculatedCondition.PiecesOnTurn[coords];
        
        _selectedCoords = new Coords[piece.PossibleMoves.Length + 1];
        _selectedCoords[^1] = coords;
        
        SelectSquare(coords, false, true);
        
        for (var i = 0; i < piece.PossibleMoves.Length; i++)
        {
            _selectedCoords[i] = piece.PossibleMoves[i];
            SelectSquare(piece.PossibleMoves[i]);
        }
    }
    
    /// <summary>
    /// Graphically selects a square.
    /// </summary>
    private void SelectSquare(Coords coords, bool ai = false, bool main = false)
    {
        var uie = (_buttons[coords.Row, coords.Column].Content as Grid)!.Children[1];

        (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.RemoveRange(0, 2);

        if (ai is false)
        {
            (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.Add(main is false
                ? GenerateSelectedSquare(Brushes.Purple)
                : GenerateSelectedSquare(Brushes.Maroon));
        }
        else
        {
            (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.Add(GenerateSelectedSquare(Brushes.Green));
        }
            
        (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.Add(uie);
    }
    
    /// <summary>
    /// Graphically deselects a square.
    /// </summary>
    private void DeselectSquare(Coords coords)
    {
        var uie = (_buttons[coords.Row, coords.Column].Content as Grid)!.Children[1];

        (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.RemoveRange(0, 2);
        (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.Add(new UIElement());
        (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.Add(uie);
    }
    
    /// <summary>
    /// Graphically deselects squares during the Emulator turn.
    /// </summary>
    private void DeselectAiCoords()
    {
        if (_selectedAiCoords is null)
        {
            return;
        }
        
        foreach (Coords selectedAiCoords in _selectedAiCoords)
        {
            DeselectSquare(selectedAiCoords);
        }
        
        _selectedAiCoords = null;
    }
    
    /// <summary>
    /// Load condition from file after method call.
    /// </summary>
    private void LoadingUnsuccessful()
    {
        if (PlayerOnTurn() is false)
        {
            Task.Run(() => _turnTask.Wait())
                .ContinueWith(_ => EmulatorTurn(), TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
    
    /// <summary>
    /// Generates rectangle that highlights the selected square.
    /// </summary>
    private static Rectangle GenerateSelectedSquare(Brush color) => new()
    {
        Fill = color,
        Opacity = 0.3,
        Stretch = Stretch.UniformToFill,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        IsHitTestVisible = false
    };

    /// <summary>
    /// Generates piece image.
    /// </summary>
    /// <param name="pc">Based on square it generates the respective image of the piece</param>
    /// <param name="forWrapPanel">Whether to generate the image for the wrap panel of lost pieces</param>
    /// <returns></returns>
    private Image GeneratePieceImage(PieceChar pc, bool forWrapPanel = false)
    {
        var image = new Image
        {
            IsHitTestVisible = false,
            Tag = pc
        };
        
        image.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);

        if (forWrapPanel)
        {
            image.Height = ImageSizeWrapPanel;
        }

        image.Source = ImageLoader.GetImage(pc.Status, pc.White);
        return image;
    }

    /// <summary>
    /// Generates text block with parameters for inserting into wrap panel.
    /// </summary>
    private static TextBlock GenerateNoLostPiecesTextBlock() => new()
    {
        FontSize = 13,
        FontWeight = FontWeights.Normal,
        Padding = new Thickness(3),
        Text = "No piece has been lost."
    };

    /// <summary>
    /// Executes half turn generated by the Emulator.
    /// </summary>
    private void EmulatorTurn()
    {
        if (PlayerOnTurn())
        {
            return;
        }

        _timerUpdateUi.Start();
        ProgressValueStatic++;
        _dt = DateTime.Now;

        _turnTask = Task.Run(() =>
            {
                var difficulty = _condition.WhiteOnTurn ? Settings.WhiteDifficulty : Settings.BlackDifficulty;
                var halfTurn = _emulator.BestHalfTurn(_calculatedCondition, _condition, difficulty);

                _timerUpdateUi.Stop();
                TimerUpdateUi_Tick(null, null);

                CalculatedConditionData.ClearTrash();

                var calculationTime = DateTime.Now - _dt;
                var turn = Emulator.InterruptHalfTurn ? null : halfTurn;

                if (halfTurn is null)
                {
                    return turn;
                }

                var wait = (int)(TimeSpan.FromSeconds(Settings.CalculationMinTime) - calculationTime)
                    .TotalMilliseconds;

                if (wait > 0)
                {
                    Task.Delay(wait).Wait();
                }

                return turn;
            })
            .ContinueWith(task =>
                {
                    // Task run after the turn calculation is complete.
                    ProgressValueStatic = 0;
                    ProgressValue = 0;

                    var halfTurn = task.Result;

                    if (halfTurn is null && Settings.NewGame is false)
                    {
                        return;
                    }

                    TurnLength = Math.Round((DateTime.Now - _dt).TotalSeconds, 1);
                    DeselectAiCoords();

                    if (halfTurn is null)
                    {
                        return;
                    }

                    MovePiece(halfTurn.From, halfTurn.To, true);
                    DrawCalculatedSituation();

                    _selectedAiCoords = new Coords[2];
                    _selectedAiCoords[0] = halfTurn.From;

                    SelectSquare(halfTurn.From, true);

                    _selectedAiCoords[1] = halfTurn.To;

                    SelectSquare(halfTurn.To, true);
                },
                TaskScheduler.FromCurrentSynchronizationContext());
    }

    /// <summary>
    /// Refreshing UI during turn calculation.
    /// </summary>
    private void TimerUpdateUi_Tick(object? sender, EventArgs? e)
    {
        if (Math.Abs(ProgressValueStatic - _progressValue) > 0.0)
        {
            ProgressValue = ProgressValueStatic;
        }

        if (Math.Abs(ProgressMaximumStatic - _progressMaximum) > 0.0)
        {
            ProgressMaximum = ProgressMaximumStatic;
        }
    }
}
