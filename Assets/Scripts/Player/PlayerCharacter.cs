using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private MovementConfig movementConfig;
    [SerializeField] private CharacterMotor motor;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    public IInputProvider InputProvider { get; private set; }
    public CharacterMotor Motor => motor;
    public IMovementStrategy CurrentStrategy { get; set; }
    public Transform CameraTransform => cameraTransform;

    private CharacterState currentState;

    void Awake()
    {
        InputProvider = new PlayerInputProvider();
        motor.config = movementConfig;
        CurrentStrategy = new NormalMovement();
        ChangeState(new IdleState(this));
    }

    void Update()
    {
        currentState?.HandleInput();
        currentState?.Update();

        UpdateAnimator();
    }

    public void ChangeState(CharacterState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public Vector3 GetCameraRelativeMove(Vector2 moveAxis)
    {
        Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;
        return (camForward * moveAxis.y + camRight * moveAxis.x).normalized;
    }

    private void UpdateAnimator()
    {
        // 이동 입력값
        float hor = Input.GetAxisRaw("Horizontal");
        float vert = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Hor", hor);
        animator.SetFloat("Vert", vert);

        // 땅에 닿아있는지 여부만 처리
        animator.SetBool("IsGrounded", motor.IsGrounded());
        // 점프 애니메이션은 JumpState에서 SetJumping()으로 관리
    }

    public void SetJumping(bool isJumping)
    {
        animator.SetBool("IsJump", isJumping);
    }
}
