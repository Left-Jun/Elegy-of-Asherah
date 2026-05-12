using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

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
    private MaskControl maskControl;
    
    public bool isDashing { get; private set; }
    private bool canDash = true;
    private int airDashCount;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;
    private Coroutine afterImageCoroutine;

    [Header("冲刺拖影")]
    [SerializeField] private bool useAfterImages = true;
    [SerializeField] private float afterImageInterval = 0.035f;
    [SerializeField] private float afterImageLifetime = 0.28f;
    [SerializeField, Range(0f, 1f)] private float afterImageStartAlpha = 0.55f;
    [SerializeField] private Color afterImageTint = Color.white;
    [SerializeField] private int afterImageSortingOrderOffset = -1;

    [Header("Enhanced Dash Break")]
    [SerializeField] private bool enhancedDashBreaksObjects = true;
    [SerializeField] private string breakableWallTag = "BreakableWall";
    [SerializeField] private float breakAnimationDestroyDelay = 0.5f;
    [SerializeField] private int fallbackTileSearchRadius = 1;
    [SerializeField] private int fallbackTileBreakRadius = 0;
    [SerializeField] private float closeBreakProbeDistance = 0.5f;
    [SerializeField] private float closeBreakProbeRadius = 0.15f;
    
    // 用于检测 facingWall 状态变化
    private bool lastFacingWall = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerJump = GetComponent<PlayerJump>();
        wallcheck = GetComponentInChildren<WallCheck>();
        maskControl = GetComponent<MaskControl>();
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
        if (TryGetWallDashDirection(out Vector2 wallDashDirection))
        {
            dashDirection = wallDashDirection;
        }
        else
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float dashX = Mathf.Abs(horizontalInput) > 0.01f ? horizontalInput : transform.localScale.x;
            dashDirection = new Vector2(Mathf.Sign(dashX), 0f);
        }

        if (freezeYVelocity) dashDirection.y = 0f;
        dashDirection.Normalize();
    }

    private bool TryGetWallDashDirection(out Vector2 wallDashDirection)
    {
        wallDashDirection = Vector2.zero;
        if (playerJump == null || playerJump.isGrounded || wallcheck == null || !wallcheck.isWall || wallcheck.wallDirection == 0)
            return false;

        int wallDirection = wallcheck.wallDirection;
        if (ShouldDashIntoBreakableWall(wallcheck.wallCollider))
            wallDashDirection = new Vector2(wallDirection, 0f);
        else
            wallDashDirection = new Vector2(-wallDirection, 0f);

        return true;
    }

    private bool ShouldDashIntoBreakableWall(Collider2D wallCollider)
    {
        if (!enhancedDashBreaksObjects || wallCollider == null)
            return false;

        if (maskControl == null || maskControl.currentState != MaskControl.State.Enhanced)
            return false;

        return IsBreakableTarget(wallCollider);
    }
    
    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        canDash = false;
        dashCooldownTimer = dashCooldown;

        if (useAfterImages)
        {
            if (afterImageCoroutine != null)
                StopCoroutine(afterImageCoroutine);

            afterImageCoroutine = StartCoroutine(SpawnDashAfterImages());
        }
        
        float originalGravity = rb.gravityScale;
        if (freezeYVelocity) rb.gravityScale = 0f;
        TryBreakCloseDashTargets();
        
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            float speedMultiplier = speedCurve.Evaluate(t);
            float currentSpeed = dashSpeed * speedMultiplier;
            Vector2 velocity = dashDirection * currentSpeed;
            if (freezeYVelocity) velocity.y = 0f;
            rb.velocity = velocity;
            TryBreakCloseDashTargets();
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        isDashing = false;
        rb.gravityScale = originalGravity;
        if (freezeYVelocity)
            rb.velocity = new Vector2(rb.velocity.x * 0.8f, 0f);

        if (afterImageCoroutine != null)
        {
            StopCoroutine(afterImageCoroutine);
            afterImageCoroutine = null;
        }
        
        if (dashSound != null) dashSound.Play();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null || collision.collider == null)
            return;

        TryBreakDashTarget(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision == null || collision.collider == null)
            return;

        TryBreakDashTarget(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryBreakDashTarget(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryBreakDashTarget(collision);
    }

    public bool TryBreakDashTarget(Collider2D targetCollider)
    {
        if (!CanBreakDuringDash(targetCollider))
            return false;

        if (TryBreakTilemapTarget(targetCollider, out bool brokeTilemap))
        {
            if (brokeTilemap)
                KeepDashVelocity();

            return brokeTilemap;
        }

        GameObject breakableRoot = GetBreakableRoot(targetCollider);
        if (breakableRoot == null || !breakableRoot.activeSelf)
            return false;

        DisableBreakableColliders(breakableRoot);

        Animator breakAnimator = breakableRoot.GetComponent<Animator>();
        if (breakAnimator != null)
        {
            breakAnimator.SetTrigger("Break");
            Destroy(breakableRoot, Mathf.Max(0f, breakAnimationDestroyDelay));
        }
        else
        {
            breakableRoot.SetActive(false);
        }

        KeepDashVelocity();
        return true;
    }

    private void TryBreakCloseDashTargets()
    {
        if (!isDashing || !enhancedDashBreaksObjects)
            return;

        if (wallcheck != null && wallcheck.wallCollider != null)
        {
            TryBreakDashTarget(wallcheck.wallCollider);
        }

        if (dashDirection.sqrMagnitude <= 0.001f)
            return;

        float probeDistance = Mathf.Max(0f, closeBreakProbeDistance);
        if (probeDistance <= 0f)
            return;

        float probeRadius = Mathf.Max(0f, closeBreakProbeRadius);
        Vector2 origin = transform.position;
        Vector2 direction = dashDirection.normalized;

        if (probeRadius > 0f)
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, probeRadius, direction, probeDistance);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                    TryBreakDashTarget(hit.collider);
            }
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, probeDistance);
            if (hit.collider != null)
                TryBreakDashTarget(hit.collider);
        }
    }

    private bool CanBreakDuringDash(Collider2D targetCollider)
    {
        if (!enhancedDashBreaksObjects || targetCollider == null || !isDashing)
            return false;

        if (maskControl == null || maskControl.currentState != MaskControl.State.Enhanced)
            return false;

        return IsBreakableTarget(targetCollider);
    }

    private bool IsBreakableTarget(Collider2D targetCollider)
    {
        if (targetCollider == null)
            return false;

        if (GetBreakableTilemap(targetCollider) != null)
            return true;

        if (GetTilemap(targetCollider) != null && HasBreakableTag(targetCollider.transform))
            return true;

        return HasBreakableTag(targetCollider.transform);
    }

    private bool HasBreakableTag(Transform target)
    {
        while (target != null)
        {
            if (target.gameObject.tag == breakableWallTag)
                return true;

            target = target.parent;
        }

        return false;
    }

    private GameObject GetBreakableRoot(Collider2D targetCollider)
    {
        Transform current = targetCollider.transform;
        while (current != null)
        {
            if (current.gameObject.tag == breakableWallTag)
                return current.gameObject;

            current = current.parent;
        }

        return targetCollider.gameObject;
    }

    private bool TryBreakTilemapTarget(Collider2D targetCollider, out bool brokeTilemap)
    {
        brokeTilemap = false;
        Vector2 breakPoint = targetCollider.ClosestPoint(transform.position);

        BreakableTilemap breakableTilemap = GetBreakableTilemap(targetCollider);
        if (breakableTilemap != null)
        {
            brokeTilemap = breakableTilemap.TryBreakAtWorldPosition(breakPoint);
            return true;
        }

        Tilemap tilemap = GetTilemap(targetCollider);
        if (tilemap == null || !HasBreakableTag(targetCollider.transform))
            return false;

        brokeTilemap = BreakableTilemap.TryBreakTilemapAtWorldPosition(
            tilemap,
            breakPoint,
            fallbackTileSearchRadius,
            fallbackTileBreakRadius);

        return true;
    }

    private BreakableTilemap GetBreakableTilemap(Collider2D targetCollider)
    {
        BreakableTilemap breakableTilemap = targetCollider.GetComponent<BreakableTilemap>();
        if (breakableTilemap == null)
            breakableTilemap = targetCollider.GetComponentInParent<BreakableTilemap>();

        return breakableTilemap;
    }

    private Tilemap GetTilemap(Collider2D targetCollider)
    {
        Tilemap tilemap = targetCollider.GetComponent<Tilemap>();
        if (tilemap == null)
            tilemap = targetCollider.GetComponentInParent<Tilemap>();

        return tilemap;
    }

    private void DisableBreakableColliders(GameObject breakableRoot)
    {
        Collider2D[] colliders = breakableRoot.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D breakableCollider in colliders)
        {
            if (breakableCollider != null)
                breakableCollider.enabled = false;
        }
    }

    private void KeepDashVelocity()
    {
        if (rb != null && dashDirection.sqrMagnitude > 0.001f)
            rb.velocity = dashDirection.normalized * dashSpeed;
    }

    private IEnumerator SpawnDashAfterImages()
    {
        CreateAfterImage();

        float interval = Mathf.Max(0.01f, afterImageInterval);
        WaitForSeconds wait = new WaitForSeconds(interval);

        while (isDashing)
        {
            yield return wait;

            if (isDashing)
                CreateAfterImage();
        }

        afterImageCoroutine = null;
    }

    private void CreateAfterImage()
    {
        SpriteRenderer[] sourceRenderers = GetAfterImageSourceRenderers();
        if (sourceRenderers == null || sourceRenderers.Length == 0)
            return;

        GameObject afterImageRoot = new GameObject("Dash After Image");
        afterImageRoot.transform.SetPositionAndRotation(transform.position, transform.rotation);
        afterImageRoot.transform.localScale = Vector3.one;

        List<SpriteRenderer> afterImageRenderers = new List<SpriteRenderer>();
        List<float> initialAlphas = new List<float>();

        foreach (SpriteRenderer sourceRenderer in sourceRenderers)
        {
            if (sourceRenderer == null || !sourceRenderer.enabled || sourceRenderer.sprite == null || sourceRenderer.color.a <= 0f)
                continue;

            GameObject afterImagePart = new GameObject(sourceRenderer.gameObject.name + " After Image");
            afterImagePart.transform.SetPositionAndRotation(sourceRenderer.transform.position, sourceRenderer.transform.rotation);
            afterImagePart.transform.localScale = sourceRenderer.transform.lossyScale;
            afterImagePart.transform.SetParent(afterImageRoot.transform, true);

            SpriteRenderer afterImageRenderer = afterImagePart.AddComponent<SpriteRenderer>();
            CopySpriteRenderer(sourceRenderer, afterImageRenderer);

            Color afterImageColor = sourceRenderer.color;
            afterImageColor.r *= afterImageTint.r;
            afterImageColor.g *= afterImageTint.g;
            afterImageColor.b *= afterImageTint.b;
            afterImageColor.a = sourceRenderer.color.a * afterImageTint.a * afterImageStartAlpha;
            afterImageRenderer.color = afterImageColor;

            afterImageRenderers.Add(afterImageRenderer);
            initialAlphas.Add(afterImageColor.a);
        }

        if (afterImageRenderers.Count == 0)
        {
            Destroy(afterImageRoot);
            return;
        }

        StartCoroutine(FadeAndDestroyAfterImage(afterImageRoot, afterImageRenderers, initialAlphas));
    }

    private SpriteRenderer[] GetAfterImageSourceRenderers()
    {
        if (maskControl != null)
        {
            GameObject activeVisual = maskControl.GetActiveVisual();
            if (activeVisual != null)
                return activeVisual.GetComponentsInChildren<SpriteRenderer>(false);
        }

        return GetComponentsInChildren<SpriteRenderer>(false);
    }

    private void CopySpriteRenderer(SpriteRenderer sourceRenderer, SpriteRenderer targetRenderer)
    {
        targetRenderer.sprite = sourceRenderer.sprite;
        targetRenderer.flipX = sourceRenderer.flipX;
        targetRenderer.flipY = sourceRenderer.flipY;
        targetRenderer.drawMode = sourceRenderer.drawMode;
        targetRenderer.size = sourceRenderer.size;
        targetRenderer.tileMode = sourceRenderer.tileMode;
        targetRenderer.maskInteraction = sourceRenderer.maskInteraction;
        targetRenderer.spriteSortPoint = sourceRenderer.spriteSortPoint;
        targetRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
        targetRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        targetRenderer.sortingOrder = sourceRenderer.sortingOrder + afterImageSortingOrderOffset;
    }

    private IEnumerator FadeAndDestroyAfterImage(
        GameObject afterImageRoot,
        List<SpriteRenderer> afterImageRenderers,
        List<float> initialAlphas)
    {
        float lifetime = Mathf.Max(0.01f, afterImageLifetime);
        float elapsedTime = 0f;

        while (elapsedTime < lifetime)
        {
            float normalizedTime = elapsedTime / lifetime;
            for (int i = 0; i < afterImageRenderers.Count; i++)
            {
                SpriteRenderer renderer = afterImageRenderers[i];
                if (renderer == null)
                    continue;

                Color color = renderer.color;
                color.a = Mathf.Lerp(initialAlphas[i], 0f, normalizedTime);
                renderer.color = color;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(afterImageRoot);
    }
    
    // ========== 兼容旧脚本的公共方法 ==========
    public Vector2 GetDashDirection() => dashDirection;

    public bool IsDashing() => isDashing;
    
    public void SetDashSpeed(float speed) => dashSpeed = speed;

    public void SetDashDuration(float duration) => dashDuration = Mathf.Max(0f, duration);

    public void SetDashCooldown(float cooldown)
    {
        dashCooldown = Mathf.Max(0f, cooldown);
        if (!isDashing)
            dashCooldownTimer = Mathf.Min(dashCooldownTimer, dashCooldown);
    }

    public void SetMaxAirDashes(int count, bool refillCurrentCount = false)
    {
        maxAirDashes = Mathf.Max(0, count);
        if (refillCurrentCount || airDashCount > maxAirDashes)
            airDashCount = maxAirDashes;
    }
    
    public int GetCurrentAirDashCount() => airDashCount;
    
    public void AddAirDash(int amount)
    {
        if (amount > 0)
            airDashCount = Mathf.Min(airDashCount + amount, maxAirDashes);
        else if (amount < 0)
            Debug.LogWarning("AddAirDash called with negative amount, ignored.");
    }
}
