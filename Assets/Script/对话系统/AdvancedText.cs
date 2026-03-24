using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using System;

/// <summary>
/// Ruby注音数据，存储注音的范围和内容
/// </summary>
public class RubyData
{
    public RubyData(int startIndex, string content)
    {
        StartIndex = startIndex;
        RubyContent = content;
        EndIndex = StartIndex;
    }
    public int StartIndex { get; }
    public int EndIndex { get; set; }
    public string RubyContent { get; }
}

/// <summary>
/// 自定义文本预处理器，用于解析自定义标签：<数字> 表示停顿，<r=...> 和 </r> 表示注音
/// </summary>
public class AdvancedTextPreprocessor : ITextPreprocessor
{
    public Dictionary<int, float> IntervalDictionary;      // 字符索引 -> 停顿时间
    public List<RubyData> RubyList;                        // 所有注音数据列表

    public bool TryGetRubyStartFrom(int index, out RubyData data)
    {
        data = new RubyData(0, "");
        foreach (var item in RubyList)
        {
            if (item.StartIndex == index)
            {
                data = item;
                return true;
            }
        }
        return false;
    }

    public AdvancedTextPreprocessor()
    {
        IntervalDictionary = new Dictionary<int, float>();
        RubyList = new List<RubyData>();
    }

    /// <summary>
    /// 预处理文本，提取标签并记录，最后移除所有标签
    /// </summary>
    public string PreprocessText(string text)
    {
        IntervalDictionary.Clear();
        RubyList.Clear();

        string processingText = text;
        string pattern = "<.*?>";
        Match match = Regex.Match(processingText, pattern);
        while (match.Success)
        {
            string label = match.Value.Substring(1, match.Length - 2);

            if (float.TryParse(label, out float result))
            {
                // 停顿标签，记录索引和延迟时间（索引为前一个字符的位置）
                IntervalDictionary[match.Index - 1] = result;
            }
            else if (Regex.IsMatch(label, "^r=.+"))
            {
                // 注音开始标签
                RubyList.Add(new RubyData(match.Index, label.Substring(2)));
            }
            else if (label == "/r")
            {
                // 注音结束标签，设置最后一个注音数据的结束索引
                if (RubyList.Count > 0)
                {
                    RubyList[RubyList.Count - 1].EndIndex = match.Index - 1;
                }
            }

            processingText = processingText.Remove(match.Index, match.Length);
            
            if (Regex.IsMatch(label, "^sprite=.+"))
            {
                // sprite标签替换为占位符（实际由TMP处理）
                processingText = processingText.Insert(match.Index, "*");
            }

            match = Regex.Match(processingText, pattern);
        }

        // 最终移除所有自定义标签，只保留显示文本
        processingText = text;
        pattern = @"(<(\d+)(\.\d+)?>)|(</r>)|(<r=.*?>)";
        processingText = Regex.Replace(processingText, pattern, "");
        return processingText;
    }
}

/// <summary>
/// 高级文本组件，继承自TextMeshProUGUI，实现打字机效果、Ruby注音、自定义停顿
/// </summary>
[RequireComponent(typeof(Widget))]
public class AdvancedText : TextMeshProUGUI
{
    public enum DisplayType
    {
        Default,    // 直接显示
        Fading,     // 整体淡入
        Typing      // 逐字打印
    }

    private Widget _widget;                     // 用于整体淡入淡出

    protected override void Awake()
    {
        base.Awake();
        _widget = GetComponent<Widget>();
        if (_widget == null)
            Debug.LogError("AdvancedText requires a Widget component on the same GameObject.");
    }

    public AdvancedText()
    {
        textPreprocessor = new AdvancedTextPreprocessor();   // 设置自定义预处理器
    }

    private AdvancedTextPreprocessor SelfPreprocessor => (AdvancedTextPreprocessor)textPreprocessor;

    public Action OnFinished;                   // 打印完成回调
    private Coroutine _typingCoroutine;
    private int _typingIndex;
    private float _defaultInterval = 0.06f;    // 默认字符间隔

    /// <summary>
    /// 初始化，清空文本和注音子物体
    /// </summary>
    public void Initialise()
    {
        SetText("");
        ClearRubyText();
    }

    /// <summary>
    /// 淡出整个文本（用于切换句子时的过渡）
    /// </summary>
    public void Disappear(float duration = 0.2f)
    {
        _widget?.Fade(0, duration, null);
    }

