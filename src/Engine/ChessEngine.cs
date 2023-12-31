﻿using Engine.AI;
using Engine.Conditions;
using Engine.IO;
using Engine.Pieces.Types;
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
    
    // PieceColor lost pieces.
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
    private readonly Func<bool, Status> _openPromotionOptions;
    
    // Logically and memory inexpensive recorded condition.
    private Conditions.Condition _condition = null!;
    
    // Calculated condition, including pieces and their possible moves.
    private CalculatedCondition _calculatedCondition = null!;

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
        new(50_000);
    
    public ChessEngine(
        Button[,] buttons,
        TextBlock textBlockOnTurn,
        TextBlock textBlockStatus,
        MenuItem back,
        MenuItem forward,
        WrapPanel whiteLost,
        WrapPanel blackLost,
        Grid progressBar,
        Func<bool, Status> openPromotionOptions)
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
        var status = condition.Chessboard[from.Row, from.Column].Status;
        
        // Taking pieces and promotions.
        // Checking whether piece has taken a pawn en passant.
        if (status is Status.Pawn)
        {
            // If the pawn moves, draw50 is invalid.
            draw50 = false;
            status = condition.Chessboard[to.Row, to.Column].Status;
            
            if (status is Status.EnPassant)
            {
                var row = to.Row is 2 ? to.Row + 1 : to.Row - 1;
                condition.Chessboard[row, to.Column].Status = Status.Empty;
            }
        }
        
        // Removing previous taking en passant.
        for (var i = 0; i < 8; i++)
        {
            status = condition.Chessboard[2, i].Status;
            
            if (status is Status.EnPassant)
            {
                condition.Chessboard[2, i].Status = Status.Empty;
                break;
            }

            status = condition.Chessboard[5, i].Status;

            if (status is not Status.EnPassant)
            {
                continue;
            }
            
            condition.Chessboard[5, i].Status = Status.Empty;
            break;
        }

        switch (condition.Chessboard[from.Row, from.Column].Status)
        {
            // Checking new taking en passant.
            case Status.Pawn:
            {
                var move = from.Row - to.Row;
                
                // If the pawn moved 2 squares.
                if (Math.Abs(move) is 2)
                {
                    // If the movement is negative, a white pawn was moved.
                    if (move < 0)
                    {
                        // Creating selection for taking en passant.
                        condition.Chessboard[from.Row + 1, from.Column].Status = Status.EnPassant;
                        
                        // Setting pawn color to square for taking en passant.
                        condition.Chessboard[from.Row + 1, from.Column].PieceColor =
                            condition.Chessboard[from.Row, from.Column].PieceColor;
                    }
                    // If positive, a black pawn was moved.
                    else 
                    {
                        // Creating selection for taking en passant.
                        condition.Chessboard[from.Row - 1, from.Column].Status = Status.EnPassant;
                        
                        // Setting pawn color to square for taking en passant.
                        condition.Chessboard[from.Row - 1, from.Column].PieceColor =
                            condition.Chessboard[from.Row, from.Column].PieceColor;
                    }
                }

                break;
            }
            // Castling.
            case Status.Rook when from.Row is 0:
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
            case Status.Rook:
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
            case Status.King:
            {
                var color = condition.Chessboard[from.Row, from.Column].PieceColor;
                
                // Castling.
                if (color is PieceColor.White)
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
                        MovePiece(
                            new Coords(from.Row, from.Column - 4),
                            new Coords(from.Row, from.Column - 1), condition);
                    }
                    else
                    {
                        // Right.
                        MovePiece(
                            new Coords(from.Row, from.Column + 3),
                            new Coords(from.Row, from.Column + 1), condition);
                    }
                    
                    // Rook movement caused swapping white on turn.
                    condition.WhiteOnTurn = !condition.WhiteOnTurn;
                }

                break;
            }
        }

        status = condition.Chessboard[to.Row, to.Column].Status;
        
        if (status is not Status.Empty and not Status.EnPassant)
        {
            draw50 = false;
        }

        // Moving piece to new position.
        condition.Chessboard[to.Row, to.Column] = condition.Chessboard[from.Row, from.Column];
        
        // Removing piece from old position.
        condition.Chessboard[from.Row, from.Column].Status = Status.Empty;
        
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
            status = condition.Chessboard[to.Row, to.Column].Status;
            
            if (status is Status.Pawn)
            {
                if (to.Row is 7 or 0)
                {
                    condition.Chessboard[to.Row, to.Column].Status = Status.Queen;
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
        Settings.IsNewGame = false;
        Emulator.InterruptHalfTurn = true;

        Task
            .Run(() => _turnTask.Wait())
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
                    foreach (var coords in _selectedCoords)
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

        var status = _condition.Chessboard[coords.Row, coords.Column].Status;
        var color = _condition.Chessboard[coords.Row, coords.Column].PieceColor is PieceColor.White;

        if (_selectedCoords is null)
        {
            // If the square contains a piece of player that is on turn...
            if (status is Status.Empty or Status.EnPassant || color != _condition.WhiteOnTurn)
            {
                return;
            }

            DeselectAiCoords();
            SelectPossibleMoves(coords);
        }
        else
        {
            // Graphically selected squares are unselected.
            foreach (var coordsPossibleMove in _selectedCoords)
            {
                DeselectSquare(coordsPossibleMove);
            }
            
            // Whether selected square was clicked.
            var selectedClicked = false;
            bool coordsEquals;
            
            // Length - 1, because the last coord depends on piece position.
            for (var i = 0; i < _selectedCoords.Length - 1; i++)
            {
                coordsEquals = _selectedCoords[i].Equals(coords);
                
                // Selected button clicked?
                if (coordsEquals is false)
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
                coordsEquals = _selectedCoords[^1].Equals(coords);
                
                // If different piece was clicked, its square is selected.
                if (coordsEquals is false)
                {
                    // Necessary to null after condition (selectedCoords is used in the condition).
                    _selectedCoords = null;

                    status = _condition.Chessboard[coords.Row, coords.Column].Status;
                    color = _condition.Chessboard[coords.Row, coords.Column].PieceColor is PieceColor.White;
                    
                    // If the square contains piece of player that is on turn...
                    if (status is not Status.Empty and not Status.EnPassant && color == _condition.WhiteOnTurn)
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

        Task
            .Run(() => _turnTask.Wait())
            .ContinueWith(_ =>
            {
                // Deselects selected squares.
                DeselectAiCoords();

                if (_selectedCoords is not null)
                {
                    foreach (var c in _selectedCoords)
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
                        var whiteCurrentCount = _condition.Chessboard
                            .Cast<PieceId>()
                            .Where(id => id.Status is not Status.Empty and not Status.EnPassant)
                            .Count(id => id.PieceColor is PieceColor.White);

                        // Calculating white and black pieces in the previous condition.
                        var whitePreviousCount = previousSituation.Chessboard
                            .Cast<PieceId>()
                            .Where(id => id.Status is not Status.Empty and not Status.EnPassant)
                            .Count(id => id.PieceColor is PieceColor.White);

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

        Task
            .Run(() => _turnTask.Wait())
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
                    var futureCondition = _future.Pop();

                    // Returning taken piece.
                    if (_condition.ToString().Length > futureCondition.ToString().Length)
                    {
                        // PieceColor pieces in current check condition.
                        List<PieceId> whiteCurrent = new();

                        // Black pieces in current check condition.
                        List<PieceId> blackCurrent = new();

                        // PieceColor pieces in future check condition.
                        List<PieceId> whiteFuture = new();

                        // Black pieces in future check condition.
                        List<PieceId> blackFuture = new();

                        // Loading white and black pieces in current condition.
                        foreach (var id in _condition.Chessboard)
                        {
                            if (id.Status is Status.Empty or Status.EnPassant)
                            {
                                continue;
                            }

                            if (id.PieceColor is PieceColor.White)
                            {
                                whiteCurrent.Add(id);
                            }
                            else
                            {
                                blackCurrent.Add(id);
                            }
                        }

                        // Loading white and black pieces in previous condition.
                        foreach (var id in futureCondition.Chessboard)
                        {
                            if (id.Status is Status.Empty or Status.EnPassant)
                            {
                                continue;
                            }

                            if (id.PieceColor is PieceColor.White)
                            {
                                whiteFuture.Add(id);
                            }
                            else
                            {
                                blackFuture.Add(id);
                            }
                        }

                        WrapPanel taken;
                        var takenPc = new PieceId(Status.Empty, PieceColor.None);

                        // Finding taken piece.
                        if (whiteCurrent.Count > whiteFuture.Count)
                        {
                            taken = _whiteLost;

                            foreach (var id in whiteCurrent)
                            {
                                if (whiteFuture.Contains(id))
                                {
                                    whiteFuture.Remove(id);
                                }
                                else
                                {
                                    takenPc = id;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            taken = _blackLost;

                            foreach (var id in blackCurrent)
                            {
                                if (blackFuture.Contains(id))
                                    blackFuture.Remove(id);
                                else
                                {
                                    takenPc = id;
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
                    _condition = futureCondition;
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

        var fileDialog = new SaveFileDialog
        {
            Filter = "Text files|*.txt",
            FileName = name
        };

        if (fileDialog.ShowDialog() is true)
        {
            // Loading taken pieces into logical representation.
            var blackTakenPiece = _blackLost.Children
                .Cast<UIElement>()
                .TakeWhile(e => e is not TextBlock)
                .Select(e => (PieceId)(e as Image)!.Tag).ToList();

            var whiteTakenList = _whiteLost.Children
                .Cast<UIElement>()
                .TakeWhile(e => e is not TextBlock)
                .Select(e => (PieceId)(e as Image)!.Tag).ToList();

            try
            {
                File file = new(_condition, blackTakenPiece, whiteTakenList, _history);
                file.Save(fileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Saving failure", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        // If player is not on turn, the Emulator executes turn after saving.
        if (PlayerOnTurn() is false)
        {
            Task
                .Run(() => _turnTask.Wait())
                .ContinueWith(_ => EmulatorTurn(), TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
    
    /// <summary>
    /// Load chess condition from file.
    /// </summary>
    public void LoadFromFile()
    {
        // Pausing game and preparing it for resetting. Deselect selected squares.
        DeselectAiCoords();
        
        if (_selectedCoords is not null)
        {
            foreach (var selectedCoords in _selectedCoords)
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
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Loading failure", MessageBoxButton.OK, MessageBoxImage.Error);
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
            foreach (var id in file.BlackTaken)
            {
                _blackLost.Children.Add(GeneratePieceImage(id, true));
            }
        }
        
        _whiteLost.Children.Clear();
        
        if (file.WhiteTaken.Count is 0)
        {
            _whiteLost.Children.Add(GenerateNoLostPiecesTextBlock());
        }
        else
        {
            foreach (var id in file.WhiteTaken)
            {
                _whiteLost.Children.Add(GeneratePieceImage(id, true));
            }
        }

        _condition = file.Condition;
        _calculatedCondition = new CalculatedCondition(_condition);
        _history.Clear();
        _history = file.History;
        
        Settings.WhiteIsPlayer = true;
        Settings.BlackIsPlayer = true;
        
        DrawCalculatedSituation();
    }
    
    /// <summary>
    /// After the chess engine is loaded, settings need to be checked again.
    /// </summary>
    public void LoadedChessUserControl()
    {
        if (Settings.IsNewGame)
        {
            NewGame();
        }
        else if (PlayerOnTurn() is false)
        {
            DrawCalculatedSituation();
            
            Task
                .Run(() => _turnTask.Wait())
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
    
    // Moving piece with graphical representation.
    // If the promotion is false, the user won't be able to choose any promotion option.
    private void MovePiece(Coords from, Coords to, bool automaticPromotion = false)
    {
        _future.Clear();
        _history.Push(new Conditions.Condition(_condition));
        
        var toStatus = _condition.Chessboard[to.Row, to.Column].Status;

        // Deselection of taken piece.
        // Piece moves to a square with another piece, therefore attacking it.
        if (toStatus is not Status.Empty)
        {
            var fromStatus = _condition.Chessboard[from.Row, from.Column].Status;
            
            // It isn't en passant, or it is but a pawn is taken by a pawn.
            if (toStatus is not Status.EnPassant || fromStatus is Status.Pawn)
            {
                var color = _condition.Chessboard[to.Row, to.Column].PieceColor;
                
                // Graphical representation of taken piece.
                if (color is PieceColor.White)
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
            toStatus = _condition.Chessboard[to.Row, to.Column].Status;
            
            if (toStatus is Status.Pawn)
            {
                if (to.Row is 7 or 0)
                {
                    var color = _condition.Chessboard[to.Row, to.Column].PieceColor is PieceColor.White;
                    var options = _openPromotionOptions(color);
                    
                    _condition.Chessboard[to.Row, to.Column].Status = options;
                }
            }
        }

        _calculatedCondition = new CalculatedCondition(_condition);
        EmulatorTurn();
    }

    // Check player turn.
    private bool PlayerOnTurn() =>
        _condition.WhiteOnTurn && Settings.WhiteIsPlayer || _condition.WhiteOnTurn is false && Settings.BlackIsPlayer;
    
    // Draws condition of chessboard.
    private void DrawCalculatedSituation()
    {
        for (var row = 0; row < 8; row++)
        {
            for (var column = 0; column < 8; column++)
            {
                // Free square.
                if (_condition.Chessboard[row, column].Status is Status.Empty or Status.EnPassant)
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
            foreach (var coords in _selectedAiCoords)
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

        if (Settings.WhiteIsPlayer && Settings.BlackIsPlayer)
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
    
    // Graphically sets up the back MenuItem.
    private void SetupBack()
    {
        _back.IsEnabled = _history.Count - _backCount > 0;

        if (Settings.WhiteIsPlayer is false || Settings.BlackIsPlayer is false)
        {
            _back.Header = $"Back ({_backCount}/{_history.Count})";
        }
        else
        {
            _back.Header = _history.Count > 0 ? $"Back ({_history.Count})" : "Back";
        }
    }
    
    // Graphically sets up the forward MenuItem.
    private void SetupForward()
    {
        _forward.IsEnabled = _future.Count - _forwardCount > 0;

        if (Settings.WhiteIsPlayer is false || Settings.BlackIsPlayer is false)
        {
            _forward.Header = $"Forward ({_forwardCount}/{_future.Count})";
        }
        else
        {
            _forward.Header = _future.Count > 0 ? $"Forward ({_future.Count})" : "Forward";
        }
    }
    
    // According to the square status, it generates and selects possible moves.
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
    
    // Graphically selects a square.
    private void SelectSquare(Coords coords, bool ai = false, bool main = false)
    {
        var uiElement = (_buttons[coords.Row, coords.Column].Content as Grid)!.Children[1];

        (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.RemoveRange(0, 2);

        (_buttons[coords.Row, coords.Column].Content as Grid)!.Children
            .Add(ai is false
                ? GenerateSelectedSquare(main is false ? Brushes.SeaGreen : Brushes.DarkGreen)
                : GenerateSelectedSquare(Brushes.Green));

        (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.Add(uiElement);
    }
    
    // Graphically deselects a square.
    private void DeselectSquare(Coords coords)
    {
        var uiElement = (_buttons[coords.Row, coords.Column].Content as Grid)!.Children[1];

        (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.RemoveRange(0, 2);
        (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.Add(new UIElement());
        (_buttons[coords.Row, coords.Column].Content as Grid)!.Children.Add(uiElement);
    }
    
    // Graphically deselects squares during the Emulator turn.
    private void DeselectAiCoords()
    {
        if (_selectedAiCoords is null)
        {
            return;
        }
        
        foreach (var selectedAiCoords in _selectedAiCoords)
        {
            DeselectSquare(selectedAiCoords);
        }
        
        _selectedAiCoords = null;
    }
    
    // Load condition from file after method call.
    private void LoadingUnsuccessful()
    {
        if (PlayerOnTurn() is false)
        {
            Task
                .Run(() => _turnTask.Wait())
                .ContinueWith(_ => EmulatorTurn(), TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
    
    // Generates rectangle that highlights the selected square.
    private static Rectangle GenerateSelectedSquare(Brush color) => new()
    {
        Fill = color,
        Opacity = 0.3,
        Stretch = Stretch.UniformToFill,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        IsHitTestVisible = false
    };

    // Generates piece image.
    private Image GeneratePieceImage(PieceId id, bool forWrapPanel = false)
    {
        var image = new Image
        {
            IsHitTestVisible = false,
            Tag = id
        };
        
        image.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);

        if (forWrapPanel)
        {
            image.Height = ImageSizeWrapPanel;
        }

        image.Source = ImageLoader.GenerateImage(id.Status, id.PieceColor);
        return image;
    }

    // Generates text block with parameters for inserting into wrap panel.
    private static TextBlock GenerateNoLostPiecesTextBlock() => new()
    {
        FontSize = 13,
        FontWeight = FontWeights.Normal,
        Padding = new Thickness(3),
        Text = "No piece has been lost."
    };
    
    // Executes half turn generated by the Emulator.
    private void EmulatorTurn()
    {
        if (PlayerOnTurn())
        {
            return;
        }

        _timerUpdateUi.Start();
        ProgressValueStatic++;
        _dt = DateTime.Now;

        _turnTask = Task
            .Run(() =>
            {
                var difficulty = _condition.WhiteOnTurn ? Settings.WhiteDifficulty : Settings.BlackDifficulty;
                var halfTurn = Emulator.FindBestHalfTurn(_calculatedCondition, _condition, difficulty);

                _timerUpdateUi.Stop();
                TimerUpdateUi_Tick(null, null);

                CalculatedConditionData.ClearTrash();

                var calculationTime = DateTime.Now - _dt;
                var turn = Emulator.InterruptHalfTurn ? null : halfTurn;

                if (halfTurn is null)
                {
                    return turn;
                }

                var wait = (int) (TimeSpan.FromSeconds(Settings.CalculationMinTime) - calculationTime)
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

                if (halfTurn is null && Settings.IsNewGame is false)
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
    
    // Refreshing UI during turn calculation.
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
