using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimpleTyper : MonoBehaviour
{
    public float charDelay = 0.05f;
    public string[] sentences;          // 对话数组
    private int index;
    private TextMeshProUGUI textComp;

    void Start()
    {
        textComp = GetComponent<TextMeshProUGUI>();
    }

    public void StartDialogue()
    {
        index = 0;
        ShowCurrentSentence();
    }

    void ShowCurrentSentence()
    {
        if (index < sentences.Length)
            StartCoroutine(TypeText(sentences[index]));
        else
            Debug.Log("对话结束");
    }

    IEnumerator TypeText(string message)
    {
        textComp.text = "";
        foreach (char c in message)
        {
            textComp.text += c;
            yield return new WaitForSeconds(charDelay);
        }
        // 等待玩家按“继续”按钮
        // 这里需要外部按钮来调用 NextSentence()
    }

    public void NextSentence()
    {
        if (textComp.text == sentences[index])  // 当前句已完成
        {
            index++;
            ShowCurrentSentence();
        }
    }
}