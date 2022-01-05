using Engine.Conditions;
using Engine.UtilityComponents;

namespace Engine.Pieces.Base;

/// <summary>
/// Piece and its possible moves and attacks.
/// </summary>
internal abstract class Piece: IPiece
{
    /// <summary>
    /// Piece color
    /// </summary>
    public bool White { get; }
    
    /// <summary>
    /// Indicates whether the piece protects king from check
    /// </summary>
    public bool ProtectingKing { get; set; }
    
    /// <summary>
    /// Possible moves in given condition.
    /// All possible moves are moves that can be executed in the current condition if the player is on turn.
    /// Also squares with enemies on them belong here.
    /// </summary>
    public Coords[] PossibleMoves { get; protected set; } = null!;

    /// <summary>
    /// Possible attacks in given condition.
    /// This method is called for pieces that aren't on turn.
    /// Into possible attacks belong even friendly pieces, because if enemy takes the friendly piece,
    /// he will appear on possibly attacked position.
    /// This method is mainly for the king who cannot enter any square of possible attack.
    /// </summary>
    public Coords[] PossibleAttacks { get; protected set; } = null!;

    protected Piece(bool white)
    {
        White = white;
    }
    
    /// <summary>
    /// Updates possible moves based on check condition.
    /// </summary>
    public abstract void UpdatePossibleMoves(Condition condition, bool check, Coords coords);
    
    /// <summary>
    /// Updates possible attacks based on check condition.
    /// </summary>
    public abstract void UpdatePossibleAttacks(Condition condition, Coords coords);
}
