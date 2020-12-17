using UnityEngine;
using Unity.Entities;

public class ColliderConvert: IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<Player>(entity);
        dstManager.SetComponentData<Player>(entity, new Player { });
    }
};