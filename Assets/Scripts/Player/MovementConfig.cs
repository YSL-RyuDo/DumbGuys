using UnityEngine;

[CreateAssetMenu(menuName = "Config/MovementConfig")]
public class MovementConfig : ScriptableObject
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 7f;
}
