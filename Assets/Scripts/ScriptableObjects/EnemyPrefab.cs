using UnityEngine;

[System.Serializable]
public struct EnemyPrefabData
{
    public float speed;
    public Mesh mesh;
    public Material material;
    public float sightRange;
    public float health;
    public float attackRange;
    public float leeway;
}

[CreateAssetMenu(fileName = "EnemyPrefab", menuName = "ScriptableObjects/EnemyPrefab")]
public class EnemyPrefab : ScriptableObject
{
    public EnemyPrefabData data;
}