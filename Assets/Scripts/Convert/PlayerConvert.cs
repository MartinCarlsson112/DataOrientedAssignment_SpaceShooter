using UnityEngine;
using Unity.Entities;

public class PlayerConvert : MonoBehaviour, IConvertGameObjectToEntity
{
    public PlayerSettings settings;
    public GameObject bullet;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<Player>(entity);
        dstManager.AddComponent<Velocity>(entity);
        dstManager.SetComponentData<Velocity>(entity, new Velocity { value = Unity.Mathematics.float2.zero });
        dstManager.AddComponent<Speed>(entity);
        dstManager.SetComponentData<Speed>(entity, new Speed { value = settings.speed});
        dstManager.AddComponent<Faction>(entity);
        dstManager.SetComponentData<Faction>(entity, new Faction() { value = FactionUtil.PLAYER_FACTION });
        dstManager.AddComponent<ColliderComponent>(entity);
        dstManager.SetComponentData(entity, ColliderHelper.MakeBoxCollider(new Unity.Mathematics.float3(0.5f, 0.5f, 0.5f), false));
        dstManager.AddBuffer<CollisionResult>(entity);
    }
};