using Unity.Entities;


public class LifetimeSystem : SystemBase
{
    EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    protected override void OnUpdate()
    {
        var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
        float dt = Time.DeltaTime;
        
        Dependency = Entities.ForEach((Entity entity, int entityInQueryIndex, ref Lifetime lifetime) => {
            lifetime.accu += dt;
            if(lifetime.accu > lifetime.time)
            {
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }
        }).ScheduleParallel(Dependency);
        barrier.AddJobHandleForProducer(Dependency);
        Dependency.Complete();
    }
}