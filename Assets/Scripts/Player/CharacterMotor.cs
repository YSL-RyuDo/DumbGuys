using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMotor : MonoBehaviour
{
    private Rigidbody rb;
    [HideInInspector] public MovementConfig config;

    private float currentMoveSpeed;
    private float currentJumpForce;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.drag = 0.5f;
        rb.angularDrag = 9999f;

        if (config != null)
        {
            currentMoveSpeed = config.baseMoveSpeed;
            currentJumpForce = config.baseJumpForce;
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentMoveSpeed = config.baseMoveSpeed * multiplier;
    }

    public void SetJumpMultiplier(float multiplier)
    {
        currentJumpForce = config.baseJumpForce * multiplier;
    }


    public void Move(Vector3 direction)
    {
        Vector3 targetVelocity = direction * currentMoveSpeed;
        Vector3 currentVelocity = rb.velocity;

        Vector3 velocityXZ = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        Vector3 targetXZ = new Vector3(targetVelocity.x, 0, targetVelocity.z);

        Vector3 newXZ = Vector3.Lerp(velocityXZ, targetXZ, Time.deltaTime * 10f);
        rb.velocity = new Vector3(newXZ.x, currentVelocity.y, newXZ.z);
    }

    public void Rotate(Vector3 lookDirection)
    {
        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * config.rotationSpeed);
        }
    }

    public void Jump()
    {
        if (IsGrounded())
            rb.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
