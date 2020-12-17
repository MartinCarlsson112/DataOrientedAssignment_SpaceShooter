using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings", order = 5)]
public class PlayerSettings : ScriptableObject
{
    public float speed;

    public Mesh mesh;
    public Material material;

    public float cd;
    public float health;

    public Vector3 colliderSize;

}