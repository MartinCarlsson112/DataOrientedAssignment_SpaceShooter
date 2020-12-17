using UnityEngine;

[System.Serializable]
public struct BulletPrefabData
{
    public float lifeTime;
    public float speed;
    public float damage;
    public Mesh mesh;
    public Material material;
}

[CreateAssetMenu(fileName = "BulletPrefab", menuName = "ScriptableObjects/BulletPrefab", order = 5)]
public class BulletPrefab : ScriptableObject
{
    public BulletPrefabData data;
}