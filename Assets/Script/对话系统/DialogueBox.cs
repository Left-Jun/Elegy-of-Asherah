using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 对话框控制器，负责显示对话内容、处理输入、控制显示流程
/// </summary>
public class DialogueBox : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image _background;                // 背景图，支持多种样式
    [SerializeField] private Widget _widget;                  // 自身淡入淡出
    [SerializeField] private TextMeshProUGUI _speaker;        // 说话人名字
    [SerializeField] private AdvancedText _content;           // 对话内容组件
    [SerializeField] private Widget _nextCursorWidget;        // 继续提示光标
    [SerializeField] private Animator _nextCursorAnimator;    // 光标动画
    private static readonly int _click = Animator.StringToHash("Click");

    [Header("Configs")]
    [SerializeField] private Sprite[] _backgroundStyles;      // 不同风格的背景

    private bool _interactable;          // 是否可交互（等待输入）
    private bool _printFinished;         // 当前句是否打印完毕
    private bool _canQuickShow;          // 是否允许快速显示剩余文字
    private bool _autoNext;              // 是否自动下一句

    private bool CanQuickShow => !_printFinished && _canQuickShow;
    private bool CanNext => _printFinished;

    public Action<bool> OnNext;          // 下一句回调，参数表示是否强制直接显示

    private void Awake()
    {
        if (_content != null)
            _content.OnFinished = PrintFinished;
        else
            Debug.LogError("DialogueBox: AdvancedText component not assigned.");
    }

    private void PrintFinished()
    {
        if (_autoNext)
        {
            _interactable = false;
            OnNext(false);
        }
        else
        {
            _interactable = true;
            _printFinished = true;
            if (_nextCursorWidget != null)
                _nextCursorWidget.Fade(1, 0.2f, null);
        }
    }

    private void Update()
    {
        if (_interactable) UpdateInput();
    }

    private void UpdateInput()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (CanQuickShow && _content != null)
            {
                _content.QuickShowRemaining();
                PrintFinished();   // 强制完成
            }
            else if (CanNext)
            {
                _interactable = false;
                if (_nextCursorAnimator != null)
                    _nextCursorAnimator.SetTrigger(_click);
                if (_nextCursorWidget != null)
                    _nextCursorWidget.Fade(0, 0.5f, null);
                OnNext(true);      // 强制直接显示下一句
            }
        }
        else if (Input.GetButtonDown("Submit"))
        {
            if (CanNext)
            {
                _interactable = false;
                if (_nextCursorAnimator != null)
                    _nextCursorAnimator.SetTrigger(_click);
                if (_nextCursorWidget != null)
                    _nextCursorWidget.Fade(0, 0.5f, null);
                OnNext(false);     // 正常下一句
            }
        }
    }

    /// <summary>
    /// 打开对话框
    /// </summary>
    public void Open(Action<bool> nextEvent, int boxStyle = 0)
    {
        OnNext = nextEvent;
        if (_background != null && _backgroundStyles != null && boxStyle < _backgroundStyles.Length)
            _background.sprite = _backgroundStyles[boxStyle];
        else if (_background != null)
            Debug.LogWarning("Background style index out of range or no styles assigned.");

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            if (_widget != null)
            {
                _widget.RenderOpacity = 0;
                _widget.Fade(1, 0.2f, null);
            }
            if (_speaker != null)
                _speaker.SetText("");
            if (_content != null)
                _content.Initialise();
        }

        if (_nextCursorWidget != null)
            _nextCursorWidget.RenderOpacity = 0;
        OnNext(false);
    }

    /// <summary>
    /// 关闭对话框
    /// </summary>
    public void Close(Action onClosed)
    {
        if (_widget != null)
        {
            _widget.Fade(0, 0.4f, () =>
            {
                gameObject.SetActive(false);
                onClosed?.Invoke();
            });
        }
        else
        {
            gameObject.SetActive(false);
            onClosed?.Invoke();
        }
    }

    /// <summary>
    /// 打印一段对话（协程，用于控制显示顺序）
    /// </summary>
    public IEnumerator PrintDialogue(string content, string speaker, bool needTyping = true,
        bool autoNext = false, bool canQuickShow = true)
    {
        _interactable = false;
        _printFinished = false;

        // 如果已经有文字，先淡出
        if (_content != null && _content.text != "")
        {
            _content.Disappear();
            yield return new WaitForSecondsRealtime(0.3f);
        }

        _canQuickShow = canQuickShow;
        _autoNext = autoNext;
        if (_speaker != null)
            _speaker.SetText(speaker);

        if (needTyping && _content != null)
        {
            _interactable = true;
            _content.StartCoroutine(_content.SetText(content, AdvancedText.DisplayType.Typing));
        }
        else if (_content != null)
        {
            _content.StartCoroutine(_content.SetText(content, AdvancedText.DisplayType.Fading));
        }
        else
        {
            Debug.LogError("DialogueBox: AdvancedText component missing.");
        }
    }
}