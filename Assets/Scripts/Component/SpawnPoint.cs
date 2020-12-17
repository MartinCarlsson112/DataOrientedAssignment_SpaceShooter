using Unity.Entities;

public struct SpawnPointComponent :IComponentData
{
    public int requiredWaveToActivate;
}