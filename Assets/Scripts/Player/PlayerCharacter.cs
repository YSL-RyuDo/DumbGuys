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
    private bool inputLocked = false; // 입력 잠금 상태 추가

    private float sendInterval = 0.1f;
    private float sendTimer = 0f;

    void Awake()
    {
        InputProvider = new PlayerInputProvider();
        motor.config = movementConfig;
        CurrentStrategy = new NormalMovement();
        ChangeState(new IdleState(this));
    }

    void Update()
    {
        // 입력이 잠금 상태라면 아무것도 하지 않음
        if (inputLocked)
        {
            animator.SetFloat("Hor", 0);
            animator.SetFloat("Vert", 0);
            return;
        }

        currentState?.HandleInput();
        currentState?.Update();

        UpdateAnimator();

        // === 매 순간 (주기적으로) 패킷 전송 ===
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            SendMovePacket();
        }
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
        float hor = Input.GetAxisRaw("Horizontal");
        float vert = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Hor", hor);
        animator.SetFloat("Vert", vert);
        animator.SetBool("IsGrounded", motor.IsGrounded());
    }

    public void SetJumping(bool isJumping)
    {
        animator.SetBool("IsJump", isJumping);
    }

    private void SendMovePacket()
    {
        if (NetworkManager.Instance == null || !ClientSession.Instance.IsLoggedIn)
            return;

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        var packet = new MovePacket
        {
            nickname = ClientSession.Instance.Nickname,
            px = pos.x,
            py = pos.y,
            pz = pos.z,
            rx = rot.eulerAngles.x,
            ry = rot.eulerAngles.y,
            rz = rot.eulerAngles.z
        };

        NetworkManager.Instance.SendJson(packet);
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked)
        {
            motor.Move(Vector3.zero);
            animator.SetFloat("Hor", 0);
            animator.SetFloat("Vert", 0);
        }
    }
}
