using UnityEngine;
using System.Collections;

public class PlayerDash : MonoBehaviour
{
    [Header("冲刺参数")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private int maxAirDashes = 1;
    [SerializeField] private AnimationCurve speedCurve;
    
    [Header("冲刺效果")]
    [SerializeField] private bool freezeYVelocity = true;
    
    [Header("音频")]
    [SerializeField] private AudioSource dashSound;
    
    private Rigidbody2D rb;
    private PlayerJump playerJump;
    private WallCheck wallcheck;
    
    public bool isDashing { get; private set; }
    private bool canDash = true;
    private int airDashCount;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;
    
    // 用于检测 facingWall 状态变化
    private bool lastFacingWall = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerJump = GetComponent<PlayerJump>();
        wallcheck = GetComponentInChildren<WallCheck>();
        airDashCount = maxAirDashes;
    }
    
    void Update()
    {
        // 冷却计时
        if (!canDash && !isDashing)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f) canDash = true;
        }
        
        // 地面时重置空中冲刺次数（每帧都重置，因为落地后应该立即恢复）
        if (playerJump != null && playerJump.isGrounded)
            airDashCount = maxAirDashes;
        
        // 面向墙壁状态变化检测（仅在进入面向墙壁时重置一次）
        bool currentFacingWall = playerJump != null && !playerJump.isGrounded && playerJump.facingWall;
        if (currentFacingWall && !lastFacingWall)
        {
            // 刚进入面向墙壁状态，重置冲刺次数和冷却
            airDashCount = maxAirDashes;
            canDash = true;
            dashCooldownTimer = 0f;
            Debug.Log("进入面向墙壁状态，重置冲刺次数");
        }
        lastFacingWall = currentFacingWall;
        
        // 冲刺输入检测
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (canDash && !isDashing)
            {
                bool hasDashChance = (playerJump != null && playerJump.isGrounded) || airDashCount > 0;
                if (hasDashChance)
                    StartDash();
            }
        }
    }
    
    private void StartDash()
    {
        DetermineDashDirection();
        if (playerJump != null && !playerJump.isGrounded)
            airDashCount--;
        StartCoroutine(DashCoroutine());
    }
    
    private void DetermineDashDirection()
    {
        if (playerJump != null && playerJump.facingWall && wallcheck != null)
        {
            float wallDir = wallcheck.wallDirection;
            dashDirection = new Vector2(-wallDir, 0f);
        }
        else
        {
            float facing = Mathf.Sign(transform.localScale.x);
            dashDirection = new Vector2(facing, 0f);
        }
        if (freezeYVelocity) dashDirection.y = 0f;
        dashDirection.Normalize();
    }
    
    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        canDash = false;
        dashCooldownTimer = dashCooldown;
        
        float originalGravity = rb.gravityScale;
        if (freezeYVelocity) rb.gravityScale = 0f;
        
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            float speedMultiplier = speedCurve.Evaluate(t);
            float currentSpeed = dashSpeed * speedMultiplier;
            Vector2 velocity = dashDirection * currentSpeed;
            if (freezeYVelocity) velocity.y = 0f;
            rb.velocity = velocity;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        isDashing = false;
        rb.gravityScale = originalGravity;
        if (freezeYVelocity)
            rb.velocity = new Vector2(rb.velocity.x * 0.8f, 0f);
        
        if (dashSound != null) dashSound.Play();
    }
    
    // ========== 兼容旧脚本的公共方法 ==========
    public Vector2 GetDashDirection() => dashDirection;
    
    public void SetDashSpeed(float speed) => dashSpeed = speed;
    
    public int GetCurrentAirDashCount() => airDashCount;
    
    public void AddAirDash(int amount)
    {
        if (amount > 0)
            airDashCount = Mathf.Min(airDashCount + amount, maxAirDashes);
        else if (amount < 0)
            Debug.LogWarning("AddAirDash called with negative amount, ignored.");
    }
}