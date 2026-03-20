using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 单例实例
    public GameTimer gameTimer; // 将主相机上的GameTimer脚本拖拽到这里
    public TextMeshProUGUI timeResultText; // 显示最终时间的UI文本（可选）
    
    [Header("收集目标")]
    public int totalShardsRequired = 5; // 需要收集的总碎片数
    private int collectedShards = 0;    // 当前已收集数
    
    [Header("UI引用")]
    public TextMeshProUGUI shardCounterText;       // 将UI上的Text组件拖拽到这里
    public GameObject winScreen;        // 胜利画面UI（可选）
    
    [Header("结局设置")]
    public MaskControl playerMaskControl; // 将玩家的MaskControl脚本拖拽到这里
    
    void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 如果不需要跨场景，可以不用这行
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        UpdateShardUI(); // 初始化UI显示
        if (winScreen != null) winScreen.SetActive(false);
         // 确保计时器开始计时
        if (gameTimer != null)
        {
            // 游戏管理器启动时，计时器应该已经开始（在GameTimer的Start中）
            // 这里可以再加一个确保开始的调用
            gameTimer.StartTimer();
        }
    }
    
    // 被HurtCheck调用
    public void CollectShard()
    {
        collectedShards++;
        UpdateShardUI();
        
        // 检查是否达成收集目标
        if (collectedShards >= totalShardsRequired)
        {
            StartCoroutine(EndGameSequence());
        }
    }
    
    private void UpdateShardUI()
    {
        if (shardCounterText != null)
        {
            shardCounterText.text = $":{collectedShards}/{totalShardsRequired}";
            // 可以在这里根据collectedShards改变文本颜色
        }
    }
    
    private System.Collections.IEnumerator EndGameSequence()
    {
        Debug.Log("所有情绪碎片收集完毕！游戏即将结束...");
        
        // ===== 第一步：立即强制切换至结局音乐 =====
    MusicManager musicManager = FindObjectOfType<MusicManager>();
    if (musicManager != null)
    {
        musicManager.PlayEndingMusic();
        yield return new WaitForSeconds(0.1f); // 短暂延迟，确保音乐系统完全接管
    }

        //====第二步： 显示最终用时
    if (gameTimer != null)
    {
        gameTimer.StopTimer();
        string finalTime = gameTimer.GetFormattedTime();
        Debug.Log("最终用时: " + finalTime);
        
        // 如果有显示时间的UI，更新它
        if (timeResultText != null)
        {
            timeResultText.text = "Game Time: " + finalTime;
        }
    }
        // 1. 锁定玩家输入：禁用玩家控制相关的脚本
        PlayerMove playerMove = playerMaskControl.GetComponent<PlayerMove>();
        PlayerJump playerJump = playerMaskControl.GetComponent<PlayerJump>();
        PlayerDash playerDash = playerMaskControl.GetComponent<PlayerDash>();
        if (playerMove != null) playerMove.enabled = false;
        if (playerJump != null) playerJump.enabled = false;
        if (playerDash != null) playerDash.enabled = false;

        // 2. 核心：将角色情绪强制切换并锁定为“高兴”（Happiness）
        if (playerMaskControl != null)
        {
            playerMaskControl.SetEmotion(MaskControl.EmotionState.Neutral);
            playerMaskControl.enabled = false;
        //
                GameObject happyVisual = playerMaskControl.HappyVisual; // 需要确保happyVisual是public或你有getter方法
        
        if (happyVisual != null)
        {
            // 2. 在happyVisual下寻找两个子对象
            Transform normalFace = happyVisual.transform.Find("Happy_Normal");
            Transform trueFace = happyVisual.transform.Find("Happy_True");
            
            if (normalFace != null && trueFace != null)
            {
                // 3. 切换显示：隐藏普通脸，展示真笑脸
                normalFace.gameObject.SetActive(false);
                trueFace.gameObject.SetActive(true);
                Debug.Log("已切换至真正高兴的表情！");
            }
            else
            {
                Debug.LogWarning("未找到Happy_Normal或Happy_True子对象，请检查命名和层级。");
            }
        }
        }
        
        // 3. 播放一段“治愈”动画或特效（例如，面具破碎、露出真心的笑容）
        // Animator playerAnim = playerMaskControl.GetComponent<Animator>();
        // if (playerAnim != null) playerAnim.SetTrigger("TrueSmile");
        
        // 4. 短暂延迟，让玩家感受结局
        yield return new WaitForSeconds(2.0f);
        
        // 5. 显示胜利画面或直接结束游戏
        if (winScreen != null)
        {
            winScreen.SetActive(true);
            // 胜利画面可以有一个“按任意键返回菜单”的按钮
        }
        else
        {
            // 或者直接重新加载场景
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    // 供UI按钮调用：返回主菜单或重玩
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0); // 你的主菜单场景名
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}