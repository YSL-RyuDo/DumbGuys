using UnityEngine;
using UnityEngine.Splines;

public class PipeSplineTrigger : MonoBehaviour
{
    public SplineContainer spline;
    public float travelTime = 2f;
    public Transform playerModel;  // 반드시 실제 외형의 Transform
    private Transform playerRoot;
    private Rigidbody playerRb;
    private float t = 0f;
    private bool moving = false;

    private Vector3 originalScale;
    private Vector3 pipeScale = new Vector3(0.7f, 0.7f, 0.7f);

    private void OnTriggerEnter(Collider other)
    {
        if (moving) return; // 중복 트리거 방지

        if (other.CompareTag("Player"))
        {
            playerRoot = other.transform;
            playerRb = playerRoot.GetComponent<Rigidbody>();

            // 실제로 눈에 보이는 모델의 스케일 저장
            originalScale = playerModel.localScale;

            // 축소
            playerModel.localScale = pipeScale;

            // 물리 잠금
            playerRb.isKinematic = true;

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

            // 원래 크기 복구
            playerModel.localScale = originalScale;

            // 물리 복구
            playerRb.isKinematic = false;

            return;
        }

        Vector3 pos = spline.EvaluatePosition(t);
        Vector3 tangent = spline.EvaluateTangent(t);

        playerRoot.position = pos;
        playerRoot.rotation = Quaternion.LookRotation(tangent);
    }
}
