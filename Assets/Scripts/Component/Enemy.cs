using Unity.Entities;
public struct Enemy : IComponentData
{
    public float sightRange;
    public float attackRange;
    public float leeWay;
}