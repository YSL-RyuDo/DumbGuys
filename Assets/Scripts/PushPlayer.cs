using UnityEngine;

namespace ithappy
{
    public class PushPlayer : MonoBehaviour
    {
        [Header("Push Settings")]
        public float pushForce = 7f; // 밀치는 힘

        void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                Rigidbody playerRb = collision.collider.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    // 공 → 플레이어 방향 계산
                    Vector3 dir = (collision.transform.position - transform.position).normalized;

                    // 밀어내기
                    playerRb.AddForce(dir * pushForce, ForceMode.Impulse);
                }
            }
        }
    }
}
