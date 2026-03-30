using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TreeTopInteraction : MonoBehaviour
{
    [Header("交互设置")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;   // 交互按键
    [SerializeField] private string playerTag = "Player";          // 玩家标签

    private bool playerInRange = false;
    private bool alreadyTriggered = false;   // 防止重复触发结局

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            Debug.Log("玩家进入树顶范围，按 " + interactionKey + " 触发结局");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            Debug.Log("玩家离开树顶范围");
        }
    }

    private void Update()
    {
        if (playerInRange && !alreadyTriggered && Input.GetKeyDown(interactionKey))
        {
            Debug.Log("按下交互键，触发结局");
            if (EndingManager.Instance != null)
            {
                EndingManager.Instance.TriggerEnding();
                alreadyTriggered = true;   // 触发后不再响应
            }
            else
            {
                Debug.LogError("TreeTopInteraction: 未找到 EndingManager 实例！");
            }
        }
    }

    /// <summary>
    /// 外部调用，重置交互（例如重新启用物体时）
    /// </summary>
    public void ResetInteraction()
    {
        alreadyTriggered = false;
        playerInRange = false;   // 可选，但最好让玩家重新进入才能触发
    }
}