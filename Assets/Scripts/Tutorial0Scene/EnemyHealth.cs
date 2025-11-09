using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    private void Start() => ResetHealth();

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        gameObject.SetActive(true);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        gameObject.SetActive(false);

        if (Tutorial0Manager.Instance != null)
            Tutorial0Manager.Instance.OnEnemyDefeated(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FallZone"))
            Die();
    }
}
