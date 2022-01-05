using Engine.Conditions;
using Engine.UtilityComponents;

namespace Engine.Pieces.Base;

internal interface IPiece
{
    bool White { get; }
        
    Coords[] PossibleMoves { get; }
        
    Coords[] PossibleAttacks { get; }
        
    void UpdatePossibleMoves(Condition condition, bool check, Coords coords);
        
    void UpdatePossibleAttacks(Condition condition, Coords coords);
}
