using UnityEngine;

public class Platformmove : MonoBehaviour
{
    [SerializeField] Transform PiontA, PiontB;
    private Transform Pos;
    [SerializeField] private float speed;

    private bool isBeingDestroyed = false;
    private Rigidbody2D rb;
    private Vector2 lastPosition;
    public Vector2 CurrentVelocity { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (PiontA != null && PiontB != null)
            Pos = PiontA;
        lastPosition = rb != null ? rb.position : (Vector2)transform.position;
    }

    void FixedUpdate()
    {
        if (PiontA == null || PiontB == null || Pos == null)
        {
            CurrentVelocity = Vector2.zero;
            return;
        }

        Vector2 currentPosition = rb != null ? rb.position : (Vector2)transform.position;

        if (Vector2.Distance(currentPosition, PiontA.position) < 0.1f)
            Pos = PiontB;
        else if (Vector2.Distance(currentPosition, PiontB.position) < 0.1f)
            Pos = PiontA;

        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, Pos.position, speed * Time.fixedDeltaTime);
        if (rb != null && rb.bodyType == RigidbodyType2D.Kinematic)
            rb.MovePosition(nextPosition);
        else
            transform.position = nextPosition;

        CurrentVelocity = (nextPosition - lastPosition) / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
        lastPosition = nextPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null || collision.gameObject == null) return;
        if (!collision.CompareTag("Player")) return;

        if (!isBeingDestroyed && gameObject != null && gameObject.activeInHierarchy)
        {
            PlayerMove playerMove = collision.GetComponent<PlayerMove>();
            if (playerMove != null)
                playerMove.SetCurrentPlatform(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null || collision.gameObject == null) return;
        if (!collision.CompareTag("Player")) return;

        // 如果平台正在销毁或已失活，则放弃处理（场景即将卸载，玩家也会被销毁）
        if (isBeingDestroyed || gameObject == null || !gameObject.activeInHierarchy)
        {
            return; // 直接返回，不进行任何操作
        }

        PlayerMove playerMove = collision.GetComponent<PlayerMove>();
        if (playerMove != null)
            playerMove.ClearCurrentPlatform(this);
    }

    private void OnDestroy()
    {
        isBeingDestroyed = true;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player == null) continue;

            PlayerMove playerMove = player.GetComponent<PlayerMove>();
            if (playerMove != null)
                playerMove.ClearCurrentPlatform(this);

            if (player.transform.parent == transform)
            {
                player.transform.SetParent(null);
            }
        }
    }
}
