using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.AddressableAssets;
using Unity.Mathematics;
using UnityEngine.ResourceManagement.AsyncOperations;

//use this to get enemy settings anywhere in the project
//int enemySettingsExist = enemySettingsEntityQuery.CalculateEntityCount();
//if (!(enemySettingsExist > 0))
//{
//    return;
//}
////var enemySettings = enemySettingsEntityQuery.ToComponentDataArray<BulletPrefabListComponent>(Allocator.TempJob);

public class EnemySpawnSystem : SystemBase
{
    EntityArchetype enemyArchetype;
    bool enemyPrefabsLoaded = false;
    bool enemyWaveSettingsLoaded = false;
    EntityQuery enemySettingsEntityQuery;
    EntityQuery enemyEntityQuery;

    EnemyWaveSettings waveSettings;

    float timeToNextWave;
    bool waitForAllDead = false;
    int waveCounter = -1;

    protected override void OnCreate()
    {
        enemyArchetype = EntityManager.CreateArchetype(
                         typeof(LocalToWorld),
                         typeof(Translation),
                         typeof(Rotation),
                         typeof(Velocity),
                         typeof(Faction),
                         typeof(Health),
                         typeof(Cooldown),
                         typeof(ColliderComponent),
                         typeof(Speed),
                         typeof(RenderBounds),
                         typeof(RenderMesh),
                         typeof(Enemy));
        Addressables.LoadAssetAsync<EnemySettings>("EnemyPrefabs").Completed += OnEnemyPrefabListLoadComplete;
        Addressables.LoadAssetAsync<EnemyWaveSettings>("EnemyWaveSettings").Completed += OnEnemyWaveSettingsLoadComplete;

        var query = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(EnemySettingsComponent) }
        };
        enemySettingsEntityQuery = GetEntityQuery(query);

        var enemyQuery = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Enemy) }
        };
        enemyEntityQuery = GetEntityQuery(enemyQuery);
    }

    void OnEnemyWaveSettingsLoadComplete(AsyncOperationHandle<EnemyWaveSettings> obj)
    {
        waveSettings = obj.Result;
        timeToNextWave = obj.Result.initialSpawnDelay;
        waveCounter = 0;
        enemyWaveSettingsLoaded = true;
    }

    void OnEnemyPrefabListLoadComplete(AsyncOperationHandle<EnemySettings> obj)
    {
        using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
        {
            ref EnemySettingsBlobAsset blobPrefabList = ref blobBuilder.ConstructRoot<EnemySettingsBlobAsset>();
            BlobBuilderArray<EnemyPrefabData> blobArray = blobBuilder.Allocate(ref blobPrefabList.array, obj.Result.enemyPrefabs.Count);
            for (int i = 0; i < obj.Result.enemyPrefabs.Count; i++)
            {
                blobArray[i] = obj.Result.enemyPrefabs[i].data;
            }

            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponent<EnemySettingsComponent>(entity);
            EntityManager.SetComponentData(entity, new EnemySettingsComponent { enemySettings = blobBuilder.CreateBlobAssetReference<EnemySettingsBlobAsset>(Allocator.Persistent) });
        };
        enemyPrefabsLoaded = true;
    }
    void SpawnEnemy(EnemyPrefabData prefab)
    {
        var enemyEntity = EntityManager.CreateEntity(enemyArchetype);
        EntityManager.SetComponentData(enemyEntity, new Translation() { Value = new float3(UnityEngine.Random.Range(-25, 25), UnityEngine.Random.Range(5, 15), 0)});
        EntityManager.SetComponentData(enemyEntity, new Velocity() { value = new float2(0, 0) });
        EntityManager.SetComponentData(enemyEntity, new Speed() { value = prefab.speed});
        EntityManager.SetComponentData(enemyEntity, new RenderBounds() { Value = new Unity.Mathematics.AABB() { Center = float3.zero, Extents = new float3(1, 1, 1) } });
        EntityManager.SetComponentData(enemyEntity, new Faction() { value = FactionUtil.ENEMY_FACTION });   
        EntityManager.SetComponentData(enemyEntity, ColliderHelper.MakeBoxCollider(new Unity.Mathematics.float3(3.5f, 1.0f, 0.5f), false));
        EntityManager.SetComponentData(enemyEntity, new Health() { current = prefab.health, max = prefab.health });
        EntityManager.SetComponentData(enemyEntity, new Enemy() { sightRange = prefab.sightRange, attackRange = prefab.attackRange, leeWay = prefab.leeway, bulletPrefab = prefab.bulletPrefabIndex });
        EntityManager.SetComponentData(enemyEntity, new Cooldown() { accu = 0, cd = prefab.shootCd });
        EntityManager.AddBuffer<CollisionResult>(enemyEntity);
        EntityManager.AddBuffer<DamageEvent>(enemyEntity);
        EntityManager.AddComponent<Damageable>(enemyEntity);
        EntityManager.SetSharedComponentData(enemyEntity, new RenderMesh() { mesh = prefab.mesh, material = prefab.material });
    }

    protected override void OnUpdate()
    {
        if(!enemyPrefabsLoaded || !enemyWaveSettingsLoaded)
        {
            return;
        }

        if(waveCounter >= waveSettings.waves.Count)
        {
            //wave spawning finished
            return;
        }

        var entityCount = enemyEntityQuery.CalculateEntityCount();
        if(entityCount == 0 || !waitForAllDead)
        {
            timeToNextWave -= Time.DeltaTime;
            if(timeToNextWave <= 0)
            {
                var enemiesToSpawn = waveSettings.waves[waveCounter].enemiesToSpawn;
                for (int i = 0; i < enemiesToSpawn.Count; i++)
                {
                    SpawnEnemy(enemiesToSpawn[i].data);
                }
                timeToNextWave = waveSettings.waves[waveCounter].timeUntilNextWave;
                waitForAllDead = waveSettings.waves[waveCounter].waitForAllDead;
                waveCounter++;
            }
        }
    }

    protected override void OnDestroy()
    {
        var enemySettingsComponent = enemySettingsEntityQuery.ToComponentDataArray<EnemySettingsComponent>(Allocator.Temp);
        enemySettingsComponent[0].enemySettings.Dispose();
        enemySettingsComponent.Dispose();
    }
}