using Unity.Entities;

[GenerateAuthoringComponent]
public struct Cooldown : IComponentData
{
    public float cd;
    public float accu;
}
