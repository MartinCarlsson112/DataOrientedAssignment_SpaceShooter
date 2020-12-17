using Unity.Entities;
using Unity.Collections;
public class CollisionResponseSystem : SystemBase
{
    EntityCommandBufferSystem Barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    EntityQuery query;
    EntityQuery damageableQuery;

    protected override void OnCreate()
    {
        var desc = new EntityQueryDesc { All = new ComponentType[] { typeof(Faction) } };
        var damageableDesc = new EntityQueryDesc { All = new ComponentType[] { typeof(Damageable) } };
        query = GetEntityQuery(desc);
        damageableQuery = GetEntityQuery(damageableDesc);
    }

    protected override void OnUpdate()
    {
        var commandBuffer =  Barrier.CreateCommandBuffer().ToConcurrent();
        var entities = query.ToEntityArray(Allocator.TempJob);
        var damageableEntities = damageableQuery.ToEntityArray(Allocator.TempJob);
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
                if(entities.Contains(collisions[i].other))
                {
                    var otherFaction = GetComponent<Faction>(collisions[i].other);
                    if (otherFaction.value != faction.value)
                    {
                        if (damageableEntities.Contains(collisions[i].other))
                        {
                            commandBuffer.AppendToBuffer<DamageEvent>(entityInQueryIndex, collisions[i].other, new DamageEvent() { damage = bullet.damage });
                        }
                        commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                    }
                }
            }
        }).WithNativeDisableParallelForRestriction(entities).ScheduleParallel(Dependency);
        Barrier.AddJobHandleForProducer(Dependency);
        Dependency.Complete();

        Dependency = Entities.ForEach((Entity entity, int entityInQueryIndex, ref DynamicBuffer<CollisionResult> collisions, in Enemy enemy) =>
        {
            for (int i = 0; i < collisions.Length; i++)
            {


            }
        }).ScheduleParallel(Dependency);
        Barrier.AddJobHandleForProducer(Dependency);

        entities.Dispose();
        damageableEntities.Dispose();
    }
}