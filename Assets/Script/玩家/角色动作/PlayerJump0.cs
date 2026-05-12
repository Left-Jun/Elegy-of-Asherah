using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    private Rigidbody2D rb;//刚体组件
    //private Animator animator;//动画组件
    [Header("音效设置")]
    [SerializeField]private AudioSource jumpSound;//跳跃音效
    [Header("跳跃力设置")]
    public float jumpForce;//跳跃力
    public float airjumpForce;//空中跳跃力
    public float additionalJumpForce;//追加跳跃力
    public float wallJumpForceX;//蹬墙跳跃力
    public float wallJumpForceY;//蹬墙跳跃力
    [Header("追加跳跃时间及加速度设置")]
    public float additionalJumpTime;//追加跳跃时间
    public float additionalFallForce; //下落加速度
    [Header("输入冷却设置（按键防抖）")]
    [SerializeField] private float inputCooldown = 0.1f; // 输入冷却时间（秒）
    // 输入冷却相关变量
    private float lastJumpInputTime = 0f; // 初始设为0确保第一次可以输入
    private bool inputOnCooldown = false;
    // 跳跃输入相关变量
    private bool jumpTrigger = false;      // 跳跃触发器（按下瞬间）
    private bool jumpHeld = false;         // 跳跃键长按状态
    private bool jumpReleased = false;     // 跳跃键松开状态
    // ===================================
    private float jumpTimeCounter;//跳跃时间计时器
    private bool airjumpChance;//空中跳跃机会
    private bool canAirJump = true;   // 默认允许

public void SetCanAirJump(bool can)
{
    canAirJump = can;
    if (!canAirJump)
    {
        // 如果当前状态不允许二段跳，则重置空中跳跃机会，防止残留
        airjumpChance = false;
    }
}
    //============================================================
    private WallCheck wallcheck;//贴墙状态
    private GroundCheck groundcheck;
    private PlayerMove playerMove;
    private Coroutine wallJumpCoroutine;//蹬墙跳协程引用
    //============================================================
    [Header("状态（通用）")]
    public bool walljumpHeld;//蹬墙跳长按状态
    public bool walljumpReleased;//蹬墙跳松开状态
    public bool toWallJump;//蹬墙跳状态
    //====================================================
    public bool isGrounded;//地面状态
    public bool isJumping;//跳跃状态
    public bool isFalling;//下落状态
    //====================================================
    public bool facingWall { get; private set; } // 是否面向墙壁（新增）

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //animator = GetComponent<Animator>();
        wallcheck = GetComponentInChildren<WallCheck>();
        groundcheck = GetComponent<GroundCheck>();
        playerMove = GetComponent<PlayerMove>();
    }

    // Update is called once per frame
    void Update()
    {
        //跳跃输入检测
        HandleJumpInput();
        //扶墙输入检测
        HandleWallJumpInput();
        // 更新输入冷却状态
        UpdateInputCooldown();
        //地面检测
        GroundeCheck();

        //蹬墙跳
        WallJumping();
        //跳跃
        Jumping();
        //更新跳跃状态
        Jumpingcheck();
        //更新下落状态
        Fallingcheck();
        //动画参数更新
        AnimationControler();
    }

//============================================================================================================================
    //跳跃函数
    private void Jumping()
    {    
        //重置空中跳跃机会(地面检测)
        if(isGrounded && !jumpHeld)//在地面且跳跃键未被按下
        {
            airjumpChance = true;//重置空中跳跃机会为真
        }
        
        //执行跳跃
        if(jumpTrigger && isGrounded && IsJumpKeyDown())//跳跃触发器被触发且在地面且跳跃键被按下s
        {
            jumpTimeCounter = 0f;//重置跳跃时间计时器
            jumpTrigger = false;//重置跳跃触发器
            rb.velocity = new Vector2(rb.velocity.x,jumpForce);//施加跳跃力
            PlayJumpSound();//播放跳跃音效
            airjumpChance = true;//重置空中跳跃机会为真
            walljumpHeld = false;//重置蹬墙跳长按状态
        }
        //追加跳跃(长按跳跃键)
        if(isJumping && jumpHeld && jumpTimeCounter < additionalJumpTime)//在跳跃状态且跳跃键被按下且未超过追加跳跃时间
        {
            rb.velocity += new Vector2(0,-Physics2D.gravity.y * Time.deltaTime* additionalJumpForce);//施加追加跳跃力
            jumpTimeCounter += Time.deltaTime;//增加跳跃时间计时器
        }
        //空中跳跃
        if(airjumpChance && !isGrounded && jumpTrigger && canAirJump)//在空中且空中跳跃未使用且跳跃键被按下
        {
            PlayJumpSound();//播放跳跃音效
            Animator currentAnim = GetCurrentAnimator();
            if (currentAnim != null) currentAnim.SetTrigger("isAirjump");//触发空中跳跃动画
            rb.velocity = new Vector2(rb.velocity.x,airjumpForce);//施加跳跃力
            airjumpChance = false;//设置空中跳跃已使用
            jumpTrigger = false;//重置跳跃触发器
            jumpTimeCounter = 0f;//重置跳跃时间计时器
            walljumpHeld = false;//重置蹬墙跳长按状态
        }
        //下降
        if(isFalling)
        {
            rb.velocity -= new Vector2(0,-Physics2D.gravity.y * Time.deltaTime* additionalFallForce);
        }
    }
