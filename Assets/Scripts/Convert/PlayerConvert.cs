using UnityEngine;
using Unity.Entities;

public class PlayerConvert : MonoBehaviour, IConvertGameObjectToEntity
{
    public PlayerSettings settings;
    public GameObject bullet;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
  
    }




};