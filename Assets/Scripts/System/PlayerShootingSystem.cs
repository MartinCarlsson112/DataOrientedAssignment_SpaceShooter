using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine.AddressableAssets;
using Unity.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.Burst;



public class PlayerShootingSystem : SystemBase
{
    PlayerInputActions playerInput;
    PlayerInputActions.PlayerActions pInput;
    EntityArchetype bulletArchetype;

    EntityQuery bulletPrefabListQuery;
    EntityCommandBufferSystem Barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    bool readyToStart = false;

    protected override void OnCreate()
    {
        playerInput = new PlayerInputActions();
        playerInput.Enable();
        pInput = playerInput.Player;

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
        Addressables.LoadAssetAsync<BulletPrefabList>("BulletPrefabList").Completed += OnBulletPrefabListLoadDone;
        var query = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(BulletPrefabListComponent) }
        };
        bulletPrefabListQuery = GetEntityQuery(query);
    }

    private void OnBulletPrefabListLoadDone(AsyncOperationHandle<BulletPrefabList> obj)
    {
        using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
        {
            ref BulletPrefabListBlobAsset blobPrefabList = ref blobBuilder.ConstructRoot<BulletPrefabListBlobAsset>();
            BlobBuilderArray<BulletPrefabData> blobArray = blobBuilder.Allocate(ref blobPrefabList.array, obj.Result.bulletPrefabs.Count);
            for(int i = 0; i < obj.Result.bulletPrefabs.Count; i++)
            {
                blobArray[i] = obj.Result.bulletPrefabs[i].data;
            }

            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponent<BulletPrefabListComponent>(entity);
            EntityManager.SetComponentData<BulletPrefabListComponent>(entity, new BulletPrefabListComponent { bulletPrefabListReference = blobBuilder.CreateBlobAssetReference<BulletPrefabListBlobAsset>(Allocator.Persistent) });
        };
        readyToStart = true;
    }


    [BurstCompile]
    static void SpawnBullet(ref EntityCommandBuffer.Concurrent commandBuffer, EntityArchetype archetype, int entityInQueryIndex, in float3 pos, in quaternion rot, in BulletPrefabData prefab)
    {
        var newBullet = commandBuffer.CreateEntity(entityInQueryIndex, archetype);
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Translation() { Value = pos });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Lifetime { accu = 0, time = prefab.lifeTime });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Velocity() { value = math.cross(math.forward(rot), new float3(1, 0, 0)).xy });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Speed() { value = prefab.speed });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Scale() { Value = 0.2f });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new RenderBounds() { Value = new Unity.Mathematics.AABB() { Center = float3.zero, Extents = new float3(1, 1, 1) } });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Faction() { value = FactionUtil.PLAYER_FACTION });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, new Bullet() { damage = prefab.damage });
        commandBuffer.SetSharedComponent(entityInQueryIndex, newBullet, new RenderMesh() { mesh = prefab.mesh, material = prefab.material });
        commandBuffer.SetComponent(entityInQueryIndex, newBullet, ColliderHelper.MakeBoxCollider(new Unity.Mathematics.float3(0.2f, 0.2f, 0.2f), false));
     
        commandBuffer.AddBuffer<CollisionResult>(entityInQueryIndex, newBullet);
    }

    protected override void OnUpdate()
    {
        if (!readyToStart)
        {
            return;
        }

        int bulletPrefabListExists = bulletPrefabListQuery.CalculateEntityCount();
        if (!(bulletPrefabListExists > 0))
        {
            return;
        }
        var bulletPrefabList = bulletPrefabListQuery.ToComponentDataArray<BulletPrefabListComponent>(Allocator.TempJob);
        
        var archetype = bulletArchetype;
        var shootButton = pInput.Shoot.ReadValue<float>();
        var commandBuffer = Barrier.CreateCommandBuffer().ToConcurrent();
        Dependency = Entities.WithoutBurst().ForEach((Entity entity, int entityInQueryIndex, ref Cooldown cd, in Translation pos, in Player player, in Rotation rotation, in Faction faction) =>
        {
            if (shootButton > 0 && cd.accu >= cd.cd)
            {
                ref var bulletPrefab = ref bulletPrefabList[0].bulletPrefabListReference.Value;
                SpawnBullet(ref commandBuffer, archetype, entityInQueryIndex, pos.Value + new float3(-1.0f, -0.5f, 0), rotation.Value, bulletPrefab.array[player.bulletPrefab]);
                SpawnBullet(ref commandBuffer, archetype, entityInQueryIndex, pos.Value + new float3(1.0f, -0.5f, 0), rotation.Value, bulletPrefab.array[player.bulletPrefab]);
                cd.accu = 0;
            }
        }).ScheduleParallel(Dependency);
        Barrier.AddJobHandleForProducer(Dependency);
        Dependency.Complete();
        bulletPrefabList.Dispose();
    }

    protected override void OnDestroy()
    {
        var bulletPrefabList = bulletPrefabListQuery.ToComponentDataArray<BulletPrefabListComponent>(Allocator.Temp);
        bulletPrefabList[0].bulletPrefabListReference.Dispose();
        bulletPrefabList.Dispose();
    }
}