using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    [Header("情绪背景音乐")]
    [SerializeField] private AudioClip neutralMusic;
    [SerializeField] private AudioClip happyMusic;
    [SerializeField] private AudioClip angryMusic;
    
    [Header("结局音乐")]
    [SerializeField] private AudioClip endingMusic;

    [Header("音频设置")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float maxVolume = 0.7f;

    private AudioSource audioSource;
    private MaskControl playerMaskControl;
    private Coroutine fadeCoroutine;
    
    // 新增：更严格的结局状态控制
    private bool isInEndingSequence = false;
    private bool isEndingMusicPlaying = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = 0f;
        }

        playerMaskControl = FindObjectOfType<MaskControl>();
        
        if (playerMaskControl != null)
        {
            playerMaskControl.OnEmotionChangedEvent += OnEmotionChanged;
            OnEmotionChanged(playerMaskControl.currentEmotion);
        }
        else
        {
            Debug.LogWarning("MusicManager: 未找到MaskControl，音乐将不会随情绪变化。");
            if (neutralMusic != null)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(SwitchMusic(neutralMusic));
            }
        }
    }

    // 情绪变化处理 - 增加更严格的检查
    private void OnEmotionChanged(MaskControl.EmotionState newEmotion)
    {
        // 如果已经在播放结局音乐，完全忽略所有情绪变化
        if (isEndingMusicPlaying) return;
        
        // 如果即将进入结局流程，也忽略
        if (isInEndingSequence) return;
        
        AudioClip clipToPlay = GetClipForEmotion(newEmotion);
        
        if (clipToPlay != null && clipToPlay != audioSource.clip)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(SwitchMusic(clipToPlay));
            Debug.Log($"情绪音乐切换至: {newEmotion}");
        }
    }

    private AudioClip GetClipForEmotion(MaskControl.EmotionState emotion)
    {
        switch (emotion)
        {
            case MaskControl.EmotionState.Neutral: return neutralMusic;
            case MaskControl.EmotionState.Happy: return happyMusic;
            case MaskControl.EmotionState.Angry: return angryMusic;
            default: return neutralMusic;
        }
    }

    // 普通音乐切换
    private IEnumerator SwitchMusic(AudioClip newClip)
    {
        if (audioSource.isPlaying && audioSource.volume > 0)
        {
            yield return StartCoroutine(FadeVolume(0f, fadeDuration / 2));
        }

        audioSource.clip = newClip;
        audioSource.Play();
        yield return StartCoroutine(FadeVolume(maxVolume, fadeDuration / 2));

        fadeCoroutine = null;
    }

    // 音量淡入淡出
    private IEnumerator FadeVolume(float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    // ========== 结局音乐核心方法（增强版）==========
    public void PlayEndingMusic()
    {
        if (endingMusic == null)
        {
            Debug.LogWarning("MusicManager: 未分配结局音乐，将仅停止当前音乐。");
            StopAllMusic();
            return;
        }

        // 立即设置状态，阻止任何情绪音乐干扰
        isInEndingSequence = true;
        isEndingMusicPlaying = true;
        
        // 立即取消所有情绪变化订阅
        if (playerMaskControl != null)
        {
            playerMaskControl.OnEmotionChangedEvent -= OnEmotionChanged;
        }
        
        // 停止当前所有音乐切换
        if (fadeCoroutine != null) 
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        // 强制立即开始播放结局音乐（不等待淡出）
        fadeCoroutine = StartCoroutine(PlayEndingMusicImmediately());
    }

    // 强制播放结局音乐的协程
    private IEnumerator PlayEndingMusicImmediately()
    {
        Debug.Log("强制播放结局庆祝音乐，忽略所有情绪音乐...");
        
        // 1. 立即停止当前音乐（不淡出）
        audioSource.Stop();
        
        // 2. 直接设置结局音乐并播放
        audioSource.clip = endingMusic;
        audioSource.loop = false;
        audioSource.volume = 0f; // 从0音量开始
        audioSource.Play();
        
        // 3. 快速淡入结局音乐（比平时快）
        yield return StartCoroutine(FadeVolume(maxVolume, fadeDuration / 3));
        
        fadeCoroutine = null;
        
        // 4. 等待音乐播放完毕（可选）
        // yield return new WaitForSeconds(endingMusic.length);
        // Debug.Log("结局音乐播放完毕");
    }

    // 停止所有音乐（立即）
    public void StopAllMusic()
    {
        isInEndingSequence = true;
        isEndingMusicPlaying = true;
        
        if (fadeCoroutine != null) 
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    // 清理
    private void OnDestroy()
    {
        if (playerMaskControl != null)
        {
            playerMaskControl.OnEmotionChangedEvent -= OnEmotionChanged;
        }
    }
}