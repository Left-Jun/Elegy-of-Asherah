/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 选项面板，继承自Widget，管理选项按钮的显示和销毁
/// </summary>
public class ChoicePanel : Widget
{
    private List<AdvancedButton> _buttons = new List<AdvancedButton>();

    /// <summary>
    /// 添加一个选项按钮
    /// </summary>
    public void AddButton(AdvancedButton button)
    {
        if (button == null) return;
        _buttons.Add(button);
        button.transform.SetParent(transform);
        button.transform.localScale = Vector3.one;
        button.onClick.AddListener(DisableAllButtons);
        button.OnConfirm += (_) => { Close(); };
    }

    private void DisableAllButtons()
    {
        foreach (var item in _buttons)
        {
            if (item != null)
                item.enabled = false;
        }
    }

    /// <summary>
    /// 打开面板，淡入并默认选中某个按钮
    /// </summary>
    public void Open(int defaultSelectIndex, float duration = 0.2f)
    {
        RenderOpacity = 0.0f;
        Fade(1f, duration, () =>
        {
            if (_buttons.Count == 0) return;
            int index = (defaultSelectIndex >= 0 && defaultSelectIndex < _buttons.Count) ? defaultSelectIndex : 0;
            _buttons[index].Select();
        });
    }

    /// <summary>
    /// 关闭面板，淡出并销毁所有按钮
    /// </summary>
    public void Close(float duration = 0.2f)
    {
        UIManager.SetCurrentSelectable(null);
        Fade(0f, duration, () =>
        {
            foreach (var item in _buttons)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _buttons.Clear();
            UIManager.HideCursorA();
        });
    }
}
*/