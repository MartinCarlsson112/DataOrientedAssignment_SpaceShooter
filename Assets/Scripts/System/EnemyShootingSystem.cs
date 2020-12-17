using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine.AddressableAssets;
using Unity.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.Burst;


public class EnemyShootingSystem : SystemBase
{
    
    EntityArchetype bulletArchetype;

    EntityQuery bulletPrefabListQuery;
    EntityCommandBufferSystem Barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    protected override void OnCreate()
    {
        bulletArchetype = EntityManager.CreateArchetype(
                 typeof(LocalToWorld),
                 typeof(Translation),
                 typeof(Rotation),
                 typeof(Scale),
                 typeof(Velocity),
                 typeof(Speed),
                 typeof(Lifetime),
                 typeof(Faction),
                 typeof(ColliderComponent),
                 typeof(RenderBounds),
                 typeof(RenderMesh),
                 typeof(Bullet));

        var query = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(BulletPrefabListComponent) }
        };
        bulletPrefabListQuery = GetEntityQuery(query);
    }



    [BurstCompile]
    static void SpawnBullet(ref EntityCommandBuffer.Concurrent commandBuffer, int entityInQueryIndex, in EntityArchetype archetype, in float3 pos, in quaternion rot, in BulletPrefabData prefab)
    {
        var newBullet = commandBuffer.CreateEntity(entityInQueryIndex, archetype);
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Translation() { Value = pos });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Lifetime { accu = 0, time = prefab.lifeTime });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Velocity() { value = math.cross(-math.forward(rot), new float3(1, 0, 0)).xy });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Speed() { value = prefab.speed });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Scale() { Value = 0.2f });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new RenderBounds() { Value = new Unity.Mathematics.AABB() { Center = float3.zero, Extents = new float3(1, 1, 1) } });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Faction() { value = FactionUtil.ENEMY_FACTION });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Bullet() { damage = prefab.damage });
        commandBuffer.SetSharedComponent(entityInQueryIndex, newBullet, new RenderMesh() { mesh = prefab.mesh, material = prefab.material });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, ColliderHelper.MakeBoxCollider(new Unity.Mathematics.float3(0.2f, 0.2f, 0.2f), false));

        commandBuffer.AddBuffer<CollisionResult>(entityInQueryIndex, newBullet);
    }

    protected override void OnUpdate()
    {
        int bulletPrefabListExists = bulletPrefabListQuery.CalculateEntityCount();
        if (!(bulletPrefabListExists > 0))
        {
            return;
        }
        var bulletPrefabList = bulletPrefabListQuery.ToComponentDataArray<BulletPrefabListComponent>(Allocator.TempJob);
        var archetype = bulletArchetype;

        var commandBuffer = Barrier.CreateCommandBuffer().ToConcurrent();
        Dependency = Entities.WithoutBurst().ForEach((Entity entity, int entityInQueryIndex, ref Cooldown cd, in Translation pos, in Enemy enemy, in Rotation rotation, in Faction faction) =>
        {
            if (cd.accu >= cd.cd)
            {
                ref var bulletPrefab = ref bulletPrefabList[0].bulletPrefabListReference.Value;
                SpawnBullet(ref commandBuffer, entityInQueryIndex, archetype, pos.Value + new float3(0, 0.5f, 0), rotation.Value, bulletPrefab.array[enemy.bulletPrefab]);
                cd.accu = 0;
            }
        }).ScheduleParallel(Dependency);
        Barrier.AddJobHandleForProducer(Dependency);
        Dependency.Complete();
        bulletPrefabList.Dispose();
    }

    protected override void OnDestroy()
    {

    }
}