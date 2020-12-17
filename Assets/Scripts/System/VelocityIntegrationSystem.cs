using Unity.Entities;
using Unity.Transforms;

public class VelocityIntegrationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;
        Dependency = Entities.ForEach((ref Translation pos, in Velocity velocity, in Speed speed) =>
        {
            pos.Value.xy += velocity.value * dt * speed.value;
        }).ScheduleParallel(Dependency);
    }
}