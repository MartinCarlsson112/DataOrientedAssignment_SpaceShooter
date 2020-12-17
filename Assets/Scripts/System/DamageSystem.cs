using Unity.Entities;


public class DamageSystem : SystemBase
{
    EntityCommandBufferSystem Barrier => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

    protected override void OnUpdate()
    {
        var commandBuffer = Barrier.CreateCommandBuffer().ToConcurrent();
        Dependency = Entities.ForEach((ref DynamicBuffer<DamageEvent> damageEvents, ref Health health) =>
        {
            for(int i = 0; i < damageEvents.Length; i++)
            {
                health.current -= damageEvents[i].damage;
            }
        }).ScheduleParallel(Dependency);

        Dependency.Complete();


        Dependency = Entities.ForEach((ref DynamicBuffer<DamageEvent> damageEvents) =>
        {
            damageEvents.Clear();
        }).ScheduleParallel(Dependency);

        Dependency.Complete();
        Dependency = Entities.ForEach((Entity entity, int entityInQueryIndex, in Health health) =>
        {
            if(health.current < 0)
            {
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }
        }).ScheduleParallel(Dependency);
        Barrier.AddJobHandleForProducer(Dependency);

        Dependency.Complete();
    }
}