//============================================================================================================================    
    //蹬墙跳函数
    private void WallJumping()
    {
        if (wallcheck == null)
        {
            facingWall = false;
            return;
        }

        // 是否主动朝墙按住方向键；不再依赖速度或朝向，避免竖直墙面上速度归零后无法攀附
        facingWall = wallcheck.isWall && IsPressingTowardWall();
        
        if (facingWall)
        {
            airjumpChance = true;//重置空中跳跃机会为真
        }

        float wallPlatformVelocityY = GetWallPlatformVelocity().y;

        if (facingWall && !isGrounded && !walljumpHeld)
        {
            float relativeVerticalVelocity = rb.velocity.y - wallPlatformVelocityY;
            rb.velocity = new Vector2(rb.velocity.x, relativeVerticalVelocity * 0.95f + wallPlatformVelocityY);//减少下落速度
        }
        if (facingWall && walljumpHeld && !isGrounded && !toWallJump)
        {
            float relativeVerticalVelocity = rb.velocity.y - wallPlatformVelocityY;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(relativeVerticalVelocity * 0.3f, -2f) + wallPlatformVelocityY);//减少下落速度
        }

        if (wallcheck.isWall && !isGrounded && jumpTrigger && IsJumpKeyDown())
        {
            PlayJumpSound();//播放跳跃音效
            float wallDirection = wallcheck.wallDirection; //获取墙壁方向
            Animator currentAnim = GetCurrentAnimator();
            if (currentAnim != null) currentAnim.SetTrigger("isWalljump");
            if (wallJumpCoroutine != null)
            {
                StopCoroutine(wallJumpCoroutine);
            }
            wallJumpCoroutine = StartCoroutine(ToWallJump());
            // 完全重置垂直速度，确保每次蹬墙跳获得统一的跳跃力
            Vector2 newVelocity = rb.velocity;
            newVelocity.y = 0;
            rb.velocity = newVelocity;
            rb.velocity = new Vector2(-wallDirection * wallJumpForceX, wallJumpForceY);
            if (playerMove != null)
                playerMove.FaceByHorizontalDirection(rb.velocity.x);
            airjumpChance = true;
            jumpTrigger = false;
            jumpTimeCounter = 0f;
        }
    }

    private Vector2 GetWallPlatformVelocity()
    {
        if (wallcheck == null || wallcheck.wallCollider == null)
            return Vector2.zero;

        Platformmove wallPlatform = wallcheck.wallCollider.GetComponentInParent<Platformmove>();
        return wallPlatform != null ? wallPlatform.CurrentVelocity : Vector2.zero;
    }

    //蹬墙跳协程
    IEnumerator ToWallJump()
    {
        toWallJump = true;
        yield return new WaitForSeconds(0.2f);
        toWallJump = false;
        wallJumpCoroutine = null;
    }
