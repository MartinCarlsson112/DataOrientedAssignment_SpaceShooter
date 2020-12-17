using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;
public struct BulletPrefabListComponent : IComponentData
{
    public BlobAssetReference<BulletPrefabListBlobAsset> bulletPrefabListReference;
}

public struct BulletPrefabListBlobAsset
{
    public BlobArray<BulletPrefabData> array;
}

[CreateAssetMenu(fileName = "BulletPrefabList", menuName = "ScriptableObjects/BulletPrefabList", order = 5)]
public class BulletPrefabList : ScriptableObject
{
    public List<BulletPrefab> bulletPrefabs;
};
