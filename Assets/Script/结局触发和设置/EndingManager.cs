using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance { get; private set; }

    [Header("结局判定（灵能数值区间）")]
    [SerializeField] private float highEndingMin = 80f;
    [SerializeField] private float normalEndingMin = 50f;
    [SerializeField] private float lowEndingMin = 0f;

    [Header("各结局面板与流程配置")]
    [SerializeField] private EndingConfig highEndingConfig;
    [SerializeField] private EndingConfig normalEndingConfig;
    [SerializeField] private EndingConfig lowEndingConfig;
    [SerializeField] private EndingConfig deathEndingConfig;

    [System.Serializable]
    public class EndingConfig
    {
        [Header("第一个面板")]
        public GameObject panel1;
        [Tooltip("显示第一个面板后等待的秒数")]
        public float delay1 = 2f;

        [Header("第二个面板")]
        public GameObject panel2;
        [Tooltip("显示第二个面板后等待的秒数")]
        public float delay2 = 2f;

        [Header("返回设置")]
        public bool waitForInput = false;           // 是否等待按键返回
        public KeyCode returnKey = KeyCode.Escape;  // 返回按键（仅在 waitForInput 为 true 时生效）
        [Tooltip("最终场景（waitForInput 为 false 时自动加载）")]
        public string targetScene = "MainMenu";
    }

    private EnergyManager energyManager;
    private bool endingTriggered = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        energyManager = EnergyManager.Instance;
        endingTriggered = false;
        if (energyManager == null)
            Debug.LogError("EndingManager: 未找到 EnergyManager！");
    }

    public void TriggerEnding()
    {
        if (endingTriggered) return;
        if (energyManager == null) return;

        float energy = energyManager.currentEnergy;
        EndingType ending = DetermineEnding(energy);
        StartCoroutine(EndingSequence(ending));
        endingTriggered = true;
    }

    public void TriggerDeathEnding()
    {
        if (endingTriggered) return;
        StartCoroutine(EndingSequence(EndingType.Death));
        endingTriggered = true;
    }

    private EndingType DetermineEnding(float energy)
    {
        if (energy >= highEndingMin)
            return EndingType.High;
        else if (energy >= normalEndingMin)
            return EndingType.Normal;
        else
            return EndingType.Low;
    }

    private IEnumerator EndingSequence(EndingType ending)
    {
        Debug.Log($"=== 触发结局：{ending} ===");
        Debug.Log($"当前灵能：{energyManager.currentEnergy}/{energyManager.maxEnergy}");

        // 播放结局音乐
        MusicManager musicManager = FindObjectOfType<MusicManager>();
        if (musicManager != null)
            musicManager.PlayEndingMusic(ending);
        else
            Debug.LogWarning("EndingManager: 未找到 MusicManager，无法播放结局音乐");

        // 禁用玩家控制
        DisablePlayerControls();

        EndingConfig config = GetConfigForEnding(ending);
        if (config == null)
        {
            Debug.LogError($"EndingManager: 未找到 {ending} 结局的配置！");
            yield break;
        }

        // 显示第一个面板
        if (config.panel1 != null)
        {
            config.panel1.SetActive(true);
            yield return new WaitForSeconds(config.delay1);
        }

        // 显示第二个面板
        if (config.panel2 != null)
        {
            config.panel2.SetActive(true);
            yield return new WaitForSeconds(config.delay2);
        }

        // 根据配置决定返回方式
        if (config.waitForInput)
        {
            Debug.Log($"等待按下 {config.returnKey} 键返回主菜单...");
            while (!Input.GetKeyDown(config.returnKey))
            {
                yield return null;
            }
            if (!string.IsNullOrEmpty(config.targetScene))
                SceneManager.LoadScene(config.targetScene);
            else
                Debug.LogWarning("EndingManager: 未指定目标场景，无法加载。");
        }
        else
        {
            if (!string.IsNullOrEmpty(config.targetScene))
                SceneManager.LoadScene(config.targetScene);
            else
                Debug.LogWarning("EndingManager: 未指定目标场景，将停留在当前场景。");
        }
    }

    private EndingConfig GetConfigForEnding(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.High:   return highEndingConfig;
            case EndingType.Normal: return normalEndingConfig;
            case EndingType.Low:    return lowEndingConfig;
            case EndingType.Death:  return deathEndingConfig;
            default: return null;
        }
    }

    private void DisablePlayerControls()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("EndingManager: 未找到玩家物体，无法禁用控制。");
            return;
        }

        PlayerMove move = player.GetComponent<PlayerMove>();
        if (move != null) move.enabled = false;

        PlayerJump jump = player.GetComponent<PlayerJump>();
        if (jump != null) jump.enabled = false;

        PlayerDash dash = player.GetComponent<PlayerDash>();
        if (dash != null) dash.enabled = false;

        MaskControl mask = player.GetComponent<MaskControl>();
        if (mask != null) mask.enabled = false;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        Debug.Log("玩家控制已禁用");
    }

    public enum EndingType
    {
        High,
        Normal,
        Low,
        Death
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
