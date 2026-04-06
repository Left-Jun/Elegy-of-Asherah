using UnityEngine;

public class DialogueStarter : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    public GameObject dialoguePanel;   // 直接拖入对话框 Panel（不需要引用 SimpleDialogue 组件）


    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);   // 激活后，Panel 的 OnEnable 会自动开始对话
            else
                Debug.LogError("DialogueStarter: 未指定 dialoguePanel");
        }
    }
}