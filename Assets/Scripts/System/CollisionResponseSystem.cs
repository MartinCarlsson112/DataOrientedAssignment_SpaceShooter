using Unity.Entities;
public class CollisionResponseSystem : SystemBase
{
    EntityCommandBufferSystem Barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    protected override void OnUpdate()
    {
        var em = World.EntityManager;
        var commandBuffer =  Barrier.CreateCommandBuffer().ToConcurrent();

        Dependency = Entities.ForEach((Entity entity, int entityInQueryIndex, ref DynamicBuffer<CollisionResult> collisions, in Player player) => {
            for(int i = 0; i<  collisions.Length; i++)
            {

            }
        }).ScheduleParallel(Dependency);
        Barrier.AddJobHandleForProducer(Dependency);

        Dependency.Complete();

        Dependency = Entities.ForEach((Entity entity, int entityInQueryIndex, ref DynamicBuffer<CollisionResult> collisions, in Faction faction, in Bullet bullet) =>
        {
            for (int i = 0; i < collisions.Length; i++)
            {
                if (em.HasComponent<Faction>(collisions[i].other))
                {
                    var otherFaction = em.GetComponentData<Faction>(collisions[i].other);
                    if (otherFaction.value != faction.value)
                    {
                        if(em.HasComponent<Damageable>(collisions[i].other))
                        {
                            commandBuffer.AppendToBuffer<DamageEvent>(entityInQueryIndex, collisions[i].other, new DamageEvent() { damage = bullet.damage });
                        }
                        commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                    }
                }

            }
        }).WithNativeDisableParallelForRestriction(em).ScheduleParallel(Dependency);
        Barrier.AddJobHandleForProducer(Dependency);
        Dependency.Complete();

        Dependency = Entities.ForEach((Entity entity, int entityInQueryIndex, ref DynamicBuffer<CollisionResult> collisions, in Enemy enemy) =>
        {
            for (int i = 0; i < collisions.Length; i++)
            {


            }
        }).ScheduleParallel(Dependency);
        Barrier.AddJobHandleForProducer(Dependency);
    }
}