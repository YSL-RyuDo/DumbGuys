using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMotor : MonoBehaviour
{
    private Rigidbody rb;
    [HideInInspector] public MovementConfig config;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // 회전 튀는 것 방지
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // 공기저항 살짝만 (너무 크면 점프 캔슬됨)
        rb.drag = 0.5f;
        rb.angularDrag = 9999f;
    }

    public void Move(Vector3 direction)
    {
        Vector3 targetVelocity = direction * config.moveSpeed;

        // 현재 속도
        Vector3 currentVelocity = rb.velocity;

        // XZ만 보간 (Y축은 중력/점프 영향 그대로 둠)
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
            rb.AddForce(Vector3.up * config.jumpForce, ForceMode.Impulse);
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
