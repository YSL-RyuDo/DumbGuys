using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform playerSpawnA;
    [SerializeField] private Transform playerSpawnB;

    [Header("Characters")]
    [SerializeField] private CharacterAppearance playerAppearance;
    [SerializeField] private CharacterAppearance enemyAppearance;

    [Header("UI Elements")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private TMPro.TextMeshProUGUI playerNameText;
    [SerializeField] private TMPro.TextMeshProUGUI enemyNameText;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;

    private bool spawnApplied = false;
    private string enemyNickname = "";
    private string playerSpawnPos = ""; // "A" 또는 "B"
    private int playerScore = 0;
    private int enemyScore = 0;

    // enemy move target
    private Vector3 targetEnemyPos;
    private Quaternion targetEnemyRot;
    private bool hasEnemyTarget = false;
    private bool isEnemyJumping = false;

    [SerializeField] private float lerpSpeed = 12f;

    private Animator cachedEnemyAnimator;
    private Coroutine enemyJumpRoutine;


    private void Start()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived += HandleServerMessage;

        RequestSpawnInfo();

        if (scorePanel != null)
            scorePanel.SetActive(false);

        targetEnemyPos = enemyAppearance.transform.position;
        targetEnemyRot = enemyAppearance.transform.rotation;

        cachedEnemyAnimator = enemyAppearance.GetComponentInChildren<Animator>();
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    private void Update()
    {
        if (!hasEnemyTarget || enemyAppearance == null)
            return;

        Vector3 cur = enemyAppearance.transform.position;
        Vector3 tgt = targetEnemyPos;

        float newX = Mathf.Lerp(cur.x, tgt.x, Time.deltaTime * lerpSpeed);
        float newZ = Mathf.Lerp(cur.z, tgt.z, Time.deltaTime * lerpSpeed);

        float newY = isEnemyJumping ? cur.y : tgt.y;

        enemyAppearance.transform.position = new Vector3(newX, newY, newZ);

        enemyAppearance.transform.rotation = Quaternion.Slerp(
            enemyAppearance.transform.rotation,
            targetEnemyRot,
            Time.deltaTime * lerpSpeed
        );

        UpdateEnemyAnimator();
    }


    // === 스폰 요청 ===
    private void RequestSpawnInfo()
    {
        var req = new SpawnInfoRequest { nickname = ClientSession.Instance.Nickname };
        NetworkManager.Instance.SendJson(req);
    }

    // === 서버 메시지 처리 ===
    private void HandleServerMessage(string json)
    {
        if (json.Contains("\"type\":\"spawn_info\""))
        {
            if (spawnApplied) return;

            var data = JsonUtility.FromJson<SpawnInfoResponse>(json);
            spawnApplied = true;
            enemyNickname = data.enemyNickname;
            ApplySpawnPosition(data);
        }
        else if (json.Contains("\"type\":\"appearance_info\""))
        {
            var data = JsonUtility.FromJson<AppearanceResponse>(json);

            if (data.nickname == ClientSession.Instance.Nickname)
                playerAppearance.ApplyAppearance(data);
            else if (data.nickname == enemyNickname)
                enemyAppearance.ApplyAppearance(data);
        }
        else if (json.Contains("\"type\":\"enemy_move\""))
        {
            // 여기서는 값만 저장
            var move = JsonUtility.FromJson<EnemyMovePacket>(json);
            targetEnemyPos = new Vector3(
                move.px,
                enemyAppearance.transform.position.y,
                move.pz
            );
            targetEnemyRot = Quaternion.Euler(move.rx, move.ry, move.rz);
            hasEnemyTarget = true;
        }
        else if (json.Contains("\"type\":\"player_hit\""))
        {
            var data = JsonUtility.FromJson<PlayerHitPacket>(json);
            HandlePlayerHit(data);
        }
        else if (json.Contains("\"type\":\"enemy_attack_anim\""))
        {
            PlayEnemyTrigger(dataType: "Attack", json);
        }
        else if (json.Contains("\"type\":\"enemy_die_anim\""))
        {
            PlayEnemyTrigger(dataType: "Die", json);
        }
        else if (json.Contains("\"type\":\"score_update\""))
        {
            var data = JsonUtility.FromJson<ScoreUpdatePacket>(json);
            UpdateScoreUI(data);
        }
        else if (json.Contains("\"type\":\"round_end\""))
        {
            StartCoroutine(DelayedSceneTransition("ChooseScene", 5f));
        }
        else if (json.Contains("\"type\":\"game_end\""))
        {
            StartCoroutine(DelayedSceneTransition("ResultScene", 5f));
        }
        else if (json.Contains("\"type\":\"enemy_jump\""))
        {
            var data = JsonUtility.FromJson<EnemyJumpPacket>(json);

            if (enemyJumpRoutine != null)
                StopCoroutine(enemyJumpRoutine);

            isEnemyJumping = true;

            if (cachedEnemyAnimator != null)
                cachedEnemyAnimator.SetBool("IsJump", true);

            enemyJumpRoutine = StartCoroutine(
                VisualEnemyJump(enemyAppearance.transform, data.jumpVelocityY)
            );
        }



    }

    // === 적 애니메이션 업데이트 ===
    private void UpdateEnemyAnimator()
    {
        if (cachedEnemyAnimator == null)
            return;

        Vector3 delta = targetEnemyPos - enemyAppearance.transform.position;
        Vector3 localDelta = enemyAppearance.transform.InverseTransformDirection(delta);

        cachedEnemyAnimator.SetFloat("Hor", Mathf.Clamp(localDelta.x * 10f, -1f, 1f));
        cachedEnemyAnimator.SetFloat("Vert", Mathf.Clamp(localDelta.z * 10f, -1f, 1f));

        //cachedEnemyAnimator.SetBool("IsGrounded", true);
        //cachedEnemyAnimator.SetBool("IsJump", false);
    }


    // === 피격 처리 ===
    private void HandlePlayerHit(PlayerHitPacket data)
    {
        string myNick = ClientSession.Instance.Nickname;

        CharacterAppearance target =
            data.target == myNick ? playerAppearance :
            data.target == enemyNickname ? enemyAppearance : null;

        if (target == null) return;

        var anim = target.GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger("Hit");

        if (data.target == myNick)
        {
            var player = FindFirstObjectByType<PlayerCharacter>();
            if (player != null)
                player.TakeDamage(data.damage);
        }
    }

    // === 적 애니메이션 트리거 ===
    private void PlayEnemyTrigger(string dataType, string json)
    {
        var data = JsonUtility.FromJson<EnemyAttackAnimPacket>(json);

        if (enemyAppearance.name != data.attacker)
            return;

        var anim = enemyAppearance.GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger(dataType);
    }

    // === 점수 UI ===
    private void UpdateScoreUI(ScoreUpdatePacket data)
    {
        playerNameText.text = data.player1;
        enemyNameText.text = data.player2;
        scoreText.text = $"{data.score1} : {data.score2}";

        if (scorePanel != null)
            scorePanel.SetActive(true);

        var player = FindFirstObjectByType<PlayerCharacter>();
        if (player != null)
            player.SetInputLocked(true);
    }

    private IEnumerator DelayedSceneTransition(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    // === 스폰 적용 ===
    private void ApplySpawnPosition(SpawnInfoResponse data)
    {
        Transform pSpawn = data.playerPosition == "A" ? playerSpawnA : playerSpawnB;
        Transform eSpawn = data.playerPosition == "A" ? playerSpawnB : playerSpawnA;

        playerSpawnPos = data.playerPosition;

        playerAppearance.transform.position = pSpawn.position;
        enemyAppearance.transform.position = eSpawn.position;

        playerAppearance.name = data.playerNickname;
        enemyAppearance.name = data.enemyNickname;

        playerAppearance.tag = "Player";
        enemyAppearance.tag = "Enemy";

        StartCoroutine(RequestAppearancesDelayed(data.enemyNickname));
    }

    private IEnumerator RequestAppearancesDelayed(string enemyNick)
    {
        yield return new WaitForSeconds(0.2f);

        NetworkManager.Instance.SendJson(
            new AppearanceRequest { nickname = ClientSession.Instance.Nickname });

        yield return new WaitForSeconds(0.1f);

        NetworkManager.Instance.SendJson(
            new AppearanceRequest { nickname = enemyNick });
    }

    public void RespawnCharacter(CharacterAppearance character)
    {
        Vector3 pos =
            character == playerAppearance
                ? (playerSpawnPos == "A" ? playerSpawnA.position : playerSpawnB.position)
                : (playerSpawnPos == "A" ? playerSpawnB.position : playerSpawnA.position);

        character.transform.position = pos;
        var rb = character.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = Vector3.zero;
    }

    private IEnumerator VisualEnemyJump(Transform target, float velocityY)
    {
        isEnemyJumping = true;

        float gravity = 9.8f;
        float jumpHeight = (velocityY * velocityY) / (2f * gravity);
        float upTime = velocityY / gravity;
        float downTime = upTime;

        float startY = target.position.y;
        float peakY = startY + jumpHeight;

        float t = 0f;

        // 상승
        while (t < upTime)
        {
            t += Time.deltaTime;
            float y = Mathf.Lerp(startY, peakY, t / upTime);

            target.position = new Vector3(
                target.position.x,
                y,
                target.position.z
            );

            yield return null;
        }

        // 하강
        t = 0f;
        while (t < downTime)
        {
            t += Time.deltaTime;
            float y = Mathf.Lerp(peakY, startY, t / downTime);

            target.position = new Vector3(
                target.position.x,
                y,
                target.position.z
            );

            yield return null;
        }

        // 착지
        target.position = new Vector3(
            target.position.x,
            startY,
            target.position.z
        );

        isEnemyJumping = false;

        if (cachedEnemyAnimator != null)
            cachedEnemyAnimator.SetBool("IsJump", false);
    }

}
