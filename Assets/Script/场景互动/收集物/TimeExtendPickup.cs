using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
public class TimeExtendPickup : MonoBehaviour
{
    [Header("加时设置")]
    [SerializeField] private float addSeconds = 20f;      // 增加的时间（秒）

    [Header("效果设置（可选）")]
    [SerializeField] private GameObject pickupEffect;      // 拾取特效预制体
    [SerializeField] private AudioClip pickupSound;        // 拾取音效

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家
        if (!other.CompareTag("Player")) return;

        // 获取 SafetyTimer 实例
        SafetyTimer timer = FindObjectOfType<SafetyTimer>();
        if (timer != null)
        {
            timer.AddTime(addSeconds);
            Debug.Log($"拾取加时道具，增加 {addSeconds} 秒安全时间");
        }
        else
        {
            Debug.LogWarning("场景中未找到 SafetyTimer，无法加时！");
        }

        // 播放特效
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        // 播放音效（如果挂载了 AudioSource 且 pickupSound 不为空）
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // 销毁道具物体
        Destroy(gameObject);
    }
}