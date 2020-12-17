using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;



public struct EnemySettingsComponent : IComponentData
{
    public BlobAssetReference<EnemySettingsBlobAsset> enemySettings;
}

public struct EnemySettingsBlobAsset
{
    public BlobArray<EnemyPrefabData> array;
}

[CreateAssetMenu(fileName = "EnemySettings", menuName = "ScriptableObjects/EnemySettings")]
public class EnemySettings : ScriptableObject
{
    public List<EnemyPrefab> enemyPrefabs;
};
