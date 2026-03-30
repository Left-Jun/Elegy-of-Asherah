using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using System;

/// <summary>
/// Ruby注音数据
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
/// 文本预处理器，解析自定义标签
/// </summary>
public class AdvancedTextPreprocessor : ITextPreprocessor
{
    public Dictionary<int, float> IntervalDictionary;
    public List<RubyData> RubyList;

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
                IntervalDictionary[match.Index - 1] = result;
            }
            else if (Regex.IsMatch(label, "^r=.+"))
            {
                RubyList.Add(new RubyData(match.Index, label.Substring(2)));
            }
            else if (label == "/r")
            {
                if (RubyList.Count > 0)
                {
                    RubyList[RubyList.Count - 1].EndIndex = match.Index - 1;
                }
            }

            processingText = processingText.Remove(match.Index, match.Length);
            
            if (Regex.IsMatch(label, "^sprite=.+"))
            {
                processingText = processingText.Insert(match.Index, "*");
            }

            match = Regex.Match(processingText, pattern);
        }

        processingText = text;
        pattern = @"(<(\d+)(\.\d+)?>)|(</r>)|(<r=.*?>)";
        processingText = Regex.Replace(processingText, pattern, "");
        return processingText;
    }
}

/// <summary>
/// 高级文本组件
/// </summary>
[RequireComponent(typeof(Widget))]
public class AdvancedText : TextMeshProUGUI
{
    public enum DisplayType
    {
        Default,
        Fading,
        Typing
    }

    private Widget _widget;

    [Header("打字机设置")]
    [SerializeField] private float defaultCharDelay = 0.06f;

    public float DefaultCharDelay
    {
        get => defaultCharDelay;
        set => defaultCharDelay = Mathf.Max(0.01f, value);
    }

    protected override void Awake()
    {
        base.Awake();
        _widget = GetComponent<Widget>();
        if (_widget == null)
            Debug.LogError("AdvancedText requires a Widget component on the same GameObject.");
    }

    public AdvancedText()
    {
        textPreprocessor = new AdvancedTextPreprocessor();
    }

    private AdvancedTextPreprocessor SelfPreprocessor => (AdvancedTextPreprocessor)textPreprocessor;

    public Action OnFinished;
    private Coroutine _typingCoroutine;
    private int _typingIndex;

    public void Initialise()
    {
        SetText("");
        ClearRubyText();
    }

    public void Disappear(float duration = 0.2f)
    {
        _widget?.Fade(0, duration, null);
    }

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
        Vector3 startPos = textInfo.characterInfo[data.StartIndex].topLeft;
        Vector3 endPos = textInfo.characterInfo[data.EndIndex].topRight;
        ruby.transform.localPosition = (startPos + endPos) / 2;
    }

    private void SetAllRubyText()
    {
        foreach (var item in SelfPreprocessor.RubyList)
        {
            SetRubyText(item);
        }
    }

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

    public void QuickShowRemaining()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            for (; _typingIndex < m_characterCount; _typingIndex++)
            {
                StartCoroutine(FadeInCharacter(_typingIndex));
            }
        }
    }

    public IEnumerator SetText(string content, DisplayType type, float fadingDuration = 0.2f)
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        ClearRubyText();

        switch (type)
        {
            case DisplayType.Default:
                SetText(content);
                _widget.RenderOpacity = 1;
                SetAllRubyText();
                OnFinished?.Invoke();
                break;
            case DisplayType.Fading:
                SetText(content);
                _widget.Fade(1, fadingDuration, OnFinished.Invoke);
                SetAllRubyText();
                break;
            case DisplayType.Typing:
                _widget.Fade(1, fadingDuration, null);
                SetText(content);
                ForceMeshUpdate();
                for (int i = 0; i < m_characterCount; i++)
                {
                    SetSingleCharacterAlpha(i, 0);
                }
                _typingCoroutine = StartCoroutine(Typing());
                break;
            default:
                break;
        }
        yield break;
    }

    IEnumerator Typing()
    {
        _typingIndex = 0;
        while (_typingIndex < m_characterCount)
        {
            StartCoroutine(FadeInCharacter(_typingIndex));
            if (SelfPreprocessor.IntervalDictionary.TryGetValue(_typingIndex, out float result))
            {
                yield return new WaitForSecondsRealtime(result);
            }
            else
            {
                yield return new WaitForSecondsRealtime(defaultCharDelay);
            }
            _typingIndex++;
        }
        OnFinished.Invoke();
    }

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

    public void Clear()
    {
        SetText("");
        ForceMeshUpdate();
        ClearRubyText();
    }
}