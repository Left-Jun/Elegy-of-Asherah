using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTrigger : MonoBehaviour
{
    public void OnInteract()
    {
        // 创建对话数据
        var data = new DialogueData
        {
            Speaker = "NPC",
            Content = "你好，欢迎来到这个世界！\n这是一个测试。",
            NeedTyping = true,
            AutoNext = false,
            CanQuickShow = true
        };
        UIManager.OpenDialogueBox(OnNext, 0);
        UIManager.PrintDialogue(data);
    }

    void OnNext(bool forceDirect)
    {
        var data2 = new DialogueData
        {
            Speaker = "NPC",
            Content = "这是第二句话。",
            NeedTyping = !forceDirect,
            AutoNext = false,
            CanQuickShow = true
        };
        UIManager.PrintDialogue(data2);
        // 这里可以继续添加更多对话，或最终调用 CloseDialogueBox
    }

    // 可选：玩家按E键交互
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger, starting dialogue.");
            OnInteract();
        }
    }
}