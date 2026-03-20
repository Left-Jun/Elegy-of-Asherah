using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerJump playerJump;
    private PlayerMove playerMove;
    private Wallcheck wallcheck;
    private MaskControl maskControl;
    
    [Header("冲刺基本设置")]
    [SerializeField] private float defaultDashSpeed = 30f;
    [SerializeField] private float defaultDashDuration = 0.15f;
    [SerializeField] private float defaultDashCooldown = 0.5f;
    
    [Header("冲刺类型设置")]
    public bool groundDashOnly = false;
    public bool airDash = true;
    [SerializeField] private int defaultMaxAirDashes = 1;
    public bool wallDash = true;
    public bool diagonalDash = true;
    
    [Header("冲刺方向设置")]
    public bool useInputDirection = true;
    public bool useFacingDirection = false;
    public Vector2 fixedDashDirection = Vector2.right;
    
    [Header("水平冲刺设置")]
    public bool horizontalDashOnly = false; // 新增：是否只进行X轴冲刺
    public bool freezeYVelocity = true; // 新增：是否冻结Y轴速度
    private float originalGravityScale; // 记录原始重力比例
    private float originalVerticalVelocity; // 记录冲刺前的垂直速度
    
    [Header("冲刺物理效果")]
    public bool ignoreGravity = true;
    public float dashEndDeceleration = 0.8f;
    
    [Header("冲刺效果")]
    [SerializeField] private AudioSource dashSound;
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField] private GameObject dashTrail;
    [SerializeField] private float trailDuration = 0.3f;
    
    // 实际使用的参数
    private float dashSpeed;
    private float dashDuration;
    private float dashCooldown;
    private int maxAirDashes;
    
    public bool isDashing { get; private set; }
    public bool canDash { get; private set; }
    public int airDashCount { get; private set; }
    
    // 内部变量
    private float dashCooldownTimer = 0f;
    private float dashDurationTimer = 0f;
    private Vector2 dashDirection = Vector2.right;
    private bool dashTrigger;
    private Coroutine dashCoroutine;
    
    // 输入相关
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode altDashKey = KeyCode.RightShift;
    
    // 冲刺轨迹相关
    private GameObject activeTrail;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerJump = GetComponent<PlayerJump>();
        playerMove = GetComponent<PlayerMove>();
        wallcheck = GetComponentInChildren<Wallcheck>();
        maskControl = GetComponent<MaskControl>();
        
        InitializeDashStats();
        
        canDash = true;
        airDashCount = maxAirDashes;
        
        // 记录原始重力比例
        originalGravityScale = rb.gravityScale;
        
        if (dashParticles == null)
        {
            dashParticles = GetComponentInChildren<ParticleSystem>();
        }
    }
    
    private void InitializeDashStats()
    {
        if (maskControl != null && maskControl.currentStats != null)
        {
            dashSpeed = maskControl.currentStats.dashSpeed;
            dashDuration = maskControl.currentStats.dashDuration;
            dashCooldown = maskControl.currentStats.dashCooldown;
            maxAirDashes = maskControl.currentStats.maxAirDashes;
        }
        else
        {
            dashSpeed = defaultDashSpeed;
            dashDuration = defaultDashDuration;
            dashCooldown = defaultDashCooldown;
            maxAirDashes = defaultMaxAirDashes;
        }
    }
    
    void Update()
    {
        UpdateDashStatsFromMaskControl();
        HandleDashInput();
        UpdateCooldown();
        UpdateDashState();
        UpdateAnimation();
    }
    
    void FixedUpdate()
    {
        if (isDashing)
        {
            ExecuteDash();
        }
    }
    
    private void UpdateDashStatsFromMaskControl()
    {
        if (maskControl != null && maskControl.currentStats != null)
        {
            dashSpeed = maskControl.currentStats.dashSpeed;
            dashCooldown = maskControl.currentStats.dashCooldown;
            maxAirDashes = maskControl.currentStats.maxAirDashes;
            
            if (airDashCount < maxAirDashes)
            {
                airDashCount = maxAirDashes;
            }
        }
    }
    
    private void HandleDashInput()
    {
        bool dashKeyDown = Input.GetKeyDown(dashKey) || Input.GetKeyDown(altDashKey);
        
        if (dashKeyDown && canDash && !dashTrigger)
        {
            dashTrigger = true;
        }
    }
    
    private void UpdateCooldown()
    {
        if (!canDash && !isDashing)
        {
            dashCooldownTimer -= Time.deltaTime;
            
            if (dashCooldownTimer <= 0f)
            {
                canDash = true;
                dashCooldownTimer = 0f;
            }
        }
    }
    
    private void UpdateDashState()
    {
        if (playerJump != null && playerJump.isGrounded)
        {
            airDashCount = maxAirDashes;
        }
        
        if (wallcheck != null && wallcheck.isWall)
        {
            RefreshDashOnWall();
        }
        
        bool dashAllowed = CheckDashConditions();
        
        if (dashTrigger && dashAllowed && !isDashing)
        {
            StartDash();
            dashTrigger = false;
        }
        
        if (isDashing)
        {
            dashDurationTimer -= Time.deltaTime;
            
            if (dashDurationTimer <= 0f)
            {
                EndDash();
            }
        }
    }
    
    private bool CheckDashConditions()
    {
        if (!canDash) return false;
        
        if (groundDashOnly && playerJump != null && !playerJump.isGrounded)
        {
            return false;
        }
        
        if (!groundDashOnly && airDash)
        {
            if (playerJump != null && !playerJump.isGrounded)
            {
                if (airDashCount <= 0)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    private void StartDash()
    {
        // 记录冲刺前的垂直速度
        originalVerticalVelocity = rb.velocity.y;
        
        // 确定冲刺方向
        DetermineDashDirection();
        
        // 如果启用水平冲刺，强制设置Y轴分量为0
        if (horizontalDashOnly)
        {
            dashDirection.y = 0;
            if (dashDirection.magnitude < 0.01f)
            {
                dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            }
            dashDirection.Normalize();
        }
        
        // 设置状态
        isDashing = true;
        dashDurationTimer = dashDuration;
        
        // 消耗空中冲刺次数
        if (playerJump != null && !playerJump.isGrounded && airDash)
        {
            airDashCount--;
        }
        
        // 冲刺时处理重力和垂直速度
        if (ignoreGravity || (horizontalDashOnly && freezeYVelocity))
        {
            // 记录原始重力比例
            originalGravityScale = rb.gravityScale;
            // 设置重力比例为0以冻结垂直运动
            rb.gravityScale = 0f;
            
            // 如果启用垂直速度冻结，立即将垂直速度设为0
            if (horizontalDashOnly && freezeYVelocity)
            {
                Vector2 currentVelocity = rb.velocity;
                currentVelocity.y = 0f;
                rb.velocity = currentVelocity;
            }
        }
        
        // 播放音效
        if (dashSound != null)
        {
            dashSound.Play();
        }
        
        // 播放粒子效果
        if (dashParticles != null)
        {
            dashParticles.Play();
            
            var main = dashParticles.main;
            main.startRotation = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg * Mathf.Deg2Rad;
        }
        
        // 生成冲刺拖尾
        if (dashTrail != null)
        {
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }
            dashCoroutine = StartCoroutine(SpawnDashTrail());
        }
        
        // 触发动画
        if (animator != null)
        {
            animator.SetTrigger("Dash");
        }
        
        Debug.Log($"冲刺开始！方向: {dashDirection}, 水平冲刺: {horizontalDashOnly}, 冻结Y轴: {freezeYVelocity}");
    }
    
    private void DetermineDashDirection()
    {
        if (useInputDirection)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            if (horizontal == 0 && vertical == 0)
            {
                dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                dashDirection = new Vector2(horizontal, diagonalDash ? vertical : 0).normalized;
            }
        }
        else if (useFacingDirection)
        {
            dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            dashDirection = fixedDashDirection.normalized;
        }
        
        // 检查蹬墙冲刺
        if (wallDash && wallcheck != null && wallcheck.isWall)
        {
            float wallDirection = wallcheck.wallDirection;
            
            if (Mathf.Abs(wallDirection) > 0.1f)
            {
                // 如果启用水平冲刺，蹬墙冲刺也限制为水平
                if (horizontalDashOnly)
                {
                    dashDirection = new Vector2(-wallDirection, 0f).normalized;
                }
                else
                {
                    dashDirection = new Vector2(-wallDirection, 0.3f).normalized;
                }
                Debug.Log("蹬墙冲刺！方向: " + dashDirection);
            }
        }
    }
    
    private void ExecuteDash()
    {
        Vector2 dashVelocity = dashDirection * dashSpeed;
        
        // 如果启用水平冲刺并冻结Y轴速度，保持垂直速度为0
        if (horizontalDashOnly && freezeYVelocity)
        {
            dashVelocity.y = 0f;
        }
        // 如果不是水平冲刺且不忽略重力，保持当前垂直速度
        else if (!ignoreGravity)
        {
            dashVelocity.y = rb.velocity.y;
        }
        
        rb.velocity = dashVelocity;
    }
    
    private void EndDash()
    {
        isDashing = false;
        canDash = false;
        dashCooldownTimer = dashCooldown;
        
        // 恢复重力
        if (ignoreGravity || (horizontalDashOnly && freezeYVelocity))
        {
            rb.gravityScale = originalGravityScale;
        }
        
        // 如果启用了水平冲刺，冲刺结束后重置垂直速度
        if (horizontalDashOnly)
        {
            Vector2 finalVelocity = rb.velocity;
            
            // 应用减速效果到水平方向
            if (dashEndDeceleration < 1f)
            {
                finalVelocity.x *= dashEndDeceleration;
            }
            
            // 垂直速度设为0，让重力立即开始作用
            finalVelocity.y = 0f;
            rb.velocity = finalVelocity;
            
            Debug.Log("水平冲刺结束，垂直速度重置为0");
        }
        else
        {
            // 正常冲刺的减速效果
            if (dashEndDeceleration < 1f)
            {
                rb.velocity *= dashEndDeceleration;
            }
        }
        
        // 停止粒子效果
        if (dashParticles != null)
        {
            dashParticles.Stop();
        }
        
        // 停止拖尾效果协程
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }
        
        // 销毁拖尾
        if (activeTrail != null)
        {
            Destroy(activeTrail, 0.5f);
        }
        
        Debug.Log("冲刺结束");
    }
    
    private IEnumerator SpawnDashTrail()
    {
        if (dashTrail == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < trailDuration && isDashing)
        {
            GameObject trailInstance = Instantiate(dashTrail, transform.position, Quaternion.identity);
            
            SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                SpriteRenderer trailRenderer = trailInstance.GetComponent<SpriteRenderer>();
                if (trailRenderer != null)
                {
                    trailRenderer.sprite = playerSprite.sprite;
                    trailRenderer.flipX = playerSprite.flipX;
                    trailRenderer.flipY = playerSprite.flipY;
                }
            }
            
            SpriteRenderer trailSprite = trailInstance.GetComponent<SpriteRenderer>();
            if (trailSprite != null)
            {
                trailSprite.color = new Color(1f, 1f, 1f, 0.5f);
            }
            
            if (activeTrail == null)
            {
                activeTrail = trailInstance;
            }
            else
            {
                Destroy(trailInstance, 0.1f);
            }
            
            elapsedTime += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isDashing", isDashing);
        }
    }
    
    private void RefreshDashOnWall()
    {
        if (playerJump != null && !playerJump.isGrounded)
        {
            airDashCount = maxAirDashes;
            Debug.Log("贴墙刷新冲刺次数，当前可用空中冲刺: " + airDashCount);
        }
    }
    
    // 外部调用的强制冲刺方法
    public void ForceDash(Vector2 direction, bool isHorizontal = false)
    {
        if (!isDashing)
        {
            dashDirection = direction.normalized;
            
            // 如果是水平冲刺，强制Y轴分量为0
            if (isHorizontal)
            {
                dashDirection.y = 0;
                if (dashDirection.magnitude < 0.01f)
                {
                    dashDirection = Vector2.right;
                }
                dashDirection.Normalize();
                horizontalDashOnly = true;
            }
            
            StartDash();
        }
    }
    
    // 重置冲刺状态
    public void ResetDash()
    {
        isDashing = false;
        canDash = true;
        airDashCount = maxAirDashes;
        dashCooldownTimer = 0f;
        
        // 恢复重力
        rb.gravityScale = originalGravityScale;
        
        // 停止所有协程
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }
    }
    
    // 检查是否正在冲刺
    public bool IsDashing()
    {
        return isDashing;
    }
    
    // 获取当前冲刺方向
    public Vector2 GetDashDirection()
    {
        return dashDirection;
    }
    
    // 设置是否可以冲刺
    public void SetCanDash(bool value)
    {
        canDash = value;
    }
    
    // 设置冲刺速度
    public void SetDashSpeed(float speed)
    {
        dashSpeed = speed;
    }
    
    // 增加空中冲刺次数
    public void AddAirDash(int amount = 1)
    {
        airDashCount = Mathf.Min(airDashCount + amount, maxAirDashes);
    }
    
    // 获取当前可用空中冲刺次数
    public int GetCurrentAirDashCount()
    {
        return airDashCount;
    }
    
    // 获取最大空中冲刺次数
    public int GetMaxAirDashCount()
    {
        return maxAirDashes;
    }
    
    // 设置是否只进行水平冲刺
    public void SetHorizontalDashOnly(bool value)
    {
        horizontalDashOnly = value;
    }
    
    // 设置是否冻结Y轴速度
    public void SetFreezeYVelocity(bool value)
    {
        freezeYVelocity = value;
    }
}