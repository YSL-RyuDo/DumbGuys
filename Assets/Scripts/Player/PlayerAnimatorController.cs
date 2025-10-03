using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    void Update()
    {
        // Unity의 Input 시스템 (기본 Horizontal/Vertical 축 이용)
        float hor = Input.GetAxisRaw("Horizontal");  // A(-1) ~ D(1)
        float vert = Input.GetAxisRaw("Vertical");  // S(-1) ~ W(1)

        // Animator 파라미터에 값 전달
        animator.SetFloat("HOR", hor);
        animator.SetFloat("VERT", vert);
    }
}
