﻿using Chess.ResourceManagement;
using Chess.View.UserControls;
using Engine.ResourceManagement;
using Engine.ResourceManagement.Types;
using System.Windows.Input;

namespace Chess.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly ChessUserControls _chessUserControl;

    public MainWindow()
    {
        ImageLoader.SetResources(ResourcePaths._icons, ResourcePaths._pieceImages);
        InitializeComponent();
        
        Title = "Chess";
        Icon = ImageLoader.GenerateImage(IconType.Window);

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