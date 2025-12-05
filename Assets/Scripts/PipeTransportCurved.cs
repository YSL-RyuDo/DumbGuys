using UnityEngine;
using UnityEngine.Splines;

public class PipeSplineTrigger : MonoBehaviour
{
    public SplineContainer spline;
    public float travelTime = 2f;

    private Transform player;
    private Rigidbody playerRb;

    private float t = 0f;
    private bool moving = false;

    private Vector3 originalScale;
    private Vector3 pipeScale = new Vector3(0.7f, 0.7f, 0.7f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            playerRb = player.GetComponent<Rigidbody>();

            // 원래 크기 저장
            originalScale = player.localScale;

            // 파이프 들어가는 크기로 축소
            player.localScale = pipeScale;

            // 물리 잠금
            playerRb.isKinematic = true;

            // 이동 시작
            moving = true;
            t = 0f;
        }
    }

    private void Update()
    {
        if (!moving) return;

        t += Time.deltaTime / travelTime;

        if (t >= 1f)
        {
            moving = false;

            // 플레이어 원래 스케일 복구
            player.localScale = originalScale;

            // 물리 복구
            if (playerRb != null)
                playerRb.isKinematic = false;

            return;
        }

        // 스플라인 따라 이동
        Vector3 pos = spline.EvaluatePosition(t);
        Vector3 tangent = spline.EvaluateTangent(t);

        player.position = pos;
        player.rotation = Quaternion.LookRotation(tangent);
    }
}
