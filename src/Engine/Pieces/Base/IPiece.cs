using Engine.Conditions;
using Engine.Pieces.Types;
using Engine.UtilityComponents;

namespace Engine.Pieces.Base;

internal interface IPiece
{
    PieceColor PieceColor { get; }
        
    Coords[] PossibleMoves { get; }
        
    Coords[] PossibleAttacks { get; }
        
    void UpdatePossibleMoves(Condition condition, bool check, Coords coords);
        
    void UpdatePossibleAttacks(Condition condition, Coords coords);
}
