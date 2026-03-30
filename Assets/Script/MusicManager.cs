using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MusicManager : MonoBehaviour
{
    [Header("状态背景音乐")]
    [SerializeField] private AudioClip normalMusic;
    [SerializeField] private AudioClip enhancedMusic;
    
    [Header("结局音乐（可自定义）")]
    [SerializeField] private AudioClip highEndingMusic;
    [SerializeField] private AudioClip normalEndingMusic;
    [SerializeField] private AudioClip lowEndingMusic;
    [SerializeField] private AudioClip deathEndingMusic;

    [Header("音频设置")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float maxVolume = 0.7f;

    private AudioSource audioSource;
    private MaskControl playerMaskControl;
    private Coroutine fadeCoroutine;
    
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
            playerMaskControl.OnStateChangedEvent += OnStateChanged;
            OnStateChanged(playerMaskControl.currentState);
        }
        else
        {
            Debug.LogWarning("MusicManager: 未找到MaskControl，音乐将不会随状态变化。");
            if (normalMusic != null)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(SwitchMusic(normalMusic));
            }
        }
    }

    private void OnStateChanged(MaskControl.State newState)
    {
        if (isEndingMusicPlaying) return;
        if (isInEndingSequence) return;
        
        AudioClip clipToPlay = GetClipForState(newState);
        if (clipToPlay != null && clipToPlay != audioSource.clip)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(SwitchMusic(clipToPlay));
        }
    }

    private AudioClip GetClipForState(MaskControl.State state)
    {
        return state == MaskControl.State.Normal ? normalMusic : enhancedMusic;
    }

    private IEnumerator SwitchMusic(AudioClip newClip)
    {
        if (audioSource.isPlaying && audioSource.volume > 0)
            yield return StartCoroutine(FadeVolume(0f, fadeDuration / 2));

        audioSource.clip = newClip;
        audioSource.Play();
        yield return StartCoroutine(FadeVolume(maxVolume, fadeDuration / 2));
        fadeCoroutine = null;
    }

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

    /// <summary>
    /// 根据结局类型播放对应的音乐
    /// </summary>
    public void PlayEndingMusic(EndingManager.EndingType endingType)
    {
        AudioClip targetMusic = null;
        switch (endingType)
        {
            case EndingManager.EndingType.High:
                targetMusic = highEndingMusic;
                break;
            case EndingManager.EndingType.Normal:
                targetMusic = normalEndingMusic;
                break;
            case EndingManager.EndingType.Low:
                targetMusic = lowEndingMusic;
                break;
            case EndingManager.EndingType.Death:
                targetMusic = deathEndingMusic;
                break;
        }

        if (targetMusic == null)
        {
            Debug.LogWarning($"MusicManager: 未配置 {endingType} 结局的音乐，仅停止当前音乐。");
            StopAllMusic();
            return;
        }

        isInEndingSequence = true;
        isEndingMusicPlaying = true;
        
        if (playerMaskControl != null)
            playerMaskControl.OnStateChangedEvent -= OnStateChanged;
        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        
        fadeCoroutine = StartCoroutine(PlayEndingMusicImmediately(targetMusic));
    }

    private IEnumerator PlayEndingMusicImmediately(AudioClip music)
    {
        Debug.Log($"播放结局音乐：{music.name}");
        audioSource.Stop();
        audioSource.clip = music;
        audioSource.loop = false;
        audioSource.volume = 0f;
        audioSource.Play();
        yield return StartCoroutine(FadeVolume(maxVolume, fadeDuration / 3));
        fadeCoroutine = null;
    }

    public void StopAllMusic()
    {
        isInEndingSequence = true;
        isEndingMusicPlaying = true;
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        if (audioSource != null)
            audioSource.Stop();
    }

    private void OnDestroy()
    {
        if (playerMaskControl != null)
            playerMaskControl.OnStateChangedEvent -= OnStateChanged;
    }
}