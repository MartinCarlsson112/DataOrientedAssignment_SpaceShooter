using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Unity.Rendering;

public class PlayerMovementSystem : SystemBase
{
    PlayerInputActions PlayerInput;
    PlayerInputActions.PlayerActions pInput;

    EntityArchetype playerArchetype;

    void SpawnPlayer(PlayerSettings settings)
    {
        var entity = EntityManager.CreateEntity(playerArchetype);
        EntityManager.SetComponentData(entity, new Translation { Value = new float3(0, 0, 0) });
        EntityManager.SetComponentData(entity, new Velocity { value = Unity.Mathematics.float2.zero });
        EntityManager.SetComponentData(entity, new Speed { value = settings.speed });
        EntityManager.SetComponentData(entity, new Faction() { value = FactionUtil.PLAYER_FACTION });
        EntityManager.SetComponentData(entity, ColliderHelper.MakeBoxCollider(new Unity.Mathematics.float3(settings.colliderSize.x, settings.colliderSize.y, settings.colliderSize.z), false));
        EntityManager.SetComponentData(entity, new Health() { current = settings.health, max = settings.health });
        EntityManager.SetComponentData(entity, new Cooldown() { cd = settings.cd });
        EntityManager.SetComponentData(entity, new Player() { bulletPrefab = 0 });
        EntityManager.SetSharedComponentData(entity, new RenderMesh() { mesh = settings.mesh, material = settings.material });

        EntityManager.AddBuffer<CollisionResult>(entity);
        EntityManager.AddBuffer<DamageEvent>(entity);
    }

    protected override void OnCreate()
    {
        playerArchetype = EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(Speed),
            typeof(Velocity),
            typeof(ColliderComponent),
            typeof(Faction),
            typeof(Damageable),
            typeof(Health),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld),
            typeof(Player),
            typeof(Cooldown));
        Addressables.LoadAssetAsync<PlayerSettings>("PlayerSettings").Completed += OnPlayerSettingsLoad;

        PlayerInput = new PlayerInputActions();
        PlayerInput.Enable();
        pInput = PlayerInput.Player;
    }

    private void OnPlayerSettingsLoad(AsyncOperationHandle<PlayerSettings> obj)
    {
        SpawnPlayer(obj.Result);
    }

    protected override void OnUpdate()
    {
        float horizontalInput = pInput.MoveHorizontal.ReadValue<float>();
        float verticalInput = pInput.MoveVertical.ReadValue<float>();
        Dependency =  Entities.ForEach((ref Velocity velocity, in Player player) => {
            float2 value = float2.zero;
            value.x += horizontalInput;
            value.y += verticalInput;
            velocity.value = value;
        }).ScheduleParallel(Dependency);


    }
}