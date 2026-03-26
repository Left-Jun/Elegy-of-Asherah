using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DialogueController : MonoBehaviour
{
    public AdvancedText textComp;
    public Widget dialogueWidget;   // 拖拽 DialogueBox 上的 Widget
    public string[] sentences;
    private int index;

    void Start()
    {
        dialogueWidget.RenderOpacity = 0;
    }

    public void StartDialogue()
    {
        index = 0;
        dialogueWidget.Fade(1, 0.3f, null);
        ShowNext();
    }

    public void ShowNext()
    {
        if (index < sentences.Length)
        {
            textComp.StartCoroutine(textComp.SetText(sentences[index], AdvancedText.DisplayType.Typing));
            index++;
        }
        else
        {
            dialogueWidget.Fade(0, 0.3f, null);
        }
    }
}