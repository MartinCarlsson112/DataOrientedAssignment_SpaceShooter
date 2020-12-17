using Unity.Entities;
public struct FactionUtil
{
    public static readonly int PLAYER_FACTION = 0;
    public static readonly int ENEMY_FACTION = 1;
}

public struct Faction : IComponentData
{

    public int value;
}