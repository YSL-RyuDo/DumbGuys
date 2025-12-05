using UnityEngine;

namespace ithappy
{
    public class KnockUpOnly : MonoBehaviour
    {
        [Header("Upward Knock Settings")]
        public float upwardForce = 15f;   // 위로 튀어 오르는 힘

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 순수한 위 방향 힘만 적용
                    Vector3 force = Vector3.up * upwardForce;

                    rb.AddForce(force, ForceMode.Impulse);
                }
            }
        }
    }
}
