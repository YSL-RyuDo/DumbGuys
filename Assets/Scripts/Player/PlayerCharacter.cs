using UnityEngine;
using System.Collections;

public class PlayerCharacter : MonoBehaviour
{
    [Header("congig")]
    [SerializeField] private MovementConfig movementConfig;
    [SerializeField] private CharacterMotor motor;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    [Header("combat")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 0.5f;

    private float lastAttackTime;
    private int currentHealth;
    private bool isDead = false;

    public IInputProvider InputProvider { get; private set; }
    public CharacterMotor Motor => motor;
    public IMovementStrategy CurrentStrategy { get; set; }
    public Transform CameraTransform => cameraTransform;

    private CharacterState currentState;
    //private bool inputLocked = false; // 입력 잠금 상태 추가

    private float sendInterval = 0.1f;
    private float sendTimer = 0f;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;


    void Awake()
    {
        InputProvider = new PlayerInputProvider();
        motor.config = movementConfig;
        CurrentStrategy = new NormalMovement();
        ChangeState(new IdleState(this));
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        //if (inputLocked)
        //{
        //    animator.SetFloat("Hor", 0);
        //    animator.SetFloat("Vert", 0);
        //    //return;
        //}

        currentState?.HandleInput();
        currentState?.Update();

        UpdateAnimator();

        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }

        // === 매 순간 (주기적으로) 패킷 전송 ===
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            SendMovePacket();
        }
    }

    private void TryAttack()
    {
        Debug.Log($"[Attack] TryAttack() called by {name} (isDead={isDead})");

        if (isDead)
        {
            Debug.Log("[Attack] Attack ignored (player is dead).");
            return;
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            Debug.Log($"[Attack] On cooldown. Time since last attack: {Time.time - lastAttackTime:F2}s / Cooldown: {attackCooldown}s");
            return;
        }

        lastAttackTime = Time.time;

        // 애니메이션 트리거
        Debug.Log($"[Attack] {name} triggered attack animation.");
        animator.SetTrigger("Attack");
        var attackAnimReq = new AttackAnimRequest
        {
            attacker = ClientSession.Instance.Nickname
        };

        NetworkManager.Instance.SendJson(attackAnimReq);
        Debug.Log($"[Network] Sent attack animation packet for {attackAnimReq.attacker}");

        // 실제 공격 판정
        PerformAttack();
        StartCoroutine(ResetAttackTrigger());
    }

    private IEnumerator ResetAttackTrigger()
    {
        // 공격 애니메이션이 끝난 뒤에 트리거 초기화
        yield return new WaitForSeconds(0.3f); // 공격 애니메이션 길이에 맞게 조정
        animator.ResetTrigger("Attack");
    }

    private void PerformAttack()
    {
        Debug.Log($"[Attack] {name} performing attack check (range={attackRange})");

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        Debug.Log($"[Attack] {name} detected {hits.Length} colliders in range.");

        foreach (var hit in hits)
        {
            Debug.Log($"[Attack] Checking collider: {hit.name} (tag={hit.tag})");

            // 태그가 "Enemy"인 대상만 공격
            if (hit.CompareTag("Enemy"))
            {
                Vector3 toTarget = (hit.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, toTarget);

                // 정면 60도 이내만 공격
                if (dot > 0.5f)
                {
                    // 클라이언트는 피격 판정하지 않고 서버에 패킷만 보냄
                    var nickname = ClientSession.Instance.Nickname;
                    var attackReq = new AttackRequest
                    {
                        attacker = nickname,
                        target = hit.name,   // Enemy의 이름은 닉네임으로 설정되어 있음
                        damage = attackDamage
                    };

                    Debug.Log($"[Network] Sending attack packet → {attackReq.attacker} -> {attackReq.target}, dmg={attackReq.damage}");
                    NetworkManager.Instance.SendJson(attackReq);
                }
                else
                {
                    Debug.Log($"[Attack] {hit.name} ignored (not in forward arc, dot={dot:F2})");
                }
            }
            else
            {
                Debug.Log($"[Attack] Ignored collider: {hit.name} (tag={hit.tag})");
            }
        }

        Debug.Log($"[Attack] {name} finished PerformAttack()");
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            Debug.Log($"[Damage] {name} ignored damage ({damage}) because already dead.");
            return;
        }

        Debug.Log($"[Damage] {name} taking {damage} damage. (Before HP={currentHealth})");

        currentHealth -= damage;
        Debug.Log($"[Damage] {name} HP after hit = {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"[Damage] {name} HP <= 0 → triggering Die()");
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        motor.Move(Vector3.zero);
        Debug.Log($"{name} is Dead!");
        animator.ResetTrigger("Hit");
        // === 서버에 죽음 알림 패킷 전송 ===
        var deathReq = new DeathAnimRequest
        {
            attacker = ClientSession.Instance.Nickname
        };

        NetworkManager.Instance.SendJson(deathReq);
        Debug.Log($"[Network] Sent death_anim packet for {deathReq.attacker}");

        StartCoroutine(RespawnAfterDelay(3f));
    }


    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // GameSceneController 찾아서 리스폰 요청
        var controller = FindFirstObjectByType<GameSceneController>();
        if (controller != null)
        {
            var appearance = GetComponent<CharacterAppearance>();
            controller.RespawnCharacter(appearance);
        }

        // 체력 회복 및 상태 초기화
        currentHealth = maxHealth;
        isDead = false;
        //SetInputLocked(false);
        gameObject.SetActive(true);

        animator.ResetTrigger("Die");
        animator.SetTrigger("Respawn");

        Debug.Log($"{name} respawned with full HP.");
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
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

    //public void SetInputLocked(bool locked)
    //{
    //    inputLocked = locked;
    //    if (locked)
    //    {
    //        motor.Move(Vector3.zero);
    //        animator.SetFloat("Hor", 0);
    //        animator.SetFloat("Vert", 0);
    //    }
    //}
}
