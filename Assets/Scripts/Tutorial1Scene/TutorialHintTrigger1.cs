using UnityEngine;

public class TutorialHintTrigger1 : MonoBehaviour
{
    [Tooltip("0 for first cube, 1 for second cube, etc.")]
    public int stepIndex = 0;

    private bool consumed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (consumed) return;

        var appearance = other.GetComponent<CharacterAppearance>();
        if (appearance == null) return;
        if (!appearance.CompareTag("Player")) return;

        if (Tutorial1Manager.Instance != null)
        {
            Tutorial1Manager.Instance.OnHintStepReached(stepIndex);
            consumed = true; // prevent re-trigger
        }
    }
}
