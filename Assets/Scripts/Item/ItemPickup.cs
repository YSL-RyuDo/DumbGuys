using UnityEngine;
using System.Collections;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { SpeedUp, AttackUp, Heal, Push, JumpUp }

    [SerializeField] private float respawnDelay = 5f; // 5초 뒤 리스폰

    private Collider itemCollider;
    private MeshRenderer itemRenderer;

    private void Awake()
    {
        itemCollider = GetComponent<Collider>();
        itemRenderer = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Enemy"))
            return;

        Debug.Log($"[ItemPickup] {other.name} picked up {name}");

        // 플레이어만 아이템 효과 적용
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerCharacter>();
            if (player != null)
                player.OnItemPicked(RandomItem());
        }

        // 아이템 숨기기 + 일정 시간 뒤 리스폰
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // 일단 안보이게 처리
        itemCollider.enabled = false;
        itemRenderer.enabled = false;

        // 5초 대기
        yield return new WaitForSeconds(respawnDelay);

        // 다시 활성화
        itemCollider.enabled = true;
        itemRenderer.enabled = true;

        Debug.Log($"[ItemPickup] {name} respawned!");
    }

    private ItemType RandomItem()
    {
        return (ItemType)Random.Range(0, System.Enum.GetValues(typeof(ItemType)).Length);
    }
}