//============================================================================================================================
    //跳跃输入检测函数
    private void HandleJumpInput()
    {
        // 检测按键按下（跳跃触发器）
        if (!inputOnCooldown && IsJumpKeyDown()&& airjumpChance)
        {
            jumpTrigger = true;
            jumpHeld = true;
            lastJumpInputTime = Time.time;
            inputOnCooldown = true;
        }
        
        // 检测按键持续按下（长按状态）
        if (IsJumpKeyDown())
        {
            jumpHeld = true;
        }
        
        // 检测按键松开
        if (IsJumpKeyUp())
        {
            jumpReleased = true;
            jumpHeld = false;
        }
     }
    // 辅助方法：检查跳跃键按下
    private bool IsJumpKeyDown()
    {
        return Input.GetKeyDown(KeyCode.Space) || 
               Input.GetKeyDown(KeyCode.W) || 
               Input.GetKeyDown(KeyCode.UpArrow);
    }
    
    // 辅助方法：检查跳跃键持续按下
    private bool IsJumpKey()
    {
        return Input.GetKey(KeyCode.Space) || 
               Input.GetKey(KeyCode.W) || 
               Input.GetKey(KeyCode.UpArrow);
    }
    
    // 辅助方法：检查跳跃键松开
    private bool IsJumpKeyUp()
    {
        return Input.GetKeyUp(KeyCode.Space) || 
               Input.GetKeyUp(KeyCode.W) || 
               Input.GetKeyUp(KeyCode.UpArrow);
    }

    // 更新输入冷却状态
    private void UpdateInputCooldown()
    {
        if (inputOnCooldown)
        {
            if (Time.time - lastJumpInputTime >= inputCooldown)
            {
                inputOnCooldown = false;
            }
        }
    }
//============================================================================================================================
    //扶墙输入检测函数
    private void HandleWallJumpInput()
    {
        if (wallcheck == null)
        {
            walljumpHeld = false;
            walljumpReleased = true;
            return;
        }

        //检测扶墙按键按下
        if(IsWallKeyDown())
        {
            walljumpHeld = true;
        }
        //检测扶墙按键持续按下
        if(IsWallKey()&&wallcheck.isWall)
        {
            walljumpHeld = true;
            walljumpReleased = false;
        }
        //检测扶墙按键松开
        if(IsWallKeyUp())
        {
            walljumpReleased = true;
            walljumpHeld = false;
        }
    }
    //辅助方法：检查扶墙按键按下
    private bool IsWallKeyDown()
    {
        int wallDirection = wallcheck != null ? wallcheck.wallDirection : 0;
        return ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && wallDirection < 0) ||
               ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && wallDirection > 0);
    }
    //辅助方法：检查扶墙按键持续按下
    private bool IsWallKey()
    {
        return IsPressingTowardWall();
    }
    //辅助方法：检查扶墙按键松开
    private bool IsWallKeyUp()
    {
        int wallDirection = wallcheck != null ? wallcheck.wallDirection : 0;
        return ((Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow)) && wallDirection < 0) ||
               ((Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow)) && wallDirection > 0);
    }

    private bool IsPressingTowardWall()
    {
        if (wallcheck == null || wallcheck.wallDirection == 0)
            return false;

        return ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && wallcheck.wallDirection < 0) ||
               ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && wallcheck.wallDirection > 0);
    }
//============================================================================================================================
    //地面检测函数
    private void GroundeCheck()
    {
        isGrounded = groundcheck != null && groundcheck.isGrounded;
    }
    
    //跳跃状态更新函数
    private void Jumpingcheck()
    {
        isJumping = rb.velocity.y > 0.3f&& isGrounded == false;
    }

    //下落状态更新函数
    private void Fallingcheck()
    {
        isFalling = isJumping == false && isGrounded == false;
    }

//============================================================================================================================
    //动画控制函数
    private void AnimationControler()
    {
        Animator currentAnim = GetCurrentAnimator();
        if (currentAnim != null)
        {
            currentAnim.SetBool("isJumping", isJumping);
            currentAnim.SetBool("isFalling", isFalling);
            currentAnim.SetBool("isGrounded", isGrounded);
        }
    }
    // 辅助方法：获取当前有效的Animator
    private Animator GetCurrentAnimator()
    {
        MaskControl maskControl = GetComponent<MaskControl>();
        if (maskControl != null)
        {
            return maskControl.GetCurrentActiveAnimator();
        }
        return null;
    }

    private void PlayJumpSound()
    {
        if (jumpSound != null)
            jumpSound.Play();
    }
}
/*        //斜向上走路
        if(!jumpPressed && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x,rb.velocity.y * 0.5f);
        }
*/
