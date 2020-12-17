using Unity.Entities;

public struct Lifetime : IComponentData
{
    public float time;
    public float accu;
}