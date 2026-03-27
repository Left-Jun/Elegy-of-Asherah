using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 选项面板，管理选项按钮的显示和销毁
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ChoicePanel : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private List<AdvancedButton> _buttons = new List<AdvancedButton>();

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = true;
    }

    public void AddButton(AdvancedButton button)
    {
        if (button == null) return;
        _buttons.Add(button);
        button.transform.SetParent(transform);
        button.transform.localScale = Vector3.one;
        button.onClick.AddListener(() => DisableAllButtons());
        button.OnConfirm += (_) => { Close(); };
    }

    private void DisableAllButtons()
    {
        foreach (var btn in _buttons)
            if (btn != null) btn.enabled = false;
    }

    public void Open(int defaultSelectIndex, float duration = 0.2f)
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = true;
        // 简单的淡入
        StartCoroutine(FadeCanvasGroup(0, 1, duration, () =>
        {
            if (_buttons.Count > 0)
            {
                int idx = (defaultSelectIndex >= 0 && defaultSelectIndex < _buttons.Count) ? defaultSelectIndex : 0;
                _buttons[idx].Select();
            }
        }));
    }

    public void Close(float duration = 0.2f)
    {
        UIManager.SetCurrentSelectable(null);
        StartCoroutine(FadeCanvasGroup(1, 0, duration, () =>
        {
            foreach (var btn in _buttons)
                if (btn != null) Destroy(btn.gameObject);
            _buttons.Clear();
            gameObject.SetActive(false);
            UIManager.HideCursorA();
        }));
    }

    private System.Collections.IEnumerator FadeCanvasGroup(float start, float end, float duration, System.Action onComplete)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(start, end, t / duration);
            _canvasGroup.alpha = alpha;
            yield return null;
        }
        _canvasGroup.alpha = end;
        onComplete?.Invoke();
    }
}