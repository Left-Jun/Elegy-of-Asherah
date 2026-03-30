using UnityEngine;

public class EnergyPickup : MonoBehaviour
{
    public enum PickupType
    {
        Energy,
        Shard,
        // 加时器在计时器系统处理，这里仅作示例
    }

    public PickupType type;
    public float energyAmount = 10f;      // 灵能拾取量
    public int shardAmount = 1;           // 碎片拾取量

    // 可选：拾取特效、声音
    public GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 假设玩家有 "Player" 标签
        if (!other.CompareTag("Player")) return;

        switch (type)
        {
            case PickupType.Energy:
                EnergyManager.Instance.AddEnergy(energyAmount);
                break;
            case PickupType.Shard:
                EnergyManager.Instance.AddShard(shardAmount);
                break;
        }

        // 触发特效
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}