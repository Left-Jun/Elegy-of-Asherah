using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 全局UI管理器，单例模式，负责协调对话框、选项面板、光标等
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        _instance._pfbButtonA = Resources.Load<GameObject>("UI/Button/ButtonA");
        if (_instance._pfbButtonA == null)
            Debug.LogError("ButtonA prefab not found in Resources/UI/Button/");
        HideCursorA();
        DontDestroyOnLoad(gameObject);
    }

    private Selectable _currentSelectable;
    public static void SetCurrentSelectable(Selectable obj)
    {
        if (_instance != null)
            _instance._currentSelectable = obj;
    }

    private void Update()
    {
        // 当没有选中任何UI元素时，恢复上次选中的（用于手柄/键盘导航）
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            if (null != _instance._currentSelectable)
            {
                _instance._currentSelectable.Select();
            }
        }
    }

    [SerializeField] private DialogueBox _dialogueBox;
    public static void OpenDialogueBox(Action<bool> onNextEvent, int boxStyle = 0)
    {
        if (_instance != null && _instance._dialogueBox != null)
            _instance._dialogueBox.Open(onNextEvent, boxStyle);
        else
            Debug.LogError("UIManager: DialogueBox not assigned.");
    }

    public static void CloseDialogueBox()
    {
        if (_instance != null && _instance._dialogueBox != null)
            _instance._dialogueBox.Close(null);
    }

    public static void PrintDialogue(DialogueData data)
    {
        if (_instance != null && _instance._dialogueBox != null)
            _instance._dialogueBox.StartCoroutine(_instance._dialogueBox.PrintDialogue(
                data.Content,
                data.Speaker,
                data.NeedTyping,
                data.AutoNext,
                data.CanQuickShow
            ));
        else
            Debug.LogError("UIManager: DialogueBox not assigned.");
    }

    [SerializeField] private RectTransform _cursorA;
    private static readonly int Click = Animator.StringToHash("Click");

    public static void UpdateCursorA(Vector3 position)
    {
        if (_instance == null || _instance._cursorA == null) return;
        if (!_instance._cursorA.gameObject.activeSelf)
        {
            _instance._cursorA.gameObject.SetActive(true);
        }
        _instance._cursorA.position = position;
    }

    public static void ClickCursorA()
    {
        if (_instance == null || _instance._cursorA == null) return;
        Animator anim = _instance._cursorA.GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger(Click);
    }

    public static void HideCursorA()
    {
        if (_instance != null && _instance._cursorA != null)
            _instance._cursorA.gameObject.SetActive(false);
    }

    private GameObject _pfbButtonA;
    [SerializeField] private ChoicePanel _choicePanel;

    /// <summary>
    /// 创建选项按钮
    /// </summary>
    public static void CreateDialogueChocies(ChoiceData[] datas, Action<int> onConfirmEvent, int defaultSelectIndex)
    {
        if (_instance == null || _instance._pfbButtonA == null || _instance._choicePanel == null)
        {
            Debug.LogError("UIManager: Missing references for choices.");
            return;
        }

        for (int i = 0; i < datas.Length; i++)
        {
            AdvancedButtonA button = Instantiate(_instance._pfbButtonA).GetComponent<AdvancedButtonA>();
            if (button == null)
            {
                Debug.LogError("ButtonA prefab missing AdvancedButtonA component.");
                continue;
            }
            button.gameObject.name = "ButtonA" + i;
            button.Init(datas[i].Content, i, onConfirmEvent);
            _instance._choicePanel.AddButton(button);
        }
        _instance._choicePanel.Open(defaultSelectIndex);
    }
}
