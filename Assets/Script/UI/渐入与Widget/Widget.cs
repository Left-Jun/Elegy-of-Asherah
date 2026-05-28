using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// UI动画组件，控制CanvasGroup的淡入淡出，支持自定义曲线
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class Widget : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    [SerializeField] private AnimationCurve _fadingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

public float RenderOpacity
{
    get
    {
        EnsureCanvasGroup();
        return _canvasGroup != null ? _canvasGroup.alpha : 0f;
    }
    set
    {
        EnsureCanvasGroup();
        if (_canvasGroup != null) _canvasGroup.alpha = value;
    }
}

    private void Awake()
    {
        EnsureCanvasGroup();
    }

    private Coroutine _fadeCoroutine;

    /// <summary>
    /// 淡入淡出到目标透明度
    /// </summary>
    /// <param name="opacity">目标透明度</param>
    /// <param name="duration">持续时间</param>
    /// <param name="onFinished">完成回调</param>
    public void Fade(float opacity, float duration, Action onFinished)
    {
        EnsureCanvasGroup();
        if (_canvasGroup == null)
            return;

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        if (!gameObject.activeInHierarchy)
        {
            // 如果物体未激活，直接设置透明度并回调
            _canvasGroup.alpha = opacity;
            onFinished?.Invoke();
            return;
        }
        if (duration <= 0)
        {
            _canvasGroup.alpha = opacity;
            onFinished?.Invoke();
        }
        else
        {
            _fadeCoroutine = StartCoroutine(Fading(opacity, duration, onFinished));
        }
    }

    private IEnumerator Fading(float opacity, float duration, Action onFinished)
    {
        float timer = 0;
        float start = RenderOpacity;
        while (timer < duration)
        {
            timer = Mathf.Min(duration, timer + Time.unscaledDeltaTime);
            RenderOpacity = Mathf.Lerp(start, opacity, _fadingCurve.Evaluate(timer / duration));
            yield return null;
        }
        onFinished?.Invoke();
    }

    private void EnsureCanvasGroup()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
}
