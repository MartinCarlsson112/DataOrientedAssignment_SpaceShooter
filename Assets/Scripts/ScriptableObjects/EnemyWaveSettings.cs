using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Wave
{
    public List<EnemyPrefab> enemiesToSpawn;
    public bool waitForAllDead;
    public float timeUntilNextWave;
}
[CreateAssetMenu(fileName = "EnemyWaveSettings", menuName = "ScriptableObjects/EnemyWaveSettings")]
public class EnemyWaveSettings : ScriptableObject
{
    public int initialSpawnDelay;
    public List<Wave> waves;
}