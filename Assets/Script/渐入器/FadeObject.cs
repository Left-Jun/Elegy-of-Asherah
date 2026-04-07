using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制物体及其所有子物体的淡入淡出（支持 SpriteRenderer, MeshRenderer 等）
/// 使用方法：调用 Show() 显示（淡入），调用 Hide() 隐藏（淡出后禁用物体）
/// </summary>
public class FadeObjectHierarchy : MonoBehaviour
{
    [Header("淡入淡出设置")]
    [SerializeField] private float defaultFadeDuration = 0.3f;
    [SerializeField] private bool autoFadeInOnStart = true;  // 开始时自动淡入（如果物体是激活的）

    private List<Material> _materials = new List<Material>();
    private List<float> _originalAlphas = new List<float>();
    private Coroutine _currentFade;

    private void Awake()
    {
        CollectMaterials();
    }

    private void Start()
    {
        if (autoFadeInOnStart && gameObject.activeSelf)
        {
            // 初始状态设为完全透明，然后淡入
            SetOpacity(0f);
            FadeIn(defaultFadeDuration);
        }
    }

    /// <summary>
    /// 收集自身及所有子物体中的 Renderer 材质（支持透明通道的材质）
    /// </summary>
    private void CollectMaterials()
    {
        _materials.Clear();
        _originalAlphas.Clear();

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            // 跳过粒子系统（如果需要可注释）
            if (renderer is ParticleSystemRenderer) continue;

            Material mat = renderer.material; // 创建实例材质，避免互相影响
            if (mat != null && mat.HasProperty("_Color"))
            {
                _materials.Add(mat);
                _originalAlphas.Add(mat.color.a);
            }
        }
    }

    /// <summary>
    /// 设置所有材质的透明度（0-1）
    /// </summary>
    private void SetOpacity(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        foreach (var mat in _materials)
        {
            if (mat != null)
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
        }
    }

    /// <summary>
    /// 获取当前透明度（取第一个材质的alpha）
    /// </summary>
    private float GetOpacity()
    {
        if (_materials.Count == 0) return 1f;
        return _materials[0] != null ? _materials[0].color.a : 1f;
    }

    /// <summary>
    /// 淡入（显示）
    /// </summary>
    public void FadeIn(float duration = -1f, Action onComplete = null)
    {
        if (duration <= 0) duration = defaultFadeDuration;
        StartFade(1f, duration, onComplete);
    }

    /// <summary>
    /// 淡出（隐藏，不会自动禁用物体）
    /// </summary>
    public void FadeOut(float duration = -1f, Action onComplete = null)
    {
        if (duration <= 0) duration = defaultFadeDuration;
        StartFade(0f, duration, onComplete);
    }

    /// <summary>
    /// 显示物体（先激活物体，再淡入）
    /// </summary>
    public void Show(float duration = -1f, Action onComplete = null)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        FadeIn(duration, onComplete);
    }

    /// <summary>
    /// 隐藏物体（先淡出，淡出完成后禁用物体）
    /// </summary>
    public void Hide(float duration = -1f, Action onComplete = null)
    {
        if (!gameObject.activeSelf)
        {
            onComplete?.Invoke();
            return;
        }
        FadeOut(duration, () =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    private void StartFade(float targetAlpha, float duration, Action onComplete)
    {
        if (_currentFade != null)
            StopCoroutine(_currentFade);
        _currentFade = StartCoroutine(FadeRoutine(targetAlpha, duration, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, Action onComplete)
    {
        float startAlpha = GetOpacity();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            SetOpacity(Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }
        SetOpacity(targetAlpha);
        onComplete?.Invoke();
        _currentFade = null;
    }

    private void OnDestroy()
    {
        // 释放实例材质，避免内存泄漏
        foreach (var mat in _materials)
            if (mat != null) Destroy(mat);
        _materials.Clear();
    }
}