    /// <summary>
    /// 为指定的注音数据创建注音子物体
    /// </summary>
    private void SetRubyText(RubyData data)
    {
        GameObject pfb = Resources.Load<GameObject>("RubyText");
        if (pfb == null)
        {
            Debug.LogError("RubyText prefab not found in Resources folder.");
            return;
        }
        GameObject ruby = Instantiate(pfb, transform);
        TextMeshProUGUI rubyText = ruby.GetComponent<TextMeshProUGUI>();
        if (rubyText == null)
        {
            Debug.LogError("RubyText prefab must have a TextMeshProUGUI component.");
            Destroy(ruby);
            return;
        }
        rubyText.SetText(data.RubyContent);
        rubyText.color = textInfo.characterInfo[data.StartIndex].color;

        // 计算注音位置：位于起始字符和结束字符的顶部中点
        Vector3 startPos = textInfo.characterInfo[data.StartIndex].topLeft;
        Vector3 endPos = textInfo.characterInfo[data.EndIndex].topRight;
        ruby.transform.localPosition = (startPos + endPos) / 2;
    }

    /// <summary>
    /// 为所有注音数据创建子物体
    /// </summary>
    private void SetAllRubyText()
    {
        foreach (var item in SelfPreprocessor.RubyList)
        {
            SetRubyText(item);
        }
    }

    /// <summary>
    /// 清除所有注音子物体
    /// </summary>
    private void ClearRubyText()
    {
        foreach (var item in GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (item != this)
            {
                Destroy(item.gameObject);
            }
        }
    }

    /// <summary>
    /// 快速显示剩余所有字符（跳过打字动画）
    /// </summary>
    public void QuickShowRemaining()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            // 从当前索引开始，淡入所有剩余字符
            for (; _typingIndex < m_characterCount; _typingIndex++)
            {
                StartCoroutine(FadeInCharacter(_typingIndex));
            }
        }
    }

    /// <summary>
    /// 设置文本并开始显示效果
    /// </summary>
    /// <param name="content">显示内容（可包含TMP标签和自定义标签）</param>
    /// <param name="type">显示类型</param>
    /// <param name="fadingDuration">整体淡入持续时间</param>
    public IEnumerator SetText(string content, DisplayType type, float fadingDuration = 0.2f)
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        ClearRubyText();
        SetText(content);          // 设置文本，会自动调用预处理器
        yield return null;        // 等待一帧，确保文本更新完成

        switch (type)
        {
            case DisplayType.Default:
                _widget.RenderOpacity = 1;
                SetAllRubyText();
                OnFinished?.Invoke();
                break;
            case DisplayType.Fading:
                _widget.Fade(1, fadingDuration, OnFinished.Invoke);
                SetAllRubyText();
                break;
            case DisplayType.Typing:
                _widget.Fade(1, fadingDuration, null);
                _typingCoroutine = StartCoroutine(Typing());
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 打字机协程：逐字符淡入显示
    /// </summary>
    IEnumerator Typing()
    {
        ForceMeshUpdate();
        // 先将所有字符设为不可见
        for (int i = 0; i < m_characterCount; i++)
        {
            SetSingleCharacterAlpha(i, 0);
        }

        _typingIndex = 0;
      
        while (_typingIndex < m_characterCount)
        {
            StartCoroutine(FadeInCharacter(_typingIndex));
            
            // 检查是否有自定义停顿时间
            if (SelfPreprocessor.IntervalDictionary.TryGetValue(_typingIndex, out float result))
            {
                yield return new WaitForSecondsRealtime(result);
            }
            else
            {
                yield return new WaitForSecondsRealtime(_defaultInterval);
            }

            _typingIndex++;
        }

        OnFinished.Invoke();
    }

    /// <summary>
    /// 设置单个字符的透明度（0-255）
    /// </summary>
    private void SetSingleCharacterAlpha(int index, byte newAlpha)
    {
        if (index < 0 || index >= textInfo.characterCount) return;
        TMP_CharacterInfo charInfo = textInfo.characterInfo[index];
        if (!charInfo.isVisible) return;
        int matIndex = charInfo.materialReferenceIndex;
        int vertIndex = charInfo.vertexIndex;
        for (int i = 0; i < 4; i++)
        {
            textInfo.meshInfo[matIndex].colors32[vertIndex + i].a = newAlpha;
        }
        UpdateVertexData();
    }

    /// <summary>
    /// 淡入单个字符
    /// </summary>
    IEnumerator FadeInCharacter(int index, float duration = 0.2f)
    {
        if (SelfPreprocessor.TryGetRubyStartFrom(index, out RubyData data))
        {
            SetRubyText(data);
        }
        
        if (duration <= 0)
        {
            SetSingleCharacterAlpha(index, 255);
        }
        else
        {
            float timer = 0;
            while (timer < duration)
            {
                timer = Mathf.Min(duration, timer + Time.unscaledDeltaTime);
                SetSingleCharacterAlpha(index, (byte)(255 * timer / duration));
                yield return null;
            }
        }
    }
}