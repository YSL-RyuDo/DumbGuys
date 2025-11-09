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
    private Vector3 targetEnemyPos;
    private Quaternion targetEnemyRot;
    [SerializeField] private float lerpSpeed = 12f;

    private void Start()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived += HandleServerMessage;

        RequestSpawnInfo();

        // 처음엔 비활성화
        if (scorePanel != null)
            scorePanel.SetActive(false);

        targetEnemyPos = enemyAppearance.transform.position;
        targetEnemyRot = enemyAppearance.transform.rotation;
    }


    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    private void Update()
    {
        if (enemyAppearance != null)
        {
            enemyAppearance.transform.position =
                Vector3.Lerp(enemyAppearance.transform.position, targetEnemyPos, Time.deltaTime * lerpSpeed);

            enemyAppearance.transform.rotation =
                Quaternion.Slerp(enemyAppearance.transform.rotation, targetEnemyRot, Time.deltaTime * lerpSpeed);
        }
    }


    // === 1. 스폰 요청 ===
    private void RequestSpawnInfo()
    {
        string nickname = ClientSession.Instance.Nickname;
        var req = new SpawnInfoRequest { nickname = nickname };
        NetworkManager.Instance.SendJson(req);
        Debug.Log($"[GameScene] Requested spawn info for {nickname}");
    }

    // === 2. 서버 응답 ===
    private void HandleServerMessage(string json)
    {
        if (json.Contains("\"type\":\"spawn_info\""))
        {
            if (spawnApplied)
            {
                Debug.LogWarning("[GameScene] Duplicate spawn_info ignored.");
                return;
            }

            var data = JsonUtility.FromJson<SpawnInfoResponse>(json);
            spawnApplied = true;
            enemyNickname = data.enemyNickname;

            Debug.Log($"[GameScene] Spawn Info: {data.playerNickname}={data.playerPosition}");
            ApplySpawnPosition(data);
        }
        else if (json.Contains("\"type\":\"appearance_info\""))
        {
            var data = JsonUtility.FromJson<AppearanceResponse>(json);

            if (data.nickname == ClientSession.Instance.Nickname)
            {
                playerAppearance.ApplyAppearance(data);
            }
            else if (data.nickname == enemyNickname)
            {
                enemyAppearance.ApplyAppearance(data);
            }
            //player.SetInputLocked(false);

        }
        else if (json.Contains("\"type\":\"enemy_move\""))
        {
            var move = JsonUtility.FromJson<EnemyMovePacket>(json);

            // 타깃 갱신
            targetEnemyPos = new Vector3(move.px, move.py, move.pz);
            targetEnemyRot = Quaternion.Euler(move.rx, move.ry, move.rz);

            // (선택) 애니메이션 파라미터 업데이트
            var enemyAnimator = enemyAppearance.GetComponentInChildren<Animator>();
            if (enemyAnimator != null)
            {
                Vector3 delta = targetEnemyPos - enemyAppearance.transform.position;
                Vector3 localDelta = enemyAppearance.transform.InverseTransformDirection(delta);
                enemyAnimator.SetFloat("Hor", Mathf.Clamp(localDelta.x * 10f, -1f, 1f));
                enemyAnimator.SetFloat("Vert", Mathf.Clamp(localDelta.z * 10f, -1f, 1f));
                enemyAnimator.SetBool("IsGrounded", true);
                enemyAnimator.SetBool("IsJump", Mathf.Abs(delta.y) > 0.05f);
            }
        }
        else if (json.Contains("\"type\":\"player_hit\""))
        {
            var data = JsonUtility.FromJson<PlayerHitPacket>(json);
            Debug.Log($"[GameScene] Player hit → {data.attacker} attacked {data.target} for {data.damage} damage.");

            // 공격자와 피격자 닉네임 확인
            string myNickname = ClientSession.Instance.Nickname;

            // 피격 대상 오브젝트 찾기
            CharacterAppearance hitTarget = null;
            if (data.target == myNickname)
                hitTarget = playerAppearance;
            else if (data.target == enemyNickname)
                hitTarget = enemyAppearance;

            if (hitTarget != null)
            {
                Debug.Log($"[GameScene] Applying hit animation to {hitTarget.name} ({data.target})");

                // 피격 애니메이션 트리거
                var animator = hitTarget.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger("Hit");
                }

                // 본인이 맞은 경우에는 체력 처리도 수행
                if (data.target == myNickname)
                {
                    var player = FindFirstObjectByType<PlayerCharacter>();
                    if (player != null)
                    {
                        player.TakeDamage(data.damage);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[GameScene] Could not find target {data.target} in scene.");
            }
        }
        else if (json.Contains("\"type\":\"enemy_attack_anim\""))
        {
            var data = JsonUtility.FromJson<EnemyAttackAnimPacket>(json);
            Debug.Log($"[GameScene] Enemy {data.attacker} triggered attack animation.");

            // 태그가 "Enemy"인 모든 오브젝트를 탐색
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (var enemy in enemies)
            {
                // 닉네임 일치하는 적 찾기
                if (enemy.name == data.attacker)
                {
                    var animator = enemy.GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Attack");
                        Debug.Log($"[GameScene] Triggered Attack animation on enemy '{enemy.name}'");
                    }
                    else
                    {
                        Debug.LogWarning($"[GameScene] Enemy '{enemy.name}' has no Animator component.");
                    }
                    return; // 해당 적 찾았으면 종료
                }
            }

            Debug.LogWarning($"[GameScene] Could not find enemy with name '{data.attacker}' among tagged Enemies.");
        }
        else if (json.Contains("\"type\":\"enemy_die_anim\""))
        {
            var data = JsonUtility.FromJson<EnemyDieAnimPacket>(json);
            Debug.Log($"[GameScene] Enemy {data.attacker} died → playing death animation.");

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                if (enemy.name == data.attacker)
                {
                    var animator = enemy.GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Die");
                        Debug.Log($"[GameScene] Played 'Die' animation on enemy '{enemy.name}'.");
                    }
                }
            }
        }
        else if (json.Contains("\"type\":\"score_update\""))
        {
            var data = JsonUtility.FromJson<ScoreUpdatePacket>(json);

            playerNameText.text = data.player1;
            enemyNameText.text = data.player2;
            scoreText.text = $"{data.score1} : {data.score2}";

            playerScore = data.score1;
            enemyScore = data.score2;

            if (scorePanel != null)
                scorePanel.SetActive(true);

            var player = FindFirstObjectByType<PlayerCharacter>();
            if (player != null)
            {
                player.SetInputLocked(true);
                Debug.Log("[GameScene] Player input locked due to score screen.");
            }

            Debug.Log($"[GameScene] Score Updated → {playerScore}:{enemyScore}");
        }
        else if (json.Contains("\"type\":\"round_end\""))
        {
            Debug.Log("[GameScene] Round ended → returning to ChooseScene in 5 seconds...");
            StartCoroutine(DelayedSceneTransition("ChooseScene", 5f));
        }
        else if (json.Contains("\"type\":\"game_end\""))
        {
            var data = JsonUtility.FromJson<GameEndPacket>(json);
            Debug.Log($"[GameScene] Game ended! Winner: {data.winner}");
            StartCoroutine(DelayedSceneTransition("ResultScene", 5f));
        }
    }

    private IEnumerator DelayedSceneTransition(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    // === 3. 스폰 위치 적용 ===
    private void ApplySpawnPosition(SpawnInfoResponse data)
    {
        Transform playerSpawn, enemySpawn;

        if (data.playerPosition == "A")
        {
            playerSpawn = playerSpawnA;
            enemySpawn = playerSpawnB;
            playerSpawnPos = "A";
        }
        else
        {
            playerSpawn = playerSpawnB;
            enemySpawn = playerSpawnA;
            playerSpawnPos = "B";
        }

        // === 위치 배치 ===
        playerAppearance.transform.position = playerSpawn.position;
        enemyAppearance.transform.position = enemySpawn.position;

        playerAppearance.transform.LookAt(enemyAppearance.transform);
        enemyAppearance.transform.LookAt(playerAppearance.transform);

        // === 태그 및 이름 지정 ===
        string myNickname = data.playerNickname;
        string enemyNick = data.enemyNickname;

        playerAppearance.tag = "Player";
        playerAppearance.name = myNickname;

        enemyAppearance.tag = "Enemy";
        enemyAppearance.name = enemyNick;

        Debug.Log($"[GameScene] Tag/Name Assigned → Player: {playerAppearance.name} (Tag={playerAppearance.tag}), Enemy: {enemyAppearance.name} (Tag={enemyAppearance.tag})");

        // === UI 적용 ===
        var ui = FindFirstObjectByType<GameUIManager>();
        if (ui != null)
        {
            ui.SetPlayerName(myNickname);
            ui.SetEnemyName(enemyNick);
            Debug.Log($"[UI] Player: {myNickname}, Enemy: {enemyNick} 이름 적용 완료");
        }

        // === 외형 요청 ===
        StartCoroutine(RequestAppearancesDelayed(enemyNick));
    }

    private IEnumerator RequestAppearancesDelayed(string enemyNick)
    {
        yield return new WaitForSeconds(0.2f);

        string myNickname = ClientSession.Instance.Nickname;

        var reqPlayer = new AppearanceRequest { nickname = myNickname };
        NetworkManager.Instance.SendJson(reqPlayer);
        Debug.Log($"[GameScene] Requested appearance for player: {myNickname}");

        yield return new WaitForSeconds(0.1f);

        var reqEnemy = new AppearanceRequest { nickname = enemyNick };
        NetworkManager.Instance.SendJson(reqEnemy);
        Debug.Log($"[GameScene] Requested appearance for enemy: {enemyNick}");
    }

    public void RespawnCharacter(CharacterAppearance character)
    {
        if (character == playerAppearance)
        {
            Vector3 respawnPos = playerSpawnPos == "A" ? playerSpawnA.position : playerSpawnB.position;
            playerAppearance.transform.position = respawnPos;
            playerAppearance.GetComponent<Rigidbody>().velocity = Vector3.zero;

            Debug.Log($"[GameScene] Player respawned at {playerSpawnPos}");
        }
        else if (character == enemyAppearance)
        {
            Vector3 respawnPos = playerSpawnPos == "A" ? playerSpawnB.position : playerSpawnA.position;
            enemyAppearance.transform.position = respawnPos;
            enemyAppearance.GetComponent<Rigidbody>().velocity = Vector3.zero;

            Debug.Log($"[GameScene] Enemy respawned opposite to player ({playerSpawnPos})");
        }
    }
}
