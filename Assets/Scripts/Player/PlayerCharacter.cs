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

    [Header("push item")]
    [SerializeField] private float pushRange = 3f;
    [SerializeField] private float pushAngleDot = 0.6f;

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

    private bool isInputLocked = true;
    public bool IsInputLocked => isInputLocked;

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
        bool isChatting = GameUIManagerExistsAndChatting();
        bool isTutorial = Tutorial0Manager.Instance != null;

        if ((isChatting && !isTutorial) || isInputLocked)
        {
            animator.SetFloat("Hor", 0);
            animator.SetFloat("Vert", 0);
            motor.Move(Vector3.zero);
        }
        else
        {
            currentState?.HandleInput();
            currentState?.Update();
            UpdateAnimator();

            if (Input.GetMouseButtonDown(0))
                TryAttack();
        }

        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            SendMovePacket();
        }
    }


    private bool GameUIManagerExistsAndChatting()
    {
        var obj = FindFirstObjectByType<GameUIManager>();
        if (obj == null)
            return false;

        // GameUIManager.IsChatting이 static이라면 그냥 바로 반환 가능
        return GameUIManager.IsChatting;
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
            Vector3 toTarget = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, toTarget);
            if (dot < 0.5f)
                continue;

            var enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                Debug.Log($"[TutorialAttack] {hit.name} took {attackDamage} damage!");
                continue; // 튜토리얼 적이면 여기서 끝
            }
            if (hit.CompareTag("Enemy"))
            {
                var nickname = ClientSession.Instance.Nickname;
                var attackReq = new AttackRequest
                {
                    attacker = nickname,
                    target = hit.name,
                    damage = attackDamage
                };

                Debug.Log($"[Network] Sending attack packet → {attackReq.attacker} -> {attackReq.target}, dmg={attackReq.damage}");
                NetworkManager.Instance.SendJson(attackReq);
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

        var tutorial = Tutorial0Manager.Instance;
        if (tutorial != null)
            tutorial.UpdateTutorialHP(this);


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

    public void OnItemPicked(ItemPickup.ItemType itemType)
    {
        Debug.Log($"[Item] Picked up: {itemType}");

        // UI 매니저에 전달만 (즉시 적용 X)
        var ui = FindFirstObjectByType<GameUIManager>();
        if (ui != null)
            ui.OnItemPicked(itemType);
    }

    public void UseItem(ItemPickup.ItemType itemType)
    {
        Debug.Log($"[Item] Using item: {itemType}");

        switch (itemType)
        {
            case ItemPickup.ItemType.SpeedUp:
                StartCoroutine(ApplySpeedBoost());
                break;

            case ItemPickup.ItemType.AttackUp:
                StartCoroutine(ApplyAttackBoost());
                break;

            case ItemPickup.ItemType.Heal:
                Heal(30);
                break;

            case ItemPickup.ItemType.Push:
                TryPushItem();
                break;

            case ItemPickup.ItemType.JumpUp:
                StartCoroutine(ApplyJumpBoost());
                break;
        }
    }


    private IEnumerator ApplySpeedBoost()
    {
        Debug.Log("[Item] Speed Up activated!");
        motor.SetSpeedMultiplier(1.5f);
        yield return new WaitForSeconds(5f);
        motor.SetSpeedMultiplier(1f);
        Debug.Log("[Item] Speed Up ended.");
    }

    private IEnumerator ApplyAttackBoost()
    {
        Debug.Log("[Item] Attack Up activated!");
        attackDamage += 10;
        yield return new WaitForSeconds(5f);
        attackDamage -= 10;
        Debug.Log("[Item] Attack Up ended.");
    }

    private void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"[Item] Healed! HP = {currentHealth}/{maxHealth}");

        var tutorial = Tutorial0Manager.Instance;
        if (tutorial != null)
            tutorial.UpdateTutorialHP(this);
    }

    private void TryPushItem()
    {
        Debug.Log("[Item] Push use requested.");

        Collider[] hits = Physics.OverlapSphere(transform.position, pushRange);
        Debug.Log($"[Item] Push detected {hits.Length} targets.");

        bool isTutorial = FindFirstObjectByType<Tutorial0Manager>() != null;
        bool hitSomething = false;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy"))
                continue;

            // 튜토리얼 모드 → 모든 적에게 넉백
            if (isTutorial)
            {
                var enemyHealth = hit.GetComponentInParent<EnemyHealth>();
                var enemyRb = hit.GetComponentInParent<Rigidbody>();

                if (enemyHealth != null && enemyRb != null)
                {
                    Vector3 knockDir = (hit.transform.position - transform.position).normalized;
                    knockDir.y = 0f;

                    enemyRb.AddForce(knockDir * 10f, ForceMode.Impulse);
                    Debug.Log($"[TutorialPush] Knockback applied to {hit.name}");
                    hitSomething = true;
                }
            }
            else
            {
                // 일반 모드 → 각도/정면 판정 + 서버 전송
                Vector3 toTarget = (hit.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, toTarget);
                if (dot < pushAngleDot) continue;

                var req = new PushRequest
                {
                    cmd = "push",
                    attacker = ClientSession.Instance.Nickname,
                    target = hit.name
                };

                NetworkManager.Instance.SendJson(req);
                Debug.Log($"[Network] Sending push request → {req.attacker} -> {req.target}");
                hitSomething = true;
            }
        }

        if (!hitSomething)
            Debug.Log("[Item] Push no valid target.");
    }


    public void ApplyPushKnockback(Vector3 dir, float force)
    {
        if (isDead) return;

        var rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        dir.y = 0f;
        dir.Normalize();

        rb.AddForce(dir * force, ForceMode.Impulse);
        Debug.Log($"[Push] Knockback applied. dir={dir}, force={force}");
    }


    private IEnumerator ApplyJumpBoost()
    {
        Debug.Log("[Item] Jump Up activated!");
        motor.SetJumpMultiplier(1.5f);
        yield return new WaitForSeconds(5f);
        motor.SetJumpMultiplier(1f);
        Debug.Log("[Item] Jump Up ended.");
    }

    public void SetInputLocked(bool locked)
    {
        isInputLocked = locked;

        // 즉시 멈추게 하려면 속도도 0으로
        if (locked)
        {
            motor.Move(Vector3.zero);
            animator.SetFloat("Hor", 0);
            animator.SetFloat("Vert", 0);
        }
    }
}
