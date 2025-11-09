using UnityEngine;

[CreateAssetMenu(menuName = "Config/MovementConfig")]
public class MovementConfig : ScriptableObject
{
    public float baseMoveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float baseJumpForce = 7f;
}
